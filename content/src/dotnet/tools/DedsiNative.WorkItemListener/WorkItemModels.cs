namespace DedsiNative.WorkItemListener;

/// <summary>
/// Azure DevOps 工作项的监听器所需快照。
/// </summary>
public sealed record WorkItemSnapshot(
    int Id,
    int Revision,
    string Title,
    string Description,
    string AcceptanceCriteria,
    string State,
    TagSet Tags);

/// <summary>
/// 已成功领取的工作项及其运行身份。
/// </summary>
public sealed record ClaimedWorkItem(
    WorkItemSnapshot WorkItem,
    string RunId,
    int Attempt,
    DateTimeOffset LeaseUntil);

/// <summary>
/// 为工作项创建的 Git worktree。
/// </summary>
public sealed record WorktreeContext(
    string Path,
    string Branch,
    string RunId);

/// <summary>
/// Azure Repos Pull Request 摘要。
/// </summary>
public sealed record PullRequestSnapshot(
    int Id,
    string Status,
    string? CreatedById,
    string? WebUrl);

/// <summary>
/// Copilot CLI 执行结果状态。
/// </summary>
public enum CopilotResultStatus
{
    /// <summary>
    /// Copilot 已完成实现和验证。
    /// </summary>
    Completed,

    /// <summary>
    /// 工作项存在业务、权限或契约阻塞。
    /// </summary>
    Blocked,

    /// <summary>
    /// Copilot 执行失败，可以根据尝试上限重试。
    /// </summary>
    Failed,

    /// <summary>
    /// Copilot 进程超过配置时限。
    /// </summary>
    TimedOut,
}

/// <summary>
/// Copilot CLI 的进程结果与最终摘要。
/// </summary>
public sealed record CopilotExecutionResult(
    CopilotResultStatus Status,
    int ExitCode,
    string Summary,
    string StandardError);

/// <summary>
/// 外部进程执行结果。
/// </summary>
public sealed record ProcessResult(
    int ExitCode,
    string StandardOutput,
    string StandardError,
    bool TimedOut);
