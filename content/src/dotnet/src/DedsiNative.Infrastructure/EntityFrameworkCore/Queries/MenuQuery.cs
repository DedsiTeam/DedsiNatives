using DedsiNative.Menus;
using Microsoft.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Queries;

public sealed class MenuQuery(IDedsiNativeDbContext db) : IMenuQuery
{
    public async Task<MenuPagedQueryResult> GetPagedAsync(MenuPagedQuery q, CancellationToken ct)
    {
        var x = db.Menus.AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(q.SystemId), i => i.SystemId == q.SystemId!.Trim())
            .WhereIf(!string.IsNullOrWhiteSpace(q.Name), i => i.Name.Contains(q.Name!.Trim()))
            .WhereIf(!string.IsNullOrWhiteSpace(q.Code), i => i.Code.Contains(q.Code!.Trim()))
            .WhereIf(q.Type.HasValue, i => i.Type == q.Type)
            .WhereIf(!string.IsNullOrWhiteSpace(q.ParentId), i => i.ParentId == q.ParentId!.Trim())
            .WhereIf(q.IsVisible.HasValue, i => i.IsVisible == q.IsVisible)
            .WhereIf(q.IsDisabled.HasValue, i => i.IsDisabled == q.IsDisabled)
            .WhereIf(q.IsExternal.HasValue, i => i.IsExternal == q.IsExternal);

        var total = await x.LongCountAsync(ct);
        x = x.OrderBy(i => i.SystemId).ThenBy(i => i.ParentId).ThenBy(i => i.Sort).ThenBy(i => i.Id);
        if (!q.IsExport)
        {
            x = x.Skip(q.SkipCount).Take(q.MaxResultCount);
        }

        var items = await x.Select(i => new MenuQueryItem(
            i.Id,
            i.SystemId,
            i.SystemName,
            i.Code,
            i.Name,
            i.ParentId,
            i.Type,
            i.RoutePath,
            i.Component,
            i.Redirect,
            i.Icon,
            i.PermissionId,
            i.PermissionName,
            i.Sort,
            i.Level,
            i.IsVisible,
            i.IsDisabled,
            i.IsExternal,
            i.ExternalUrl,
            i.KeepAlive,
            i.IsAffix,
            i.Description)).ToArrayAsync(ct);

        return new MenuPagedQueryResult(total, items);
    }

    public async Task<bool> HasChildrenAsync(string id, CancellationToken cancellationToken)
    {
        return await db.Menus.AnyAsync(menu => menu.ParentId == id, cancellationToken);
    }

    public async Task<bool> ExistsBySystemAndCodeAsync(
        string systemId,
        string code,
        CancellationToken cancellationToken,
        string? excludedMenuId = null)
    {
        var menus = db.Menus.Where(menu => menu.SystemId == systemId && menu.Code == code);
        if (!string.IsNullOrWhiteSpace(excludedMenuId))
        {
            menus = menus.Where(menu => menu.Id != excludedMenuId);
        }

        return await menus.AnyAsync(cancellationToken);
    }

    public async Task<bool> WouldCreateCycleAsync(
        string menuId,
        string candidateParentId,
        CancellationToken cancellationToken)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var currentId = candidateParentId;

        // 从候选父级向上追溯；同时防御存量脏数据中的既有环。
        while (visited.Add(currentId))
        {
            if (currentId == menuId)
            {
                return true;
            }

            var parentId = await db.Menus
                .Where(menu => menu.Id == currentId)
                .Select(menu => menu.ParentId)
                .SingleOrDefaultAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(parentId))
            {
                return false;
            }

            currentId = parentId;
        }

        return true;
    }
}
