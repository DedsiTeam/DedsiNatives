using System.Diagnostics;

namespace DedsiNative.WorkItemListener;

/// <summary>
/// 无 shell 插值地执行外部进程，并统一处理输出、取消与超时。
/// </summary>
public sealed class ProcessRunner
{
    /// <summary>
    /// 执行外部命令。
    /// </summary>
    /// <param name="fileName">
    /// 可执行文件名或绝对路径。
    /// </param>
    /// <param name="arguments">
    /// 已分隔的参数数组。
    /// </param>
    /// <param name="workingDirectory">
    /// 进程工作目录。
    /// </param>
    /// <param name="timeout">
    /// 最长执行时间。
    /// </param>
    /// <param name="environment">
    /// 需要覆盖的环境变量。
    /// </param>
    /// <param name="cancellationToken">
    /// 取消令牌。
    /// </param>
    /// <returns>
    /// 退出码和完整标准输出。
    /// </returns>
    public async Task<ProcessResult> RunAsync(
        string fileName,
        IEnumerable<string> arguments,
        string workingDirectory,
        TimeSpan timeout,
        IReadOnlyDictionary<string, string?>? environment,
        CancellationToken cancellationToken)
    {
        return await RunAsync(
            fileName,
            arguments,
            workingDirectory,
            timeout,
            environment,
            null,
            cancellationToken);
    }

    /// <summary>
    /// 执行外部命令，并通过标准输入传递可能超过命令行长度限制的内容。
    /// </summary>
    /// <param name="fileName">
    /// 可执行文件名或绝对路径。
    /// </param>
    /// <param name="arguments">
    /// 已分隔的参数数组。
    /// </param>
    /// <param name="workingDirectory">
    /// 进程工作目录。
    /// </param>
    /// <param name="timeout">
    /// 最长执行时间。
    /// </param>
    /// <param name="environment">
    /// 需要覆盖的环境变量。
    /// </param>
    /// <param name="standardInput">
    /// 写入子进程标准输入的完整内容；为空时不重定向标准输入。
    /// </param>
    /// <param name="cancellationToken">
    /// 取消令牌。
    /// </param>
    /// <returns>
    /// 退出码和完整标准输出。
    /// </returns>
    public async Task<ProcessResult> RunAsync(
        string fileName,
        IEnumerable<string> arguments,
        string workingDirectory,
        TimeSpan timeout,
        IReadOnlyDictionary<string, string?>? environment,
        string? standardInput,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = standardInput is not null,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        if (environment is not null)
        {
            foreach (var pair in environment)
            {
                if (pair.Value is null)
                {
                    startInfo.Environment.Remove(pair.Key);
                }
                else
                {
                    startInfo.Environment[pair.Key] = pair.Value;
                }
            }
        }

        using var process = new Process { StartInfo = startInfo };
        if (!process.Start())
        {
            throw new InvalidOperationException($"Failed to start process: {fileName}");
        }

        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout);
        var processToken = timeoutSource.Token;
        var stdoutTask = process.StandardOutput.ReadToEndAsync(processToken);
        var stderrTask = process.StandardError.ReadToEndAsync(processToken);

        var timedOut = false;
        try
        {
            if (standardInput is not null)
            {
                await process.StandardInput.WriteAsync(standardInput.AsMemory(), processToken);
                await process.StandardInput.FlushAsync(processToken);
                process.StandardInput.Close();
            }

            await process.WaitForExitAsync(processToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            timedOut = true;
            TryKill(process);
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            throw;
        }

        var standardOutput = await AwaitOutputAsync(stdoutTask);
        var standardError = await AwaitOutputAsync(stderrTask);
        return new ProcessResult(
            timedOut ? -1 : process.ExitCode,
            standardOutput,
            standardError,
            timedOut);
    }

    private static async Task<string> AwaitOutputAsync(Task<string> outputTask)
    {
        try
        {
            return await outputTask;
        }
        catch (OperationCanceledException)
        {
            return string.Empty;
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit();
            }
        }
        catch (InvalidOperationException)
        {
            // 进程已并发退出时无需再次处理。
        }
    }
}
