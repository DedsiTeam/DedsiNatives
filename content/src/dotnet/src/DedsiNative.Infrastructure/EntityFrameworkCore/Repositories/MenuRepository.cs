using Dedsi.EntityFrameworkCore.Repositories;
using DedsiNative.Menus;
using Volo.Abp.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Repositories;

/// <summary>
/// 菜单聚合仓储的 EF Core 实现。
/// </summary>
public sealed class MenuRepository(IDbContextProvider<DedsiNativeDbContext> dbContextProvider)
    : DedsiDddEfCoreRepository<DedsiNativeDbContext, Menu, string>(dbContextProvider), IMenuRepository;
