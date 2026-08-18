using Dedsi.EntityFrameworkCore.Repositories;
using DedsiNative.Menus;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Repositories;

/// <summary>
/// 菜单聚合仓储的 EF Core 实现。
/// </summary>
public sealed class MenuRepository(IDbContextProvider<DedsiNativeDbContext> dbContextProvider)
    : DedsiDddEfCoreRepository<DedsiNativeDbContext, Menu, string>(dbContextProvider), IMenuRepository
{
    /// <inheritdoc />
    public async Task<bool> HasChildrenAsync(
        string id,
        CancellationToken cancellationToken)
    {
        var dbContext = await dbContextProvider.GetDbContextAsync();
        return await dbContext.Menus.AnyAsync(menu => menu.ParentId == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsBySystemAndCodeAsync(
        string systemId,
        string code,
        CancellationToken cancellationToken,
        string? excludedMenuId = null)
    {
        var dbContext = await dbContextProvider.GetDbContextAsync();
        var menus = dbContext.Menus.Where(menu => menu.SystemId == systemId && menu.Code == code);

        if (!string.IsNullOrWhiteSpace(excludedMenuId))
        {
            menus = menus.Where(menu => menu.Id != excludedMenuId);
        }

        return await menus.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> WouldCreateCycleAsync(
        string menuId,
        string candidateParentId,
        CancellationToken cancellationToken)
    {
        var dbContext = await dbContextProvider.GetDbContextAsync();
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var currentId = candidateParentId;

        // 从候选父级向上追溯；同时防御存量脏数据中的既有环。
        while (visited.Add(currentId))
        {
            if (currentId == menuId)
            {
                return true;
            }

            var parentId = await dbContext.Menus
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
