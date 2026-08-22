using DedsiNative.Users;
using FastEndpoints;

namespace DedsiNative.Endpoints.UserEndpoints;

/// <summary>
/// 重置用户密码端点，将有效用户的登录密码恢复为系统默认密码。
/// </summary>
/// <param name="userRepository">用户仓储，用于加载和保存完整用户聚合。</param>
public sealed class ResetUserPasswordEndpoint(IUserRepository userRepository)
    : EndpointWithoutRequest<bool>
{
    /// <summary>
    /// 配置重置用户密码接口的路由和 HTTP 方法。
    /// </summary>
    public override void Configure()
    {
        Post("/api/user/resetPassword/{id}");
        Policies(ManagementPermissions.Users.ResetPassword);
        Description(x => x.WithTags("用户管理"));
        Summary(s =>
        {
            s.Summary = "重置用户密码";
            s.Description = "将指定有效用户的密码恢复为系统默认密码。";
        });
    }

    /// <summary>
    /// 使用新的随机盐值生成默认密码的哈希，并通过聚合完成密码重置。
    /// </summary>
    /// <param name="ct">用于取消异步操作的令牌。</param>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var user = await userRepository.GetAsync(id, true, ct);
        var (passwordHash, passwordSalt) = UserPasswordHasher.Hash(UserConsts.DefaultPassword);

        user.ResetPassword(passwordHash, passwordSalt);
        await userRepository.UpdateAsync(user, true, ct);

        await Send.OkAsync(true, ct);
    }
}
