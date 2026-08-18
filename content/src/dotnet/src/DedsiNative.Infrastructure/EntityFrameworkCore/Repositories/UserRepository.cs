using Dedsi.EntityFrameworkCore.Repositories;
using DedsiNative.Users;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Repositories;

/// <summary>
/// 用户仓储的 EF Core 实现，继承自 Dedsi DDD EF Core 仓储基类，实现 <see cref="IUserRepository"/> 接口。
/// </summary>
/// <param name="dbContextProvider">数据库上下文提供者，用于获取 <see cref="DedsiNativeDbContext"/> 实例。</param>
public class UserRepository(IDbContextProvider<DedsiNativeDbContext> dbContextProvider)
    : DedsiDddEfCoreRepository<DedsiNativeDbContext, User, Guid>(dbContextProvider),
        IUserRepository
{
    /// <inheritdoc />
    public async Task<User?> FindByAccountAsync(string account, CancellationToken cancellationToken)
    {
        var normalizedAccount = account.Trim();
        var dbContext = await dbContextProvider.GetDbContextAsync();
        return await dbContext.Users
            .Include(user => user.LoginInfo)
            .Include(user => user.Positions)
            .SingleOrDefaultAsync(
                user => user.LoginInfo != null && user.LoginInfo.Account == normalizedAccount,
                cancellationToken);
    }
}
