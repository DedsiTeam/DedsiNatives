using Dedsi.EntityFrameworkCore.Repositories;
using DedsiNative.Permissions;
using Volo.Abp.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Repositories;

/// <summary>权限仓储的 EF Core 实现。</summary>
/// <param name="dbContextProvider">用于获取权限数据库上下文的提供者。</param>
public sealed class PermissionRepository(IDbContextProvider<DedsiNativeDbContext> dbContextProvider)
    : DedsiDddEfCoreRepository<DedsiNativeDbContext, Permission, string>(dbContextProvider), IPermissionRepository;
