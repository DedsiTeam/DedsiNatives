using DedsiNative.Menus;
using DedsiNative.Permissions;
using DedsiNative.Systems;
using FastEndpoints;

namespace DedsiNative.Endpoints.MenuEndpoints;

/// <summary>
/// 创建菜单端点，负责校验跨聚合关系并持久化菜单。
/// </summary>
/// <param name="menus">菜单聚合仓储。</param>
/// <param name="systems">系统聚合仓储。</param>
/// <param name="permissions">权限聚合仓储。</param>
public sealed class CreateMenuEndpoint(
    IMenuRepository menus,
    ISystemRepository systems,
    IPermissionRepository permissions) : Endpoint<MenuInput, string>
{
    /// <summary>
    /// 配置创建菜单接口的路由和 HTTP 方法。
    /// </summary>
    public override void Configure()
    {
        Post("/api/menu/create");
    }

    /// <summary>
    /// 创建菜单，并保存系统和权限的名称快照。
    /// </summary>
    /// <param name="req">菜单输入参数。</param>
    /// <param name="ct">用于取消异步操作的令牌。</param>
    public override async Task HandleAsync(MenuInput req, CancellationToken ct)
    {
        var system = await systems.GetAsync(req.SystemId, true, ct);

        var relationValidation = await MenuEndpointValidation.ValidateRelationsAsync(
            menus,
            permissions,
            system.Id,
            req.ParentId,
            req.PermissionId,
            null,
            ct);

        foreach (var error in relationValidation.Errors)
        {
            ThrowError(error);
        }

        if (await menus.ExistsBySystemAndCodeAsync(system.Id, req.Code, ct))
        {
            ThrowError("同一系统内的菜单编码不能重复。");
        }

        ThrowIfAnyErrors();

        var menu = new Menu(
            Ulid.NewUlid().ToString(),
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

        await menus.InsertAsync(menu, true, ct);
        await Send.OkAsync(menu.Id, ct);
    }
}
