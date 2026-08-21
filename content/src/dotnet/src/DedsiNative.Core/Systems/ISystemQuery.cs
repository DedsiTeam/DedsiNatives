using Dedsi.Ddd.Domain.Queries;

namespace DedsiNative.Systems;

/// <summary>系统分页查询条件。</summary>
/// <param name="Name">系统名称筛选条件，为空时不筛选。</param>
/// <param name="SkipCount">需要跳过的记录数。</param>
/// <param name="MaxResultCount">单页最多返回的记录数。</param>
/// <param name="IsExport">是否为导出查询；导出时不分页。</param>
public sealed record SystemPagedQuery(
    string? Name,
    int SkipCount,
    int MaxResultCount,
    bool IsExport);

/// <summary>系统分页查询中的单行结果。</summary>
/// <param name="Id">系统唯一标识。</param>
/// <param name="Name">系统名称。</param>
/// <param name="Description">系统说明。</param>
/// <param name="Sort">系统展示排序。</param>
public sealed record SystemQueryItem(string Id, string Name, string? Description, int Sort);

/// <summary>系统分页查询结果。</summary>
/// <param name="TotalCount">符合条件的记录总数。</param>
/// <param name="Items">当前查询返回的系统列表。</param>
public sealed record SystemPagedQueryResult(
    long TotalCount,
    SystemQueryItem[] Items);

/// <summary>系统只读查询接口，隔离 Core 与具体持久化技术。</summary>
public interface ISystemQuery : IDedsiQuery
{
    /// <summary>按名称筛选系统，并按排序值和 ID 稳定排序。</summary>
    /// <param name="query">系统分页查询条件。</param>
    /// <param name="cancellationToken">用于取消异步查询的令牌。</param>
    /// <returns>系统分页查询结果。</returns>
    Task<SystemPagedQueryResult> GetPagedAsync(
        SystemPagedQuery query,
        CancellationToken cancellationToken);
}
