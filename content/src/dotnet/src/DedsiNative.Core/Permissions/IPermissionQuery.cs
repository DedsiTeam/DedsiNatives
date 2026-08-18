using Dedsi.Ddd.Domain.Queries;

namespace DedsiNative.Permissions;

/// <summary>权限分页查询条件。</summary>
/// <param name="SystemId">系统筛选条件，为空时不筛选。</param>
/// <param name="Name">权限名称筛选条件，为空时不筛选。</param>
/// <param name="IsEnabled">启用状态筛选，为空时查询全部状态。</param>
/// <param name="SkipCount">需要跳过的记录数。</param>
/// <param name="MaxResultCount">单页最多返回的记录数。</param>
/// <param name="IsExport">是否为导出查询；导出时不分页。</param>
public sealed record PermissionPagedQuery(
    string? SystemId,
    string? Name,
    bool? IsEnabled,
    int SkipCount,
    int MaxResultCount,
    bool IsExport);

/// <summary>权限分页查询中的单行结果。</summary>
/// <param name="Id">权限唯一标识。</param>
/// <param name="SystemId">所属系统 ID。</param>
/// <param name="SystemName">所属系统名称。</param>
/// <param name="Name">权限名称。</param>
/// <param name="Description">权限说明。</param>
/// <param name="IsEnabled">是否启用。</param>
public sealed record PermissionQueryItem(
    string Id,
    string SystemId,
    string SystemName,
    string Name,
    string? Description,
    bool IsEnabled);

/// <summary>权限分页查询结果。</summary>
/// <param name="TotalCount">符合条件的记录总数。</param>
/// <param name="Items">当前查询返回的权限列表。</param>
public sealed record PermissionPagedQueryResult(
    long TotalCount,
    IReadOnlyList<PermissionQueryItem> Items);

/// <summary>权限只读查询接口，隔离 Core 与具体持久化技术。</summary>
public interface IPermissionQuery : IDedsiQuery
{
    /// <summary>按系统、名称和启用状态筛选权限。</summary>
    /// <param name="query">权限分页查询条件。</param>
    /// <param name="cancellationToken">用于取消异步查询的令牌。</param>
    /// <returns>权限分页查询结果。</returns>
    Task<PermissionPagedQueryResult> GetPagedAsync(
        PermissionPagedQuery query,
        CancellationToken cancellationToken);
}
