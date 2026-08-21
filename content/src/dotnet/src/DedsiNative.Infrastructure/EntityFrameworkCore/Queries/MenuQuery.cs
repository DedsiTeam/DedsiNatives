using DedsiNative.Menus;
using Microsoft.EntityFrameworkCore;
namespace DedsiNative.EntityFrameworkCore.Queries;
public sealed class MenuQuery(IDedsiNativeDbContext db) : IMenuQuery
{
    public async Task<MenuPagedQueryResult> GetPagedAsync(MenuPagedQuery q, CancellationToken ct)
    {
        var x = db.Menus.AsNoTracking().WhereIf(!string.IsNullOrWhiteSpace(q.SystemId), i=>i.SystemId==q.SystemId!.Trim()).WhereIf(!string.IsNullOrWhiteSpace(q.Name), i=>i.Name.Contains(q.Name!.Trim())).WhereIf(!string.IsNullOrWhiteSpace(q.Code), i=>i.Code.Contains(q.Code!.Trim())).WhereIf(q.Type.HasValue,i=>i.Type==q.Type).WhereIf(!string.IsNullOrWhiteSpace(q.ParentId),i=>i.ParentId==q.ParentId!.Trim()).WhereIf(q.IsVisible.HasValue,i=>i.IsVisible==q.IsVisible).WhereIf(q.IsDisabled.HasValue,i=>i.IsDisabled==q.IsDisabled).WhereIf(q.IsExternal.HasValue,i=>i.IsExternal==q.IsExternal);
        var total=await x.LongCountAsync(ct); x=x.OrderBy(i=>i.SystemId).ThenBy(i=>i.ParentId).ThenBy(i=>i.Sort).ThenBy(i=>i.Id); if(!q.IsExport)x=x.Skip(q.SkipCount).Take(q.MaxResultCount);
        return new MenuPagedQueryResult(total,await x.Select(i=>new MenuQueryItem(i.Id,i.SystemId,i.SystemName,i.Code,i.Name,i.ParentId,i.Type,i.RoutePath,i.Component,i.Redirect,i.Icon,i.PermissionId,i.PermissionName,i.Sort,i.Level,i.IsVisible,i.IsDisabled,i.IsExternal,i.ExternalUrl,i.KeepAlive,i.IsAffix,i.Description)).ToArrayAsync(ct));
    }
}
