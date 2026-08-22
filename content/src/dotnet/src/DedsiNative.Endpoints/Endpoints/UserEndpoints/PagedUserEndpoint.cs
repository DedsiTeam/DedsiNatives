using Dedsi.Ddd.Application.Contracts.Dtos;
using DedsiNative.Users;
using FastEndpoints;

namespace DedsiNative.Endpoints.UserEndpoints;

/// <summary>
/// 用户分页查询请求参数，继承自公共分页请求基类。
/// </summary>
public class PagedUserRequest : DedsiPagedRequestDto
{
    /// <summary>
    /// 按用户名称模糊筛选，为空时不过滤。
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// 按邮箱地址模糊筛选，为空时不过滤。
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 按所属组织机构筛选，为空时不过滤。
    /// </summary>
    public string? OrganizationId { get; set; }
}

/// <summary>
/// 用户分页查询结果中的单行数据传输对象。
/// </summary>
/// <param name="Id">用户唯一标识。</param>
/// <param name="Name">用户名称。</param>
/// <param name="Email">用户邮箱地址。</param>
/// <param name="Phone">用户联系电话。</param>
/// <param name="LastUpdatedAt">用户资料最后更新时间。</param>
public record PagedUserRowDto(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    DateTime LastUpdatedAt);

/// <summary>
/// 用户分页查询结果，包含总记录数和当前页的数据列表。
/// </summary>
public class PagedUserResponse : DedsiPagedResultDto<PagedUserRowDto>;

/// <summary>
/// 用户分页查询端点，处理 POST /api/user/pagedQuery 请求，支持按名称、邮箱及所属组织过滤，
/// 并根据是否为导出模式决定是否分页。
/// </summary>
/// <param name="userQuery">用户只读查询服务。</param>
public class PagedUserEndpoint(IUserQuery userQuery)
    : Endpoint<PagedUserRequest, PagedUserResponse>
{
    /// <summary>
    /// 配置端点路由和权限策略。
    /// </summary>
    public override void Configure()
    {
        Post("/api/user/pagedQuery");
        Policies(ManagementPermissions.Users.View);
        Description(x => x.WithTags("用户管理"));
        Summary(s =>
        {
            s.Summary = "分页查询用户";
            s.Description = "按用户名称、邮箱和所属组织查询用户列表，支持分页和导出。";
        });
    }

    /// <summary>
    /// 处理用户分页查询请求，动态拼接过滤条件，统计总记录数后按创建时间倒序分页返回结果。
    /// </summary>
    /// <param name="req">分页查询请求，包含筛选条件和分页参数。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(PagedUserRequest req, CancellationToken ct)
    {
        var query = new UserPagedQuery(
            req.Name,
            req.Email,
            req.GetSkipCount(),
            req.PageSize,
            req.IsExport,
            req.OrganizationId);
        var result = await userQuery.GetPagedAsync(query, ct);

        await Send.OkAsync(new PagedUserResponse
        {
            TotalCount = result.TotalCount,
            Items = result.Items
                .Select(item => new PagedUserRowDto(
                    item.Id,
                    item.Name,
                    item.Email,
                    item.Phone,
                    item.LastUpdatedAt))
                .ToList()
        }, ct);
    }
}
