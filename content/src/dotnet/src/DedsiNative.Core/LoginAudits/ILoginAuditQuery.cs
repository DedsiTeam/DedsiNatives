using Dedsi.Ddd.Domain.Queries;

namespace DedsiNative.LoginAudits;

/// <summary>
/// 登录审计分页查询条件。
/// </summary>
/// <param name="StartTimeUtc">登录时间下限（含）。</param>
/// <param name="EndTimeUtc">登录时间上限（含）。</param>
/// <param name="Result">认证结果筛选条件。</param>
/// <param name="Reason">认证原因筛选条件。</param>
/// <param name="Account">登录账号模糊筛选条件。</param>
/// <param name="UserName">用户名称模糊筛选条件。</param>
/// <param name="UserId">用户标识筛选条件。</param>
/// <param name="ClientIp">客户端 IP 模糊筛选条件。</param>
/// <param name="SkipCount">要跳过的记录数。</param>
/// <param name="MaxResultCount">单页最多返回的记录数。</param>
public sealed record LoginAuditPagedQuery(
    DateTime? StartTimeUtc,
    DateTime? EndTimeUtc,
    LoginResult? Result,
    LoginReason? Reason,
    string? Account,
    string? UserName,
    Guid? UserId,
    string? ClientIp,
    int SkipCount,
    int MaxResultCount);

/// <summary>
/// 登录审计分页结果的一行投影。
/// </summary>
/// <param name="Id">审计记录标识。</param>
/// <param name="LoginTimeUtc">登录尝试发生时间（UTC）。</param>
/// <param name="Result">认证结果。</param>
/// <param name="Reason">认证原因。</param>
/// <param name="Account">提交的登录账号。</param>
/// <param name="UserName">可识别用户的名称。</param>
/// <param name="UserId">可识别用户的标识。</param>
/// <param name="ClientIp">客户端 IP。</param>
/// <param name="FailureDescription">脱敏后的失败说明。</param>
public sealed record LoginAuditQueryItem(
    string Id,
    DateTime LoginTimeUtc,
    LoginResult Result,
    LoginReason Reason,
    string Account,
    string? UserName,
    Guid? UserId,
    string? ClientIp,
    string? FailureDescription);

/// <summary>
/// 登录审计分页查询结果。
/// </summary>
/// <param name="TotalCount">符合筛选条件的总记录数。</param>
/// <param name="Items">当前页的审计记录投影。</param>
public sealed record LoginAuditPagedQueryResult(
    long TotalCount,
    IReadOnlyList<LoginAuditQueryItem> Items);

/// <summary>
/// 登录审计读侧查询契约，隔离 Host 与具体持久化技术。
/// </summary>
public interface ILoginAuditQuery : IDedsiQuery
{
    /// <summary>
    /// 按条件分页查询登录审计记录。
    /// </summary>
    /// <param name="query">登录审计筛选与分页条件。</param>
    /// <param name="cancellationToken">用于取消异步查询的令牌。</param>
    /// <returns>登录审计分页查询结果。</returns>
    Task<LoginAuditPagedQueryResult> GetPagedAsync(
        LoginAuditPagedQuery query,
        CancellationToken cancellationToken);
}
