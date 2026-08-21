using DedsiNative.LoginAudits;
using Microsoft.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Queries;

/// <summary>
/// 登录审计读侧查询服务的 EF Core 实现。
/// </summary>
/// <param name="dbContext">DedsiNative 数据库上下文。</param>
public sealed class LoginAuditQuery(IDedsiNativeDbContext dbContext) : ILoginAuditQuery
{
    /// <summary>
    /// 按时间、认证结果、原因、账号、用户和客户端 IP 条件分页查询登录审计。
    /// </summary>
    /// <param name="query">登录审计筛选及分页条件。</param>
    /// <param name="cancellationToken">用于取消异步查询的令牌。</param>
    /// <returns>登录审计分页查询结果。</returns>
    public async Task<LoginAuditPagedQueryResult> GetPagedAsync(
        LoginAuditPagedQuery query,
        CancellationToken cancellationToken)
    {
        var account = query.Account?.Trim();
        var userName = query.UserName?.Trim();
        var clientIp = query.ClientIp?.Trim();
        var startTimeUtc = query.StartTimeUtc?.ToUniversalTime();
        var endTimeUtc = query.EndTimeUtc?.ToUniversalTime();

        var audits = dbContext.LoginAudits
            .AsNoTracking()
            .WhereIf(startTimeUtc.HasValue, audit => audit.LoginTimeUtc >= startTimeUtc!.Value)
            .WhereIf(endTimeUtc.HasValue, audit => audit.LoginTimeUtc <= endTimeUtc!.Value)
            .WhereIf(query.Result.HasValue, audit => audit.Result == query.Result!.Value)
            .WhereIf(query.Reason.HasValue, audit => audit.Reason == query.Reason!.Value)
            .WhereIf(!string.IsNullOrEmpty(account), audit => audit.Account.Contains(account!))
            .WhereIf(!string.IsNullOrEmpty(userName), audit => audit.UserName != null && audit.UserName.Contains(userName!))
            .WhereIf(query.UserId.HasValue, audit => audit.UserId == query.UserId)
            .WhereIf(!string.IsNullOrEmpty(clientIp), audit => audit.ClientIp != null && audit.ClientIp.Contains(clientIp!));

        var totalCount = await audits.LongCountAsync(cancellationToken);
        audits = audits
            .OrderByDescending(audit => audit.LoginTimeUtc)
            .ThenByDescending(audit => audit.Id);

        audits = audits
            .Skip(query.SkipCount)
            .Take(query.MaxResultCount);

        var items = await audits
            .Select(audit => new LoginAuditQueryItem(
                audit.Id,
                audit.LoginTimeUtc,
                audit.Result,
                audit.Reason,
                audit.Account,
                audit.UserName,
                audit.UserId,
                audit.ClientIp,
                audit.FailureDescription))
            .ToArrayAsync(cancellationToken);

        return new LoginAuditPagedQueryResult(totalCount, items);
    }
}
