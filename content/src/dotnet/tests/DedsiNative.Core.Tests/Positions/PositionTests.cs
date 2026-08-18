using DedsiNative.Positions;
using Xunit;

namespace DedsiNative.Core.Tests.Positions;

/// <summary>岗位聚合根和子实体的领域规则测试。</summary>
public sealed class PositionTests
{
    /// <summary>创建岗位时应保存基本信息并默认启用。</summary>
    [Fact]
    public void Constructor_Should_Set_Properties()
    {
        var position = new Position(
            Ulid.NewUlid().ToString(),
            "管理员",
            Ulid.NewUlid().ToString(),
            "统一身份认证",
            "系统管理员岗位");

        Assert.Equal("管理员", position.Name);
        Assert.Equal("统一身份认证", position.SystemName);
        Assert.True(position.IsEnabled);
        Assert.Empty(position.Permissions);
        Assert.Empty(position.Organizations);
    }

    /// <summary>岗位不能接受非法标识或空白名称。</summary>
    [Fact]
    public void Constructor_Should_Reject_Invalid_Values()
    {
        Assert.Throws<ArgumentException>(() => new Position(
            "invalid", "岗位", Ulid.NewUlid().ToString(), "系统"));
        Assert.Throws<ArgumentException>(() => new Position(
            Ulid.NewUlid().ToString(), " ", Ulid.NewUlid().ToString(), "系统"));
    }

    /// <summary>岗位应拒绝重复权限和组织机构关联。</summary>
    [Fact]
    public void Add_Assignments_Should_Reject_Duplicates()
    {
        var systemId = Ulid.NewUlid().ToString();
        var position = new Position(Ulid.NewUlid().ToString(), "岗位", systemId, "系统");
        var permissionId = Ulid.NewUlid().ToString();
        var organizationId = Ulid.NewUlid().ToString();

        position.AddPermission(permissionId, "user.read", systemId, "系统");
        position.AddOrganization(organizationId, "总部");

        Assert.Throws<ArgumentException>(() => position.AddPermission(permissionId, "user.read", systemId, "系统"));
        Assert.Throws<ArgumentException>(() => position.AddOrganization(organizationId, "总部"));
    }

    /// <summary>岗位状态和关联清理行为应生效。</summary>
    [Fact]
    public void Status_And_Clear_Methods_Should_Work()
    {
        var systemId = Ulid.NewUlid().ToString();
        var position = new Position(Ulid.NewUlid().ToString(), "岗位", systemId, "系统");
        position.AddPermission(Ulid.NewUlid().ToString(), "user.read", systemId, "系统");
        position.AddOrganization(Ulid.NewUlid().ToString(), "总部");

        position.Disable().ClearPermissions().ClearOrganizations();

        Assert.False(position.IsEnabled);
        Assert.Empty(position.Permissions);
        Assert.Empty(position.Organizations);
    }

    /// <summary>
    /// 岗位应只更新指定权限的名称快照。
    /// </summary>
    [Fact]
    public void ChangePermissionName_Should_UpdateOnlyMatchingSnapshot()
    {
        var systemId = Ulid.NewUlid().ToString();
        var firstPermissionId = Ulid.NewUlid().ToString();
        var secondPermissionId = Ulid.NewUlid().ToString();
        var position = new Position(Ulid.NewUlid().ToString(), "岗位", systemId, "系统")
            .AddPermission(firstPermissionId, "user.read", systemId, "系统")
            .AddPermission(secondPermissionId, "user.write", systemId, "系统");

        position.ChangePermissionName(firstPermissionId, "user.view");

        Assert.Equal("user.view", position.Permissions.Single(item => item.PermissionId == firstPermissionId).PermissionName);
        Assert.Equal("user.write", position.Permissions.Single(item => item.PermissionId == secondPermissionId).PermissionName);
    }
}
