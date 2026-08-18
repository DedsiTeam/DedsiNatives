using DedsiNative.Positions;
using DedsiNative.Permissions;
using DedsiNative.Systems;
using FastEndpoints;

namespace DedsiNative.Endpoints.PositionEndpoints;

/// <summary>创建岗位的请求参数。</summary>
/// <param name="Name">岗位名称。</param>
/// <param name="SystemId">所属系统的 26 位 ULID。</param>
/// <param name="Description">岗位说明，可为空。</param>
/// <param name="IsEnabled">是否启用，默认启用。</param>
/// <param name="PermissionIds">初始关联的权限 ID 列表。</param>
/// <param name="Organizations">初始关联的组织机构列表。</param>
public sealed record CreatePositionRequest(
    string Name,
    string SystemId,
    string? Description,
    bool IsEnabled = true,
    IReadOnlyList<string>? PermissionIds = null,
    IReadOnlyList<PositionOrganizationRequest>? Organizations = null);

/// <summary>创建岗位的响应。</summary>
/// <param name="Id">新岗位的 26 位 ULID 标识。</param>
public sealed record CreatePositionResponse(string Id);

/// <summary>创建岗位端点，校验系统归属后创建岗位聚合。</summary>
public sealed class CreatePositionEndpoint(
    IPositionRepository positionRepository,
    IPermissionRepository permissionRepository,
    ISystemRepository systemRepository)
    : Endpoint<CreatePositionRequest, CreatePositionResponse>
{
    /// <summary>配置岗位创建接口。</summary>
    public override void Configure()
    {
        Post("/api/position/create");
        Description(x => x.WithTags("岗位管理"));
        Summary(s =>
        {
            s.Summary = "创建岗位";
            s.Description = "在指定系统下创建岗位，并同时建立初始权限和组织机构关联。";
        });
    }

    /// <summary>加载系统和权限快照，创建并持久化包含初始关联的岗位聚合。</summary>
    public override async Task HandleAsync(CreatePositionRequest req, CancellationToken ct)
    {
        var system = await systemRepository.GetAsync(req.SystemId, true, ct);
        var id = Ulid.NewUlid().ToString();
        var position = new Position(id, req.Name, system.Id, system.Name, req.Description, req.IsEnabled);

        foreach (var permissionId in (req.PermissionIds ?? []).Distinct(StringComparer.Ordinal))
        {
            var permission = await permissionRepository.GetAsync(permissionId, true, ct);
            position.AddPermission(
                permission.Id,
                permission.Name,
                permission.SystemId,
                permission.SystemName);
        }

        foreach (var organization in (req.Organizations ?? []).DistinctBy(item => item.OrganizationId))
        {
            position.AddOrganization(organization.OrganizationId, organization.OrganizationName);
        }

        await positionRepository.InsertAsync(position, true, ct);
        await Send.OkAsync(new CreatePositionResponse(id), ct);
    }
}
