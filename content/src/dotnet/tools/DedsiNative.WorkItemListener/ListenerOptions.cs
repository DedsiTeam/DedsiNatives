using System.Globalization;

namespace DedsiNative.WorkItemListener;

/// <summary>
/// 工作项监听器运行配置。
/// </summary>
public sealed class ListenerOptions
{
    /// <summary>
    /// Azure DevOps 组织名。
    /// </summary>
    public required string Organization { get; init; }

    /// <summary>
    /// Azure DevOps Project 名称。
    /// </summary>
    public required string Project { get; init; }

    /// <summary>
    /// Azure Repos 仓库名；为空时从 Git origin 推导。
    /// </summary>
    public string? Repository { get; init; }

    /// <summary>
    /// Azure DevOps 原始 PAT 或 Bearer Token。
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// Azure DevOps 认证方案，支持 Basic 和 Bearer。
    /// </summary>
    public required string AuthenticationScheme { get; init; }

    /// <summary>
    /// Git 主仓库根目录。
    /// </summary>
    public required string RepositoryPath { get; init; }

    /// <summary>
    /// PR 目标分支。
    /// </summary>
    public required string TargetBranch { get; init; }

    /// <summary>
    /// Copilot CLI 可执行文件。
    /// </summary>
    public required string CopilotCommand { get; init; }

    /// <summary>
    /// Copilot CLI 使用的可选模型。
    /// </summary>
    public string? CopilotModel { get; init; }

    /// <summary>
    /// 非交互环境中提供给 Copilot CLI 的可选认证 Token。
    /// </summary>
    public string? CopilotToken { get; init; }

    /// <summary>
    /// 正常轮询间隔。
    /// </summary>
    public TimeSpan PollInterval { get; init; }

    /// <summary>
    /// 轮询异常后的重试间隔。
    /// </summary>
    public TimeSpan RetryInterval { get; init; }

    /// <summary>
    /// 单个 Copilot CLI 进程超时。
    /// </summary>
    public TimeSpan CopilotTimeout { get; init; }

    /// <summary>
    /// 工作项领取租约时长。
    /// </summary>
    public TimeSpan LeaseDuration { get; init; }

    /// <summary>
    /// 活跃任务续租间隔。
    /// </summary>
    public TimeSpan HeartbeatInterval { get; init; }

    /// <summary>
    /// HTTP 请求超时秒数。
    /// </summary>
    public int HttpTimeoutSeconds { get; init; }

    /// <summary>
    /// 单次最多并行执行的工作项数。
    /// </summary>
    public int MaxParallelism { get; init; }

    /// <summary>
    /// 单个工作项最多自动尝试次数。
    /// </summary>
    public int MaxAttempts { get; init; }

    /// <summary>
    /// Copilot Autopilot 最大自动续轮次数。
    /// </summary>
    public int MaxAutopilotContinues { get; init; }

    /// <summary>
    /// Copilot 同时运行的研发子智能体上限。
    /// </summary>
    public int MaxSubagents { get; init; }

    /// <summary>
    /// 是否只运行一次轮询。
    /// </summary>
    public bool RunOnce { get; init; }

    /// <summary>
    /// 是否为 Copilot CLI 启用本地 sandbox。
    /// </summary>
    public bool UseSandbox { get; init; }

    /// <summary>
    /// 是否为新 PR 启用 autocomplete。
    /// </summary>
    public bool EnableAutoComplete { get; init; }

    /// <summary>
    /// PR 合并后是否清理本地 worktree。
    /// </summary>
    public bool CleanupMergedWorktrees { get; init; }

    /// <summary>
    /// 从环境变量和仓库根目录的 .env.local 加载配置。
    /// </summary>
    /// <param name="args">
    /// 命令行参数。
    /// </param>
    /// <returns>
    /// 合并后的监听器配置。
    /// </returns>
    public static ListenerOptions Load(string[] args)
    {
        var requestedRepository = Environment.GetEnvironmentVariable("WORK_ITEM_LISTENER_REPOSITORY_PATH");
        var repositoryPath = DiscoverRepositoryRoot(
            string.IsNullOrWhiteSpace(requestedRepository) ? Environment.CurrentDirectory : requestedRepository);
        var fileValues = ReadEnvironmentFile(Path.Combine(repositoryPath, ".env.local"));

        string? Get(string key)
        {
            var environmentValue = Environment.GetEnvironmentVariable(key);
            return string.IsNullOrWhiteSpace(environmentValue)
                ? fileValues.GetValueOrDefault(key)
                : environmentValue;
        }

        var runOnce = args.Contains("--once", StringComparer.OrdinalIgnoreCase)
            || GetBoolean(Get("WORK_ITEM_LISTENER_RUN_ONCE"), false);

        return new ListenerOptions
        {
            Organization = Get("ADO_ORG") ?? "{{ADO_ORG}}",
            Project = Get("ADO_PROJECT") ?? "{{ADO_PROJECT}}",
            Repository = Get("ADO_REPOSITORY"),
            Token = Get("ADO_PAT") ?? Get("ADO_TOKEN") ?? string.Empty,
            AuthenticationScheme = Get("ADO_AUTH_SCHEME") ?? "Basic",
            RepositoryPath = repositoryPath,
            TargetBranch = Get("WORK_ITEM_LISTENER_TARGET_BRANCH") ?? "main",
            CopilotCommand = Get("WORK_ITEM_LISTENER_COPILOT_COMMAND") ?? "copilot",
            CopilotModel = Get("WORK_ITEM_LISTENER_COPILOT_MODEL"),
            CopilotToken = Get("COPILOT_GITHUB_TOKEN"),
            PollInterval = TimeSpan.FromSeconds(GetInt(Get("WORK_ITEM_LISTENER_POLL_SECONDS"), 60)),
            RetryInterval = TimeSpan.FromSeconds(GetInt(Get("WORK_ITEM_LISTENER_RETRY_SECONDS"), 300)),
            CopilotTimeout = TimeSpan.FromMinutes(GetInt(Get("WORK_ITEM_LISTENER_COPILOT_TIMEOUT_MINUTES"), 120)),
            LeaseDuration = TimeSpan.FromMinutes(GetInt(Get("WORK_ITEM_LISTENER_LEASE_MINUTES"), 30)),
            HeartbeatInterval = TimeSpan.FromMinutes(GetInt(Get("WORK_ITEM_LISTENER_HEARTBEAT_MINUTES"), 5)),
            HttpTimeoutSeconds = GetInt(Get("WORK_ITEM_LISTENER_HTTP_TIMEOUT_SECONDS"), 120),
            MaxParallelism = GetInt(Get("WORK_ITEM_LISTENER_MAX_PARALLELISM"), 2),
            MaxAttempts = GetInt(Get("WORK_ITEM_LISTENER_MAX_ATTEMPTS"), 3),
            MaxAutopilotContinues = GetInt(Get("WORK_ITEM_LISTENER_MAX_AUTOPILOT_CONTINUES"), 10),
            MaxSubagents = GetInt(Get("WORK_ITEM_LISTENER_MAX_SUBAGENTS"), 2),
            RunOnce = runOnce,
            UseSandbox = GetBoolean(Get("WORK_ITEM_LISTENER_USE_SANDBOX"), true),
            EnableAutoComplete = GetBoolean(Get("WORK_ITEM_LISTENER_AUTO_COMPLETE"), true),
            CleanupMergedWorktrees = GetBoolean(Get("WORK_ITEM_LISTENER_CLEANUP_MERGED_WORKTREES"), true),
        };
    }

    /// <summary>
    /// 验证启动所需的配置和目录。
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Organization) || Organization.Contains("{{", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("ADO_ORG is not configured.");
        }

        if (string.IsNullOrWhiteSpace(Project) || Project.Contains("{{", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("ADO_PROJECT is not configured.");
        }

        if (string.IsNullOrWhiteSpace(Token))
        {
            throw new InvalidOperationException("ADO_PAT or ADO_TOKEN is not configured in .env.local or the process environment.");
        }

        if (!Directory.Exists(RepositoryPath) ||
            (!Directory.Exists(Path.Combine(RepositoryPath, ".git")) && !File.Exists(Path.Combine(RepositoryPath, ".git"))))
        {
            throw new InvalidOperationException($"RepositoryPath is not a Git repository: {RepositoryPath}");
        }

        if (!AuthenticationScheme.Equals("Basic", StringComparison.OrdinalIgnoreCase) &&
            !AuthenticationScheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("ADO_AUTH_SCHEME must be Basic or Bearer.");
        }

        if (MaxParallelism is < 1 or > 16)
        {
            throw new InvalidOperationException("WORK_ITEM_LISTENER_MAX_PARALLELISM must be between 1 and 16.");
        }

        if (MaxAttempts is < 1 or > 20)
        {
            throw new InvalidOperationException("WORK_ITEM_LISTENER_MAX_ATTEMPTS must be between 1 and 20.");
        }

        if (MaxSubagents is < 1 or > 8)
        {
            throw new InvalidOperationException("WORK_ITEM_LISTENER_MAX_SUBAGENTS must be between 1 and 8.");
        }

        if (HeartbeatInterval >= LeaseDuration)
        {
            throw new InvalidOperationException("Heartbeat interval must be shorter than the lease duration.");
        }
    }

    private static string DiscoverRepositoryRoot(string? startPath)
    {
        var current = new DirectoryInfo(Path.GetFullPath(startPath ?? Environment.CurrentDirectory));
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, ".git")) ||
                File.Exists(Path.Combine(current.FullName, ".git")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return Path.GetFullPath(startPath ?? Environment.CurrentDirectory);
    }

    private static Dictionary<string, string> ReadEnvironmentFile(string path)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(path))
        {
            return values;
        }

        foreach (var sourceLine in File.ReadLines(path))
        {
            var line = sourceLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim();
            if (value.Length >= 2 &&
                ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\'')))
            {
                value = value[1..^1];
            }

            values[key] = value;
        }

        return values;
    }

    private static int GetInt(string? value, int defaultValue)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : defaultValue;
    }

    private static bool GetBoolean(string? value, bool defaultValue)
    {
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }
}
