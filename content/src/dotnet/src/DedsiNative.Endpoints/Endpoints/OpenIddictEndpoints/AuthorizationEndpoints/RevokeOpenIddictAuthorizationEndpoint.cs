using FastEndpoints;
using OpenIddict.Abstractions;

namespace DedsiNative.Endpoints.OpenIddictEndpoints.AuthorizationEndpoints;

/// <summary>
/// 吊销指定用户应用授权端点。
/// </summary>
public class RevokeOpenIddictAuthorizationEndpoint(IOpenIddictAuthorizationManager authorizationManager)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/api/openiddict/authorizations/{id}/revoke");
        Policies(OpenIddict.OpenIddictPermissions.Manage);
        Description(x => x.WithTags("SSO 授权与令牌查看"));
        Summary(s =>
        {
            s.Summary = "吊销用户应用授权";
            s.Description = "将指定的授权记录状态标记为已吊销 (Revoked)，并废止其下关联的所有有效令牌。";
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

        var authorization = await authorizationManager.FindByIdAsync(id, ct);
        if (authorization is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await authorizationManager.TryRevokeAsync(authorization, ct);
        await Send.NoContentAsync(ct);
    }
}
