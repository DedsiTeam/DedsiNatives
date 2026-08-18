using DedsiNative.Menus;
namespace DedsiNative.Endpoints.MenuEndpoints;
public sealed record MenuInput(string SystemId,string Code,string Name,string? ParentId,MenuType Type,string? RoutePath,string? Component,string? Redirect,string? Icon,string? PermissionId,int Sort,int Level,bool IsVisible,bool IsDisabled,bool IsExternal,string? ExternalUrl,bool KeepAlive,bool IsAffix,string? Description);
public sealed record MenuResponse(string Id,string SystemId,string SystemName,string Code,string Name,string? ParentId,MenuType Type,string? RoutePath,string? Component,string? Redirect,string? Icon,string? PermissionId,string? PermissionName,int Sort,int Level,bool IsVisible,bool IsDisabled,bool IsExternal,string? ExternalUrl,bool KeepAlive,bool IsAffix,string? Description);
