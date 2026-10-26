using DedsiNative.OpenIddict;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using Volo.Abp.DependencyInjection;

namespace DedsiNative.EntityFrameworkCore.Queries;

/// <summary>
/// 使用 EF Core 查询 OpenIddict 客户端，集中处理筛选、分页及明细转换。
/// </summary>
/// <param name="dbContext">
/// 数据库上下文。
/// </param>
public sealed class OpenIddictApplicationQuery(DedsiNativeDbContext dbContext) : ITransientDependency
{
    /// <summary>
    /// 按条件分页查询 OpenIddict 客户端。
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
    public async Task<OpenIddictApplicationPagedQueryOutput> GetPagedAsync(
        OpenIddictApplicationPagedQueryInput input,
        CancellationToken ct)
    {
        var clientId = input.ClientId?.Trim();
        var displayName = input.DisplayName?.Trim();
        var clientType = input.ClientType?.Trim();

        var query = dbContext.Set<OpenIddictEntityFrameworkCoreApplication>()
            .AsNoTracking()
            .WhereIf(
                !string.IsNullOrEmpty(clientId),
                a => a.ClientId != null && a.ClientId.Contains(clientId!))
            .WhereIf(
                !string.IsNullOrEmpty(displayName),
                a => a.DisplayName != null && a.DisplayName.Contains(displayName!))
            .WhereIf(
                !string.IsNullOrEmpty(clientType),
                a => a.ClientType == clientType);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(a => a.ClientId)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToArrayAsync(ct);

        var rows = items.Select(item => new OpenIddictApplicationQueryItemDto(
            item.Id ?? string.Empty,
            item.ClientId,
            item.DisplayName,
            item.ClientType,
            item.ConsentType,
            string.IsNullOrEmpty(item.RedirectUris) ? [] : System.Text.Json.JsonSerializer.Deserialize<string[]>(item.RedirectUris) ?? [],
            string.IsNullOrEmpty(item.PostLogoutRedirectUris) ? [] : System.Text.Json.JsonSerializer.Deserialize<string[]>(item.PostLogoutRedirectUris) ?? [],
            string.IsNullOrEmpty(item.Permissions) ? [] : System.Text.Json.JsonSerializer.Deserialize<string[]>(item.Permissions) ?? []
        )).ToArray();

        return new OpenIddictApplicationPagedQueryOutput(totalCount, rows);
    }
}
