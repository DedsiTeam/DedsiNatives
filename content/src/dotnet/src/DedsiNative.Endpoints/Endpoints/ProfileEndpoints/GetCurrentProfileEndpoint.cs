using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DedsiNative.Users;
using FastEndpoints;

namespace DedsiNative.Endpoints.ProfileEndpoints;

/// <summary>
/// 当前登录用户个人资料响应。
/// </summary>
/// <param name="Id">用户唯一标识。</param>
/// <param name="Name">用户姓名。</param>
/// <param name="Email">用户邮箱。</param>
/// <param name="Phone">用户联系电话。</param>
/// <param name="Account">登录账号。</param>
/// <param name="AccountStatus">账户状态。</param>
/// <param name="LastLoginTime">最近一次成功登录时间（UTC）。</param>
public sealed record GetCurrentProfileResponse(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string Account,
    AccountStatus AccountStatus,
    DateTime? LastLoginTime);

/// <summary>
/// 获取当前登录用户个人资料端点。
/// </summary>
/// <param name="userRepository">用户聚合仓储。</param>
public sealed class GetCurrentProfileEndpoint(IUserRepository userRepository)
    : EndpointWithoutRequest<GetCurrentProfileResponse>
{
    /// <summary>
    /// 配置当前登录用户资料接口。
    /// </summary>
    public override void Configure()
    {
        Get("/api/profile");
        Description(x => x.WithTags("个人中心"));
        Summary(s =>
        {
            s.Summary = "获取当前用户资料";
            s.Description = "根据访问令牌中的用户标识返回当前用户允许展示的基本资料。";
        });
    }

    /// <summary>
    /// 加载并返回当前登录用户的安全基本资料。
    /// </summary>
    /// <param name="ct">用于取消异步操作的令牌。</param>
    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var user = await userRepository.GetAsync(userId, true, ct);
        if (user.SoftDeletedAt is not null || user.LoginInfo is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        await Send.OkAsync(new GetCurrentProfileResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Phone,
            user.LoginInfo.Account,
            user.LoginInfo.Status,
            user.LastLoginTime), ct);
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        // JWT 默认入站映射可能将 sub 映射为 NameIdentifier，兼容两种声明名称。
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(userIdValue, out userId);
    }
}
