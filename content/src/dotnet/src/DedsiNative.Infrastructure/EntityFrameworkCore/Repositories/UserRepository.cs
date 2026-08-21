using Dedsi.EntityFrameworkCore.Repositories;
using DedsiNative.Users;
using Volo.Abp.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Repositories;

/// <summary>
/// 用户仓储的 EF Core 实现，继承自 Dedsi DDD EF Core 仓储基类，实现 <see cref="IUserRepository"/> 接口。
/// </summary>
/// <param name="dbContextProvider">数据库上下文提供者，用于获取 <see cref="DedsiNativeDbContext"/> 实例。</param>
public class UserRepository(IDbContextProvider<DedsiNativeDbContext> dbContextProvider)
    : DedsiDddEfCoreRepository<DedsiNativeDbContext, User, Guid>(dbContextProvider),
        IUserRepository;
