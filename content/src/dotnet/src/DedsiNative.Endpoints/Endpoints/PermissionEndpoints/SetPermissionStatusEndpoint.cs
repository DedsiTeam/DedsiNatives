using DedsiNative.Permissions;
using FastEndpoints;

namespace DedsiNative.Endpoints.PermissionEndpoints;

/// <summary>修改权限启用状态的请求参数。</summary>
/// <param name="IsEnabled">目标启用状态。</param>
public sealed record SetPermissionStatusRequest(bool IsEnabled);

/// <summary>修改权限启用状态端点，统一调用权限聚合的状态行为。</summary>
/// <param name="permissionRepository">权限聚合仓储。</param>
public sealed class SetPermissionStatusEndpoint(IPermissionRepository permissionRepository)
    : Endpoint<SetPermissionStatusRequest, bool>
{
    /// <summary>配置权限状态接口的路由和 HTTP 方法。</summary>
    public override void Configure()
    {
        Post("/api/permission/status/{id}");
        Policies(ManagementPermissions.Permissions.Update);
        Description(x => x.WithTags("权限管理"));
        Summary(s =>
        {
            s.Summary = "设置权限状态";
            s.Description = "启用或停用指定权限。";
        });
    }

    /// <summary>加载权限并调用 Enable 或 Disable 领域行为。</summary>
    /// <param name="req">状态变更请求。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(SetPermissionStatusRequest req, CancellationToken ct)
    {
        var id = Route<string>("id")!;
        var permission = await permissionRepository.GetAsync(id, true, ct);
        if (BuiltInPermissionNames.Contains(permission.Name))
        {
            ThrowError("平台内置权限不能停用或启用。");
        }

        if (req.IsEnabled)
        {
            permission.Enable();
        }
        else
        {
            permission.Disable();
        }

        await permissionRepository.UpdateAsync(permission, true, ct);
        await Send.OkAsync(true, ct);
    }
}
