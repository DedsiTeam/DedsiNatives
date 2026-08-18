using DedsiNative.Systems;
using Microsoft.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Queries;

/// <summary>系统查询服务的 EF Core 实现。</summary>
/// <param name="dbContext">DedsiNative 数据库上下文。</param>
public sealed class SystemQuery(IDedsiNativeDbContext dbContext) : ISystemQuery
{
    /// <inheritdoc />
    public async Task<SystemPagedQueryResult> GetPagedAsync(
        SystemPagedQuery query,
        CancellationToken cancellationToken)
    {
        var name = query.Name?.Trim();
        var systems = dbContext.Systems
            .AsNoTracking()
            .WhereIf(
                !string.IsNullOrEmpty(name),
                system => system.Name.Contains(name!));

        var totalCount = await systems.LongCountAsync(cancellationToken);
        systems = systems.OrderBy(system => system.Sort).ThenBy(system => system.Id);

        if (!query.IsExport)
        {
            systems = systems.Skip(query.SkipCount).Take(query.MaxResultCount);
        }

        var items = await systems
            .Select(system => new SystemQueryItem(
                system.Id,
                system.Name,
                system.Description,
                system.Sort))
            .ToListAsync(cancellationToken);

        return new SystemPagedQueryResult(totalCount, items);
    }
}
