using DedsiNative.Users;
using Microsoft.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Queries;

/// <summary>
/// 用户查询服务的 EF Core 实现。
/// </summary>
/// <param name="dbContext">DedsiNative 数据库上下文。</param>
public sealed class UserQuery(IDedsiNativeDbContext dbContext) : IUserQuery
{
    /// <inheritdoc />
    public async Task<UserPagedQueryResult> GetPagedAsync(
        UserPagedQuery query,
        CancellationToken cancellationToken)
    {
        var name = query.Name?.Trim();
        var email = query.Email?.Trim();
        var organizationId = query.OrganizationId?.Trim();

        var users = dbContext.Users
            .AsNoTracking()
            .WhereIf(
                !string.IsNullOrEmpty(name),
                user => user.Name.Contains(name!))
            .WhereIf(
                !string.IsNullOrEmpty(email),
                user => user.Email.Contains(email!))
            .WhereIf(
                !string.IsNullOrEmpty(organizationId),
                user => user.Organizations.Any(org => org.OrganizationId == organizationId));

        var totalCount = await users.LongCountAsync(cancellationToken);

        users = users.OrderByDescending(user => user.CreationTime);
        if (!query.IsExport)
        {
            users = users
                .Skip(query.SkipCount)
                .Take(query.MaxResultCount);
        }

        var items = await users
            .Select(user => new UserQueryItem(
                user.Id,
                user.Name,
                user.Email,
                user.Phone,
                user.LastUpdatedAt))
            .ToListAsync(cancellationToken);

        return new UserPagedQueryResult(totalCount, items);
    }
}
