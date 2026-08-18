using DedsiNative.LoginAudits;
using FastEndpoints;

namespace DedsiNative.Endpoints.LoginAuditEndpoints;

/// <summary>
/// 登录审计详情响应。
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
/// <param name="UserAgent">请求提供的 User-Agent。</param>
public sealed record GetLoginAuditResponse(
    string Id,
    DateTime LoginTimeUtc,
    LoginResult Result,
    LoginReason Reason,
    string Account,
    string? UserName,
    Guid? UserId,
    string? ClientIp,
    string? FailureDescription,
    string? UserAgent);

/// <summary>
/// 获取单条登录审计详情的端点，仅允许拥有审计查看权限的用户访问。
/// </summary>
/// <param name="loginAuditRepository">登录审计写侧仓储，按聚合详情读取记录。</param>
public sealed class GetLoginAuditEndpoint(ILoginAuditRepository loginAuditRepository)
    : EndpointWithoutRequest<GetLoginAuditResponse>
{
    /// <summary>
    /// 配置登录审计详情接口、HTTP 方法和查看权限。
    /// </summary>
    public override void Configure()
    {
        Get("/api/login-audit/{id}");
        Policies(LoginAuditPermissions.View);
        Description(description => description.WithTags("登录审计"));
        Summary(summary =>
        {
            summary.Summary = "获取登录审计详情";
            summary.Description = "根据审计标识返回受权的完整登录审计快照。";
        });
    }

    /// <summary>
    /// 按路由标识加载完整登录审计聚合并返回详情快照。
    /// </summary>
    /// <param name="ct">用于取消异步操作的令牌。</param>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id")!;
        var audit = await loginAuditRepository.GetAsync(id, true, ct);

        await Send.OkAsync(new GetLoginAuditResponse(
            audit.Id,
            audit.LoginTimeUtc,
            audit.Result,
            audit.Reason,
            audit.Account,
            audit.UserName,
            audit.UserId,
            audit.ClientIp,
            audit.FailureDescription,
            audit.UserAgent), ct);
    }
}
