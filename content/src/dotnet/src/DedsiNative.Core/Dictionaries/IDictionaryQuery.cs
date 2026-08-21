using Dedsi.Ddd.Domain.Queries;

namespace DedsiNative.Dictionaries;

/// <summary>
/// 字典分页查询条件。
/// </summary>
/// <param name="SystemId">所属系统筛选条件，为空时不筛选。</param>
/// <param name="Name">字典分组名称筛选条件，为空时不筛选。</param>
/// <param name="SkipCount">跳过的记录数。</param>
/// <param name="MaxResultCount">最多返回的记录数。</param>
/// <param name="IsExport">是否为导出查询，导出时不分页。</param>
public sealed record DictionaryPagedQuery(
    string? SystemId,
    string? Name,
    int SkipCount,
    int MaxResultCount,
    bool IsExport);

/// <summary>
/// 字典分页查询中的单行数据。
/// </summary>
/// <param name="Id">字典分组标识。</param>
/// <param name="SystemId">所属系统标识。</param>
/// <param name="SystemName">所属系统名称快照。</param>
/// <param name="Name">字典分组名称。</param>
/// <param name="ItemCount">字典项数量。</param>
public sealed record DictionaryQueryItem(
    string Id,
    string SystemId,
    string SystemName,
    string Name,
    int ItemCount);

/// <summary>
/// 字典分页查询结果。
/// </summary>
/// <param name="TotalCount">符合条件的总记录数。</param>
/// <param name="Items">当前页字典分组列表。</param>
public sealed record DictionaryPagedQueryResult(
    long TotalCount,
    DictionaryQueryItem[] Items);

/// <summary>
/// 字典只读查询接口，隔离领域层与具体持久化技术。
/// </summary>
public interface IDictionaryQuery : IDedsiQuery
{
    /// <summary>
    /// 按系统和名称分页查询字典分组。
    /// </summary>
    /// <param name="query">分页及筛选条件。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>字典分页查询结果。</returns>
    Task<DictionaryPagedQueryResult> GetPagedAsync(
        DictionaryPagedQuery query,
        CancellationToken cancellationToken);
}
