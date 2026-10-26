using Dedsi.Ddd.Domain.Entities;
using Volo.Abp;

namespace DedsiNative.Menus;

/// <summary>
/// 菜单聚合根。
/// </summary>
public class Menu : DedsiAggregateRoot<string>
{
    /// <summary>
    /// 供 ORM 使用。
    /// </summary>
    protected Menu()
    {
    }

    /// <summary>
    /// 创建菜单。
    /// </summary>
    /// <param name="id">菜单唯一标识。</param>
    /// <param name="systemId">所属系统标识。</param>
    /// <param name="systemName">所属系统名称快照。</param>
    /// <param name="code">菜单编码。</param>
    /// <param name="name">菜单名称。</param>
    /// <param name="parentId">父级标识；为空表示根节点。</param>
    /// <param name="type">菜单类型，区分目录、页面菜单和按钮。</param>
    /// <param name="routePath">前端路由路径；页面菜单必须提供。</param>
    /// <param name="component">前端组件标识；目录仅允许为空或 Layout。</param>
    /// <param name="redirect">重定向目标路径。</param>
    /// <param name="icon">菜单图标标识。</param>
    /// <param name="permissionId">关联权限标识。</param>
    /// <param name="permissionName">关联权限名称快照。</param>
    /// <param name="sort">显示排序值。</param>
    /// <param name="level">菜单层级，从 1 开始。</param>
    /// <param name="isVisible">是否在菜单中显示。</param>
    /// <param name="isDisabled">是否禁用菜单。</param>
    /// <param name="isExternal">是否为外部链接。</param>
    /// <param name="externalUrl">外部链接地址；仅外链菜单提供。</param>
    /// <param name="keepAlive">是否缓存页面组件。</param>
    /// <param name="isAffix">是否固定页面标签。</param>
    /// <param name="description">菜单说明，可为空。</param>
    public Menu(
        string id,
        string systemId,
        string systemName,
        string code,
        string name,
        string? parentId,
        MenuType type,
        string? routePath,
        string? component,
        string? redirect,
        string? icon,
        string? permissionId,
        string? permissionName,
        int sort,
        int level,
        bool isVisible,
        bool isDisabled,
        bool isExternal,
        string? externalUrl,
        bool keepAlive,
        bool isAffix,
        string? description)
        : base(id)
    {
        Update(
            systemId,
            systemName,
            code,
            name,
            parentId,
            type,
            routePath,
            component,
            redirect,
            icon,
            permissionId,
            permissionName,
            sort,
            level,
            isVisible,
            isDisabled,
            isExternal,
            externalUrl,
            keepAlive,
            isAffix,
            description);
    }
    /// <summary>
    /// 所属系统标识。
    /// </summary>
    public string SystemId { get; private set; } = string.Empty;
    /// <summary>
    /// 所属系统名称快照。
    /// </summary>
    public string SystemName { get; private set; } = string.Empty;
    /// <summary>
    /// 菜单编码。
    /// </summary>
    public string Code { get; private set; } = string.Empty;
    /// <summary>
    /// 菜单名称。
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>
    /// 父级标识；为空表示根节点。
    /// </summary>
    public string? ParentId { get; private set; }
    /// <summary>
    /// 菜单类型，区分目录、页面菜单和按钮。
    /// </summary>
    public MenuType Type { get; private set; }
    /// <summary>
    /// 前端路由路径；页面菜单必须提供。
    /// </summary>
    public string? RoutePath { get; private set; }
    /// <summary>
    /// 前端组件标识；目录仅允许为空或 Layout。
    /// </summary>
    public string? Component { get; private set; }
    /// <summary>
    /// 重定向目标路径。
    /// </summary>
    public string? Redirect { get; private set; }
    /// <summary>
    /// 菜单图标标识。
    /// </summary>
    public string? Icon { get; private set; }
    /// <summary>
    /// 关联权限标识。
    /// </summary>
    public string? PermissionId { get; private set; }
    /// <summary>
    /// 关联权限名称快照。
    /// </summary>
    public string? PermissionName { get; private set; }
    /// <summary>
    /// 显示排序值。
    /// </summary>
    public int Sort { get; private set; }
    /// <summary>
    /// 菜单层级，从 1 开始。
    /// </summary>
    public int Level { get; private set; }
    /// <summary>
    /// 是否在菜单中显示。
    /// </summary>
    public bool IsVisible { get; private set; }
    /// <summary>
    /// 是否禁用菜单。
    /// </summary>
    public bool IsDisabled { get; private set; }
    /// <summary>
    /// 是否为外部链接。
    /// </summary>
    public bool IsExternal { get; private set; }
    /// <summary>
    /// 外部链接地址；仅外链菜单提供。
    /// </summary>
    public string? ExternalUrl { get; private set; }
    /// <summary>
    /// 是否缓存页面组件。
    /// </summary>
    public bool KeepAlive { get; private set; }
    /// <summary>
    /// 是否固定页面标签。
    /// </summary>
    public bool IsAffix { get; private set; }
    /// <summary>
    /// 菜单说明，可为空。
    /// </summary>
    public string? Description { get; private set; }
    /// <summary>
    /// 更新菜单全部字段并校验不变量。
    /// </summary>
    /// <param name="systemId">所属系统标识。</param>
    /// <param name="systemName">所属系统名称快照。</param>
    /// <param name="code">菜单编码。</param>
    /// <param name="name">菜单名称。</param>
    /// <param name="parentId">父级标识；为空表示根节点。</param>
    /// <param name="type">菜单类型，区分目录、页面菜单和按钮。</param>
    /// <param name="routePath">前端路由路径；页面菜单必须提供。</param>
    /// <param name="component">前端组件标识；目录仅允许为空或 Layout。</param>
    /// <param name="redirect">重定向目标路径。</param>
    /// <param name="icon">菜单图标标识。</param>
    /// <param name="permissionId">关联权限标识。</param>
    /// <param name="permissionName">关联权限名称快照。</param>
    /// <param name="sort">显示排序值。</param>
    /// <param name="level">菜单层级，从 1 开始。</param>
    /// <param name="isVisible">是否在菜单中显示。</param>
    /// <param name="isDisabled">是否禁用菜单。</param>
    /// <param name="isExternal">是否为外部链接。</param>
    /// <param name="externalUrl">外部链接地址；仅外链菜单提供。</param>
    /// <param name="keepAlive">是否缓存页面组件。</param>
    /// <param name="isAffix">是否固定页面标签。</param>
    /// <param name="description">菜单说明，可为空。</param>
    /// <returns>当前菜单聚合根实例。</returns>
    public Menu Update(
        string systemId,
        string systemName,
        string code,
        string name,
        string? parentId,
        MenuType type,
        string? routePath,
        string? component,
        string? redirect,
        string? icon,
        string? permissionId,
        string? permissionName,
        int sort,
        int level,
        bool isVisible,
        bool isDisabled,
        bool isExternal,
        string? externalUrl,
        bool keepAlive,
        bool isAffix,
        string? description)
    {
        SystemId = systemId;
        SystemName = Check.NotNullOrWhiteSpace(
            systemName,
            nameof(systemName),
            128);
        Code = Check.NotNullOrWhiteSpace(
            code,
            nameof(code),
            MenuConsts.MaxCodeLength);
        Name = Check.NotNullOrWhiteSpace(
            name,
            nameof(name),
            MenuConsts.MaxNameLength);
        ParentId = OptionalIdentifier(parentId);
        if (ParentId == Id) throw new ArgumentException("菜单不能作为自身父级。", nameof(parentId));
        if (!Enum.IsDefined(type) || sort < 0 || level < 1) throw new ArgumentException("菜单类型、排序或层级无效。");
        Type = type;
        RoutePath = Optional(routePath, MenuConsts.MaxRouteLength);
        Component = Optional(component, MenuConsts.MaxRouteLength);
        Redirect = Optional(redirect, MenuConsts.MaxRouteLength);
        Icon = Optional(icon, MenuConsts.MaxIconLength);
        PermissionId = OptionalIdentifier(permissionId);
        PermissionName = Optional(permissionName, MenuConsts.MaxNameLength);
        if ((PermissionId is null) != (PermissionName is null)) throw new ArgumentException("权限标识和名称快照必须同时提供。");
        Sort = sort;
        Level = level;
        IsVisible = isVisible;
        IsDisabled = isDisabled;
        IsExternal = isExternal;
        ExternalUrl = Optional(externalUrl, MenuConsts.MaxExternalUrlLength);
        KeepAlive = keepAlive;
        IsAffix = isAffix;
        Description = Optional(description, MenuConsts.MaxDescriptionLength);
        if (Type == MenuType.Directory && Component is not null && !string.Equals(
            Component,
            "Layout",
            StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("目录仅允许 Layout 组件。");
        if (Type == MenuType.Menu && RoutePath is null) throw new ArgumentException("页面菜单必须配置路由路径。");
        if (Type == MenuType.Button && ParentId is null) throw new ArgumentException("按钮菜单必须配置父级菜单。");
        if (IsExternal != (ExternalUrl is not null)) throw new ArgumentException("外链状态与外链地址不一致。");
        return this;
    }
    /// <summary>
    /// 将空白标识转换为 null，其他标识保留原值。
    /// </summary>
    /// <param name="value">待处理的文本。</param>
    /// <returns>原标识；空白输入返回 null。</returns>
    private static string? OptionalIdentifier(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
    /// <summary>
    /// 将空白文本转换为 null，并校验非空文本长度。
    /// </summary>
    /// <param name="value">待处理的文本。</param>
    /// <param name="length">允许的最大字符数。</param>
    /// <returns>通过长度校验的文本；空白输入返回 null。</returns>
    private static string? Optional(
        string? value,
        int length) => string.IsNullOrWhiteSpace(value) ? null : Check.NotNullOrWhiteSpace(
            value,
            nameof(value),
            length);
}
/// <summary>
/// 菜单类型。
/// </summary>
public enum MenuType
{
    /// <summary>
    /// 目录节点，用于组织下级菜单。
    /// </summary>
    Directory = 1,
    /// <summary>
    /// 页面菜单节点，指向可访问的前端路由。
    /// </summary>
    Menu = 2,
    /// <summary>
    /// 按钮节点，用于页面内操作入口。
    /// </summary>
    Button = 3
}
