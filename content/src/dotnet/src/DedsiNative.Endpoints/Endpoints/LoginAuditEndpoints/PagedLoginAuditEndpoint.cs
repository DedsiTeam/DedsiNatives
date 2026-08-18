using Dedsi.Ddd.Application.Contracts.Dtos;
using DedsiNative.LoginAudits;
using FastEndpoints;

namespace DedsiNative.Endpoints.LoginAuditEndpoints;

/// <summary>
/// 登录审计分页查询请求。
/// </summary>
public sealed class PagedLoginAuditRequest : DedsiPagedRequestDto
{
    /// <summary>
    /// 登录时间下限（UTC），为空时不限制。
    /// </summary>
    public DateTime? StartTimeUtc { get; set; }

    /// <summary>
    /// 登录时间上限（UTC），为空时不限制。
    /// </summary>
    public DateTime? EndTimeUtc { get; set; }

    /// <summary>
    /// 认证结果筛选条件。
    /// </summary>
    public LoginResult? Result { get; set; }

    /// <summary>
    /// 认证原因筛选条件。
    /// </summary>
    public LoginReason? Reason { get; set; }

    /// <summary>
    /// 登录账号模糊筛选条件。
    /// </summary>
    public string? Account { get; set; }

    /// <summary>
    /// 用户名称模糊筛选条件。
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 用户标识筛选条件。
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 客户端 IP 模糊筛选条件。
    /// </summary>
    public string? ClientIp { get; set; }
}

/// <summary>
/// 登录审计分页结果中的单行数据。
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
public sealed record PagedLoginAuditRowResponse(
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
/// 登录审计分页查询响应。
/// </summary>
public sealed class PagedLoginAuditResponse
    : DedsiPagedResultDto<PagedLoginAuditRowResponse>;

/// <summary>
/// 登录审计分页查询端点，仅允许拥有审计查看权限的用户访问。
/// </summary>
/// <param name="loginAuditQuery">登录审计读侧查询服务。</param>
public sealed class PagedLoginAuditEndpoint(ILoginAuditQuery loginAuditQuery)
    : Endpoint<PagedLoginAuditRequest, PagedLoginAuditResponse>
{
    /// <summary>
    /// 配置登录审计分页查询接口、HTTP 方法和查看权限。
    /// </summary>
    public override void Configure()
    {
        Post("/api/login-audit/pagedQuery");
        Policies(LoginAuditPermissions.View);
        Description(description => description.WithTags("登录审计"));
        Summary(summary =>
        {
            summary.Summary = "分页查询登录审计";
            summary.Description = "按时间、结果、原因、账号、用户和客户端 IP 查询登录审计记录。";
        });
    }

    /// <summary>
    /// 按筛选条件查询登录审计并返回受权的分页投影。
    /// </summary>
    /// <param name="req">登录审计筛选和分页请求。</param>
    /// <param name="ct">用于取消异步查询的令牌。</param>
    public override async Task HandleAsync(PagedLoginAuditRequest req, CancellationToken ct)
    {
        var result = await loginAuditQuery.GetPagedAsync(
            new LoginAuditPagedQuery(
                req.StartTimeUtc,
                req.EndTimeUtc,
                req.Result,
                req.Reason,
                req.Account,
                req.UserName,
                req.UserId,
                req.ClientIp,
                req.GetSkipCount(),
                req.PageSize),
            ct);

        await Send.OkAsync(new PagedLoginAuditResponse
        {
            TotalCount = result.TotalCount,
            Items = result.Items
                .Select(item => new PagedLoginAuditRowResponse(
                    item.Id,
                    item.LoginTimeUtc,
                    item.Result,
                    item.Reason,
                    item.Account,
                    item.UserName,
                    item.UserId,
                    item.ClientIp,
                    item.FailureDescription))
                .ToList()
        }, ct);
    }
}
