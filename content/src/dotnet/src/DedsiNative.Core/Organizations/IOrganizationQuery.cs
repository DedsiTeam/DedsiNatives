using Dedsi.Ddd.Domain.Queries;

namespace DedsiNative.Organizations;

/// <summary>
/// 组织机构树查询条件参数模型。
/// </summary>
/// <param name="SystemId">
/// 所属系统筛选条件，为空时不按系统筛选。
/// </param>
/// <param name="IsEnabled">
/// 是否只查询指定启用状态的组织（null 表示全部）。
/// </param>
public sealed record OrganizationTreeQuery(
    string? SystemId,
    bool? IsEnabled = null);

/// <summary>
/// 组织机构分页检索参数模型。
/// </summary>
/// <param name="SystemId">
/// 所属系统筛选条件，为空时不筛选。
/// </param>
/// <param name="Keyword">
/// 组织名称或编码模糊搜索关键字。
/// </param>
/// <param name="ParentId">
/// 指定父级组织标识筛选（null 查全部）。
/// </param>
/// <param name="IsEnabled">
/// 启用状态筛选（null 表示全部）。
/// </param>
/// <param name="SkipCount">
/// 需要跳过的记录数。
/// </param>
/// <param name="MaxResultCount">
/// 单页最多返回的记录数。
/// </param>
/// <param name="IsExport">
/// 是否为导出查询；导出时不分页。
/// </param>
public sealed record OrganizationPagedQuery(
    string? SystemId,
    string? Keyword,
    string? ParentId,
    bool? IsEnabled,
    int SkipCount,
    int MaxResultCount,
    bool IsExport);

/// <summary>
/// 组织机构查询投影 DTO。
/// </summary>
/// <param name="Id">组织唯一标识，26 位 ULID。</param>
/// <param name="SystemId">所属系统标识。</param>
/// <param name="SystemName">所属系统名称。</param>
/// <param name="Code">组织机构编码。</param>
/// <param name="Name">组织机构主名称。</param>
/// <param name="Name1">组织机构名称 1。</param>
/// <param name="Name2">组织机构名称 2。</param>
/// <param name="Name3">组织机构名称 3。</param>
/// <param name="Name4">组织机构名称 4。</param>
/// <param name="ParentId">父级组织标识。</param>
/// <param name="Sort">同级排序序号。</param>
/// <param name="Level">组织层级深度。</param>
/// <param name="IsEnabled">是否启用。</param>
/// <param name="Description">组织说明。</param>
/// <param name="CreatedAtUtc">创建时间（UTC）。</param>
public sealed record OrganizationQueryItem(
    string Id,
    string SystemId,
    string SystemName,
    string Code,
    string Name,
    string? Name1,
    string? Name2,
    string? Name3,
    string? Name4,
    string? ParentId,
    int Sort,
    int Level,
    bool IsEnabled,
    string? Description,
    DateTime CreatedAtUtc);

/// <summary>
/// 组织机构分页查询结果集。
/// </summary>
/// <param name="TotalCount">符合条件的总记录数。</param>
/// <param name="Items">当前页组织机构记录列表。</param>
public sealed record OrganizationPagedQueryResult(
    long TotalCount,
    OrganizationQueryItem[] Items);

/// <summary>
/// 组织机构只读查询契约。
/// </summary>
public interface IOrganizationQuery : IDedsiQuery
{
    /// <summary>
    /// 获取指定系统下按层级和排序排列的扁平组织机构列表（用于快速构建多级组织树）。
    /// </summary>
    /// <param name="query">
    /// 组织树查询条件。
    /// </param>
    /// <param name="cancellationToken">
    /// 异步操作取消令牌。
    /// </param>
    /// <returns>
    /// 已按层级和排序序号升序排列的组织机构列表。
    /// </returns>
    Task<OrganizationQueryItem[]> GetTreeListAsync(
        OrganizationTreeQuery query,
        CancellationToken cancellationToken);

    /// <summary>
    /// 分页检索组织机构列表。
    /// </summary>
    /// <param name="query">
    /// 分页筛选条件。
    /// </param>
    /// <param name="cancellationToken">
    /// 异步操作取消令牌。
    /// </param>
    /// <returns>
    /// 分页查询结果集。
    /// </returns>
    Task<OrganizationPagedQueryResult> GetPagedAsync(
        OrganizationPagedQuery query,
        CancellationToken cancellationToken);

    /// <summary>
    /// 检查指定组织下是否包含未删除的子级组织节点。
    /// </summary>
    /// <param name="id">
    /// 待检查的组织机构唯一标识。
    /// </param>
    /// <param name="cancellationToken">
    /// 异步操作取消令牌。
    /// </param>
    /// <returns>
    /// 若存在子级组织返回 true，否则返回 false。
    /// </returns>
    Task<bool> HasChildrenAsync(string id, CancellationToken cancellationToken);

    /// <summary>
    /// 检查同一系统下是否已存在相同编码的组织机构。
    /// </summary>
    /// <param name="systemId">
    /// 所属系统唯一标识。
    /// </param>
    /// <param name="code">
    /// 待校验的组织机构编码。
    /// </param>
    /// <param name="cancellationToken">
    /// 异步操作取消令牌。
    /// </param>
    /// <param name="excludedId">
    /// 更新场景下需要排除的当前组织标识。
    /// </param>
    /// <returns>
    /// 若存在重复编码返回 true，否则返回 false。
    /// </returns>
    Task<bool> ExistsBySystemAndCodeAsync(
        string systemId,
        string code,
        CancellationToken cancellationToken,
        string? excludedId = null);

    /// <summary>
    /// 检查调整父级组织是否会导致树形结构出现循环引用（即候选父组织不能是当前组织或其任一后代子组织）。
    /// </summary>
    /// <param name="id">
    /// 当前组织唯一标识。
    /// </param>
    /// <param name="candidateParentId">
    /// 拟设置的目标父级组织唯一标识。
    /// </param>
    /// <param name="cancellationToken">
    /// 异步操作取消令牌。
    /// </param>
    /// <returns>
    /// 若会导致循环依赖返回 true，合法返回 false。
    /// </returns>
    Task<bool> WouldCreateCycleAsync(
        string id,
        string candidateParentId,
        CancellationToken cancellationToken);
}
