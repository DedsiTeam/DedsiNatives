using DedsiNative.OpenIddict;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using Volo.Abp.DependencyInjection;

namespace DedsiNative.EntityFrameworkCore.Queries;

/// <summary>
/// 使用 EF Core 查询 OpenIddict 授权记录，集中处理筛选、分页及明细转换。
/// </summary>
/// <param name="dbContext">
/// 数据库上下文。
/// </param>
public sealed class OpenIddictAuthorizationQuery(DedsiNativeDbContext dbContext) : ITransientDependency
{
    /// <summary>
    /// 按条件分页查询 OpenIddict 授权记录。
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
    public async Task<OpenIddictAuthorizationPagedQueryOutput> GetPagedAsync(
        OpenIddictAuthorizationPagedQueryInput input,
        CancellationToken ct)
    {
        var subject = input.Subject?.Trim();
        var applicationId = input.ApplicationId?.Trim();
        var status = input.Status?.Trim();

        var query = dbContext.Set<OpenIddictEntityFrameworkCoreAuthorization>()
            .Include(a => a.Application)
            .AsNoTracking()
            .WhereIf(
                !string.IsNullOrEmpty(subject),
                a => a.Subject == subject)
            .WhereIf(
                !string.IsNullOrEmpty(applicationId),
                a => a.Application != null && a.Application.Id == applicationId)
            .WhereIf(
                !string.IsNullOrEmpty(status),
                a => a.Status == status);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.CreationDate)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToArrayAsync(ct);

        var rows = items.Select(item => new OpenIddictAuthorizationQueryItemDto(
            item.Id ?? string.Empty,
            item.Application?.Id,
            item.Application?.ClientId,
            item.Application?.DisplayName,
            item.Subject,
            item.Status,
            item.Type,
            string.IsNullOrEmpty(item.Scopes) ? [] : System.Text.Json.JsonSerializer.Deserialize<string[]>(item.Scopes) ?? [],
            item.CreationDate
        )).ToArray();

        return new OpenIddictAuthorizationPagedQueryOutput(totalCount, rows);
    }
}
