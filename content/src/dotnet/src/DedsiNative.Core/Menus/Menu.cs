using Dedsi.Ddd.Domain.Entities;
using Volo.Abp;

namespace DedsiNative.Menus;

/// <summary>菜单聚合根。</summary>
public class Menu : DedsiAggregateRoot<string>
{
    /// <summary>供 ORM 使用。</summary>
    protected Menu() { }
    /// <summary>创建菜单。</summary>
    public Menu(string id, string systemId, string systemName, string code, string name, string? parentId, MenuType type, string? routePath, string? component, string? redirect, string? icon, string? permissionId, string? permissionName, int sort, int level, bool isVisible, bool isDisabled, bool isExternal, string? externalUrl, bool keepAlive, bool isAffix, string? description) : base(ValidateUlid(id, nameof(id)) )
    { Update(systemId, systemName, code, name, parentId, type, routePath, component, redirect, icon, permissionId, permissionName, sort, level, isVisible, isDisabled, isExternal, externalUrl, keepAlive, isAffix, description); }
    public string SystemId { get; private set; } = string.Empty;
    public string SystemName { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? ParentId { get; private set; }
    public MenuType Type { get; private set; }
    public string? RoutePath { get; private set; }
    public string? Component { get; private set; }
    public string? Redirect { get; private set; }
    public string? Icon { get; private set; }
    public string? PermissionId { get; private set; }
    public string? PermissionName { get; private set; }
    public int Sort { get; private set; }
    public int Level { get; private set; }
    public bool IsVisible { get; private set; }
    public bool IsDisabled { get; private set; }
    public bool IsExternal { get; private set; }
    public string? ExternalUrl { get; private set; }
    public bool KeepAlive { get; private set; }
    public bool IsAffix { get; private set; }
    public string? Description { get; private set; }
    /// <summary>更新菜单全部字段并校验不变量。</summary>
    public Menu Update(string systemId, string systemName, string code, string name, string? parentId, MenuType type, string? routePath, string? component, string? redirect, string? icon, string? permissionId, string? permissionName, int sort, int level, bool isVisible, bool isDisabled, bool isExternal, string? externalUrl, bool keepAlive, bool isAffix, string? description)
    {
        SystemId = ValidateUlid(systemId, nameof(systemId)); SystemName = Check.NotNullOrWhiteSpace(systemName, nameof(systemName), 128); Code = Check.NotNullOrWhiteSpace(code, nameof(code), MenuConsts.MaxCodeLength); Name = Check.NotNullOrWhiteSpace(name, nameof(name), MenuConsts.MaxNameLength);
        ParentId = OptionalUlid(parentId, nameof(parentId)); if (ParentId == Id) throw new ArgumentException("菜单不能作为自身父级。", nameof(parentId));
        if (!Enum.IsDefined(type) || sort < 0 || level < 1) throw new ArgumentException("菜单类型、排序或层级无效。");
        Type = type; RoutePath = Optional(routePath, MenuConsts.MaxRouteLength); Component = Optional(component, MenuConsts.MaxRouteLength); Redirect = Optional(redirect, MenuConsts.MaxRouteLength); Icon = Optional(icon, MenuConsts.MaxIconLength);
        PermissionId = OptionalUlid(permissionId, nameof(permissionId)); PermissionName = Optional(permissionName, MenuConsts.MaxNameLength);
        if ((PermissionId is null) != (PermissionName is null)) throw new ArgumentException("权限标识和名称快照必须同时提供。");
        Sort = sort; Level = level; IsVisible = isVisible; IsDisabled = isDisabled; IsExternal = isExternal; ExternalUrl = Optional(externalUrl, MenuConsts.MaxExternalUrlLength); KeepAlive = keepAlive; IsAffix = isAffix; Description = Optional(description, MenuConsts.MaxDescriptionLength);
        if (Type == MenuType.Directory && Component is not null && !string.Equals(Component, "Layout", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("目录仅允许 Layout 组件。");
        if (Type == MenuType.Menu && RoutePath is null) throw new ArgumentException("页面菜单必须配置路由路径。");
        if (Type == MenuType.Button && ParentId is null) throw new ArgumentException("按钮菜单必须配置父级菜单。");
        if (IsExternal != (ExternalUrl is not null)) throw new ArgumentException("外链状态与外链地址不一致。"); return this;
    }
    private static string ValidateUlid(string value, string name) => string.IsNullOrWhiteSpace(value) || value.Length != 26 || !Ulid.TryParse(value, out _) ? throw new ArgumentException("标识必须是合法的 26 位 ULID。", name) : value;
    private static string? OptionalUlid(string? value, string name) => string.IsNullOrWhiteSpace(value) ? null : ValidateUlid(value, name);
    private static string? Optional(string? value, int length) => string.IsNullOrWhiteSpace(value) ? null : Check.NotNullOrWhiteSpace(value, nameof(value), length);
}
/// <summary>菜单类型。</summary>
public enum MenuType { Directory = 1, Menu = 2, Button = 3 }
