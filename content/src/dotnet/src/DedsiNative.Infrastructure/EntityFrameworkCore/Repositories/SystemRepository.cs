using DedsiNative.Systems;
using Dedsi.EntityFrameworkCore.Repositories;
using Volo.Abp.EntityFrameworkCore;
using SystemEntity = DedsiNative.Systems.System;

namespace DedsiNative.EntityFrameworkCore.Repositories;

/// <summary>系统仓储的 EF Core 实现。</summary>
/// <param name="dbContextProvider">用于获取系统数据库上下文的提供者。</param>
public sealed class SystemRepository(IDbContextProvider<DedsiNativeDbContext> dbContextProvider)
    : DedsiDddEfCoreRepository<DedsiNativeDbContext, SystemEntity, string>(dbContextProvider), ISystemRepository;
