using DedsiNative.Permissions;
using DedsiNative.Systems;
using FastEndpoints;

namespace DedsiNative.Endpoints.PermissionEndpoints;

/// <summary>更新权限的请求参数。</summary>
/// <param name="SystemId">新的所属系统 ID。</param>
/// <param name="Name">新的权限名称。</param>
/// <param name="Description">新的权限说明。</param>
public sealed record UpdatePermissionRequest(
    string SystemId,
    string Name,
    string? Description);

/// <summary>更新权限端点，通过领域方法修改权限聚合。</summary>
/// <param name="permissionRepository">权限聚合仓储。</param>
/// <param name="systemRepository">系统聚合仓储，用于确认系统存在并读取名称。</param>
public sealed class UpdatePermissionEndpoint(
    IPermissionRepository permissionRepository,
    ISystemRepository systemRepository)
    : Endpoint<UpdatePermissionRequest, bool>
{
    /// <summary>配置更新权限接口的路由和 HTTP 方法。</summary>
    public override void Configure()
    {
        Post("/api/permission/update/{id}");
        Description(x => x.WithTags("权限管理"));
        Summary(s =>
        {
            s.Summary = "更新权限";
            s.Description = "修改权限名称、说明、所属系统和启用状态。权限名称变更后会同步更新岗位权限名称快照。";
        });
    }

    /// <summary>加载权限和目标系统，执行领域变更并持久化。</summary>
    /// <param name="req">更新权限请求。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(UpdatePermissionRequest req, CancellationToken ct)
    {
        var id = Route<string>("id")!;
        var permission = await permissionRepository.GetAsync(id, true, ct);
        var system = await systemRepository.GetAsync(req.SystemId, true, ct);

        permission
            .ChangeSystem(system.Id, system.Name)
            .ChangeName(req.Name)
            .ChangeDescription(req.Description);

        await permissionRepository.UpdateAsync(permission, true, ct);
        await Send.OkAsync(true, ct);
    }
}
