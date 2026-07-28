using DedsiNative.Users;
using FastEndpoints;

namespace DedsiNative.Endpoints.UserEndpoints;

/// <summary>
/// 更新用户信息的请求参数。
/// </summary>
/// <param name="Name">新的用户名称，不能为空。</param>
/// <param name="Email">新的用户邮箱地址，不能为空。</param>
public sealed record UpdateUserRequest(
    string Name,
    string Email
);

/// <summary>
/// 更新用户端点，处理 POST /api/user/update/{id} 请求，根据路由中的用户 ID 查询用户并更新其名称和邮箱，成功后返回 true。
/// </summary>
/// <param name="userRepository">用户仓储，用于查询和更新用户实体。</param>
public class UpdateUserEndpoint(IUserRepository userRepository) : Endpoint<UpdateUserRequest, bool>
{
    /// <summary>
    /// 配置端点路由和权限策略。
    /// </summary>
    public override void Configure()
    {
        Post("/api/user/update/{id}");
    }

    /// <summary>
    /// 处理更新用户请求，从路由读取用户 ID，查询用户后调用领域方法修改名称和邮箱，最后持久化变更。
    /// </summary>
    /// <param name="req">更新用户的请求参数，包含新的名称和邮箱。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
    {
        var id = Route<string>("id")!;

        var user = await userRepository.GetAsync(id, true, ct);

        user
            .ChangeName(req.Name)
            .ChangeEmail(req.Email);

        await userRepository.UpdateAsync(user, false, ct);

        await Send.OkAsync(true, ct);
    }
}
