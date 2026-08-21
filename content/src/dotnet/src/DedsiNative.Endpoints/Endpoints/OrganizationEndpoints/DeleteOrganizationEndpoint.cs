using DedsiNative.Organizations;
using FastEndpoints;
using Volo.Abp;

namespace DedsiNative.Endpoints.OrganizationEndpoints;

/// <summary>
/// 删除组织机构响应模型。
/// </summary>
/// <param name="Success">是否删除成功。</param>
public sealed record DeleteOrganizationResponse(bool Success);

/// <summary>
/// 删除组织机构端点。
/// </summary>
public sealed class DeleteOrganizationEndpoint(
    IOrganizationRepository organizationRepository,
    IOrganizationQuery organizationQuery)
    : EndpointWithoutRequest<DeleteOrganizationResponse>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Post("/api/organization/delete/{id}");
        Description(d => d.WithTags("组织机构管理"));
        Summary(s =>
        {
            s.Summary = "删除组织机构";
            s.Description = "删除指定组织机构。若存在下级子组织则禁止删除。";
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id");
        var hasChildren = await organizationQuery.HasChildrenAsync(id, ct);
        if (hasChildren)
        {
            throw new BusinessException(
                "DedsiNative:Organization:HasChildrenCannotDelete",
                "该组织机构下包含下级子组织，无法直接删除，请先移除或转移所有子组织。");
        }

        var org = await organizationRepository.GetAsync(id, true, ct);
        await organizationRepository.DeleteAsync(org, true, ct);

        await Send.OkAsync(new DeleteOrganizationResponse(true), ct);
    }
}
