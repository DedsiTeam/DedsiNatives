using FastEndpoints;
using OpenIddict.Abstractions;

namespace DedsiNative.Endpoints.OpenIddictEndpoints.TokenEndpoints;

/// <summary>
/// 强制吊销指定令牌端点。
/// </summary>
public class RevokeOpenIddictTokenEndpoint(IOpenIddictTokenManager tokenManager)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/api/openiddict/tokens/{id}/revoke");
        Policies(OpenIddict.OpenIddictPermissions.Manage);
        Description(x => x.WithTags("SSO 授权与令牌查看"));
        Summary(s =>
        {
            s.Summary = "吊销指定 SSO 令牌";
            s.Description = "将指定的 Token 状态标记为已吊销 (Revoked)，使其立即失效。";
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

        var token = await tokenManager.FindByIdAsync(id, ct);
        if (token is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await tokenManager.TryRevokeAsync(token, ct);
        await Send.NoContentAsync(ct);
    }
}
