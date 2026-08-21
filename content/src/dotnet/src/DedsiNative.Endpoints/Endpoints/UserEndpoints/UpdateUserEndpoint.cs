using DedsiNative.Organizations;
using DedsiNative.Positions;
using DedsiNative.Users;
using FastEndpoints;

namespace DedsiNative.Endpoints.UserEndpoints;

/// <summary>
/// 更新用户信息的请求参数。
/// </summary>
/// <param name="Name">新的用户名称，不能为空。</param>
/// <param name="Email">新的用户邮箱地址，不能为空。</param>
/// <param name="Phone">新的用户联系电话，可为空。</param>
/// <param name="IdCardNumber">新的用户身份证号码，可为空。</param>
/// <param name="PositionIds">新的岗位关联列表；为空时不修改岗位关联。</param>
/// <param name="OrganizationIds">新的组织机构关联列表；为空时不修改组织机构关联。</param>
/// <param name="LoginInfo">新的登录信息；为空时不修改登录信息。</param>
public sealed record UpdateUserRequest(
    string Name,
    string Email,
    string? Phone = null,
    string? IdCardNumber = null,
    IReadOnlyList<string>? PositionIds = null,
    IReadOnlyList<string>? OrganizationIds = null,
    UserLoginInfoRequest? LoginInfo = null
);

/// <summary>
/// 更新用户端点，处理 POST /api/user/update/{id} 请求，根据路由中的用户 ID 查询用户并更新其资料、岗位及组织机构关联。
/// </summary>
/// <param name="userRepository">用户仓储，用于查询和更新用户实体。</param>
/// <param name="positionRepository">岗位仓储，用于校验关联岗位有效性。</param>
/// <param name="organizationRepository">组织机构仓储，用于校验关联组织有效性。</param>
public class UpdateUserEndpoint(
    IUserRepository userRepository,
    IPositionRepository positionRepository,
    IOrganizationRepository organizationRepository) : Endpoint<UpdateUserRequest, bool>
{
    /// <summary>
    /// 配置端点路由和权限策略。
    /// </summary>
    public override void Configure()
    {
        Post("/api/user/update/{id}");
        Description(x => x.WithTags("用户管理"));
        Summary(s =>
        {
            s.Summary = "更新用户";
            s.Description = "修改用户基本资料、登录信息、岗位及组织机构关联。";
        });
    }

    /// <summary>
    /// 处理更新用户请求，从路由读取用户 ID，查询用户后调用领域方法修改名称和邮箱，最后持久化变更。
    /// </summary>
    /// <param name="req">更新用户的请求参数，包含新的名称和邮箱。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
    {
        var id = Route<Guid>("id");

        var user = await userRepository.GetAsync(id, true, ct);

        user
            .ChangeName(req.Name)
            .ChangeEmail(req.Email)
            .ChangePhone(req.Phone)
            .ChangeIdCardNumber(req.IdCardNumber);

        ApplyLoginInfo(user, req.LoginInfo);
        
        if (req.PositionIds is not null)
        {
            await ReplacePositionsAsync(user, req.PositionIds, ct);
        }

        if (req.OrganizationIds is not null)
        {
            await ReplaceOrganizationsAsync(user, req.OrganizationIds, ct);
        }

        ThrowIfAnyErrors();

        await userRepository.UpdateAsync(user, true, ct);

        await Send.OkAsync(true, ct);
    }

    private void ApplyLoginInfo(User user, UserLoginInfoRequest? request)
    {
        if (request is null) return;
        if (!Enum.IsDefined(request.Status))
        {
            AddError("账户状态无效。");
            return;
        }

        var current = user.LoginInfo;
        if (string.IsNullOrWhiteSpace(request.Password) && current is null)
        {
            AddError("首次设置登录信息时必须设置密码。");
            return;
        }

        var passwordHash = current?.PasswordHash;
        var passwordSalt = current?.PasswordSalt;
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            (passwordHash, passwordSalt) = UserPasswordHasher.Hash(request.Password);
        }

        user.SetLoginInfo(new UserLoginInfo(user.Id, request.Account, passwordHash!, passwordSalt!, request.Status));
    }

    private async Task ReplacePositionsAsync(User user, IReadOnlyList<string> positionIds, CancellationToken ct)
    {
        var positions = new List<Position>();
        var hasDisabledPosition = false;
        foreach (var positionId in positionIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal))
        {
            var position = await positionRepository.GetAsync(positionId, true, ct);
            if (!position.IsEnabled)
            {
                AddError($"岗位 {position.Name} 已停用，不能分配给用户。");
                hasDisabledPosition = true;
                continue;
            }

            positions.Add(position);
        }

        if (hasDisabledPosition) return;
        user.ClearPositions();
        foreach (var position in positions)
        {
            user.AssignPosition(position.Id, position.Name);
        }
    }

    private async Task ReplaceOrganizationsAsync(User user, IReadOnlyList<string> organizationIds, CancellationToken ct)
    {
        var organizations = new List<Organization>();
        var hasDisabledOrg = false;
        foreach (var organizationId in organizationIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal))
        {
            var org = await organizationRepository.GetAsync(organizationId, true, ct);
            if (!org.IsEnabled)
            {
                AddError($"组织机构 {org.Name} 已停用，不能分配给用户。");
                hasDisabledOrg = true;
                continue;
            }

            organizations.Add(org);
        }

        if (hasDisabledOrg) return;
        user.ClearOrganizations();
        foreach (var org in organizations)
        {
            user.AssignOrganization(org.Id, org.Name);
        }
    }
}
