using System.Text.RegularExpressions;

namespace DedsiNative.WorkItemListener;

/// <summary>
/// 在隔离 worktree 中以非交互 Autopilot 模式调用 GitHub Copilot CLI。
/// </summary>
public sealed partial class CopilotCliRunner
{
    private readonly ProcessRunner _processes;
    private readonly ListenerOptions _options;

    /// <summary>
    /// 初始化 Copilot CLI 执行器。
    /// </summary>
    public CopilotCliRunner(ProcessRunner processes, ListenerOptions options)
    {
        _processes = processes;
        _options = options;
    }

    /// <summary>
    /// 执行一个工作项，直到 Copilot 明确返回协议终态或进程失败。
    /// </summary>
    public async Task<CopilotExecutionResult> RunAsync(
        ClaimedWorkItem claimed,
        WorktreeContext worktree,
        CancellationToken cancellationToken)
    {
        var arguments = new List<string>
        {
            "--autopilot",
            "--no-ask-user",
            "--allow-all-tools",
            "--no-auto-update",
            "--no-bash-env",
            "--no-color",
            "--no-remote",
            "--no-remote-export",
            $"--max-autopilot-continues={_options.MaxAutopilotContinues}",
            "--output-format=text",
            "--stream=off",
            "--disable-builtin-mcps",
            "--deny-tool=shell(git push)",
            "--deny-tool=shell(git commit)",
            "--deny-tool=shell(git reset)",
            "--deny-tool=shell(git clean)",
            "--deny-tool=shell(git checkout)",
            "--deny-tool=shell(git switch)",
            "--deny-tool=shell(git merge)",
            "--deny-tool=shell(git worktree)",
            "--deny-tool=shell(az)",
            "-s",
            "-C",
            worktree.Path,
        };
        if (_options.UseSandbox)
        {
            arguments.Add("--experimental");
            arguments.Add("--sandbox");
        }

        if (!string.IsNullOrWhiteSpace(_options.CopilotModel))
        {
            arguments.Add($"--model={_options.CopilotModel}");
        }

        var prompt = BuildPrompt(claimed.WorkItem, claimed.Attempt, claimed.RunId);

        var environment = new Dictionary<string, string?>
        {
            ["GITHUB_COPILOT_PROMPT_MODE_EXTENSIONS"] = "false",
            ["GITHUB_COPILOT_PROMPT_MODE_REPO_HOOKS"] = "false",
            ["GITHUB_COPILOT_PROMPT_MODE_WORKSPACE_MCP"] = "false",
            ["COPILOT_SUBAGENT_MAX_CONCURRENT"] = _options.MaxSubagents.ToString(),
            ["COPILOT_SUBAGENT_MAX_DEPTH"] = "1",
            ["COPILOT_TASK_WAIT_TIMEOUT_SECONDS"] = ((int)_options.CopilotTimeout.TotalSeconds).ToString(),
            // Listener 的 Azure DevOps 凭据不得进入模型可执行的子进程环境。
            ["ADO_PAT"] = null,
            ["ADO_TOKEN"] = null,
            ["ADO_MCP_AUTH_TOKEN"] = null,
            ["AZURE_DEVOPS_EXT_PAT"] = null,
            ["PERSONAL_ACCESS_TOKEN"] = null,
            ["SYSTEM_ACCESSTOKEN"] = null,
        };
        if (!string.IsNullOrWhiteSpace(_options.CopilotToken))
        {
            environment["COPILOT_GITHUB_TOKEN"] = _options.CopilotToken;
        }
        var processResult = await _processes.RunAsync(
            _options.CopilotCommand,
            arguments,
            worktree.Path,
            _options.CopilotTimeout,
            environment,
            prompt,
            cancellationToken);
        return ParseResult(processResult);
    }

    /// <summary>
    /// 生成单工作项、无 Azure DevOps 写权限的实现提示。
    /// </summary>
    public static string BuildPrompt(WorkItemSnapshot workItem, int attempt, string runId)
    {
        return $$"""
            你正在无人值守模式下实现一个由 DedsiNative.WorkItemListener 已原子领取的 Azure DevOps 工作项。

            工作项 ID：{{workItem.Id}}
            标题：{{workItem.Title}}
            当前尝试：{{attempt}}
            Run ID：{{runId}}

            Description：
            {{workItem.Description}}

            Acceptance Criteria：
            {{workItem.AcceptanceCriteria}}

            执行边界：
            - 只处理这个工作项，不查询、领取或修改其他 Azure DevOps 工作项。
            - Listener 负责 Git worktree、commit、push、PR、autocomplete 和状态回写；你不得执行这些操作。
            - 完整读取仓库根 AGENTS.md 和 .github/copilot-instructions.md，按变更范围遵守代码与验证规则。
            - 先检查现有代码和真实契约，再完成实现；不得臆造业务规则、公开 API、权限或数据语义。
            - 后端范围可委派仓库 custom agent `backend`，前端范围可委派 `frontend`；全栈范围可在固定最小接口契约后并行委派两者。
            - 不得委派 `product-manager` 或 `prototype`；它们只供工作项进入自动编码队列前的产品编写和静态原型评审使用。
            - 子智能体只处理各自代码范围且不得继续嵌套；你负责集成双方结果、解决非业务语义冲突并运行最终验证。
            - 在当前 worktree 中完成必要代码、配置、迁移和文档修改，并运行适用构建与测试。
            - 不读取或输出秘密，不执行 database update、强制重置、清理仓库或其他破坏性操作。
            - 无法确认关键业务规则、接口契约、权限、秘密或危险操作时，不要猜测，返回 blocked。

            最终回复必须以且只能以以下三种机器可读行之一结束：
            DEDSI_RESULT=completed
            DEDSI_RESULT=blocked
            DEDSI_RESULT=failed

            在标记行之前简要说明修改、验证结果或阻塞证据。只有实际修改完成且适用验证通过时才能返回 completed。
            """;
    }

    /// <summary>
    /// 按退出码、超时和最终协议标记解析 Copilot CLI 结果。
    /// </summary>
    public static CopilotExecutionResult ParseResult(ProcessResult processResult)
    {
        if (processResult.TimedOut)
        {
            return new CopilotExecutionResult(
                CopilotResultStatus.TimedOut,
                processResult.ExitCode,
                Truncate(processResult.StandardOutput, 3000),
                Truncate(processResult.StandardError, 2000));
        }

        if (processResult.ExitCode != 0)
        {
            return new CopilotExecutionResult(
                CopilotResultStatus.Failed,
                processResult.ExitCode,
                Truncate(processResult.StandardOutput, 3000),
                Truncate(processResult.StandardError, 2000));
        }

        var matches = ResultMarker().Matches(processResult.StandardOutput);
        if (matches.Count == 0)
        {
            return new CopilotExecutionResult(
                CopilotResultStatus.Failed,
                processResult.ExitCode,
                "Copilot CLI exited without a DEDSI_RESULT marker. " + Truncate(processResult.StandardOutput, 2500),
                Truncate(processResult.StandardError, 2000));
        }

        var status = matches[^1].Groups[1].Value.ToLowerInvariant() switch
        {
            "completed" => CopilotResultStatus.Completed,
            "blocked" => CopilotResultStatus.Blocked,
            _ => CopilotResultStatus.Failed,
        };
        return new CopilotExecutionResult(
            status,
            processResult.ExitCode,
            Truncate(processResult.StandardOutput, 3000),
            Truncate(processResult.StandardError, 2000));
    }

    [GeneratedRegex(@"(?im)^DEDSI_RESULT=(completed|blocked|failed)\s*$")]
    private static partial Regex ResultMarker();

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
