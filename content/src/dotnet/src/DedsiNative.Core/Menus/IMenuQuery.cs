using Dedsi.Ddd.Domain.Queries;

namespace DedsiNative.Menus;

/// <summary>
/// 菜单分页查询条件。
/// </summary>
/// <param name="SystemId">
/// 所属系统标识。
/// </param>
/// <param name="Name">
/// 菜单名称。
/// </param>
/// <param name="Code">
/// 菜单编码。
/// </param>
/// <param name="Type">
/// 菜单类型，区分目录、页面菜单和按钮。
/// </param>
/// <param name="ParentId">
/// 父级标识；为空表示根节点。
/// </param>
/// <param name="IsVisible">
/// 是否在菜单中显示。
/// </param>
/// <param name="IsDisabled">
/// 是否禁用菜单。
/// </param>
/// <param name="IsExternal">
/// 是否为外部链接。
/// </param>
/// <param name="SkipCount">
/// 跳过的记录数。
/// </param>
/// <param name="MaxResultCount">
/// 本次查询最多返回的记录数。
/// </param>
/// <param name="IsExport">
/// 是否按导出模式返回全部匹配数据。
/// </param>
public sealed record MenuPagedQueryInput(
    string? SystemId,
    string? Name,
    string? Code,
    MenuType? Type,
    string? ParentId,
    bool? IsVisible,
    bool? IsDisabled,
    bool? IsExternal,
    int SkipCount,
    int MaxResultCount,
    bool IsExport);

/// <summary>
/// 菜单查询项。
/// </summary>
/// <param name="Id">
/// 菜单唯一标识。
/// </param>
/// <param name="SystemId">
/// 所属系统标识。
/// </param>
/// <param name="SystemName">
/// 所属系统名称快照。
/// </param>
/// <param name="Code">
/// 菜单编码。
/// </param>
/// <param name="Name">
/// 菜单名称。
/// </param>
/// <param name="ParentId">
/// 父级标识；为空表示根节点。
/// </param>
/// <param name="Type">
/// 菜单类型，区分目录、页面菜单和按钮。
/// </param>
/// <param name="RoutePath">
/// 前端路由路径；页面菜单必须提供。
/// </param>
/// <param name="Component">
/// 前端组件标识；目录仅允许为空或 Layout。
/// </param>
/// <param name="Redirect">
/// 重定向目标路径。
/// </param>
/// <param name="Icon">
/// 菜单图标标识。
/// </param>
/// <param name="PermissionId">
/// 关联权限标识。
/// </param>
/// <param name="PermissionName">
/// 关联权限名称快照。
/// </param>
/// <param name="Sort">
/// 显示排序值。
/// </param>
/// <param name="Level">
/// 菜单层级，从 1 开始。
/// </param>
/// <param name="IsVisible">
/// 是否在菜单中显示。
/// </param>
/// <param name="IsDisabled">
/// 是否禁用菜单。
/// </param>
/// <param name="IsExternal">
/// 是否为外部链接。
/// </param>
/// <param name="ExternalUrl">
/// 外部链接地址；仅外链菜单提供。
/// </param>
/// <param name="KeepAlive">
/// 是否缓存页面组件。
/// </param>
/// <param name="IsAffix">
/// 是否固定页面标签。
/// </param>
/// <param name="Description">
/// 菜单说明，可为空。
/// </param>
public sealed record MenuQueryItemDto(
    string Id,
    string SystemId,
    string SystemName,
    string Code,
    string Name,
    string? ParentId,
    MenuType Type,
    string? RoutePath,
    string? Component,
    string? Redirect,
    string? Icon,
    string? PermissionId,
    string? PermissionName,
    int Sort,
    int Level,
    bool IsVisible,
    bool IsDisabled,
    bool IsExternal,
    string? ExternalUrl,
    bool KeepAlive,
    bool IsAffix,
    string? Description);

/// <summary>
/// 菜单分页结果。
/// </summary>
/// <param name="TotalCount">
/// 符合筛选条件的记录总数。
/// </param>
/// <param name="Items">
/// 本次查询返回的明细数组。
/// </param>
public sealed record MenuPagedQueryOutput(
    long TotalCount,
    MenuQueryItemDto[] Items);

/// <summary>
/// 菜单读侧查询契约。
/// </summary>
public interface IMenuQuery : IDedsiQuery
{
    /// <summary>
    /// 分页查询菜单列表。
    /// </summary>
    /// <param name="query">分页及业务筛选条件。</param>
    /// <param name="cancellationToken">用于取消异步查询的令牌。</param>
    /// <returns>包含匹配总数和本次查询明细的分页结果。</returns>
    Task<MenuPagedQueryOutput> GetPagedAsync(
        MenuPagedQueryInput query,
        CancellationToken cancellationToken);

    /// <summary>
    /// 判断指定菜单是否仍包含直接子菜单。
    /// </summary>
    /// <param name="id">待删除菜单的标识。</param>
    /// <param name="cancellationToken">用于取消异步操作的令牌。</param>
    /// <returns>存在直接子菜单时返回 <see langword="true"/>。</returns>
    Task<bool> HasChildrenAsync(
        string id,
        CancellationToken cancellationToken);

    /// <summary>
    /// 判断系统内是否已经存在相同的菜单编码。
    /// </summary>
    /// <param name="systemId">菜单所属系统标识。</param>
    /// <param name="code">待校验的菜单编码。</param>
    /// <param name="cancellationToken">用于取消异步操作的令牌。</param>
    /// <param name="excludedMenuId">更新时需要排除的当前菜单标识。</param>
    /// <returns>编码已被占用时返回 <see langword="true"/>。</returns>
    Task<bool> ExistsBySystemAndCodeAsync(
        string systemId,
        string code,
        CancellationToken cancellationToken,
        string? excludedMenuId = null);

    /// <summary>
    /// 判断将候选父级设置给菜单后是否会形成循环引用。
    /// </summary>
    /// <param name="menuId">待修改菜单的标识。</param>
    /// <param name="candidateParentId">候选父菜单标识。</param>
    /// <param name="cancellationToken">用于取消异步操作的令牌。</param>
    /// <returns>会形成循环引用时返回 <see langword="true"/>。</returns>
    Task<bool> WouldCreateCycleAsync(
        string menuId,
        string candidateParentId,
        CancellationToken cancellationToken);
}
