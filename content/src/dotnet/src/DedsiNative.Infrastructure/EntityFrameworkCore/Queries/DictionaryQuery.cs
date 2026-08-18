using DedsiNative.Dictionaries;
using Microsoft.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Queries;

/// <summary>
/// 字典只读查询服务的 EF Core 实现。
/// </summary>
/// <param name="dbContext">DedsiNative 数据库上下文。</param>
public sealed class DictionaryQuery(IDedsiNativeDbContext dbContext) : IDictionaryQuery
{
    /// <inheritdoc />
    public async Task<DictionaryPagedQueryResult> GetPagedAsync(
        DictionaryPagedQuery query,
        CancellationToken cancellationToken)
    {
        var systemId = query.SystemId?.Trim();
        var name = query.Name?.Trim();
        var dictionaries = dbContext.Dictionaries
            .AsNoTracking()
            .WhereIf(
                !string.IsNullOrEmpty(systemId),
                dictionary => dictionary.SystemId == systemId)
            .WhereIf(
                !string.IsNullOrEmpty(name),
                dictionary => dictionary.Name.Contains(name!));

        var totalCount = await dictionaries.LongCountAsync(cancellationToken);
        dictionaries = dictionaries
            .OrderBy(dictionary => dictionary.SystemId)
            .ThenBy(dictionary => dictionary.Name)
            .ThenBy(dictionary => dictionary.Id);

        if (!query.IsExport)
        {
            dictionaries = dictionaries
                .Skip(query.SkipCount)
                .Take(query.MaxResultCount);
        }

        var items = await dictionaries
            .Select(dictionary => new DictionaryQueryItem(
                dictionary.Id,
                dictionary.SystemId,
                dictionary.SystemName,
                dictionary.Name,
                dictionary.Items.Count))
            .ToListAsync(cancellationToken);

        return new DictionaryPagedQueryResult(totalCount, items);
    }
}
