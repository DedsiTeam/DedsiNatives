using DedsiNative.Organizations;
using FastEndpoints;

namespace DedsiNative.Endpoints.OrganizationEndpoints;

/// <summary>
/// 组织机构详情响应模型。
/// </summary>
/// <param name="Id">组织唯一标识，26 位 ULID。</param>
/// <param name="SystemId">所属系统标识。</param>
/// <param name="SystemName">所属系统名称。</param>
/// <param name="Code">组织机构编码。</param>
/// <param name="Name">组织机构主名称。</param>
/// <param name="Name1">组织机构名称 1。</param>
/// <param name="Name2">组织机构名称 2。</param>
/// <param name="Name3">组织机构名称 3。</param>
/// <param name="Name4">组织机构名称 4。</param>
/// <param name="ParentId">父级组织标识。</param>
/// <param name="Sort">同级排序序号。</param>
/// <param name="Level">组织层级深度。</param>
/// <param name="IsEnabled">是否启用。</param>
/// <param name="Description">组织说明。</param>
public sealed record OrganizationDetailResponse(
    string Id,
    string SystemId,
    string SystemName,
    string Code,
    string Name,
    string? Name1,
    string? Name2,
    string? Name3,
    string? Name4,
    string? ParentId,
    int Sort,
    int Level,
    bool IsEnabled,
    string? Description);

/// <summary>
/// 获取组织机构详情端点。
/// </summary>
public sealed class GetOrganizationDetailEndpoint(IOrganizationRepository organizationRepository)
    : EndpointWithoutRequest<OrganizationDetailResponse>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Get("/api/organization/{id}");
        Policies(ManagementPermissions.Organizations.View);
        Description(d => d.WithTags("组织机构管理"));
        Summary(s =>
        {
            s.Summary = "获取组织机构详情";
            s.Description = "根据组织唯一标识获取组织机构的详细信息。";
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id");
        var org = await organizationRepository.GetAsync(id, true, ct);

        await Send.OkAsync(new OrganizationDetailResponse(
            org.Id,
            org.SystemId,
            org.SystemName,
            org.Code,
            org.Name,
            org.Name1,
            org.Name2,
            org.Name3,
            org.Name4,
            org.ParentId,
            org.Sort,
            org.Level,
            org.IsEnabled,
            org.Description), ct);
    }
}
