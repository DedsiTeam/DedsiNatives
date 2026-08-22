using DedsiNative.Users;
using FastEndpoints;

namespace DedsiNative.Endpoints.UserEndpoints;

/// <summary>
/// 删除用户端点，处理 POST /api/user/delete/{id} 请求，根据路由中的用户 ID 删除对应用户，成功后返回 true。
/// </summary>
/// <param name="userRepository">用户仓储，用于查询并删除用户实体。</param>
public class DeleteUserEndpoint(IUserRepository userRepository) : EndpointWithoutRequest<bool>
{
    /// <summary>
    /// 配置端点路由和权限策略。
    /// </summary>
    public override void Configure()
    {
        Post("/api/user/delete/{id}");
        Policies(ManagementPermissions.Users.Delete);
        Description(x => x.WithTags("用户管理"));
        Summary(s =>
        {
            s.Summary = "删除用户";
            s.Description = "根据用户 ID 删除用户。";
        });
    }

    /// <summary>
    /// 处理删除用户请求，从路由中读取用户 ID，查询用户后执行删除操作。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var user = await userRepository.GetAsync(id, true, ct);
        await userRepository.DeleteAsync(user, true, ct);

        await Send.OkAsync(true, ct);
    }
}
