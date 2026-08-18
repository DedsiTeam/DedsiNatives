using DedsiNative.Menus;
using Xunit;

namespace DedsiNative.Core.Tests.Menus;

/// <summary>
/// 菜单聚合根领域规则测试。
/// </summary>
public sealed class MenuTests
{
    private const string MenuId = "01J00000000000000000000000";
    private const string SystemId = "01J00000000000000000000001";
    private const string ParentId = "01J00000000000000000000002";

    /// <summary>
    /// 页面菜单应保存全部合法字段。
    /// </summary>
    [Fact]
    public void Constructor_Should_Create_Valid_Page_Menu()
    {
        var menu = CreateMenu();

        Assert.Equal(MenuId, menu.Id);
        Assert.Equal("users", menu.Code);
        Assert.Equal(MenuType.Menu, menu.Type);
        Assert.Equal("/users", menu.RoutePath);
    }

    /// <summary>
    /// 构造函数应拒绝不合法的标识、排序、层级和菜单类型。
    /// </summary>
    [Fact]
    public void Constructor_Should_Reject_Invalid_Identity_And_Numeric_Values()
    {
        Assert.Throws<ArgumentException>(() => CreateMenu(id: "invalid"));
        Assert.Throws<ArgumentException>(() => CreateMenu(sort: -1));
        Assert.Throws<ArgumentException>(() => CreateMenu(level: 0));
        Assert.Throws<ArgumentException>(() => CreateMenu(type: (MenuType)99));
    }

    /// <summary>
    /// 构造函数应拒绝超长编码和名称。
    /// </summary>
    [Fact]
    public void Constructor_Should_Reject_Values_Over_Maximum_Length()
    {
        Assert.Throws<ArgumentException>(() => CreateMenu(code: new string('a', MenuConsts.MaxCodeLength + 1)));
        Assert.Throws<ArgumentException>(() => CreateMenu(name: new string('a', MenuConsts.MaxNameLength + 1)));
    }

    /// <summary>
    /// 菜单类型、外链和权限快照的组合规则必须生效。
    /// </summary>
    [Fact]
    public void Constructor_Should_Enforce_Cross_Field_Invariants()
    {
        Assert.Throws<ArgumentException>(() => CreateMenu(type: MenuType.Menu, routePath: null));
        Assert.Throws<ArgumentException>(() => CreateMenu(type: MenuType.Button, parentId: null));
        Assert.Throws<ArgumentException>(() => CreateMenu(type: MenuType.Directory, component: "Pages/Users"));
        Assert.Throws<ArgumentException>(() => CreateMenu(isExternal: true, externalUrl: null));
        Assert.Throws<ArgumentException>(() => CreateMenu(permissionId: ParentId, permissionName: null));
        Assert.Throws<ArgumentException>(() => CreateMenu(parentId: MenuId));
    }

    private static Menu CreateMenu(
        string id = MenuId,
        string code = "users",
        string name = "用户管理",
        string? parentId = null,
        MenuType type = MenuType.Menu,
        string? routePath = "/users",
        string? component = "Pages/Users",
        int sort = 0,
        int level = 1,
        bool isExternal = false,
        string? externalUrl = null,
        string? permissionId = null,
        string? permissionName = null)
    {
        return new Menu(
            id, SystemId, "系统管理", code, name, parentId, type, routePath, component,
            null, null, permissionId, permissionName, sort, level, true, false, isExternal,
            externalUrl, true, false, null);
    }
}
