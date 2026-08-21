using DedsiNative.Positions;
using Microsoft.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Queries;

/// <summary>岗位查询服务的 EF Core 实现。</summary>
public sealed class PositionQuery(IDedsiNativeDbContext dbContext) : IPositionQuery
{
    /// <inheritdoc />
    public async Task<PositionPagedQueryResult> GetPagedAsync(
        PositionPagedQuery query,
        CancellationToken cancellationToken)
    {
        var systemId = query.SystemId?.Trim();
        var name = query.Name?.Trim();
        var positions = dbContext.Positions
            .AsNoTracking()
            .WhereIf(!string.IsNullOrEmpty(systemId), position => position.SystemId == systemId)
            .WhereIf(!string.IsNullOrEmpty(name), position => position.Name.Contains(name!))
            .WhereIf(query.IsEnabled.HasValue, position => position.IsEnabled == query.IsEnabled!.Value);

        var totalCount = await positions.LongCountAsync(cancellationToken);
        positions = positions.OrderBy(position => position.SystemId).ThenBy(position => position.Name).ThenBy(position => position.Id);

        if (!query.IsExport)
        {
            positions = positions.Skip(query.SkipCount).Take(query.MaxResultCount);
        }

        var items = await positions
            .Select(position => new PositionQueryItem(
                position.Id,
                position.Name,
                position.SystemId,
                position.SystemName,
                position.Description,
                position.IsEnabled,
                position.Permissions.Count,
                position.Organizations.Count))
            .ToArrayAsync(cancellationToken);

        return new PositionPagedQueryResult(totalCount, items);
    }

    /// <inheritdoc />
    public async Task<Position[]> GetByPermissionIdAsync(
        string permissionId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Positions
            .Include(item => item.Permissions)
            .Where(position => position.Permissions.Any(item => item.PermissionId == permissionId))
            .ToArrayAsync(cancellationToken);
    }
}
