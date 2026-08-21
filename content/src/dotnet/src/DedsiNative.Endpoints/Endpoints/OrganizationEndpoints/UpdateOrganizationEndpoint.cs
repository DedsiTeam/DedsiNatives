using DedsiNative.Organizations;
using FastEndpoints;
using FluentValidation;
using Volo.Abp;

namespace DedsiNative.Endpoints.OrganizationEndpoints;

/// <summary>
/// 更新组织机构请求模型。
/// </summary>
/// <param name="Name">组织机构主名称。</param>
/// <param name="Name1">组织机构名称 1（可选）。</param>
/// <param name="Name2">组织机构名称 2（可选）。</param>
/// <param name="Name3">组织机构名称 3（可选）。</param>
/// <param name="Name4">组织机构名称 4（可选）。</param>
/// <param name="ParentId">父级组织标识（可选，根节点为 null）。</param>
/// <param name="Sort">同级排序序号。</param>
/// <param name="Description">组织说明（可选）。</param>
public sealed record UpdateOrganizationRequest(
    string Name,
    string? Name1,
    string? Name2,
    string? Name3,
    string? Name4,
    string? ParentId,
    int Sort = 0,
    string? Description = null);

/// <summary>
/// 更新组织机构响应模型。
/// </summary>
/// <param name="Success">是否更新成功。</param>
public sealed record UpdateOrganizationResponse(bool Success);

/// <summary>
/// 更新组织机构请求验证器。
/// </summary>
public sealed class UpdateOrganizationRequestValidator : Validator<UpdateOrganizationRequest>
{
    /// <inheritdoc />
    public UpdateOrganizationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("组织机构主名称不能为空。")
            .MaximumLength(OrganizationConsts.MaxNameLength).WithMessage($"组织机构主名称最多允许 {OrganizationConsts.MaxNameLength} 个字符。");
    }
}

/// <summary>
/// 更新组织机构端点。
/// </summary>
public sealed class UpdateOrganizationEndpoint(IOrganizationRepository organizationRepository)
    : Endpoint<UpdateOrganizationRequest, UpdateOrganizationResponse>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Post("/api/organization/update/{id}");
        Description(d => d.WithTags("组织机构管理"));
        Summary(s =>
        {
            s.Summary = "更新组织机构";
            s.Description = "更新指定组织机构的基本属性、扩展名称或层级归属。";
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UpdateOrganizationRequest req, CancellationToken ct)
    {
        var id = Route<string>("id");
        var org = await organizationRepository.GetAsync(id, true, ct);

        // 如果调整了父级组织
        if (org.ParentId != req.ParentId)
        {
            var level = 1;
            if (!string.IsNullOrWhiteSpace(req.ParentId))
            {
                if (req.ParentId == org.Id)
                {
                    throw new BusinessException(
                        "DedsiNative:Organization:CannotBeParentOfSelf",
                        "组织机构不能将自身设置为上级组织。");
                }

                var wouldCycle = await organizationRepository.WouldCreateCycleAsync(org.Id, req.ParentId, ct);
                if (wouldCycle)
                {
                    throw new BusinessException(
                        "DedsiNative:Organization:CyclicDependency",
                        "目标上级组织不能为当前组织的下级或子组织，这会导致循环引用。");
                }

                var parent = await organizationRepository.GetAsync(req.ParentId, true, ct);
                if (parent.SystemId != org.SystemId)
                {
                    throw new BusinessException(
                        "DedsiNative:Organization:SystemMismatch",
                        "父级组织机构必须属于同一系统。");
                }
                level = parent.Level + 1;
            }

            org.ChangeParent(req.ParentId, level);
        }

        org.UpdateInfo(
            req.Name,
            req.Name1,
            req.Name2,
            req.Name3,
            req.Name4,
            req.Sort,
            req.Description);

        await organizationRepository.UpdateAsync(org, true, ct);
        await Send.OkAsync(new UpdateOrganizationResponse(true), ct);
    }
}
