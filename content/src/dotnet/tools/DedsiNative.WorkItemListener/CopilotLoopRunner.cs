using System.Diagnostics;

namespace DedsiNative.WorkItemListener;

internal sealed class CopilotLoopRunner(ListenerOptions options)
{
    public async Task<int> RunAsync(WorkItemCandidate candidate, CancellationToken cancellationToken)
    {
        EnsureRepositoryIsReady();

        var prompt = $"""
                     Use the repository work-item-loop skill and process only Azure DevOps Work Item #{candidate.Id}: {candidate.Title}.
                     Re-read and claim this exact item according to the repository protocol. Do not claim any other item.
                     Complete its implementation, focused verification, commit, push, pull request, pipeline check, and Azure DevOps status write-back.
                     Stop whenever the protocol requires human input or reports a blocker.
                     """;

        var startInfo = new ProcessStartInfo
        {
            FileName = options.CopilotCommand,
            WorkingDirectory = options.RepositoryPath,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("--prompt");
        startInfo.ArgumentList.Add(prompt);
        startInfo.ArgumentList.Add("--allow-tool=write");
        startInfo.ArgumentList.Add("--allow-tool=shell(git:*)");
        startInfo.ArgumentList.Add("--allow-tool=shell(dotnet:*)");
        startInfo.ArgumentList.Add("--allow-tool=shell(bun:*)");
        startInfo.ArgumentList.Add("--allow-tool=shell(gh:*)");
        startInfo.ArgumentList.Add("--allow-tool=ado");

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"无法启动 {options.CopilotCommand}。");
        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode;
    }

    private void EnsureRepositoryIsReady()
    {
        if (!Directory.Exists(options.RepositoryPath))
        {
            throw new DirectoryNotFoundException($"仓库目录不存在：{options.RepositoryPath}");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = options.RepositoryPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("status");
        startInfo.ArgumentList.Add("--porcelain");

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("无法启动 git status。");
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"无法检查 Git 工作区：{error.Trim()}");
        }

        if (!string.IsNullOrWhiteSpace(output))
        {
            throw new InvalidOperationException("Git 工作区存在未提交改动，拒绝启动自动 Loop。");
        }
    }
}
