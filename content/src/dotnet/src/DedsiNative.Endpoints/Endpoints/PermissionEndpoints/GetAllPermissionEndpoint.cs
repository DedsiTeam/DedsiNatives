using DedsiNative.Permissions;
using FastEndpoints;

namespace DedsiNative.Endpoints.PermissionEndpoints;

/// <summary>
/// 获取指定系统全部权限选项端点。
/// </summary>
/// <param name="permissionQuery">权限只读查询服务。</param>
public sealed class GetAllPermissionEndpoint(IPermissionQuery permissionQuery)
    : EndpointWithoutRequest<PermissionQueryItem[]>
{
    /// <summary>
    /// 配置按系统获取权限选项接口。
    /// </summary>
    public override void Configure()
    {
        Get("/api/permission/getAll/{systemId}");
        Policies(ManagementPermissions.Permissions.View);
    }

    /// <summary>
    /// 返回指定系统的全部权限轻量选项。
    /// </summary>
    /// <param name="ct">用于取消异步操作的令牌。</param>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var systemId = Route<string>("systemId")!;
        var result = await permissionQuery.GetPagedAsync(
            new PermissionPagedQuery(systemId, null, null, 0, 0, true),
            ct);
        await Send.OkAsync(result.Items, ct);
    }
}
