using DedsiNative.Systems;
using Xunit;
using SystemEntity = DedsiNative.Systems.System;

namespace DedsiNative.Core.Tests.Systems;

/// <summary>系统聚合根的领域规则测试。</summary>
public sealed class SystemTests
{
    /// <summary>创建系统时应保存名称、说明和排序。</summary>
    [Fact]
    public void Constructor_Should_Set_Properties()
    {
        var id = Ulid.NewUlid().ToString();
        var system = new SystemEntity(id, "统一身份认证", "身份管理系统", 10);

        Assert.Equal(id, system.Id);
        Assert.Equal("统一身份认证", system.Name);
        Assert.Equal("身份管理系统", system.Description);
        Assert.Equal(10, system.Sort);
    }

    /// <summary>系统 ID 必须是 26 位合法 ULID。</summary>
    [Theory]
    [InlineData("")]
    [InlineData("invalid-system-id")]
    public void Constructor_Should_Reject_Invalid_Id(string id)
    {
        Assert.Throws<ArgumentException>(() => new SystemEntity(id, "系统"));
    }

    /// <summary>系统名称为空时应拒绝创建。</summary>
    [Fact]
    public void Constructor_Should_Reject_Blank_Name()
    {
        Assert.Throws<ArgumentException>(() => new SystemEntity(Ulid.NewUlid().ToString(), " "));
    }

    /// <summary>系统名称和说明超过长度上限时应拒绝创建。</summary>
    [Fact]
    public void Constructor_Should_Reject_Overlong_Text()
    {
        var id = Ulid.NewUlid().ToString();

        Assert.Throws<ArgumentException>(() => new SystemEntity(
            id,
            new string('A', SystemConsts.MaxNameLength + 1)));
        Assert.Throws<ArgumentException>(() => new SystemEntity(
            id,
            "系统",
            new string('B', SystemConsts.MaxDescriptionLength + 1)));
    }

    /// <summary>系统领域方法应更新说明和排序。</summary>
    [Fact]
    public void Change_Methods_Should_Update_Properties()
    {
        var system = new SystemEntity(Ulid.NewUlid().ToString(), "系统");

        system.ChangeDescription("新的说明").ChangeSort(-1);

        Assert.Equal("新的说明", system.Description);
        Assert.Equal(-1, system.Sort);
    }
}
