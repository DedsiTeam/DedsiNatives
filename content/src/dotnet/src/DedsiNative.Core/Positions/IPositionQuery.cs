using Dedsi.Ddd.Domain.Queries;

namespace DedsiNative.Positions;

/// <summary>
/// 岗位分页查询条件。
/// </summary>
/// <param name="SystemId">
/// 所属系统标识。
/// </param>
/// <param name="Name">
/// 岗位名称。
/// </param>
/// <param name="IsEnabled">
/// 是否启用。
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
public sealed record PositionPagedQueryInput(
    string? SystemId,
    string? Name,
    bool? IsEnabled,
    int SkipCount,
    int MaxResultCount,
    bool IsExport);

/// <summary>
/// 岗位分页查询中的单行结果。
/// </summary>
/// <param name="Id">
/// 岗位唯一标识。
/// </param>
/// <param name="Name">
/// 岗位名称。
/// </param>
/// <param name="SystemId">
/// 所属系统标识。
/// </param>
/// <param name="SystemName">
/// 所属系统名称快照。
/// </param>
/// <param name="Description">
/// 岗位说明，可为空。
/// </param>
/// <param name="IsEnabled">
/// 是否启用。
/// </param>
/// <param name="PermissionCount">
/// 关联权限数量。
/// </param>
/// <param name="OrganizationCount">
/// 关联组织机构数量。
/// </param>
public sealed record PositionQueryItemDto(
    string Id,
    string Name,
    string SystemId,
    string SystemName,
    string? Description,
    bool IsEnabled,
    int PermissionCount,
    int OrganizationCount);

/// <summary>
/// 岗位分页查询结果。
/// </summary>
/// <param name="TotalCount">
/// 符合筛选条件的记录总数。
/// </param>
/// <param name="Items">
/// 本次查询返回的明细数组。
/// </param>
public sealed record PositionPagedQueryOutput(
    long TotalCount,
    PositionQueryItemDto[] Items);

/// <summary>
/// 岗位只读查询接口。
/// </summary>
public interface IPositionQuery : IDedsiQuery
{
    /// <summary>
    /// 按系统、名称和启用状态筛选岗位。
    /// </summary>
    /// <param name="query">分页及业务筛选条件。</param>
    /// <param name="cancellationToken">用于取消异步查询的令牌。</param>
    /// <returns>包含匹配总数和本次查询明细的分页结果。</returns>
    Task<PositionPagedQueryOutput> GetPagedAsync(
        PositionPagedQueryInput query,
        CancellationToken cancellationToken);

    /// <summary>
    /// 查询包含指定权限关联的岗位聚合，并加载岗位权限集合。
    /// </summary>
    /// <param name="permissionId">权限唯一标识。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>关联岗位聚合列表。</returns>
    Task<Position[]> GetByPermissionIdAsync(
        string permissionId,
        CancellationToken cancellationToken);
}
