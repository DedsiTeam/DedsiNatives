using Dedsi.Ddd.Domain.Queries;

namespace DedsiNative.Positions;

/// <summary>岗位分页查询条件。</summary>
public sealed record PositionPagedQuery(
    string? SystemId,
    string? Name,
    bool? IsEnabled,
    int SkipCount,
    int MaxResultCount,
    bool IsExport);

/// <summary>岗位分页查询中的单行结果。</summary>
public sealed record PositionQueryItem(
    string Id,
    string Name,
    string SystemId,
    string SystemName,
    string? Description,
    bool IsEnabled,
    int PermissionCount,
    int OrganizationCount);

/// <summary>岗位分页查询结果。</summary>
public sealed record PositionPagedQueryResult(long TotalCount, IReadOnlyList<PositionQueryItem> Items);

/// <summary>岗位只读查询接口。</summary>
public interface IPositionQuery : IDedsiQuery
{
    /// <summary>按系统、名称和启用状态筛选岗位。</summary>
    Task<PositionPagedQueryResult> GetPagedAsync(
        PositionPagedQuery query,
        CancellationToken cancellationToken);
}
