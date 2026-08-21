using FastEndpoints;
using OpenIddict.Abstractions;

namespace DedsiNative.Endpoints.OpenIddictEndpoints.ScopeEndpoints;

/// <summary>
/// 修改 OpenIddict 作用域请求。
/// </summary>
public record UpdateOpenIddictScopeRequest(
    string? DisplayName,
    string? Description,
    string[]? Resources);

/// <summary>
/// 修改 SSO 作用域端点。
/// </summary>
public class UpdateOpenIddictScopeEndpoint(IOpenIddictScopeManager scopeManager)
    : Endpoint<UpdateOpenIddictScopeRequest>
{
    public override void Configure()
    {
        Put("/api/openiddict/scopes/{id}");
        Policies(OpenIddict.OpenIddictPermissions.Manage);
        Description(x => x.WithTags("SSO 作用域管理"));
        Summary(s =>
        {
            s.Summary = "更新 SSO 作用域";
            s.Description = "修改指定作用域的显示名称、说明以及关联的目标资源。";
        });
    }

    public override async Task HandleAsync(UpdateOpenIddictScopeRequest req, CancellationToken ct)
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

        var descriptor = new OpenIddictScopeDescriptor();
        await scopeManager.PopulateAsync(descriptor, scope, ct);

        descriptor.DisplayName = req.DisplayName?.Trim();
        descriptor.Description = req.Description?.Trim();

        descriptor.Resources.Clear();
        if (req.Resources != null)
        {
            foreach (var res in req.Resources.Where(r => !string.IsNullOrWhiteSpace(r)))
            {
                descriptor.Resources.Add(res.Trim());
            }
        }

        await scopeManager.UpdateAsync(scope, descriptor, ct);
        await Send.NoContentAsync(ct);
    }
}
