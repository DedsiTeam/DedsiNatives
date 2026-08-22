using Dedsi.Ddd.Application.Contracts.Dtos; using DedsiNative.Menus; using FastEndpoints;
namespace DedsiNative.Endpoints.MenuEndpoints;
public sealed class PagedMenuRequest:DedsiPagedRequestDto { public string? SystemId{get;set;} public string? Name{get;set;} public string? Code{get;set;} public MenuType? Type{get;set;} public string? ParentId{get;set;} public bool? IsVisible{get;set;} public bool? IsDisabled{get;set;} public bool? IsExternal{get;set;} }
public sealed class PagedMenuResponse:DedsiPagedResultDto<MenuResponse>;
public sealed class PagedMenuEndpoint(IMenuQuery query):Endpoint<PagedMenuRequest,PagedMenuResponse>
{
    public override void Configure()
    {
        Post("/api/menu/pagedQuery");
        Policies(ManagementPermissions.Menus.View);
    }

    public override async Task HandleAsync(PagedMenuRequest r,CancellationToken ct){var x=await query.GetPagedAsync(new(r.SystemId,r.Name,r.Code,r.Type,r.ParentId,r.IsVisible,r.IsDisabled,r.IsExternal,r.GetSkipCount(),r.PageSize,r.IsExport),ct);await Send.OkAsync(new(){TotalCount=x.TotalCount,Items=x.Items.Select(i=>new MenuResponse(i.Id,i.SystemId,i.SystemName,i.Code,i.Name,i.ParentId,i.Type,i.RoutePath,i.Component,i.Redirect,i.Icon,i.PermissionId,i.PermissionName,i.Sort,i.Level,i.IsVisible,i.IsDisabled,i.IsExternal,i.ExternalUrl,i.KeepAlive,i.IsAffix,i.Description)).ToList()},ct);}
}
