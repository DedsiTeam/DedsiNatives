namespace DedsiNative.WorkItemListener;

/// <summary>
/// 轮询 Azure DevOps 队列，并以固定并发度执行完整无人值守交付流程。
/// </summary>
public sealed class WorkItemPollingService
{
    private readonly ListenerOptions _options;
    private readonly AzureDevOpsClient _azureDevOps;
    private readonly GitWorktreeManager _worktrees;
    private readonly CopilotCliRunner _copilot;
    private string? _repositoryName;

    /// <summary>
    /// 初始化工作项轮询服务。
    /// </summary>
    public WorkItemPollingService(
        ListenerOptions options,
        AzureDevOpsClient azureDevOps,
        GitWorktreeManager worktrees,
        CopilotCliRunner copilot)
    {
        _options = options;
        _azureDevOps = azureDevOps;
        _worktrees = worktrees;
        _copilot = copilot;
    }

    /// <summary>
    /// 持续轮询，直到收到取消信号；RunOnce 模式只执行一轮。
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _repositoryName = await _worktrees.ResolveRepositoryNameAsync(cancellationToken);
        Console.WriteLine(
            $"Listening to {_options.Organization}/{_options.Project}/{_repositoryName}; " +
            $"parallelism={_options.MaxParallelism}; target={_options.TargetBranch}.");

        while (!cancellationToken.IsCancellationRequested)
        {
            var delay = _options.PollInterval;
            try
            {
                await PollOnceAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                delay = _options.RetryInterval;
                Console.Error.WriteLine($"Polling failed at {DateTimeOffset.UtcNow:O}: {exception}");
            }

            if (_options.RunOnce)
            {
                return;
            }

            await Task.Delay(delay, cancellationToken);
        }
    }

    /// <summary>
    /// 完成一轮 PR 对账、过期租约恢复和新工作项执行。
    /// </summary>
    public async Task PollOnceAsync(CancellationToken cancellationToken)
    {
        var queue = await _azureDevOps.GetQueueAsync(cancellationToken);
        if (queue.Length == 0)
        {
            Console.WriteLine($"{DateTimeOffset.UtcNow:O} Queue is empty.");
            return;
        }

        await ReconcilePullRequestsAsync(queue, cancellationToken);
        await RecoverExpiredLeasesAsync(queue, cancellationToken);
        await BlockExhaustedRetriesAsync(queue, cancellationToken);

        var candidates = SelectCandidates(queue);
        if (candidates.Length == 0)
        {
            Console.WriteLine($"{DateTimeOffset.UtcNow:O} No claimable work items.");
            return;
        }

        await Task.WhenAll(candidates.Select(workItem => ExecuteWorkItemAsync(workItem, cancellationToken)));
    }

    private async Task ExecuteWorkItemAsync(
        WorkItemSnapshot workItem,
        CancellationToken cancellationToken)
    {
        var attempt = workItem.Tags.GetAttempt() + 1;
        if (attempt > _options.MaxAttempts)
        {
            Console.Error.WriteLine($"WI #{workItem.Id} reached max attempts and requires manual review.");
            return;
        }

        var runId = Guid.NewGuid().ToString("N");
        var leaseUntil = DateTimeOffset.UtcNow.Add(_options.LeaseDuration);
        var claimed = await _azureDevOps.TryClaimAsync(
            workItem,
            runId,
            attempt,
            leaseUntil,
            cancellationToken);
        if (claimed is null)
        {
            Console.WriteLine($"WI #{workItem.Id} was claimed by another listener.");
            return;
        }

        Console.WriteLine($"Claimed WI #{workItem.Id}; run={runId}; attempt={attempt}.");
        using var executionCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var executionToken = executionCancellation.Token;
        var heartbeatTask = MaintainLeaseAsync(workItem.Id, runId, executionCancellation);

        try
        {
            var worktree = await _worktrees.CreateAsync(workItem.Id, attempt, runId, executionToken);
            var copilotResult = await _copilot.RunAsync(claimed, worktree, executionToken);
            if (copilotResult.Status == CopilotResultStatus.Blocked)
            {
                await _azureDevOps.MarkBlockedAsync(
                    workItem.Id,
                    runId,
                    BuildEvidence("Copilot reported a blocking condition.", copilotResult),
                    executionToken);
                return;
            }

            if (copilotResult.Status is CopilotResultStatus.Failed or CopilotResultStatus.TimedOut)
            {
                await _azureDevOps.MarkFailedAsync(
                    workItem.Id,
                    runId,
                    BuildEvidence("Copilot execution failed.", copilotResult),
                    executionToken);
                return;
            }

            if (!await _worktrees.HasChangesAsync(worktree, executionToken))
            {
                await _azureDevOps.MarkFailedAsync(
                    workItem.Id,
                    runId,
                    "Copilot reported completed but produced no Git changes; worktree retained for diagnosis.",
                    executionToken);
                return;
            }

            var commit = await _worktrees.CommitAndPushAsync(worktree, workItem, executionToken);
            var pullRequest = await _azureDevOps.CreatePullRequestAsync(
                GetRepositoryName(),
                workItem,
                worktree,
                executionToken);
            var transitioned = await _azureDevOps.MarkPullRequestAsync(
                workItem.Id,
                runId,
                pullRequest,
                executionToken);
            if (!transitioned)
            {
                throw new InvalidOperationException(
                    $"WI #{workItem.Id} lost run ownership after PR #{pullRequest.Id} was created.");
            }

            Console.WriteLine(
                $"WI #{workItem.Id} pushed commit {commit} and created PR #{pullRequest.Id}; waiting for policies and merge.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            var failure = heartbeatTask.Exception?.GetBaseException() ?? exception;
            Console.Error.WriteLine($"WI #{workItem.Id} failed: {failure}");
            await TryMarkFailedAsync(workItem.Id, runId, failure, cancellationToken);
        }
        finally
        {
            executionCancellation.Cancel();
            await ObserveHeartbeatAsync(heartbeatTask);
        }
    }

    private async Task ReconcilePullRequestsAsync(
        IEnumerable<WorkItemSnapshot> queue,
        CancellationToken cancellationToken)
    {
        foreach (var workItem in queue.Where(item => item.Tags.Contains("copilot-pr")))
        {
            var pullRequestId = workItem.Tags.GetPullRequestId();
            var runId = workItem.Tags.GetRunId();
            if (pullRequestId is null || string.IsNullOrWhiteSpace(runId))
            {
                Console.Error.WriteLine($"WI #{workItem.Id} has copilot-pr without PR/run metadata.");
                continue;
            }

            var pullRequest = await _azureDevOps.GetPullRequestAsync(
                GetRepositoryName(),
                pullRequestId.Value,
                cancellationToken);
            if (pullRequest.Status.Equals("completed", StringComparison.OrdinalIgnoreCase))
            {
                if (await _azureDevOps.MarkCompletedAsync(workItem, pullRequest, cancellationToken))
                {
                    await _worktrees.CleanupMergedAsync(workItem.Id, runId, cancellationToken);
                    Console.WriteLine($"WI #{workItem.Id} completed after PR #{pullRequest.Id} merged.");
                }
            }
            else if (pullRequest.Status.Equals("abandoned", StringComparison.OrdinalIgnoreCase))
            {
                await _azureDevOps.MarkFailedAsync(
                    workItem.Id,
                    runId,
                    $"PR #{pullRequest.Id} was abandoned; the work item can be retried.",
                    cancellationToken);
            }
        }
    }

    private async Task RecoverExpiredLeasesAsync(
        IEnumerable<WorkItemSnapshot> queue,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var workItem in queue.Where(item => item.Tags.Contains("copilot-in-progress")))
        {
            var leaseUntil = workItem.Tags.GetLeaseUntil();
            if (leaseUntil is not null && leaseUntil > now)
            {
                continue;
            }

            if (await _azureDevOps.RecoverExpiredLeaseAsync(workItem, cancellationToken))
            {
                Console.WriteLine($"Recovered expired lease for WI #{workItem.Id}.");
            }
        }
    }

    private async Task BlockExhaustedRetriesAsync(
        IEnumerable<WorkItemSnapshot> queue,
        CancellationToken cancellationToken)
    {
        foreach (var workItem in queue.Where(item =>
                     item.Tags.Contains("copilot-failed") &&
                     item.Tags.GetAttempt() >= _options.MaxAttempts))
        {
            if (await _azureDevOps.MarkRetryLimitReachedAsync(workItem, cancellationToken))
            {
                Console.WriteLine($"WI #{workItem.Id} reached the retry limit and was blocked.");
            }
        }
    }

    private async Task MaintainLeaseAsync(
        int workItemId,
        string runId,
        CancellationTokenSource executionCancellation)
    {
        var cancellationToken = executionCancellation.Token;
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(_options.HeartbeatInterval, cancellationToken);
                var renewed = await _azureDevOps.RenewLeaseAsync(
                    workItemId,
                    runId,
                    DateTimeOffset.UtcNow.Add(_options.LeaseDuration),
                    cancellationToken);
                if (!renewed)
                {
                    throw new InvalidOperationException($"Lost lease ownership for WI #{workItemId}, run={runId}.");
                }
            }
        }
        catch (OperationCanceledException) when (executionCancellation.IsCancellationRequested)
        {
            // 主执行结束或宿主关闭时停止续租。
        }
        catch
        {
            // 续租失败意味着不再拥有工作项；立即终止 Copilot/Git/PR 流程。
            await executionCancellation.CancelAsync();
            throw;
        }
    }

    private bool IsReadyForExecution(WorkItemSnapshot workItem)
    {
        if (workItem.Tags.Contains("copilot-ready"))
        {
            return true;
        }

        return workItem.Tags.Contains("copilot-failed") && workItem.Tags.GetAttempt() < _options.MaxAttempts;
    }

    private WorkItemSnapshot[] SelectCandidates(IEnumerable<WorkItemSnapshot> queue)
    {
        var eligible = queue.Where(IsReadyForExecution).ToArray();
        if (eligible.Length == 0)
        {
            return Array.Empty<WorkItemSnapshot>();
        }

        // 独占项只在本轮没有其他 Worker 时运行，并占满全部并发容量。
        if (eligible[0].Tags.Contains("copilot-exclusive"))
        {
            return new[] { eligible[0] };
        }

        return eligible
            .Where(item => !item.Tags.Contains("copilot-exclusive"))
            .Take(_options.MaxParallelism)
            .ToArray();
    }

    private async Task TryMarkFailedAsync(
        int workItemId,
        string runId,
        Exception exception,
        CancellationToken cancellationToken)
    {
        try
        {
            await _azureDevOps.MarkFailedAsync(
                workItemId,
                runId,
                $"Listener execution failed: {Truncate(exception.ToString(), 3000)}",
                cancellationToken);
        }
        catch (Exception updateException)
        {
            Console.Error.WriteLine($"Unable to mark WI #{workItemId} failed: {updateException}");
        }
    }

    private static async Task ObserveHeartbeatAsync(Task heartbeatTask)
    {
        try
        {
            await heartbeatTask;
        }
        catch (OperationCanceledException)
        {
            // 主执行流程结束后取消续租属于正常路径。
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Lease heartbeat stopped unexpectedly: {exception}");
        }
    }

    private string GetRepositoryName()
    {
        return _repositoryName ?? throw new InvalidOperationException("Repository name is not initialized.");
    }

    private static string BuildEvidence(string prefix, CopilotExecutionResult result)
    {
        return $"{prefix}\nExitCode: {result.ExitCode}\nSummary:\n{Truncate(result.Summary, 2400)}\nError:\n{Truncate(result.StandardError, 800)}";
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
