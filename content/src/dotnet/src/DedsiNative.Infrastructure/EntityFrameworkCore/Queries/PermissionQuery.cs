using DedsiNative.Permissions;
using Microsoft.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Queries;

/// <summary>权限查询服务的 EF Core 实现。</summary>
/// <param name="dbContext">DedsiNative 数据库上下文。</param>
public sealed class PermissionQuery(IDedsiNativeDbContext dbContext) : IPermissionQuery
{
    /// <inheritdoc />
    public async Task<PermissionPagedQueryResult> GetPagedAsync(
        PermissionPagedQuery query,
        CancellationToken cancellationToken)
    {
        var systemId = query.SystemId?.Trim();
        var name = query.Name?.Trim();
        var permissions = dbContext.Permissions
            .AsNoTracking()
            .WhereIf(!string.IsNullOrEmpty(systemId), permission => permission.SystemId == systemId)
            .WhereIf(!string.IsNullOrEmpty(name), permission => permission.Name.Contains(name!))
            .WhereIf(query.IsEnabled.HasValue, permission => permission.IsEnabled == query.IsEnabled!.Value);

        var totalCount = await permissions.LongCountAsync(cancellationToken);
        permissions = permissions
            .OrderBy(permission => permission.SystemId)
            .ThenBy(permission => permission.Name)
            .ThenBy(permission => permission.Id);

        if (!query.IsExport)
        {
            permissions = permissions.Skip(query.SkipCount).Take(query.MaxResultCount);
        }

        var items = await permissions
            .Select(permission => new PermissionQueryItem(
                permission.Id,
                permission.SystemId,
                permission.SystemName,
                permission.Name,
                permission.Description,
                permission.IsEnabled))
            .ToListAsync(cancellationToken);

        return new PermissionPagedQueryResult(totalCount, items);
    }
}
