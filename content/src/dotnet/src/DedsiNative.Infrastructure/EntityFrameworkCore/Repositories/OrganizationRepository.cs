using Dedsi.EntityFrameworkCore.Repositories;
using DedsiNative.Organizations;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Repositories;

/// <summary>
/// 组织机构聚合仓储的 EF Core 实现。
/// </summary>
public sealed class OrganizationRepository(IDbContextProvider<DedsiNativeDbContext> dbContextProvider)
    : DedsiDddEfCoreRepository<DedsiNativeDbContext, Organization, string>(dbContextProvider), IOrganizationRepository
{
    /// <inheritdoc />
    public async Task<bool> HasChildrenAsync(
        string id,
        CancellationToken cancellationToken)
    {
        var dbContext = await dbContextProvider.GetDbContextAsync();
        return await dbContext.Organizations.AnyAsync(org => org.ParentId == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsBySystemAndCodeAsync(
        string systemId,
        string code,
        CancellationToken cancellationToken,
        string? excludedId = null)
    {
        var dbContext = await dbContextProvider.GetDbContextAsync();
        var query = dbContext.Organizations.Where(org => org.SystemId == systemId && org.Code == code);

        if (!string.IsNullOrWhiteSpace(excludedId))
        {
            query = query.Where(org => org.Id != excludedId);
        }

        return await query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> WouldCreateCycleAsync(
        string id,
        string candidateParentId,
        CancellationToken cancellationToken)
    {
        var dbContext = await dbContextProvider.GetDbContextAsync();
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var currentId = candidateParentId;

        // 从候选父级向上逐层追溯，如果回溯到自身则说明构成环路
        while (visited.Add(currentId))
        {
            if (currentId == id)
            {
                return true;
            }

            var parentId = await dbContext.Organizations
                .Where(org => org.Id == currentId)
                .Select(org => org.ParentId)
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
