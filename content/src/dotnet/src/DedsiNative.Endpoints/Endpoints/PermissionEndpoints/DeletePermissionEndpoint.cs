using DedsiNative.Permissions;
using FastEndpoints;

namespace DedsiNative.Endpoints.PermissionEndpoints;

/// <summary>删除权限端点，删除前由前端要求管理员明确确认。</summary>
/// <param name="permissionRepository">权限聚合仓储。</param>
public sealed class DeletePermissionEndpoint(IPermissionRepository permissionRepository)
    : EndpointWithoutRequest<bool>
{
    /// <summary>配置删除权限接口的路由和 HTTP 方法。</summary>
    public override void Configure()
    {
        Post("/api/permission/delete/{id}");
        Description(x => x.WithTags("权限管理"));
        Summary(s =>
        {
            s.Summary = "删除权限";
            s.Description = "根据权限 ID 删除权限。";
        });
    }

    /// <summary>加载目标权限并执行删除。</summary>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id")!;
        var permission = await permissionRepository.GetAsync(id, true, ct);
        await permissionRepository.DeleteAsync(permission, true, ct);
        await Send.OkAsync(true, ct);
    }
}
