using Dedsi.Ddd.Domain.Queries;

namespace DedsiNative.Users;

/// <summary>
/// 用户分页查询条件
/// </summary>
/// <param name="Name"></param>
/// <param name="Email"></param>
/// <param name="SkipCount"></param>
/// <param name="MaxResultCount"></param>
/// <param name="IsExport"></param>
/// <param name="OrganizationId"></param>
public sealed record UserPagedQueryInput(
    string? Name,
    string? Email,
    int SkipCount,
    int MaxResultCount,
    bool IsExport,
    string? OrganizationId = null);

/// <summary>
/// 用户分页查询中的单行结果。
/// </summary>
/// <param name="Id">
/// 用户唯一标识。
/// </param>
/// <param name="Name">
/// 用户名称。
/// </param>
/// <param name="Email">
/// 用户邮箱地址。
/// </param>
/// <param name="Phone">
/// 用户联系电话。
/// </param>
/// <param name="LastUpdatedAt">
/// 用户资料最后更新时间。
/// </param>
public sealed record UserQueryItemDto(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    DateTime LastUpdatedAt);

/// <summary>
/// 用户分页查询结果。
/// </summary>
/// <param name="TotalCount">
/// 符合筛选条件的总记录数。
/// </param>
/// <param name="Items">
/// 当前查询返回的用户列表。
/// </param>
public sealed record UserPagedQueryOutput(
    long TotalCount,
    UserQueryItemDto[] Items);

/// <summary>
/// 用户只读查询契约，隔离 Core 与具体持久化技术。
/// </summary>
public interface IUserQuery : IDedsiQuery
{
    /// <summary>
    /// 按名称、邮箱及所属组织筛选用户，并根据导出模式决定是否分页。
    /// </summary>
    /// <param name="query">用户分页查询条件。</param>
    /// <param name="cancellationToken">用于取消异步查询的令牌。</param>
    /// <returns>用户分页查询结果。</returns>
    Task<UserPagedQueryOutput> GetPagedAsync(
        UserPagedQueryInput query,
        CancellationToken cancellationToken);

    /// <summary>
    /// 按登录账号获取用户及其登录信息。
    /// </summary>
    /// <param name="account">待查询的登录账号。</param>
    /// <param name="cancellationToken">用于取消异步操作的令牌。</param>
    /// <returns>未找到对应用户时返回 <see langword="null"/>。</returns>
    Task<User?> FindByAccountAsync(
        string account,
        CancellationToken cancellationToken);
}
