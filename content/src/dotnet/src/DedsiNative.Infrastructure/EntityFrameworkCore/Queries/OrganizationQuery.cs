using DedsiNative.Organizations;
using Microsoft.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Queries;

/// <summary>
/// 组织机构只读查询契约的 EF Core 实现。
/// </summary>
public sealed class OrganizationQuery(IDedsiNativeDbContext dbContext) : IOrganizationQuery
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<OrganizationQueryItem>> GetTreeListAsync(
        OrganizationTreeQuery query,
        CancellationToken cancellationToken)
    {
        var dbQuery = dbContext.Organizations.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.SystemId))
        {
            var trimmedSystemId = query.SystemId.Trim();
            dbQuery = dbQuery.Where(org => org.SystemId == trimmedSystemId);
        }

        if (query.IsEnabled.HasValue)
        {
            dbQuery = dbQuery.Where(org => org.IsEnabled == query.IsEnabled.Value);
        }

        return await dbQuery
            .OrderBy(org => org.Level)
            .ThenBy(org => org.Sort)
            .ThenBy(org => org.Id)
            .Select(org => new OrganizationQueryItem(
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
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OrganizationPagedQueryResult> GetPagedAsync(
        OrganizationPagedQuery query,
        CancellationToken cancellationToken)
    {
        var dbQuery = dbContext.Organizations.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.SystemId))
        {
            var trimmedSystemId = query.SystemId.Trim();
            dbQuery = dbQuery.Where(org => org.SystemId == trimmedSystemId);
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var trimmedKeyword = query.Keyword.Trim();
            dbQuery = dbQuery.Where(org =>
                org.Name.Contains(trimmedKeyword) ||
                org.Code.Contains(trimmedKeyword) ||
                (org.Name1 != null && org.Name1.Contains(trimmedKeyword)) ||
                (org.Name2 != null && org.Name2.Contains(trimmedKeyword)) ||
                (org.Name3 != null && org.Name3.Contains(trimmedKeyword)) ||
                (org.Name4 != null && org.Name4.Contains(trimmedKeyword)));
        }

        if (!string.IsNullOrWhiteSpace(query.ParentId))
        {
            var trimmedParentId = query.ParentId.Trim();
            dbQuery = dbQuery.Where(org => org.ParentId == trimmedParentId);
        }

        if (query.IsEnabled.HasValue)
        {
            dbQuery = dbQuery.Where(org => org.IsEnabled == query.IsEnabled.Value);
        }

        var totalCount = await dbQuery.LongCountAsync(cancellationToken);

        dbQuery = dbQuery
            .OrderBy(org => org.Level)
            .ThenBy(org => org.Sort)
            .ThenBy(org => org.Id);

        if (!query.IsExport)
        {
            dbQuery = dbQuery.Skip(query.SkipCount).Take(query.MaxResultCount);
        }

        var items = await dbQuery
            .Select(org => new OrganizationQueryItem(
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
            .ToListAsync(cancellationToken);

        return new OrganizationPagedQueryResult(totalCount, items);
    }
}
