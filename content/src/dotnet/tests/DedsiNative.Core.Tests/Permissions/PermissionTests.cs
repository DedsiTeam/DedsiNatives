using DedsiNative.Permissions;
using DedsiNative.Permissions.Events;
using Xunit;

namespace DedsiNative.Core.Tests.Permissions;

/// <summary>权限聚合根的领域规则测试。</summary>
public sealed class PermissionTests
{
    /// <summary>创建权限时应保存系统归属、名称、说明和启用状态。</summary>
    [Fact]
    public void Constructor_Should_Set_Properties()
    {
        var id = Ulid.NewUlid().ToString();
        var systemId = Ulid.NewUlid().ToString();
        var permission = new Permission(id, systemId, "身份系统", "user.read", "查看用户", true);

        Assert.Equal(id, permission.Id);
        Assert.Equal(systemId, permission.SystemId);
        Assert.Equal("身份系统", permission.SystemName);
        Assert.Equal("user.read", permission.Name);
        Assert.True(permission.IsEnabled);
    }

    /// <summary>权限 ID、系统 ID 和名称不合法时应拒绝创建。</summary>
    [Fact]
    public void Constructor_Should_Reject_Invalid_Values()
    {
        Assert.Throws<ArgumentException>(() => new Permission(
            "invalid",
            Ulid.NewUlid().ToString(),
            "系统",
            "user.read"));
        Assert.Throws<ArgumentException>(() => new Permission(
            Ulid.NewUlid().ToString(),
            "invalid",
            "系统",
            "user.read"));
        Assert.Throws<ArgumentException>(() => new Permission(
            Ulid.NewUlid().ToString(),
            Ulid.NewUlid().ToString(),
            "系统",
            " "));
    }

    /// <summary>权限启用和停用必须通过领域行为完成。</summary>
    [Fact]
    public void Enable_And_Disable_Should_Change_Status()
    {
        var permission = new Permission(
            Ulid.NewUlid().ToString(),
            Ulid.NewUlid().ToString(),
            "系统",
            "user.read",
            isEnabled: false);

        permission.Enable();
        Assert.True(permission.IsEnabled);

        permission.Disable();
        Assert.False(permission.IsEnabled);
    }

    /// <summary>权限名称和说明超过长度上限时应拒绝创建。</summary>
    [Fact]
    public void Constructor_Should_Reject_Overlong_Text()
    {
        var id = Ulid.NewUlid().ToString();
        var systemId = Ulid.NewUlid().ToString();

        Assert.Throws<ArgumentException>(() => new Permission(
            id,
            systemId,
            "系统",
            new string('A', PermissionConsts.MaxNameLength + 1)));
        Assert.Throws<ArgumentException>(() => new Permission(
            id,
            systemId,
            "系统",
            "user.read",
            new string('B', PermissionConsts.MaxDescriptionLength + 1)));
    }

    /// <summary>
    /// 权限名称实际变化时应登记名称变更事件。
    /// </summary>
    [Fact]
    public void ChangeName_Should_Add_NameChangedEvent_OnlyWhenNameChanges()
    {
        var systemId = Ulid.NewUlid().ToString();
        var permission = new Permission(
            Ulid.NewUlid().ToString(),
            systemId,
            "系统",
            "user.read");

        permission.ChangeName("user.view");

        var eventRecord = Assert.Single(permission.GetLocalEvents());
        var eventData = Assert.IsType<PermissionNameChangedEvent>(eventRecord.EventData);
        Assert.Equal(permission.Id, eventData.PermissionId);
        Assert.Equal("user.read", eventData.OldName);
        Assert.Equal("user.view", eventData.NewName);
        Assert.Equal(systemId, eventData.SystemId);

        permission.ChangeName("user.view");
        Assert.Single(permission.GetLocalEvents());
    }
}
