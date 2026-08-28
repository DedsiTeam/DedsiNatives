namespace DedsiNative.WorkItemListener;

/// <summary>
/// 为无人值守工作项创建隔离分支和 Git worktree，并负责提交与推送。
/// </summary>
public sealed class GitWorktreeManager
{
    private static readonly TimeSpan GitTimeout = TimeSpan.FromMinutes(10);
    private readonly ProcessRunner _processes;
    private readonly ListenerOptions _options;

    /// <summary>
    /// 初始化 Git worktree 管理器。
    /// </summary>
    public GitWorktreeManager(ProcessRunner processes, ListenerOptions options)
    {
        _processes = processes;
        _options = options;
    }

    /// <summary>
    /// 为工作项从远程目标分支创建唯一 worktree。
    /// </summary>
    public async Task<WorktreeContext> CreateAsync(
        int workItemId,
        int attempt,
        string runId,
        CancellationToken cancellationToken)
    {
        var shortRunId = NormalizeRunId(runId);
        var branch = $"copilot/wi-{workItemId}-a{attempt}-{shortRunId}";
        var worktreeRoot = $"{_options.RepositoryPath}.worktrees";
        var worktreePath = Path.Combine(worktreeRoot, $"wi-{workItemId}-{shortRunId}");
        Directory.CreateDirectory(worktreeRoot);

        await RunGitCheckedAsync(
            new[] { "fetch", "origin", _options.TargetBranch },
            _options.RepositoryPath,
            cancellationToken);
        await RunGitCheckedAsync(
            new[]
            {
                "worktree",
                "add",
                "-b",
                branch,
                worktreePath,
                $"origin/{_options.TargetBranch}",
            },
            _options.RepositoryPath,
            cancellationToken);
        return new WorktreeContext(worktreePath, branch, runId);
    }

    /// <summary>
    /// 判断 worktree 是否包含待提交变更。
    /// </summary>
    public async Task<bool> HasChangesAsync(
        WorktreeContext worktree,
        CancellationToken cancellationToken)
    {
        var result = await RunGitCheckedAsync(
            new[] { "status", "--porcelain" },
            worktree.Path,
            cancellationToken);
        return !string.IsNullOrWhiteSpace(result.StandardOutput);
    }

    /// <summary>
    /// 提交当前 worktree 的全部变更并推送唯一分支。
    /// </summary>
    /// <returns>
    /// 新 commit SHA。
    /// </returns>
    public async Task<string> CommitAndPushAsync(
        WorktreeContext worktree,
        WorkItemSnapshot workItem,
        CancellationToken cancellationToken)
    {
        await RunGitCheckedAsync(new[] { "add", "-A" }, worktree.Path, cancellationToken);
        await RunGitCheckedAsync(
            new[] { "commit", "-m", $"feat: complete WI #{workItem.Id}", "-m", $"AB#{workItem.Id}" },
            worktree.Path,
            cancellationToken);
        await RunGitCheckedAsync(
            new[] { "push", "--set-upstream", "origin", worktree.Branch },
            worktree.Path,
            cancellationToken);
        var result = await RunGitCheckedAsync(new[] { "rev-parse", "HEAD" }, worktree.Path, cancellationToken);
        return result.StandardOutput.Trim();
    }

    /// <summary>
    /// 从配置或 origin URL 确定 Azure Repos 仓库名。
    /// </summary>
    public async Task<string> ResolveRepositoryNameAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_options.Repository))
        {
            return _options.Repository;
        }

        var result = await RunGitCheckedAsync(
            new[] { "remote", "get-url", "origin" },
            _options.RepositoryPath,
            cancellationToken);
        var remote = result.StandardOutput.Trim().TrimEnd('/');
        var gitMarker = remote.LastIndexOf("/_git/", StringComparison.OrdinalIgnoreCase);
        if (gitMarker >= 0)
        {
            return Uri.UnescapeDataString(remote[(gitMarker + "/_git/".Length)..]);
        }

        var segments = remote.Split('/', ':');
        var last = segments.LastOrDefault(value => value.Length > 0) ?? string.Empty;
        if (last.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
        {
            last = last[..^4];
        }

        if (string.IsNullOrWhiteSpace(last))
        {
            throw new InvalidOperationException("Unable to derive ADO_REPOSITORY from git origin.");
        }

        return Uri.UnescapeDataString(last);
    }

    /// <summary>
    /// PR 已合并后安全删除对应本地 worktree 和本地分支。
    /// </summary>
    public async Task CleanupMergedAsync(
        int workItemId,
        string runId,
        CancellationToken cancellationToken)
    {
        if (!_options.CleanupMergedWorktrees)
        {
            return;
        }

        var shortRunId = NormalizeRunId(runId);
        var worktreePath = Path.Combine($"{_options.RepositoryPath}.worktrees", $"wi-{workItemId}-{shortRunId}");
        if (!Directory.Exists(worktreePath))
        {
            return;
        }

        var branchResult = await RunGitCheckedAsync(
            new[] { "-C", worktreePath, "branch", "--show-current" },
            _options.RepositoryPath,
            cancellationToken);
        var branch = branchResult.StandardOutput.Trim();
        await RunGitCheckedAsync(
            new[] { "worktree", "remove", worktreePath },
            _options.RepositoryPath,
            cancellationToken);
        if (!string.IsNullOrWhiteSpace(branch))
        {
            await RunGitCheckedAsync(
                new[] { "branch", "-D", branch },
                _options.RepositoryPath,
                cancellationToken);
        }
    }

    private async Task<ProcessResult> RunGitCheckedAsync(
        IEnumerable<string> arguments,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        var result = await _processes.RunAsync(
            "git",
            arguments,
            workingDirectory,
            GitTimeout,
            null,
            cancellationToken);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Git command failed with exit code {result.ExitCode}: {Truncate(result.StandardError, 2000)}");
        }

        return result;
    }

    private static string NormalizeRunId(string runId)
    {
        var normalized = new string(runId.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        return normalized.Length <= 12 ? normalized : normalized[..12];
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
