using DedsiNative.Organizations;
using FastEndpoints;

namespace DedsiNative.Endpoints.OrganizationEndpoints;

/// <summary>
/// 设置组织机构启用状态请求模型。
/// </summary>
/// <param name="IsEnabled">是否启用。</param>
public sealed record SetOrganizationStatusRequest(bool IsEnabled);

/// <summary>
/// 设置组织机构启用状态响应模型。
/// </summary>
/// <param name="Success">是否操作成功。</param>
public sealed record SetOrganizationStatusResponse(bool Success);

/// <summary>
/// 设置组织机构启用/停用状态端点。
/// </summary>
public sealed class SetOrganizationStatusEndpoint(IOrganizationRepository organizationRepository)
    : Endpoint<SetOrganizationStatusRequest, SetOrganizationStatusResponse>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Post("/api/organization/setStatus/{id}");
        Policies(ManagementPermissions.Organizations.Update);
        Description(d => d.WithTags("组织机构管理"));
        Summary(s =>
        {
            s.Summary = "设置组织机构启用状态";
            s.Description = "启用或停用指定的组织机构。";
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(SetOrganizationStatusRequest req, CancellationToken ct)
    {
        var id = Route<string>("id");
        var org = await organizationRepository.GetAsync(id, true, ct);

        org.SetStatus(req.IsEnabled);
        await organizationRepository.UpdateAsync(org, true, ct);

        await Send.OkAsync(new SetOrganizationStatusResponse(true), ct);
    }
}
