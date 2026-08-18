using Dedsi.Ddd.Domain.Queries;

namespace DedsiNative.Users;

/// <summary>
/// 用户分页查询条件。
/// </summary>
/// <param name="Name">用户名称筛选条件，为空时不按名称过滤。</param>
/// <param name="Email">用户邮箱筛选条件，为空时不按邮箱过滤。</param>
/// <param name="SkipCount">需要跳过的记录数。</param>
/// <param name="MaxResultCount">单页最多返回的记录数。</param>
/// <param name="IsExport">是否为导出模式；导出模式不应用分页。</param>
public sealed record UserPagedQuery(
    string? Name,
    string? Email,
    int SkipCount,
    int MaxResultCount,
    bool IsExport);

/// <summary>
/// 用户分页查询中的单行结果。
/// </summary>
/// <param name="Id">用户唯一标识。</param>
/// <param name="Name">用户名称。</param>
/// <param name="Email">用户邮箱地址。</param>
/// <param name="Phone">用户联系电话。</param>
/// <param name="LastUpdatedAt">用户资料最后更新时间。</param>
public sealed record UserQueryItem(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    DateTime LastUpdatedAt);

/// <summary>
/// 用户分页查询结果。
/// </summary>
/// <param name="TotalCount">符合筛选条件的总记录数。</param>
/// <param name="Items">当前查询返回的用户列表。</param>
public sealed record UserPagedQueryResult(
    long TotalCount,
    IReadOnlyList<UserQueryItem> Items);

/// <summary>
/// 用户只读查询契约，隔离 Core 与具体持久化技术。
/// </summary>
public interface IUserQuery : IDedsiQuery
{
    /// <summary>
    /// 按名称和邮箱筛选用户，并根据导出模式决定是否分页。
    /// </summary>
    /// <param name="query">用户分页查询条件。</param>
    /// <param name="cancellationToken">用于取消异步查询的令牌。</param>
    /// <returns>用户分页查询结果。</returns>
    Task<UserPagedQueryResult> GetPagedAsync(
        UserPagedQuery query,
        CancellationToken cancellationToken);
}
