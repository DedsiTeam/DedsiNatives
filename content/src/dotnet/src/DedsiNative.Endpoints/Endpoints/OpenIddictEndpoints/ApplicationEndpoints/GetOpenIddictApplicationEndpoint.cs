using FastEndpoints;
using OpenIddict.Abstractions;

namespace DedsiNative.Endpoints.OpenIddictEndpoints.ApplicationEndpoints;

/// <summary>
/// OpenIddict 客户端详情响应。
/// </summary>
public record GetOpenIddictApplicationResponse(
    string Id,
    string? ClientId,
    string? DisplayName,
    string? ClientType,
    string? ConsentType,
    string[] RedirectUris,
    string[] PostLogoutRedirectUris,
    string[] Permissions,
    string[] Requirements);

/// <summary>
/// 获取指定 OpenIddict 客户端详情端点。
/// </summary>
public class GetOpenIddictApplicationEndpoint(IOpenIddictApplicationManager applicationManager)
    : EndpointWithoutRequest<GetOpenIddictApplicationResponse>
{
    public override void Configure()
    {
        Get("/api/openiddict/applications/{id}");
        Policies(OpenIddict.OpenIddictPermissions.View);
        Description(x => x.WithTags("SSO 客户端管理"));
        Summary(s =>
        {
            s.Summary = "获取 SSO 客户端详情";
            s.Description = "根据客户端唯一标识 ID 获取客户端应用详细配置。";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id");
        if (string.IsNullOrWhiteSpace(id))
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var app = await applicationManager.FindByIdAsync(id, ct);
        if (app is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var redirectUris = (await applicationManager.GetRedirectUrisAsync(app, ct))
            .Select(u => u.ToString())
            .ToArray();

        var postLogoutRedirectUris = (await applicationManager.GetPostLogoutRedirectUrisAsync(app, ct))
            .Select(u => u.ToString())
            .ToArray();

        var permissions = (await applicationManager.GetPermissionsAsync(app, ct)).ToArray();
        var requirements = (await applicationManager.GetRequirementsAsync(app, ct)).ToArray();

        await Send.OkAsync(new GetOpenIddictApplicationResponse(
            await applicationManager.GetIdAsync(app, ct) ?? id,
            await applicationManager.GetClientIdAsync(app, ct),
            await applicationManager.GetDisplayNameAsync(app, ct),
            await applicationManager.GetClientTypeAsync(app, ct),
            await applicationManager.GetConsentTypeAsync(app, ct),
            redirectUris,
            postLogoutRedirectUris,
            permissions,
            requirements
        ), ct);
    }
}
