using DedsiNative.Menus;
using FastEndpoints;

namespace DedsiNative.Endpoints.MenuEndpoints;

public sealed class DeleteMenuEndpoint(
    IMenuRepository menus,
    IMenuQuery menuQuery) : EndpointWithoutRequest<bool>
{
    public override void Configure()
    {
        Post("/api/menu/delete/{id}");
        Policies(ManagementPermissions.Menus.Delete);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var m = await menus.GetAsync(Route<string>("id")!, true, ct);
        if (await menuQuery.HasChildrenAsync(m.Id, ct))
        {
            ThrowError("存在子菜单，不能删除。");
            ThrowIfAnyErrors();
        }

        await menus.DeleteAsync(m, true, ct);
        await Send.OkAsync(true, ct);
    }
}
