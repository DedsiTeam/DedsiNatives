using DedsiNative.OpenIddict;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using Volo.Abp.DependencyInjection;

namespace DedsiNative.EntityFrameworkCore.Queries;

/// <summary>
/// 使用 EF Core 查询 OpenIddict 作用域，集中处理筛选、分页及明细转换。
/// </summary>
/// <param name="dbContext">
/// 数据库上下文。
/// </param>
public sealed class OpenIddictScopeQuery(DedsiNativeDbContext dbContext) : ITransientDependency
{
    /// <summary>
    /// 按条件分页查询 OpenIddict 作用域。
    /// </summary>
    /// <param name="input">
    /// 筛选条件与分页范围。
    /// </param>
    /// <param name="ct">
    /// 用于取消数据库查询的令牌。
    /// </param>
    /// <returns>
    /// 总记录数及当前页明细。
    /// </returns>
    public async Task<OpenIddictScopePagedQueryOutput> GetPagedAsync(
        OpenIddictScopePagedQueryInput input,
        CancellationToken ct)
    {
        var name = input.Name?.Trim();
        var displayName = input.DisplayName?.Trim();

        var query = dbContext.Set<OpenIddictEntityFrameworkCoreScope>()
            .AsNoTracking()
            .WhereIf(
                !string.IsNullOrEmpty(name),
                s => s.Name != null && s.Name.Contains(name!))
            .WhereIf(
                !string.IsNullOrEmpty(displayName),
                s => s.DisplayName != null && s.DisplayName.Contains(displayName!));

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(s => s.Name)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToArrayAsync(ct);

        var rows = items.Select(item => new OpenIddictScopeQueryItemDto(
            item.Id ?? string.Empty,
            item.Name,
            item.DisplayName,
            item.Description,
            string.IsNullOrEmpty(item.Resources) ? [] : System.Text.Json.JsonSerializer.Deserialize<string[]>(item.Resources) ?? []
        )).ToArray();

        return new OpenIddictScopePagedQueryOutput(totalCount, rows);
    }
}
