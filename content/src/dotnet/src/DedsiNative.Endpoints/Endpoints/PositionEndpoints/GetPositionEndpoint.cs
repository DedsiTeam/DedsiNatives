using DedsiNative.Positions;
using FastEndpoints;

namespace DedsiNative.Endpoints.PositionEndpoints;

/// <summary>岗位详情中的权限关联。</summary>
/// <param name="PermissionId">权限的 26 位 ULID。</param>
/// <param name="PermissionName">权限名称快照。</param>
/// <param name="SystemId">权限所属系统的 26 位 ULID。</param>
/// <param name="SystemName">权限所属系统名称快照。</param>
public sealed record PositionPermissionResponse(string PermissionId, string PermissionName, string SystemId, string SystemName);

/// <summary>岗位详情中的组织机构关联。</summary>
/// <param name="OrganizationId">组织机构标识。</param>
/// <param name="OrganizationName">组织机构名称快照。</param>
public sealed record PositionOrganizationResponse(string OrganizationId, string OrganizationName);

/// <summary>岗位详情响应。</summary>
/// <param name="Id">岗位的 26 位 ULID。</param>
/// <param name="Name">岗位名称。</param>
/// <param name="SystemId">所属系统的 26 位 ULID。</param>
/// <param name="SystemName">所属系统名称快照。</param>
/// <param name="Description">岗位说明。</param>
/// <param name="IsEnabled">是否启用。</param>
/// <param name="Permissions">岗位权限关联列表。</param>
/// <param name="Organizations">岗位组织机构关联列表。</param>
public sealed record GetPositionResponse(
    string Id,
    string Name,
    string SystemId,
    string SystemName,
    string? Description,
    bool IsEnabled,
    PositionPermissionResponse[] Permissions,
    PositionOrganizationResponse[] Organizations);

/// <summary>获取岗位详情端点，返回岗位及其子实体关联。</summary>
public sealed class GetPositionEndpoint(IPositionRepository positionRepository)
    : EndpointWithoutRequest<GetPositionResponse>
{
    /// <summary>配置岗位详情接口。</summary>
    public override void Configure()
    {
        Get("/api/position/{id}");
        Policies(ManagementPermissions.Positions.View);
        Description(x => x.WithTags("岗位管理"));
        Summary(s =>
        {
            s.Summary = "获取岗位详情";
            s.Description = "根据岗位 ID 获取岗位及其权限、组织机构关联。";
        });
    }

    /// <summary>加载包含权限和组织机构关联的完整岗位聚合。</summary>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id")!;
        var position = await positionRepository.GetAsync(id, true, ct);
        await Send.OkAsync(new GetPositionResponse(
            position.Id,
            position.Name,
            position.SystemId,
            position.SystemName,
            position.Description,
            position.IsEnabled,
            position.Permissions.Select(item => new PositionPermissionResponse(
                item.PermissionId, item.PermissionName, item.SystemId, item.SystemName)).ToArray(),
            position.Organizations.Select(item => new PositionOrganizationResponse(
                item.OrganizationId, item.OrganizationName)).ToArray()), ct);
    }
}
