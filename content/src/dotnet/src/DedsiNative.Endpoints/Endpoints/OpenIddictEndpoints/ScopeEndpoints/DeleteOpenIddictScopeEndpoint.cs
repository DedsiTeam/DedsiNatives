using FastEndpoints;
using OpenIddict.Abstractions;

namespace DedsiNative.Endpoints.OpenIddictEndpoints.ScopeEndpoints;

/// <summary>
/// 删除 OpenIddict 作用域端点。
/// </summary>
public class DeleteOpenIddictScopeEndpoint(IOpenIddictScopeManager scopeManager)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/api/openiddict/scopes/{id}");
        Policies(OpenIddict.OpenIddictPermissions.Manage);
        Description(x => x.WithTags("SSO 作用域管理"));
        Summary(s =>
        {
            s.Summary = "删除 SSO 作用域";
            s.Description = "永久删除指定的作用域。";
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

        var scope = await scopeManager.FindByIdAsync(id, ct);
        if (scope is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await scopeManager.DeleteAsync(scope, ct);
        await Send.NoContentAsync(ct);
    }
}
