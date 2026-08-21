using FastEndpoints;
using FluentValidation;
using OpenIddict.Abstractions;
using Volo.Abp;

namespace DedsiNative.Endpoints.OpenIddictEndpoints.ApplicationEndpoints;

/// <summary>
/// 修改 OpenIddict 客户端请求。
/// </summary>
public record UpdateOpenIddictApplicationRequest(
    string DisplayName,
    string ClientType,
    string? ConsentType,
    string[]? RedirectUris,
    string[]? PostLogoutRedirectUris,
    string[]? Permissions,
    string[]? Requirements);

/// <summary>
/// 修改 OpenIddict 客户端应用端点。
/// </summary>
public class UpdateOpenIddictApplicationEndpoint(IOpenIddictApplicationManager applicationManager)
    : Endpoint<UpdateOpenIddictApplicationRequest>
{
    public override void Configure()
    {
        Put("/api/openiddict/applications/{id}");
        Policies(OpenIddict.OpenIddictPermissions.Manage);
        Description(x => x.WithTags("SSO 客户端管理"));
        Summary(s =>
        {
            s.Summary = "更新 SSO 客户端应用";
            s.Description = "更新客户端应用的显示名称、类型、重定向地址、授权类型与作用域权限。";
        });
    }

    public override async Task HandleAsync(UpdateOpenIddictApplicationRequest req, CancellationToken ct)
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

        var descriptor = new OpenIddictApplicationDescriptor();
        await applicationManager.PopulateAsync(descriptor, app, ct);

        descriptor.DisplayName = req.DisplayName?.Trim();
        descriptor.ClientType = req.ClientType;
        descriptor.ConsentType = req.ConsentType;

        descriptor.RedirectUris.Clear();
        if (req.RedirectUris != null)
        {
            foreach (var uri in req.RedirectUris.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                if (Uri.TryCreate(uri.Trim(), UriKind.Absolute, out var parsed))
                {
                    descriptor.RedirectUris.Add(parsed);
                }
            }
        }

        descriptor.PostLogoutRedirectUris.Clear();
        if (req.PostLogoutRedirectUris != null)
        {
            foreach (var uri in req.PostLogoutRedirectUris.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                if (Uri.TryCreate(uri.Trim(), UriKind.Absolute, out var parsed))
                {
                    descriptor.PostLogoutRedirectUris.Add(parsed);
                }
            }
        }

        descriptor.Permissions.Clear();
        if (req.Permissions != null)
        {
            foreach (var perm in req.Permissions.Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                descriptor.Permissions.Add(perm.Trim());
            }
        }

        descriptor.Requirements.Clear();
        if (req.Requirements != null)
        {
            foreach (var reqItem in req.Requirements.Where(r => !string.IsNullOrWhiteSpace(r)))
            {
                descriptor.Requirements.Add(reqItem.Trim());
            }
        }

        await applicationManager.UpdateAsync(app, descriptor, ct);
        await Send.NoContentAsync(ct);
    }
}
