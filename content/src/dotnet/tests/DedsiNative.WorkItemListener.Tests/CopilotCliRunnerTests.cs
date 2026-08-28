using DedsiNative.WorkItemListener;
using Xunit;

namespace DedsiNative.WorkItemListener.Tests;

/// <summary>
/// Copilot CLI 提示与终态协议测试。
/// </summary>
public sealed class CopilotCliRunnerTests
{
    /// <summary>
    /// 提示必须携带工作项事实并明确 Git 与 Azure DevOps 边界。
    /// </summary>
    [Fact]
    public void BuildPrompt_ContainsWorkItemAndAutomationBoundary()
    {
        var workItem = new WorkItemSnapshot(
            123,
            4,
            "新增用户查询",
            "实现查询接口",
            "构建和测试通过",
            "New",
            TagSet.Parse("copilot-loop; copilot-ready"));

        var prompt = CopilotCliRunner.BuildPrompt(workItem, 2, "run-1");

        Assert.Contains("工作项 ID：123", prompt);
        Assert.Contains("新增用户查询", prompt);
        Assert.Contains("实现查询接口", prompt);
        Assert.Contains("Listener 负责 Git worktree、commit、push、PR", prompt);
        Assert.Contains("custom agent `backend`", prompt);
        Assert.Contains("`frontend`", prompt);
        Assert.Contains("不得委派 `product-manager` 或 `prototype`", prompt);
        Assert.Contains("DEDSI_RESULT=completed", prompt);
    }

    /// <summary>
    /// 成功退出但缺少机器标记时必须失败，避免错误提交半成品。
    /// </summary>
    [Fact]
    public void ParseResult_WithoutMarker_IsFailed()
    {
        var result = CopilotCliRunner.ParseResult(new ProcessResult(0, "implementation done", string.Empty, false));

        Assert.Equal(CopilotResultStatus.Failed, result.Status);
    }

    /// <summary>
    /// 最后一个机器标记决定最终执行状态。
    /// </summary>
    [Theory]
    [InlineData("DEDSI_RESULT=completed", CopilotResultStatus.Completed)]
    [InlineData("summary\nDEDSI_RESULT=blocked\n", CopilotResultStatus.Blocked)]
    [InlineData("DEDSI_RESULT=failed", CopilotResultStatus.Failed)]
    public void ParseResult_WithMarker_ReturnsExpectedStatus(string output, CopilotResultStatus expected)
    {
        var result = CopilotCliRunner.ParseResult(new ProcessResult(0, output, string.Empty, false));

        Assert.Equal(expected, result.Status);
    }
}
