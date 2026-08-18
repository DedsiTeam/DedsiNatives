using Dedsi.Ddd.Domain.Repositories;

namespace DedsiNative.Users;

/// <summary>
/// 用户仓储接口，继承自 CQRS 仓储基接口，提供针对 <see cref="User"/> 聚合根的增删改查操作。
/// </summary>
public interface IUserRepository : IDedsiCqrsRepository<User, Guid>
{
    /// <summary>
    /// 按登录账号获取用户及其登录信息。
    /// </summary>
    /// <param name="account">待查询的登录账号。</param>
    /// <param name="cancellationToken">用于取消异步操作的令牌。</param>
    /// <returns>未找到对应用户时返回 <see langword="null"/>。</returns>
    Task<User?> FindByAccountAsync(string account, CancellationToken cancellationToken);
}
