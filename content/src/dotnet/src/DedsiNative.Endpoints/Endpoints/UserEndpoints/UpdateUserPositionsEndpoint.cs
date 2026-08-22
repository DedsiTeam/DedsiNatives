using DedsiNative.Positions;
using DedsiNative.Users;
using FastEndpoints;

namespace DedsiNative.Endpoints.UserEndpoints;

/// <summary>
/// 替换用户岗位关联的请求参数。
/// </summary>
/// <param name="PositionIds">待关联的岗位 ID 列表。</param>
public sealed record UpdateUserPositionsRequest(IReadOnlyList<string> PositionIds);

/// <summary>
/// 用户岗位关联维护端点，负责校验岗位并通过用户聚合替换关联集合。
/// </summary>
/// <param name="userRepository">用户仓储。</param>
/// <param name="positionRepository">岗位仓储，用于读取岗位名称快照。</param>
public sealed class UpdateUserPositionsEndpoint(
    IUserRepository userRepository,
    IPositionRepository positionRepository)
    : Endpoint<UpdateUserPositionsRequest, bool>
{
    /// <summary>
    /// 配置用户岗位关联维护接口。
    /// </summary>
    public override void Configure()
    {
        Post("/api/user/positions/{id}");
        Policies(ManagementPermissions.Users.AssignPosition);
        Description(x => x.WithTags("用户管理"));
        Summary(s =>
        {
            s.Summary = "更新用户岗位";
            s.Description = "替换用户的岗位关联，并同步保存岗位名称快照。";
        });
    }

    /// <summary>
    /// 替换指定用户的全部岗位关联。
    /// </summary>
    /// <param name="req">岗位 ID 列表。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(UpdateUserPositionsRequest req, CancellationToken ct)
    {
        var userId = Route<Guid>("id");
        var user = await userRepository.GetAsync(userId, true, ct);
        var positionIds = req.PositionIds
            .Where(positionId => !string.IsNullOrWhiteSpace(positionId))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        user.ClearPositions();
        foreach (var positionId in positionIds)
        {
            var position = await positionRepository.GetAsync(positionId, true, ct);
            if (!position.IsEnabled)
            {
                AddError($"岗位 {position.Name} 已停用，不能分配给用户。");
                continue;
            }

            user.AssignPosition(position.Id, position.Name);
        }

        ThrowIfAnyErrors();
        await userRepository.UpdateAsync(user, true, ct);
        await Send.OkAsync(true, ct);
    }
}
