using Dedsi.EntityFrameworkCore.Repositories;
using DedsiNative.Positions;
using Volo.Abp.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Repositories;

/// <summary>岗位仓储的 EF Core 实现。</summary>
public sealed class PositionRepository(IDbContextProvider<DedsiNativeDbContext> dbContextProvider)
    : DedsiDddEfCoreRepository<DedsiNativeDbContext, Position, string>(dbContextProvider), IPositionRepository;
