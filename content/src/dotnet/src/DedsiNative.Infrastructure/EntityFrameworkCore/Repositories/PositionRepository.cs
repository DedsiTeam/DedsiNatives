using Dedsi.EntityFrameworkCore.Repositories;
using DedsiNative.Positions;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Entities;
using Volo.Abp.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Repositories;

/// <summary>岗位仓储的 EF Core 实现。</summary>
public sealed class PositionRepository(IDbContextProvider<DedsiNativeDbContext> dbContextProvider) : DedsiDddEfCoreRepository<DedsiNativeDbContext, Position, string>(dbContextProvider), IPositionRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Position>> GetByPermissionIdAsync(
        string permissionId,
        CancellationToken cancellationToken)
    {
        var dbContext = await GetDbContextAsync();
        
        return await dbContext.Positions
            .Include(item => item.Permissions)
            .Where(position => position.Permissions.Any(item => item.PermissionId == permissionId))
            .ToListAsync(cancellationToken);
    }
}
