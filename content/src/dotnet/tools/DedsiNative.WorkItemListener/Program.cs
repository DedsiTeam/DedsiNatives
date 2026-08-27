namespace DedsiNative.WorkItemListener;

internal static class Program
{
    public static async Task<int> Main()
    {
        try
        {
            var options = ListenerOptions.FromLocalConfiguration();
            using var azureDevOpsClient = new AzureDevOpsClient(options);
            var loopRunner = new CopilotLoopRunner(options);
            var service = new WorkItemPollingService(options, azureDevOpsClient, loopRunner);

            using var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cancellationTokenSource.Cancel();
            };

            await service.RunAsync(cancellationTokenSource.Token);
            return 0;
        }
        catch (OperationCanceledException)
        {
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"启动失败：{exception.Message}");
            return 1;
        }
    }
}
