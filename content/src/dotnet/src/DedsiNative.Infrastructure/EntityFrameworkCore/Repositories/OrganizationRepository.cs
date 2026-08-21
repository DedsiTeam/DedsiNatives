using Dedsi.EntityFrameworkCore.Repositories;
using DedsiNative.Organizations;
using Volo.Abp.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Repositories;

/// <summary>
/// 组织机构聚合仓储的 EF Core 实现。
/// </summary>
public sealed class OrganizationRepository(IDbContextProvider<DedsiNativeDbContext> dbContextProvider)
    : DedsiDddEfCoreRepository<DedsiNativeDbContext, Organization, string>(dbContextProvider), IOrganizationRepository;
