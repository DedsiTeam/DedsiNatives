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
}

/// <summary>
/// 用户分页查询结果中的单行数据传输对象。
/// </summary>
/// <param name="Id">用户唯一标识。</param>
/// <param name="Name">用户名称。</param>
/// <param name="Email">用户邮箱地址。</param>
public record PagedUserRowDto(string Id, string Name, string Email);

/// <summary>
/// 用户分页查询结果，包含总记录数和当前页的数据列表。
/// </summary>
public class PagedUserResponse : DedsiPagedResultDto<PagedUserRowDto>;

/// <summary>
/// 用户分页查询端点，处理 POST /api/user/pagedQuery 请求，支持按名称和邮箱过滤，
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
            req.IsExport);
        var result = await userQuery.GetPagedAsync(query, ct);

        await Send.OkAsync(new PagedUserResponse
        {
            TotalCount = result.TotalCount,
            Items = result.Items
                .Select(item => new PagedUserRowDto(
                    item.Id,
                    item.Name,
                    item.Email))
                .ToList()
        }, ct);
    }
}
