using DedsiNative.Users;

namespace DedsiNative.EntityFrameworkCore.Queries;

/// <summary>
/// 用户查询服务的 EF Core 实现，实现 <see cref="IUserQuery"/> 接口，基于 <see cref="IDedsiNativeDbContext"/> 执行数据查询。
/// </summary>
/// <param name="dedsiNativeDbContext">DedsiNative 数据库上下文，提供对数据集的直接查询能力。</param>
public class UserQuery(IDedsiNativeDbContext dedsiNativeDbContext) : IUserQuery;