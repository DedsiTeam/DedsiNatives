using DedsiNative.Menus; using FastEndpoints;
namespace DedsiNative.Endpoints.MenuEndpoints;
public sealed class GetMenuEndpoint(IMenuRepository menus):EndpointWithoutRequest<MenuResponse>
{
    public override void Configure()
    {
        Get("/api/menu/{id}");
        Policies(ManagementPermissions.Menus.View);
    }

    public override async Task HandleAsync(CancellationToken ct){var m=await menus.GetAsync(Route<string>("id")!,true,ct);await Send.OkAsync(new(m.Id,m.SystemId,m.SystemName,m.Code,m.Name,m.ParentId,m.Type,m.RoutePath,m.Component,m.Redirect,m.Icon,m.PermissionId,m.PermissionName,m.Sort,m.Level,m.IsVisible,m.IsDisabled,m.IsExternal,m.ExternalUrl,m.KeepAlive,m.IsAffix,m.Description),ct);}
}
