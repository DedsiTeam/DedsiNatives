using System.Net;
using System.Text;
using DedsiNative.WorkItemListener;
using Xunit;

namespace DedsiNative.WorkItemListener.Tests;

/// <summary>
/// Azure DevOps revision CAS 领取行为测试。
/// </summary>
public sealed class AzureDevOpsClientTests
{
    /// <summary>
    /// 领取请求必须先 test revision，再写入唯一运行和租约标签。
    /// </summary>
    [Fact]
    public async Task TryClaimAsync_UsesRevisionTestAndReturnsClaim()
    {
        string? requestBody = null;
        var handler = new StubHttpMessageHandler(async request =>
        {
            requestBody = await request.Content!.ReadAsStringAsync();
            return JsonResponse(HttpStatusCode.OK, WorkItemJson(7, 4, "copilot-loop; copilot-in-progress; copilot-attempt-2; copilot-run-run2; copilot-lease-2000000000"));
        });
        using var httpClient = new HttpClient(handler);
        var client = new AzureDevOpsClient(httpClient, CreateOptions());
        var source = new WorkItemSnapshot(
            7,
            3,
            "title",
            "description",
            "acceptance",
            "New",
            TagSet.Parse("copilot-loop; copilot-ready; copilot-attempt-1"));

        var claimed = await client.TryClaimAsync(
            source,
            "run2",
            2,
            DateTimeOffset.FromUnixTimeSeconds(2_000_000_000),
            CancellationToken.None);

        Assert.NotNull(claimed);
        Assert.Contains("\"op\":\"test\",\"path\":\"/rev\",\"value\":3", requestBody);
        Assert.Contains("copilot-run-run2", requestBody);
        Assert.Equal("run2", claimed.WorkItem.Tags.GetRunId());
    }

    /// <summary>
    /// revision 竞争失败且远程 revision 已变化时应返回 null，而不是覆盖其他 Listener。
    /// </summary>
    [Fact]
    public async Task TryClaimAsync_WhenRevisionChanged_ReturnsNull()
    {
        var call = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            call++;
            return Task.FromResult(call == 1
                ? JsonResponse(HttpStatusCode.PreconditionFailed, "{}")
                : JsonResponse(HttpStatusCode.OK, WorkItemJson(7, 9, "copilot-loop; copilot-in-progress; copilot-run-other")));
        });
        using var httpClient = new HttpClient(handler);
        var client = new AzureDevOpsClient(httpClient, CreateOptions());
        var source = new WorkItemSnapshot(
            7,
            3,
            "title",
            "description",
            "acceptance",
            "New",
            TagSet.Parse("copilot-loop; copilot-ready"));

        var claimed = await client.TryClaimAsync(
            source,
            "run2",
            1,
            DateTimeOffset.UtcNow.AddMinutes(30),
            CancellationToken.None);

        Assert.Null(claimed);
        Assert.Equal(2, call);
    }

    /// <summary>
    /// 已过期的旧 run 不得重新续租，避免租约失效后恢复所有权。
    /// </summary>
    [Fact]
    public async Task RenewLeaseAsync_WhenCurrentLeaseExpired_ReturnsFalseWithoutPatch()
    {
        var call = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            call++;
            return Task.FromResult(JsonResponse(
                HttpStatusCode.OK,
                WorkItemJson(7, 4, "copilot-loop; copilot-in-progress; copilot-run-run2; copilot-lease-1")));
        });
        using var httpClient = new HttpClient(handler);
        var client = new AzureDevOpsClient(httpClient, CreateOptions());

        var renewed = await client.RenewLeaseAsync(
            7,
            "run2",
            DateTimeOffset.UtcNow.AddMinutes(30),
            CancellationToken.None);

        Assert.False(renewed);
        Assert.Equal(1, call);
    }

    /// <summary>
    /// 即使 run 标签仍保留，已经离开 in-progress 的旧执行也不得回写终态。
    /// </summary>
    [Fact]
    public async Task MarkFailedAsync_WhenNoLongerInProgress_ReturnsFalseWithoutPatch()
    {
        var call = 0;
        var handler = new StubHttpMessageHandler(_ =>
        {
            call++;
            return Task.FromResult(JsonResponse(
                HttpStatusCode.OK,
                WorkItemJson(7, 5, "copilot-loop; copilot-failed; copilot-run-run2")));
        });
        using var httpClient = new HttpClient(handler);
        var client = new AzureDevOpsClient(httpClient, CreateOptions());

        var updated = await client.MarkFailedAsync(7, "run2", "late result", CancellationToken.None);

        Assert.False(updated);
        Assert.Equal(1, call);
    }

    private static ListenerOptions CreateOptions()
    {
        return new ListenerOptions
        {
            Organization = "org",
            Project = "project",
            Repository = "repo",
            Token = "test-token",
            AuthenticationScheme = "Basic",
            RepositoryPath = Environment.CurrentDirectory,
            TargetBranch = "main",
            CopilotCommand = "copilot",
            PollInterval = TimeSpan.FromMinutes(1),
            RetryInterval = TimeSpan.FromMinutes(1),
            CopilotTimeout = TimeSpan.FromMinutes(1),
            LeaseDuration = TimeSpan.FromMinutes(30),
            HeartbeatInterval = TimeSpan.FromMinutes(5),
            HttpTimeoutSeconds = 10,
            MaxParallelism = 1,
            MaxAttempts = 3,
            MaxAutopilotContinues = 10,
        };
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
    }

    private static string WorkItemJson(int id, int revision, string tags)
    {
        return $$"""
            {
              "id": {{id}},
              "rev": {{revision}},
              "fields": {
                "System.Title": "title",
                "System.Description": "description",
                "Microsoft.VSTS.Common.AcceptanceCriteria": "acceptance",
                "System.State": "New",
                "System.Tags": "{{tags}}"
              }
            }
            """;
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return handler(request);
        }
    }
}
