using DedsiNative.Menus;
using DedsiNative.Permissions;
using DedsiNative.Systems;
using FastEndpoints;

namespace DedsiNative.Endpoints.MenuEndpoints;

/// <summary>
/// 更新菜单端点，负责在更新前校验树形关系与关联归属。
/// </summary>
/// <param name="menus">菜单聚合仓储。</param>
/// <param name="systems">系统聚合仓储。</param>
/// <param name="permissions">权限聚合仓储。</param>
public sealed class UpdateMenuEndpoint(
    IMenuRepository menus,
    ISystemRepository systems,
    IPermissionRepository permissions) : Endpoint<MenuInput, bool>
{
    /// <summary>
    /// 配置更新菜单接口的路由和 HTTP 方法。
    /// </summary>
    public override void Configure()
    {
        Post("/api/menu/update/{id}");
    }

    /// <summary>
    /// 更新指定菜单的全部可维护状态。
    /// </summary>
    /// <param name="req">菜单输入参数。</param>
    /// <param name="ct">用于取消异步操作的令牌。</param>
    public override async Task HandleAsync(MenuInput req, CancellationToken ct)
    {
        var menu = await menus.GetAsync(Route<string>("id")!, true, ct);
        var system = await systems.GetAsync(req.SystemId, true, ct);

        var relationValidation = await MenuEndpointValidation.ValidateRelationsAsync(
            menus,
            permissions,
            system.Id,
            req.ParentId,
            req.PermissionId,
            menu.Id,
            ct);

        foreach (var error in relationValidation.Errors)
        {
            ThrowError(error);
        }

        if (await menus.ExistsBySystemAndCodeAsync(system.Id, req.Code, ct, menu.Id))
        {
            ThrowError("同一系统内的菜单编码不能重复。");
        }

        ThrowIfAnyErrors();

        menu.Update(
            system.Id,
            system.Name,
            req.Code,
            req.Name,
            req.ParentId,
            req.Type,
            req.RoutePath,
            req.Component,
            req.Redirect,
            req.Icon,
            relationValidation.Permission?.Id,
            relationValidation.Permission?.Name,
            req.Sort,
            req.Level,
            req.IsVisible,
            req.IsDisabled,
            req.IsExternal,
            req.ExternalUrl,
            req.KeepAlive,
            req.IsAffix,
            req.Description);

        await menus.UpdateAsync(menu, true, ct);
        await Send.OkAsync(true, ct);
    }
}
