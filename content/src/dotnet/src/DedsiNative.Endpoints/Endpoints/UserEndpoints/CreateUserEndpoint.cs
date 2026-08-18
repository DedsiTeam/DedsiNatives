using DedsiNative.Users;
using DedsiNative.Positions;
using FastEndpoints;

namespace DedsiNative.Endpoints.UserEndpoints;

/// <summary>
/// 创建用户的请求参数。
/// </summary>
/// <param name="Name">用户名称，不能为空。</param>
/// <param name="Email">用户邮箱地址，不能为空。</param>
/// <param name="Phone">用户联系电话，可为空。</param>
/// <param name="IdCardNumber">用户身份证号码，可为空。</param>
/// <param name="PositionIds">初始关联的岗位 ID 列表。</param>
/// <param name="LoginInfo">初始登录信息，可为空。</param>
public sealed record CreateUserRequest(
    string Name,
    string Email,
    string? Phone = null,
    string? IdCardNumber = null,
    IReadOnlyList<string>? PositionIds = null,
    UserLoginInfoRequest? LoginInfo = null
);

/// <summary>
/// 创建用户端点，处理 POST /api/user/create 请求，生成新用户并持久化到数据库，返回新用户的 ID。
/// </summary>
/// <param name="userRepository">
/// 用户仓储，用于保存新建的用户实体。
/// </param>
/// <param name="positionRepository">
/// 岗位仓储，用于校验关联岗位有效性。
/// </param>
public sealed class CreateUserEndpoint(
    IUserRepository userRepository,
    IPositionRepository positionRepository)
    : Endpoint<CreateUserRequest, Guid>
{
    /// <summary>
    /// 配置端点路由和权限策略。
    /// </summary>
    public override void Configure()
    {
        Post("/api/user/create");
        Description(x => x.WithTags("用户管理"));
        Summary(s =>
        {
            s.Summary = "创建用户";
            s.Description = "创建用户基本资料及登录信息。";
        });
    }

    /// <summary>
    /// 处理创建用户请求，生成 ULID 作为用户唯一标识，创建用户实体并写入数据库。
    /// </summary>
    /// <param name="req">创建用户的请求参数，包含名称和邮箱。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        var domainId = Guid.NewGuid();
        
        var user = new User(domainId, req.Name, req.Email)
            .ChangePhone(req.Phone)
            .ChangeIdCardNumber(req.IdCardNumber);

        ApplyLoginInfo(user, req.LoginInfo);
        
        await AssignPositionsAsync(user, req.PositionIds, ct);
        
        ThrowIfAnyErrors();

        await userRepository.InsertAsync(user, true, ct);

        await Send.OkAsync(domainId, ct);
    }

    private void ApplyLoginInfo(User user, UserLoginInfoRequest? request)
    {
        if (request is null) return;
        if (!Enum.IsDefined(request.Status))
        {
            AddError("账户状态无效。");
            return;
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            AddError("创建登录信息时必须设置密码。");
            return;
        }

        var (passwordHash, passwordSalt) = UserPasswordHasher.Hash(request.Password);
        user.SetLoginInfo(new UserLoginInfo(user.Id, request.Account, passwordHash, passwordSalt, request.Status));
    }

    private async Task AssignPositionsAsync(User user, IReadOnlyList<string>? positionIds, CancellationToken ct)
    {
        foreach (var positionId in (positionIds ?? []).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal))
        {
            var position = await positionRepository.GetAsync(positionId, true, ct);
            if (!position.IsEnabled)
            {
                AddError($"岗位 {position.Name} 已停用，不能分配给用户。");
                continue;
            }

            user.AssignPosition(position.Id, position.Name);
        }
    }
}
