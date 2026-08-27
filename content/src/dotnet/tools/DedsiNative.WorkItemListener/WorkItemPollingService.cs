namespace DedsiNative.WorkItemListener;

internal sealed class WorkItemPollingService(
    ListenerOptions options,
    AzureDevOpsClient azureDevOpsClient,
    CopilotLoopRunner loopRunner)
{
    private readonly Dictionary<(int Id, int Revision), DateTimeOffset> _lastRuns = [];

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Log($"监听已启动：每 {options.PollInterval.TotalSeconds:0} 秒查询一次。Loop 自动执行：{options.LoopEnabled}。");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await PollOnceAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                Log($"轮询失败：{exception.Message}", isError: true);
            }

            if (options.RunOnce)
            {
                break;
            }

            await Task.Delay(options.PollInterval, cancellationToken);
        }
    }

    private async Task PollOnceAsync(CancellationToken cancellationToken)
    {
        var candidates = await azureDevOpsClient.GetCandidatesAsync(cancellationToken);
        if (candidates.Count == 0)
        {
            Log("当前没有可领取工作项。");
            return;
        }

        Log($"发现 {candidates.Count} 个候选工作项：{string.Join(", ", candidates.Select(x => $"#{x.Id}"))}。");
        if (!options.LoopEnabled)
        {
            foreach (var candidate in candidates)
            {
                Log($"预览 #{candidate.Id} rev {candidate.Revision} [{candidate.State}] {candidate.Title}");
            }

            return;
        }

        var candidateToRun = candidates.FirstOrDefault(CanRun);
        if (candidateToRun is null)
        {
            Log("候选工作项仍处于重试冷却时间，本轮跳过。");
            return;
        }

        _lastRuns[(candidateToRun.Id, candidateToRun.Revision)] = DateTimeOffset.UtcNow;
        Log($"开始执行 #{candidateToRun.Id} rev {candidateToRun.Revision}：{candidateToRun.Title}");
        var exitCode = await loopRunner.RunAsync(candidateToRun, cancellationToken);
        Log(exitCode == 0
            ? $"Work Item #{candidateToRun.Id} 的 Copilot Loop 已结束。"
            : $"Work Item #{candidateToRun.Id} 的 Copilot Loop 退出码为 {exitCode}。",
            isError: exitCode != 0);
    }

    private bool CanRun(WorkItemCandidate candidate)
    {
        return !_lastRuns.TryGetValue((candidate.Id, candidate.Revision), out var lastRun) ||
               DateTimeOffset.UtcNow - lastRun >= options.RetryCooldown;
    }

    private static void Log(string message, bool isError = false)
    {
        var line = $"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}] {message}";
        if (isError)
        {
            Console.Error.WriteLine(line);
        }
        else
        {
            Console.WriteLine(line);
        }
    }
}
