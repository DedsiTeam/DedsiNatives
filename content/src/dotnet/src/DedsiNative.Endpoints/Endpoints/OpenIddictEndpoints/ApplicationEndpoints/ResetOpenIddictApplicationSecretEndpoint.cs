using FastEndpoints;
using OpenIddict.Abstractions;

namespace DedsiNative.Endpoints.OpenIddictEndpoints.ApplicationEndpoints;

/// <summary>
/// 重置 ClientSecret 请求。
/// </summary>
public record ResetOpenIddictApplicationSecretRequest(string? NewSecret);

/// <summary>
/// 重置 ClientSecret 响应。
/// </summary>
public record ResetOpenIddictApplicationSecretResponse(string NewSecret);

/// <summary>
/// 重置客户端密钥端点。
/// </summary>
public class ResetOpenIddictApplicationSecretEndpoint(IOpenIddictApplicationManager applicationManager)
    : Endpoint<ResetOpenIddictApplicationSecretRequest, ResetOpenIddictApplicationSecretResponse>
{
    public override void Configure()
    {
        Post("/api/openiddict/applications/{id}/reset-secret");
        Policies(OpenIddict.OpenIddictPermissions.Manage);
        Description(x => x.WithTags("SSO 客户端管理"));
        Summary(s =>
        {
            s.Summary = "重置客户端密钥";
            s.Description = "为指定的 Confidential 客户端重置 ClientSecret。若未指定新密钥则自动生成高强度随机密钥。";
        });
    }

    public override async Task HandleAsync(ResetOpenIddictApplicationSecretRequest req, CancellationToken ct)
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

        var newSecret = string.IsNullOrWhiteSpace(req.NewSecret)
            ? $"{Ulid.NewUlid()}{Guid.NewGuid():N}{Ulid.NewUlid()}"
            : req.NewSecret.Trim();

        var descriptor = new OpenIddictApplicationDescriptor();
        await applicationManager.PopulateAsync(descriptor, app, ct);
        descriptor.ClientSecret = newSecret;

        await applicationManager.UpdateAsync(app, descriptor, ct);

        await Send.OkAsync(new ResetOpenIddictApplicationSecretResponse(newSecret), ct);
    }
}
