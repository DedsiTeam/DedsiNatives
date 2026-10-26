using {{Namespace}};
using Microsoft.EntityFrameworkCore;
namespace DedsiNative.EntityFrameworkCore.Queries;

/// <summary>
/// 基于 EF 的标量投影查询，避免加载明细集合。
/// </summary>
public sealed class {{Name}}Query(DedsiNativeDbContext dbContext) : I{{Name}}Query
{
    /// <inheritdoc />
    public async Task<{{Name}}PagedQueryOutput> GetPagedAsync(
        {{Name}}PagedQueryInput input,
        CancellationToken ct)
    {
        if (input.SkipCount < 0 || input.MaxResultCount is < 1 or > 200)
            throw new ArgumentOutOfRangeException(nameof(input));
        var filtered = ApplyFilters(dbContext.Set<{{Name}}>().IgnoreAutoIncludes().AsNoTracking(), input);
        var total = await filtered.LongCountAsync(ct);
        var items = await filtered.OrderBy(entity => entity.Id)
            .Skip(input.SkipCount).Take(input.MaxResultCount)
            .Select(entity => new {{Name}}QueryItemDto(
{{QueryArguments}})).ToArrayAsync(ct);
        return new {{Name}}PagedQueryOutput(total, items);
    }

    /// <summary>
    /// 组合数据库可翻译的筛选表达式；空字符串不参与筛选。
    /// </summary>
    public static IQueryable<{{Name}}> ApplyFilters(
        IQueryable<{{Name}}> query,
        {{Name}}PagedQueryInput input)
    {
{{FilterLocals}}
        return query
{{Filters}};
    }
}
