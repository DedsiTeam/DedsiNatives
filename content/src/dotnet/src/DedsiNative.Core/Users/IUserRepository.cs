using Dedsi.Ddd.Domain.Repositories;

namespace DedsiNative.Users;

/// <summary>
/// 用户仓储接口，继承自 CQRS 仓储基接口，提供针对 <see cref="User"/> 聚合根的增删改查操作。
/// </summary>
public interface IUserRepository : IDedsiCqrsRepository<User, string>;