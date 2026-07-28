using DedsiNative.Users;
using FastEndpoints;

namespace DedsiNative.Endpoints.UserEndpoints;

/// <summary>
/// 获取用户详情的响应数据传输对象。
/// </summary>
/// <param name="Id">用户唯一标识。</param>
/// <param name="Name">用户名称。</param>
/// <param name="Email">用户邮箱地址。</param>
public record GetUserResponse(string Id, string Name, string Email);

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
    }

    /// <summary>
    /// 处理获取用户请求，从路由中读取用户 ID，查询用户后返回详情数据。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id")!;
        var user = await userRepository.GetAsync(id, true, ct);

        await Send.OkAsync(new GetUserResponse(user.Id, user.Name, user.Email), ct);
    }
}
