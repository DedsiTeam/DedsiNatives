using DedsiNative.Permissions;
using DedsiNative.Positions;
using FastEndpoints;

namespace DedsiNative.Endpoints.PositionEndpoints;

/// <summary>岗位关联替换请求中的组织机构数据。</summary>
/// <param name="OrganizationId">组织机构标识。</param>
/// <param name="OrganizationName">组织机构名称。</param>
public sealed record PositionOrganizationRequest(string OrganizationId, string OrganizationName);

/// <summary>替换岗位权限和组织机构关联的请求参数。</summary>
/// <param name="PermissionIds">待关联的权限 ID 列表。</param>
/// <param name="Organizations">待关联的组织机构列表。</param>
public sealed record UpdatePositionAssignmentsRequest(
    IReadOnlyList<string> PermissionIds,
    IReadOnlyList<PositionOrganizationRequest> Organizations);

/// <summary>岗位关联端点，使用聚合根统一维护两个子实体集合。</summary>
public sealed class UpdatePositionAssignmentsEndpoint(
    IPositionRepository positionRepository,
    IPermissionRepository permissionRepository)
    : Endpoint<UpdatePositionAssignmentsRequest, bool>
{
    /// <summary>配置岗位关联替换接口。</summary>
    public override void Configure()
    {
        Post("/api/position/assignments/{id}");
        Description(x => x.WithTags("岗位管理"));
        Summary(s =>
        {
            s.Summary = "更新岗位分配";
            s.Description = "替换岗位的权限和组织机构关联，并更新权限名称和系统名称快照。";
        });
    }

    /// <summary>加载权限快照，替换岗位权限和组织机构关联并持久化。</summary>
    public override async Task HandleAsync(UpdatePositionAssignmentsRequest req, CancellationToken ct)
    {
        var id = Route<string>("id")!;
        var position = await positionRepository.GetAsync(id, true, ct);
        position.ClearPermissions().ClearOrganizations();

        foreach (var permissionId in req.PermissionIds.Distinct(StringComparer.Ordinal))
        {
            var permission = await permissionRepository.GetAsync(permissionId, true, ct);
            position.AddPermission(
                permission.Id,
                permission.Name,
                permission.SystemId,
                permission.SystemName);
        }

        foreach (var organization in req.Organizations.DistinctBy(item => item.OrganizationId))
        {
            position.AddOrganization(organization.OrganizationId, organization.OrganizationName);
        }

        await positionRepository.UpdateAsync(position, true, ct);
        await Send.OkAsync(true, ct);
    }
}
