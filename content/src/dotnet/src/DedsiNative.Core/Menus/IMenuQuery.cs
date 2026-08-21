using Dedsi.Ddd.Domain.Queries;
namespace DedsiNative.Menus;
/// <summary>菜单分页查询条件。</summary>
public sealed record MenuPagedQuery(string? SystemId, string? Name, string? Code, MenuType? Type, string? ParentId, bool? IsVisible, bool? IsDisabled, bool? IsExternal, int SkipCount, int MaxResultCount, bool IsExport);
/// <summary>菜单查询项。</summary>
public sealed record MenuQueryItem(string Id, string SystemId, string SystemName, string Code, string Name, string? ParentId, MenuType Type, string? RoutePath, string? Component, string? Redirect, string? Icon, string? PermissionId, string? PermissionName, int Sort, int Level, bool IsVisible, bool IsDisabled, bool IsExternal, string? ExternalUrl, bool KeepAlive, bool IsAffix, string? Description);
/// <summary>菜单分页结果。</summary>
public sealed record MenuPagedQueryResult(long TotalCount, MenuQueryItem[] Items);
/// <summary>菜单读侧查询契约。</summary>
public interface IMenuQuery : IDedsiQuery { Task<MenuPagedQueryResult> GetPagedAsync(MenuPagedQuery query, CancellationToken cancellationToken); }
