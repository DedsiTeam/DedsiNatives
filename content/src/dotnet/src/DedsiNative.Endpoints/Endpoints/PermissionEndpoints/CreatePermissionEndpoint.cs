using DedsiNative.Permissions;
using DedsiNative.Systems;
using FastEndpoints;

namespace DedsiNative.Endpoints.PermissionEndpoints;

/// <summary>创建权限的请求参数。</summary>
/// <param name="SystemId">所属系统 ID。</param>
/// <param name="Name">权限名称。</param>
/// <param name="Description">权限说明，可为空。</param>
/// <param name="IsEnabled">是否启用，默认启用。</param>
public sealed record CreatePermissionRequest(
    string SystemId,
    string Name,
    string? Description,
    bool IsEnabled = true);

/// <summary>创建权限的响应。</summary>
/// <param name="Id">新权限的 26 位 ULID 标识。</param>
public sealed record CreatePermissionResponse(string Id);

/// <summary>创建权限端点，校验系统归属后创建权限聚合。</summary>
/// <param name="permissionRepository">权限聚合仓储。</param>
/// <param name="systemRepository">系统聚合仓储，用于确认系统存在并读取名称。</param>
public sealed class CreatePermissionEndpoint(
    IPermissionRepository permissionRepository,
    ISystemRepository systemRepository)
    : Endpoint<CreatePermissionRequest, CreatePermissionResponse>
{
    /// <summary>配置创建权限接口的路由和 HTTP 方法。</summary>
    public override void Configure()
    {
        Post("/api/permission/create");
        Policies(ManagementPermissions.Permissions.Create);
        Description(x => x.WithTags("权限管理"));
        Summary(s =>
        {
            s.Summary = "创建权限";
            s.Description = "在指定系统下创建权限。";
        });
    }

    /// <summary>加载所属系统、创建权限并持久化。</summary>
    /// <param name="req">创建权限请求。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(CreatePermissionRequest req, CancellationToken ct)
    {
        var system = await systemRepository.GetAsync(req.SystemId, true, ct);
        var id = Ulid.NewUlid().ToString();
        var permission = new Permission(id, system.Id, system.Name, req.Name, req.Description, req.IsEnabled);

        await permissionRepository.InsertAsync(permission, true, ct);
        await Send.OkAsync(new CreatePermissionResponse(id), ct);
    }
}
