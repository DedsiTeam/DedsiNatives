using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace DedsiNative.AuthServer.Controllers;

[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
public class ConsentController(
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictAuthorizationManager authorizationManager,
    IOpenIddictScopeManager scopeManager) : Controller
{
    [HttpGet("~/Consent")]
    public async Task<IActionResult> Index([FromQuery] string returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl))
        {
            return BadRequest("缺少 returnUrl 参数。");
        }

        var parameters = QueryHelpers.ParseQuery(new Uri(Request.Scheme + "://" + Request.Host + returnUrl).Query);
        var clientId = parameters.TryGetValue(Parameters.ClientId, out var cid) ? cid.ToString() : null;

        if (string.IsNullOrEmpty(clientId))
        {
            return BadRequest("无效的 ClientId。");
        }

        var application = await applicationManager.FindByClientIdAsync(clientId) ??
            throw new InvalidOperationException("客户端应用不存在。");

        var appName = await applicationManager.GetDisplayNameAsync(application) ?? clientId;
        var scopesStr = parameters.TryGetValue(Parameters.Scope, out var sc) ? sc.ToString() : string.Empty;
        var requestedScopes = scopesStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var scopeDescriptions = new List<(string Name, string DisplayName, string Description)>();
        foreach (var scopeName in requestedScopes)
        {
            var scopeObj = await scopeManager.FindByNameAsync(scopeName);
            if (scopeObj is not null)
            {
                scopeDescriptions.Add((
                    scopeName,
                    await scopeManager.GetDisplayNameAsync(scopeObj) ?? scopeName,
                    await scopeManager.GetDescriptionAsync(scopeObj) ?? string.Empty
                ));
            }
            else
            {
                scopeDescriptions.Add((scopeName, scopeName, string.Empty));
            }
        }

        ViewBag.ApplicationName = appName;
        ViewBag.Scopes = scopeDescriptions;
        ViewBag.ReturnUrl = returnUrl;
        ViewBag.UserName = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;

        return View();
    }

    [HttpPost("~/Consent")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept([FromForm] string returnUrl)
    {
        var parameters = QueryHelpers.ParseQuery(new Uri(Request.Scheme + "://" + Request.Host + returnUrl).Query);
        var clientId = parameters.TryGetValue(Parameters.ClientId, out var cid) ? cid.ToString() : null;
        var scopesStr = parameters.TryGetValue(Parameters.Scope, out var sc) ? sc.ToString() : string.Empty;
        var requestedScopes = scopesStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var application = await applicationManager.FindByClientIdAsync(clientId ?? string.Empty) ??
            throw new InvalidOperationException("客户端应用不存在。");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 创建永久授权记录，后续登录该应用将自动通过
        await authorizationManager.CreateAsync(
            principal: User,
            subject: userId ?? string.Empty,
            client: await applicationManager.GetIdAsync(application) ?? string.Empty,
            type: AuthorizationTypes.Permanent,
            scopes: requestedScopes.ToImmutableArray());

        return Redirect(returnUrl);
    }

    [HttpPost("~/Consent/Deny")]
    [ValidateAntiForgeryToken]
    public IActionResult Deny()
    {
        return Forbid(
            authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
            properties: new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.AccessDenied,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "用户拒绝了授权请求。"
            }));
    }
}
