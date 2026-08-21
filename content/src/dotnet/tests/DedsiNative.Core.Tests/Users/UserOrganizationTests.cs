using DedsiNative.Users;
using Xunit;

namespace DedsiNative.Core.Tests.Users;

/// <summary>
/// 用户组织机构关联领域行为测试。
/// </summary>
public sealed class UserOrganizationTests
{
    [Fact]
    public void AssignOrganization_ShouldCreateRelationAndRejectDuplicate()
    {
        var user = new User(Guid.NewGuid(), "测试用户", "user@example.com");
        var organizationId = Ulid.NewUlid().ToString();

        user.AssignOrganization(organizationId, "研发中心");

        Assert.Single(user.Organizations);
        Assert.Equal("研发中心", user.Organizations.Single().OrganizationName);
        var action = () => user.AssignOrganization(organizationId, "研发中心");
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void AssignOrganization_ShouldRejectInvalidIdOrName()
    {
        var user = new User(Guid.NewGuid(), "测试用户", "user@example.com");

        var invalidId = () => user.AssignOrganization("invalid-ulid", "研发中心");
        Assert.Throws<ArgumentException>(invalidId);

        var invalidName = () => user.AssignOrganization(Ulid.NewUlid().ToString(), " ");
        Assert.Throws<ArgumentException>(invalidName);
    }

    [Fact]
    public void RemoveAndClearOrganizations_ShouldMaintainRelations()
    {
        var user = new User(Guid.NewGuid(), "测试用户", "user@example.com");
        var firstId = Ulid.NewUlid().ToString();
        var secondId = Ulid.NewUlid().ToString();
        user.AssignOrganization(firstId, "研发中心").AssignOrganization(secondId, "运营部");

        user.RemoveOrganization(firstId);
        Assert.Single(user.Organizations);
        Assert.Equal(secondId, user.Organizations.Single().OrganizationId);

        user.ClearOrganizations();
        Assert.Empty(user.Organizations);
    }
}
