using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DedsiNative.Users;
using FastEndpoints;

namespace DedsiNative.Endpoints.ProfileEndpoints;

/// <summary>
/// 修改当前登录用户密码的请求参数。
/// </summary>
/// <param name="CurrentPassword">当前密码。</param>
/// <param name="NewPassword">新密码。</param>
/// <param name="ConfirmPassword">新密码确认值。</param>
public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword);

/// <summary>
/// 修改当前登录用户密码端点。
/// </summary>
/// <param name="userRepository">用户聚合仓储。</param>
public sealed class ChangePasswordEndpoint(IUserRepository userRepository)
    : Endpoint<ChangePasswordRequest, bool>
{
    /// <summary>
    /// 配置修改当前用户密码接口。
    /// </summary>
    public override void Configure()
    {
        Post("/api/profile/changePassword");
        Description(x => x.WithTags("个人中心"));
        Summary(s =>
        {
            s.Summary = "修改当前用户密码";
            s.Description = "校验当前密码和确认密码后，更新当前登录用户的密码材料。";
        });
    }

    /// <summary>
    /// 校验请求并通过用户聚合更新当前登录用户的密码。
    /// </summary>
    /// <param name="req">包含当前密码、新密码与确认密码的请求。</param>
    /// <param name="ct">用于取消异步操作的令牌。</param>
    public override async Task HandleAsync(ChangePasswordRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.CurrentPassword))
        {
            ThrowError(request => request.CurrentPassword, "当前密码不能为空。");
        }

        if (string.IsNullOrWhiteSpace(req.NewPassword))
        {
            ThrowError(request => request.NewPassword, "新密码不能为空。");
        }

        if (req.NewPassword != req.ConfirmPassword)
        {
            ThrowError(request => request.ConfirmPassword, "确认密码与新密码不一致。");
        }

        ThrowIfAnyErrors();

        if (!TryGetCurrentUserId(out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var user = await userRepository.GetAsync(userId, true, ct);
        if (user.SoftDeletedAt is not null
            || user.LoginInfo is null
            || user.LoginInfo.Status != AccountStatus.Normal)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        if (!UserPasswordHasher.Verify(
                req.CurrentPassword,
                user.LoginInfo.PasswordHash,
                user.LoginInfo.PasswordSalt))
        {
            ThrowError(request => request.CurrentPassword, "当前密码不正确。");
            ThrowIfAnyErrors();
        }

        var (passwordHash, passwordSalt) = UserPasswordHasher.Hash(req.NewPassword);
        user.ResetPassword(passwordHash, passwordSalt);
        await userRepository.UpdateAsync(user, true, ct);

        await Send.OkAsync(true, ct);
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        // JWT 默认入站映射可能将 sub 映射为 NameIdentifier，兼容两种声明名称。
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(userIdValue, out userId);
    }
}
