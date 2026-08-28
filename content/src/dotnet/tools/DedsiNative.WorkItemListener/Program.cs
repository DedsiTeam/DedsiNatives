namespace DedsiNative.WorkItemListener;

/// <summary>
/// 无人值守工作项监听器入口。
/// </summary>
public static class Program
{
    /// <summary>
    /// 启动 Azure DevOps 队列监听和 Copilot CLI 执行循环。
    /// </summary>
    /// <param name="args">
    /// 支持 <c>--once</c> 仅轮询一次，以及 <c>--validate</c> 仅验证配置。
    /// </param>
    /// <returns>
    /// 进程退出码。
    /// </returns>
    public static async Task<int> Main(string[] args)
    {
        using var shutdown = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            shutdown.Cancel();
        };

        try
        {
            var options = ListenerOptions.Load(args);
            options.Validate();

            if (args.Contains("--validate", StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine("Work item listener configuration is valid.");
                return 0;
            }

            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(options.HttpTimeoutSeconds),
            };
            var azureDevOps = new AzureDevOpsClient(httpClient, options);
            var processes = new ProcessRunner();
            var worktrees = new GitWorktreeManager(processes, options);
            var copilot = new CopilotCliRunner(processes, options);
            var listener = new WorkItemPollingService(options, azureDevOps, worktrees, copilot);

            await listener.RunAsync(shutdown.Token);
            return 0;
        }
        catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
        {
            Console.WriteLine("Work item listener stopped.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }
}
