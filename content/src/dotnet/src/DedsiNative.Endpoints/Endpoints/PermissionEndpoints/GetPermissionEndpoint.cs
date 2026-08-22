using DedsiNative.Permissions;
using FastEndpoints;

namespace DedsiNative.Endpoints.PermissionEndpoints;

/// <summary>权限详情响应。</summary>
/// <param name="Id">权限唯一标识。</param>
/// <param name="SystemId">所属系统 ID。</param>
/// <param name="SystemName">所属系统名称。</param>
/// <param name="Name">权限名称。</param>
/// <param name="Description">权限说明。</param>
/// <param name="IsEnabled">是否启用。</param>
public sealed record GetPermissionResponse(
    string Id,
    string SystemId,
    string SystemName,
    string Name,
    string? Description,
    bool IsEnabled);

/// <summary>获取权限详情端点。</summary>
/// <param name="permissionRepository">权限聚合仓储。</param>
public sealed class GetPermissionEndpoint(IPermissionRepository permissionRepository)
    : EndpointWithoutRequest<GetPermissionResponse>
{
    /// <summary>配置权限详情接口的路由和 HTTP 方法。</summary>
    public override void Configure()
    {
        Get("/api/permission/{id}");
        Policies(ManagementPermissions.Permissions.View);
        Description(x => x.WithTags("权限管理"));
        Summary(s =>
        {
            s.Summary = "获取权限详情";
            s.Description = "根据权限 ID 获取权限及所属系统信息。";
        });
    }

    /// <summary>通过仓储加载完整权限聚合并返回详情。</summary>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id")!;
        var permission = await permissionRepository.GetAsync(id, true, ct);
        await Send.OkAsync(new GetPermissionResponse(
            permission.Id,
            permission.SystemId,
            permission.SystemName,
            permission.Name,
            permission.Description,
            permission.IsEnabled), ct);
    }
}
