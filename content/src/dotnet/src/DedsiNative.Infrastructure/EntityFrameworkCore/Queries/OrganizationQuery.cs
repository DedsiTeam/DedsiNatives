using DedsiNative.Organizations;
using Microsoft.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Queries;

/// <summary>
/// 组织机构只读查询契约的 EF Core 实现。
/// </summary>
public sealed class OrganizationQuery(DedsiNativeDbContext dbContext) : IOrganizationQuery
{
    /// <inheritdoc />
    public async Task<OrganizationQueryItemDto[]> GetTreeListAsync(
        OrganizationTreeQueryInput query,
        CancellationToken cancellationToken)
    {
        var systemId = query.SystemId?.Trim();
        var dbQuery = dbContext.Organizations.AsNoTracking()
            .WhereIf(!string.IsNullOrEmpty(systemId), org => org.SystemId == systemId)
            .WhereIf(query.IsEnabled.HasValue, org => org.IsEnabled == query.IsEnabled!.Value);

        return await dbQuery
            .OrderBy(org => org.Level)
            .ThenBy(org => org.Sort)
            .ThenBy(org => org.Id)
            .Select(org => new OrganizationQueryItemDto(
                org.Id,
                org.SystemId,
                org.SystemName,
                org.Code,
                org.Name,
                org.Name1,
                org.Name2,
                org.Name3,
                org.Name4,
                org.ParentId,
                org.Sort,
                org.Level,
                org.IsEnabled,
                org.Description,
                org.CreationTime))
            .ToArrayAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OrganizationPagedQueryOutput> GetPagedAsync(
        OrganizationPagedQueryInput query,
        CancellationToken cancellationToken)
    {
        var systemId = query.SystemId?.Trim();
        var keyword = query.Keyword?.Trim();
        var parentId = query.ParentId?.Trim();

        var dbQuery = dbContext.Organizations.AsNoTracking()
            .WhereIf(!string.IsNullOrEmpty(systemId), org => org.SystemId == systemId)
            .WhereIf(!string.IsNullOrEmpty(keyword), org =>
                org.Name.Contains(keyword!) ||
                org.Code.Contains(keyword!) ||
                (org.Name1 != null && org.Name1.Contains(keyword!)) ||
                (org.Name2 != null && org.Name2.Contains(keyword!)) ||
                (org.Name3 != null && org.Name3.Contains(keyword!)) ||
                (org.Name4 != null && org.Name4.Contains(keyword!)))
            .WhereIf(!string.IsNullOrEmpty(parentId), org => org.ParentId == parentId)
            .WhereIf(query.IsEnabled.HasValue, org => org.IsEnabled == query.IsEnabled!.Value);

        var totalCount = await dbQuery.LongCountAsync(cancellationToken);

        dbQuery = dbQuery
            .OrderBy(org => org.Level)
            .ThenBy(org => org.Sort)
            .ThenBy(org => org.Id);

        if (!query.IsExport)
        {
            dbQuery = dbQuery
                .Skip(query.SkipCount)
                .Take(query.MaxResultCount);
        }

        var items = await dbQuery
            .Select(org => new OrganizationQueryItemDto(
                org.Id,
                org.SystemId,
                org.SystemName,
                org.Code,
                org.Name,
                org.Name1,
                org.Name2,
                org.Name3,
                org.Name4,
                org.ParentId,
                org.Sort,
                org.Level,
                org.IsEnabled,
                org.Description,
                org.CreationTime))
            .ToArrayAsync(cancellationToken);

        return new OrganizationPagedQueryOutput(totalCount, items);
    }

    /// <inheritdoc />
    public async Task<bool> HasChildrenAsync(
        string id,
        CancellationToken cancellationToken)
    {
        return await dbContext.Organizations.AnyAsync(org => org.ParentId == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsBySystemAndCodeAsync(
        string systemId,
        string code,
        CancellationToken cancellationToken,
        string? excludedId = null)
    {
        var query = dbContext.Organizations
            .Where(org => org.SystemId == systemId && org.Code == code)
            .WhereIf(!string.IsNullOrWhiteSpace(excludedId), org => org.Id != excludedId);

        return await query.AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> WouldCreateCycleAsync(
        string id,
        string candidateParentId,
        CancellationToken cancellationToken)
    {
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
