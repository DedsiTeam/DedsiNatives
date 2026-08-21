using System.Security.Claims;
using DedsiNative.LoginAudits;
using DedsiNative.Permissions;
using DedsiNative.Positions;
using DedsiNative.Users;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Volo.Abp.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace DedsiNative.AuthServer.Controllers;

/// <summary>
/// OpenIddict OIDC / OAuth 2.0 核心认证与令牌授权控制器。
/// </summary>
public class AuthorizationController(
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictAuthorizationManager authorizationManager,
    IOpenIddictScopeManager scopeManager,
    IUserRepository userRepository,
    IPositionRepository positionRepository,
    IPermissionQuery permissionQuery) : Controller
{
    /// <summary>
    /// 处理 OIDC 授权码/隐式等授权请求端点。
    /// </summary>
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("无法获取 OpenIddict 授权请求。");

        // 1. 检查用户是否已在 AuthServer 登录 Cookie 会话
        var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authResult.Succeeded || authResult.Principal?.Identity?.IsAuthenticated != true)
        {
            // 用户未登录，携带完整请求参数重定向至登录页面
            return Challenge(
                authenticationSchemes: [CookieAuthenticationDefaults.AuthenticationScheme],
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
        }

        // 2. 获取当前登录用户 Id
        var userIdStr = authResult.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? authResult.Principal.FindFirst(Claims.Subject)?.Value;

        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Challenge(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        User? user;
        try
        {
            user = await userRepository.GetAsync(userId, true, HttpContext.RequestAborted);
        }
        catch
        {
            user = null;
        }

        if (user is null || user.SoftDeletedAt is not null || user.LoginInfo?.Status != AccountStatus.Normal)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Forbid(
                authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "用户账户不存在或已被禁用。"
                }));
        }

        // 3. 获取申请授权的应用信息
        var application = await applicationManager.FindByClientIdAsync(request.ClientId ?? string.Empty) ??
            throw new InvalidOperationException("未找到匹配的客户端应用程序。");

        var consentType = await applicationManager.GetConsentTypeAsync(application);

        // 如果客户端要求显式确认授权，且尚未持久化授权记录，则重定向至 Consent 页面
        if (consentType == ConsentTypes.Explicit && !request.HasPromptValue(PromptValues.None))
        {
            var authorizations = await authorizationManager.FindAsync(
                subject: user.Id.ToString(),
                client: await applicationManager.GetIdAsync(application) ?? string.Empty,
                status: Statuses.Valid,
                type: AuthorizationTypes.Permanent,
                scopes: request.GetScopes()).ToListAsync();

            if (authorizations.Count == 0)
            {
                // 跳转到授权许可确认页
                return RedirectToAction("Index", "Consent", new
                {
                    returnUrl = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
            }
        }

        // 4. 创建 ClaimsPrincipal
        var principal = await CreateUserPrincipalAsync(user, request.GetScopes());

        // 5. 执行 OpenIddict 签发授权码或令牌
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// 处理 Token 兑换端点（支持授权码模式、刷新令牌模式及客户端凭据模式）。
    /// </summary>
    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("无法获取 OpenIddict 令牌请求。");

        // 模式 1：授权码模式与刷新令牌模式
        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var authResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (!authResult.Succeeded || authResult.Principal is null)
            {
                return Forbid(
                    authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "令牌无效或已过期。"
                    }));
            }

            var userIdStr = authResult.Principal.GetClaim(Claims.Subject);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Forbid(
                    authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "无效的用户主体标识。"
                    }));
            }

            User? user;
            try
            {
                user = await userRepository.GetAsync(userId, true, HttpContext.RequestAborted);
            }
            catch
            {
                user = null;
            }

            if (user is null || user.SoftDeletedAt is not null || user.LoginInfo?.Status != AccountStatus.Normal)
            {
                return Forbid(
                    authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "用户账户不存在或已被禁用。"
                    }));
            }

            var principal = await CreateUserPrincipalAsync(user, request.GetScopes().Length > 0 ? request.GetScopes() : authResult.Principal.GetScopes());

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // 模式 2：客户端凭据模式 (Machine-to-Machine)
        if (request.IsClientCredentialsGrantType())
        {
            var application = await applicationManager.FindByClientIdAsync(request.ClientId ?? string.Empty) ??
                throw new InvalidOperationException("客户端应用不存在。");

            var identity = new ClaimsIdentity(
                authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                nameType: Claims.Name,
                roleType: Claims.Role);

            identity.SetClaim(Claims.Subject, await applicationManager.GetClientIdAsync(application));
            identity.SetClaim(Claims.Name, await applicationManager.GetDisplayNameAsync(application));

            // 设置目标作用域
            identity.SetScopes(request.GetScopes());
            identity.SetResources(await scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());
            identity.SetDestinations(_ => [Destinations.AccessToken]);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("不支持指定的 Grant Type 模式。");
    }

    /// <summary>
    /// UserInfo 端点，返回当前 Access Token 对应的用户基本资料和权限 Claims。
    /// </summary>
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    [Produces("application/json")]
    public async Task<IActionResult> Userinfo()
    {
        var userIdStr = User.GetClaim(Claims.Subject);
        if (!Guid.TryParse(userIdStr, out var userId))
        {
            return Challenge(
                authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "令牌中缺少有效用户标识。"
                }));
        }

        User? user;
        try
        {
            user = await userRepository.GetAsync(userId, true, HttpContext.RequestAborted);
        }
        catch
        {
            user = null;
        }

        if (user is null)
        {
            return Challenge(
                authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "用户不存在。"
                }));
        }

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [Claims.Subject] = user.Id.ToString(),
            [Claims.Name] = user.Name,
            [Claims.PreferredUsername] = user.LoginInfo?.Account ?? string.Empty,
            [Claims.Email] = user.Email
        };

        if (User.HasScope(Scopes.Roles))
        {
            claims[Claims.Role] = user.Positions.Select(p => p.PositionName).ToArray();
        }

        // 返回由岗位计算得出的权限
        var (positions, permissions) = await GetUserPositionsAndPermissionsAsync(user);
        claims["permissions"] = permissions;
        claims["positions"] = positions;

        return Ok(claims);
    }

    /// <summary>
    /// OIDC 退出登录端点。
    /// </summary>
    [HttpGet("~/connect/logout")]
    [HttpPost("~/connect/logout")]
    public async Task<IActionResult> Logout()
    {
        // 登出本地 Cookie 会话
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // 登出 OpenIddict 协议会话并重定向回 post_logout_redirect_uri
        return SignOut(
            authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
            properties: new AuthenticationProperties
            {
                RedirectUri = "/"
            });
    }

    private async Task<ClaimsPrincipal> CreateUserPrincipalAsync(User user, IEnumerable<string> scopes)
    {
        var identity = new ClaimsIdentity(
            authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            nameType: Claims.Name,
            roleType: Claims.Role);

        // 核心 Claim
        identity.SetClaim(Claims.Subject, user.Id.ToString());
        identity.SetClaim(ClaimTypes.NameIdentifier, user.Id.ToString());
        identity.SetClaim(Claims.Name, user.Name);
        identity.SetClaim(Claims.PreferredUsername, user.LoginInfo?.Account ?? string.Empty);
        identity.SetClaim(AbpClaimTypes.Name, user.LoginInfo?.Account ?? string.Empty);
        identity.SetClaim(Claims.Email, user.Email);

        if (!string.IsNullOrWhiteSpace(user.Phone))
        {
            identity.SetClaim(Claims.PhoneNumber, user.Phone);
        }

        // 解析权限
        var (_, permissionNames) = await GetUserPositionsAndPermissionsAsync(user);
        foreach (var perm in permissionNames)
        {
            identity.AddClaim(new Claim(LoginAuditPermissions.ClaimType, perm));
        }

        foreach (var pos in user.Positions)
        {
            identity.AddClaim(new Claim(Claims.Role, pos.PositionName));
        }

        identity.SetScopes(scopes);
        identity.SetResources(await scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

        // 指定 Claim 目标（哪些放在 AccessToken，哪些放在 IdentityToken）
        identity.SetDestinations(claim => claim.Type switch
        {
            Claims.Subject or Claims.Name or Claims.PreferredUsername =>
                [Destinations.AccessToken, Destinations.IdentityToken],

            Claims.Email or Claims.PhoneNumber when identity.HasScope(Scopes.Email) =>
                [Destinations.AccessToken, Destinations.IdentityToken],

            Claims.Role or LoginAuditPermissions.ClaimType =>
                [Destinations.AccessToken, Destinations.IdentityToken],

            _ => [Destinations.AccessToken]
        });

        return new ClaimsPrincipal(identity);
    }

    private async Task<(string[] Positions, string[] Permissions)> GetUserPositionsAndPermissionsAsync(User user)
    {
        var activePositions = new List<Position>();
        foreach (var userPosition in user.Positions)
        {
            var position = await positionRepository.GetAsync(
                userPosition.PositionId,
                true,
                HttpContext.RequestAborted);
            if (!position.IsEnabled)
            {
                continue;
            }

            activePositions.Add(position);
        }

        var allPositionPermissions = activePositions
            .SelectMany(p => p.Permissions)
            .ToList();

        var enabledPermissions = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var systemId in allPositionPermissions
                     .Select(permission => permission.SystemId)
                     .Distinct(StringComparer.Ordinal))
        {
            var queryResult = await permissionQuery.GetPagedAsync(
                new PermissionPagedQuery(systemId, null, true, 0, 1, true),
                HttpContext.RequestAborted);
            foreach (var permission in queryResult.Items)
            {
                enabledPermissions[permission.Id] = permission.Name;
            }
        }

        var userPositions = activePositions
            .Select(position => position.Name)
            .ToArray();

        var permissionNames = allPositionPermissions
            .Where(permission => enabledPermissions.ContainsKey(permission.PermissionId))
            .Select(permission => enabledPermissions[permission.PermissionId])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(permissionName => permissionName, StringComparer.Ordinal)
            .ToArray();

        return (userPositions, permissionNames);
    }
}
