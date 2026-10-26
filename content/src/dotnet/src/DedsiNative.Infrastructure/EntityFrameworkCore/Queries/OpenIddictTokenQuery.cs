using DedsiNative.OpenIddict;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using Volo.Abp.DependencyInjection;

namespace DedsiNative.EntityFrameworkCore.Queries;

/// <summary>
/// 使用 EF Core 查询 OpenIddict 令牌，集中处理筛选、分页及明细转换。
/// </summary>
/// <param name="dbContext">
/// 数据库上下文。
/// </param>
public sealed class OpenIddictTokenQuery(DedsiNativeDbContext dbContext) : ITransientDependency
{
    /// <summary>
    /// 按条件分页查询 OpenIddict 令牌。
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
    public async Task<OpenIddictTokenPagedQueryOutput> GetPagedAsync(
        OpenIddictTokenPagedQueryInput input,
        CancellationToken ct)
    {
        var subject = input.Subject?.Trim();
        var applicationId = input.ApplicationId?.Trim();
        var type = input.Type?.Trim();
        var status = input.Status?.Trim();

        var query = dbContext.Set<OpenIddictEntityFrameworkCoreToken>()
            .Include(t => t.Application)
            .AsNoTracking()
            .WhereIf(
                !string.IsNullOrEmpty(subject),
                t => t.Subject == subject)
            .WhereIf(
                !string.IsNullOrEmpty(applicationId),
                t => t.Application != null && t.Application.Id == applicationId)
            .WhereIf(
                !string.IsNullOrEmpty(type),
                t => t.Type == type)
            .WhereIf(
                !string.IsNullOrEmpty(status),
                t => t.Status == status);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.CreationDate)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToArrayAsync(ct);

        var rows = items.Select(item => new OpenIddictTokenQueryItemDto(
            item.Id ?? string.Empty,
            item.Application?.Id,
            item.Application?.ClientId,
            item.Subject,
            item.Status,
            item.Type,
            item.CreationDate,
            item.ExpirationDate,
            item.RedemptionDate
        )).ToArray();

        return new OpenIddictTokenPagedQueryOutput(totalCount, rows);
    }
}
