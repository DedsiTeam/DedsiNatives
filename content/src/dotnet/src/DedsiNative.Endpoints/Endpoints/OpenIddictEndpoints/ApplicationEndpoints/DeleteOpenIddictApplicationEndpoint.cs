using FastEndpoints;
using OpenIddict.Abstractions;

namespace DedsiNative.Endpoints.OpenIddictEndpoints.ApplicationEndpoints;

/// <summary>
/// 删除 OpenIddict 客户端应用端点。
/// </summary>
public class DeleteOpenIddictApplicationEndpoint(IOpenIddictApplicationManager applicationManager)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/openiddict/applications/{id}");
        Policies(OpenIddict.OpenIddictPermissions.Manage);
        Description(x => x.WithTags("SSO 客户端管理"));
        Summary(s =>
        {
            s.Summary = "删除 SSO 客户端应用";
            s.Description = "根据客户端 ID 永久删除指定的客户端应用及其所有关联令牌与授权。";
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

        await applicationManager.DeleteAsync(app, ct);
        await Send.NoContentAsync(ct);
    }
}
