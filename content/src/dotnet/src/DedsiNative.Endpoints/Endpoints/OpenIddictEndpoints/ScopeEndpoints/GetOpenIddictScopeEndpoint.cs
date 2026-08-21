using FastEndpoints;
using OpenIddict.Abstractions;

namespace DedsiNative.Endpoints.OpenIddictEndpoints.ScopeEndpoints;

/// <summary>
/// OpenIddict 作用域详情响应。
/// </summary>
public record GetOpenIddictScopeResponse(
    string Id,
    string? Name,
    string? DisplayName,
    string? Description,
    string[] Resources);

/// <summary>
/// 获取指定 OpenIddict 作用域详情端点。
/// </summary>
public class GetOpenIddictScopeEndpoint(IOpenIddictScopeManager scopeManager)
    : EndpointWithoutRequest<GetOpenIddictScopeResponse>
{
    public override void Configure()
    {
        Get("/api/openiddict/scopes/{id}");
        Policies(OpenIddict.OpenIddictPermissions.View);
        Description(x => x.WithTags("SSO 作用域管理"));
        Summary(s =>
        {
            s.Summary = "获取 SSO 作用域详情";
            s.Description = "根据作用域 ID 获取详细配置。";
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

        var resources = (await scopeManager.GetResourcesAsync(scope, ct)).ToArray();

        await Send.OkAsync(new GetOpenIddictScopeResponse(
            await scopeManager.GetIdAsync(scope, ct) ?? id,
            await scopeManager.GetNameAsync(scope, ct),
            await scopeManager.GetDisplayNameAsync(scope, ct),
            await scopeManager.GetDescriptionAsync(scope, ct),
            resources
        ), ct);
    }
}
