using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DedsiNative.Users;
using FastEndpoints;
using Microsoft.Extensions.Logging;
using Volo.Abp;

namespace DedsiNative.Endpoints.AuthEndpoints;

/// <summary>
/// SSO 登录端点，解析 SSO Token、校验用户状态并委托统一登录服务签发系统 JWT Token 与完整用户模型。
/// </summary>
public sealed class SsoLoginEndpoint(
    IUserQuery userQuery,
    IUserRepository userRepository,
    IUserLoginService userLoginService,
    ILogger<SsoLoginEndpoint> logger)
    : Endpoint<SsoLoginRequest, LoginResponse>
{
    public override void Configure()
    {
        Post("/api/auth/sso-login");
        AllowAnonymous();
        Description(description => description.WithTags("认证管理"));
        Summary(summary =>
        {
            summary.Summary = "SSO 单点登录换取凭据";
            summary.Description = "解析 SSO 认证中心返回的 Token，验证用户有效性并返回与普通登录完全一致的标准凭据与权限模型。";
        });
    }

    public override async Task HandleAsync(SsoLoginRequest req, CancellationToken ct)
    {
        var tokenStr = req.Token?.Trim() ?? string.Empty;
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        if (string.IsNullOrWhiteSpace(tokenStr))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        // 1. 解析 JWT Token 中的用户标识
        string? subject = null;
        string? preferredUsername = null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(tokenStr))
            {
                var jwt = handler.ReadJwtToken(tokenStr);
                subject = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub || c.Type == ClaimTypes.NameIdentifier)?.Value;
                preferredUsername = jwt.Claims.FirstOrDefault(c => c.Type == "preferred_username" || c.Type == ClaimTypes.Name || c.Type == "name")?.Value;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "解析 SSO Token 失败。");
            await Send.UnauthorizedAsync(ct);
            return;
        }

        if (string.IsNullOrWhiteSpace(subject) && string.IsNullOrWhiteSpace(preferredUsername))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        // 2. 根据 Subject (Guid) 或 Account 查找本地数据库用户
        User? user = null;
        if (!string.IsNullOrWhiteSpace(subject) && Guid.TryParse(subject, out var userId))
        {
            try
            {
                user = await userRepository.GetAsync(userId, true, ct);
            }
            catch
            {
                user = null;
            }
        }

        if (user is null && !string.IsNullOrWhiteSpace(preferredUsername))
        {
            user = await userQuery.FindByAccountAsync(preferredUsername, ct);
        }

        if (user is null || user.LoginInfo is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        if (user.SoftDeletedAt is not null || user.LoginInfo.Status != AccountStatus.Normal)
        {
            throw new UserFriendlyException("用户已被停用或删除。");
        }

        // 3. 统一调用用户登录服务执行持久化和凭据签发
        var response = await userLoginService.GenerateLoginResponseAsync(
            user,
            clientIp,
            userAgent,
            "SSO 单点登录",
            ct);

        await Send.OkAsync(response, ct);
    }
}
