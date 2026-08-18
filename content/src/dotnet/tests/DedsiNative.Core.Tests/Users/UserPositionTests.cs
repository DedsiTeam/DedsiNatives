using DedsiNative.Users;
using Xunit;

namespace DedsiNative.Core.Tests.Users;

/// <summary>
/// 用户岗位关联领域行为测试。
/// </summary>
public sealed class UserPositionTests
{
    [Fact]
    public void AssignPosition_ShouldCreateRelationAndRejectDuplicate()
    {
        var user = new User(Guid.NewGuid(), "测试用户", "user@example.com");
        var positionId = Ulid.NewUlid().ToString();

        user.AssignPosition(positionId, "管理员");

        Assert.Single(user.Positions);
        Assert.Equal("管理员", user.Positions.Single().PositionName);
        var action = () => user.AssignPosition(positionId, "管理员");
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void AssignPosition_ShouldRejectInvalidIdOrName()
    {
        var user = new User(Guid.NewGuid(), "测试用户", "user@example.com");

        var invalidId = () => user.AssignPosition("invalid", "管理员");
        Assert.Throws<ArgumentException>(invalidId);

        var invalidName = () => user.AssignPosition(Ulid.NewUlid().ToString(), " ");
        Assert.Throws<ArgumentException>(invalidName);
    }

    [Fact]
    public void RemoveAndClearPositions_ShouldMaintainRelations()
    {
        var user = new User(Guid.NewGuid(), "测试用户", "user@example.com");
        var firstId = Ulid.NewUlid().ToString();
        var secondId = Ulid.NewUlid().ToString();
        user.AssignPosition(firstId, "管理员").AssignPosition(secondId, "审计员");

        user.RemovePosition(firstId);
        Assert.Single(user.Positions);
        Assert.Equal(secondId, user.Positions.Single().PositionId);

        user.ClearPositions();
        Assert.Empty(user.Positions);
    }
}
