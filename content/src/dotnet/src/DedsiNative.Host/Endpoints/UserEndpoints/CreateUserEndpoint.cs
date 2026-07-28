using DedsiNative.Users;
using FastEndpoints;

namespace DedsiNative.Endpoints.UserEndpoints;

/// <summary>
/// 创建用户的请求参数。
/// </summary>
/// <param name="Name">用户名称，不能为空。</param>
/// <param name="Email">用户邮箱地址，不能为空。</param>
public sealed record CreateUserRequest(
    string Name,
    string Email
);

/// <summary>
/// 创建用户端点，处理 POST /api/user/create 请求，生成新用户并持久化到数据库，返回新用户的 ID。
/// </summary>
/// <param name="userRepository">用户仓储，用于保存新建的用户实体。</param>
public sealed class CreateUserEndpoint(IUserRepository userRepository)
    : Endpoint<CreateUserRequest, string>
{
    /// <summary>
    /// 配置端点路由和权限策略。
    /// </summary>
    public override void Configure()
    {
        Post("/api/user/create");
    }

    /// <summary>
    /// 处理创建用户请求，生成 ULID 作为用户唯一标识，创建用户实体并写入数据库。
    /// </summary>
    /// <param name="req">创建用户的请求参数，包含名称和邮箱。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        var domainId = Ulid.NewUlid().ToString();
        var user = new User(domainId, req.Name, req.Email);

        await userRepository.InsertAsync(user, false, ct);

        await Send.OkAsync(domainId, ct);
    }
}
