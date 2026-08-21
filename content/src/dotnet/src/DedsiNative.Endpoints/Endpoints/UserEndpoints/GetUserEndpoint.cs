using DedsiNative.Users;
using FastEndpoints;

namespace DedsiNative.Endpoints.UserEndpoints;

/// <summary>
/// 获取用户详情的岗位响应数据传输对象。
/// </summary>
/// <param name="PositionId">岗位的 26 位 ULID。</param>
/// <param name="PositionName">岗位名称快照。</param>
public sealed record UserPositionResponse(string PositionId, string PositionName);

/// <summary>
/// 获取用户详情的组织机构响应数据传输对象。
/// </summary>
/// <param name="OrganizationId">组织机构的 26 位 ULID。</param>
/// <param name="OrganizationName">组织机构名称快照。</param>
public sealed record UserOrganizationResponse(string OrganizationId, string OrganizationName);

/// <summary>
/// 用户详情响应。
/// </summary>
/// <param name="Id">用户唯一标识。</param>
/// <param name="Name">用户名称。</param>
/// <param name="Email">用户邮箱地址。</param>
/// <param name="Phone">用户电话号码。</param>
/// <param name="IdCardNumber">用户身份证号码。</param>
/// <param name="LastUpdatedAt">最后更新时间。</param>
/// <param name="LastLoginTime">最后登录时间。</param>
/// <param name="LastLoginIp">最后登录 IP 地址。</param>
/// <param name="SoftDeletedAt">软删除时间；未删除时为空。</param>
/// <param name="LoginInfo">用户登录信息，不含密码材料。</param>
/// <param name="Positions">用户岗位关联列表。</param>
/// <param name="Organizations">用户组织机构关联列表。</param>
public record GetUserResponse(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? IdCardNumber,
    DateTime LastUpdatedAt,
    DateTime? LastLoginTime,
    string? LastLoginIp,
    DateTime? SoftDeletedAt,
    UserLoginInfoResponse? LoginInfo,
    UserPositionResponse[] Positions,
    UserOrganizationResponse[] Organizations);

/// <summary>
/// 获取用户详情端点，处理 GET /api/user/{id} 请求，根据路由中的用户 ID 查询并返回用户信息。
/// </summary>
/// <param name="userRepository">用户仓储，用于查询用户实体。</param>
public class GetUserEndpoint(IUserRepository userRepository)
    : EndpointWithoutRequest<GetUserResponse>
{
    /// <summary>
    /// 配置端点路由和权限策略。
    /// </summary>
    public override void Configure()
    {
        Get("/api/user/{id}");
        Description(x => x.WithTags("用户管理"));
        Summary(s =>
        {
            s.Summary = "获取用户详情";
            s.Description = "根据用户 ID 获取用户资料、登录信息、岗位及组织机构关联。";
        });
    }

    /// <summary>
    /// 处理获取用户请求，从路由中读取用户 ID，查询用户后返回详情数据。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var user = await userRepository.GetAsync(id, true, ct);

        await Send.OkAsync(new GetUserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Phone,
            user.IdCardNumber,
            user.LastUpdatedAt,
            user.LastLoginTime,
            user.LastLoginIp,
            user.SoftDeletedAt,
            user.LoginInfo is null ? null : new UserLoginInfoResponse(user.LoginInfo.Account, user.LoginInfo.Status),
            user.Positions.Select(position => new UserPositionResponse(
                position.PositionId,
                position.PositionName)).ToArray(),
            user.Organizations.Select(org => new UserOrganizationResponse(
                org.OrganizationId,
                org.OrganizationName)).ToArray()), ct);
    }
}
