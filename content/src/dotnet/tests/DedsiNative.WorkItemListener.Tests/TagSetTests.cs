using DedsiNative.WorkItemListener;
using Xunit;

namespace DedsiNative.WorkItemListener.Tests;

/// <summary>
/// Work Item 自动化标签状态转换测试。
/// </summary>
public sealed class TagSetTests
{
    /// <summary>
    /// 领取工作项时应保留业务标签并替换全部旧自动化控制标签。
    /// </summary>
    [Fact]
    public void Claim_ReplacesAutomationTagsAndPreservesBusinessTags()
    {
        var source = TagSet.Parse(
            "business-a; copilot-ready; copilot-stage-backlog; copilot-attempt-1; copilot-run-old; copilot-lease-1");
        var lease = DateTimeOffset.FromUnixTimeSeconds(2_000_000_000);

        var claimed = source.Claim("newrun", 2, lease);

        Assert.Contains("business-a", claimed.Values);
        Assert.Contains("copilot-in-progress", claimed.Values);
        Assert.Contains("copilot-stage-implementing", claimed.Values);
        Assert.Contains("copilot-attempt-2", claimed.Values);
        Assert.Contains("copilot-run-newrun", claimed.Values);
        Assert.Equal(lease, claimed.GetLeaseUntil());
        Assert.DoesNotContain("copilot-ready", claimed.Values);
        Assert.DoesNotContain("copilot-run-old", claimed.Values);
    }

    /// <summary>
    /// 等待 PR 时应移除租约并保存 PR、run 与 attempt 证据。
    /// </summary>
    [Fact]
    public void WaitingForPullRequest_KeepsRunMetadataAndRemovesLease()
    {
        var claimed = TagSet.Parse("copilot-loop; copilot-in-progress; copilot-attempt-2; copilot-run-run1; copilot-lease-9");

        var waiting = claimed.WaitingForPullRequest(42);

        Assert.Contains("copilot-pr", waiting.Values);
        Assert.Contains("copilot-stage-integrating", waiting.Values);
        Assert.Equal(42, waiting.GetPullRequestId());
        Assert.Equal("run1", waiting.GetRunId());
        Assert.Equal(2, waiting.GetAttempt());
        Assert.Null(waiting.GetLeaseUntil());
    }

    /// <summary>
    /// 完成状态不应继续持有运行租约或 PR 控制标签。
    /// </summary>
    [Fact]
    public void Completed_RemovesTransientRunMetadata()
    {
        var waiting = TagSet.Parse(
            "copilot-loop; copilot-pr; copilot-stage-integrating; copilot-attempt-2; copilot-run-run1; copilot-pull-request-42");

        var completed = waiting.Completed();

        Assert.Contains("copilot-loop", completed.Values);
        Assert.Contains("copilot-completed", completed.Values);
        Assert.Contains("copilot-stage-done", completed.Values);
        Assert.Null(completed.GetRunId());
        Assert.Null(completed.GetPullRequestId());
        Assert.Equal(0, completed.GetAttempt());
    }
}
