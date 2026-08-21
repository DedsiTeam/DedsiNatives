using DedsiNative.Organizations;
using DedsiNative.Systems;
using FastEndpoints;
using FluentValidation;
using Volo.Abp;

namespace DedsiNative.Endpoints.OrganizationEndpoints;

/// <summary>
/// 创建组织机构请求模型。
/// </summary>
/// <param name="SystemId">所属系统标识。</param>
/// <param name="Code">组织机构编码。</param>
/// <param name="Name">组织机构主名称。</param>
/// <param name="Name1">组织机构名称 1（可选）。</param>
/// <param name="Name2">组织机构名称 2（可选）。</param>
/// <param name="Name3">组织机构名称 3（可选）。</param>
/// <param name="Name4">组织机构名称 4（可选）。</param>
/// <param name="ParentId">父级组织标识（可选，根节点为 null）。</param>
/// <param name="Sort">同级排序序号（默认 0）。</param>
/// <param name="Description">组织说明（可选）。</param>
public sealed record CreateOrganizationRequest(
    string SystemId,
    string Code,
    string Name,
    string? Name1,
    string? Name2,
    string? Name3,
    string? Name4,
    string? ParentId,
    int Sort = 0,
    string? Description = null);

/// <summary>
/// 创建组织机构响应模型。
/// </summary>
/// <param name="Id">新创建的组织唯一标识，26 位 ULID。</param>
public sealed record CreateOrganizationResponse(string Id);

/// <summary>
/// 创建组织机构请求验证器。
/// </summary>
public sealed class CreateOrganizationRequestValidator : Validator<CreateOrganizationRequest>
{
    /// <inheritdoc />
    public CreateOrganizationRequestValidator()
    {
        RuleFor(x => x.SystemId)
            .NotEmpty().WithMessage("所属系统标识不能为空。")
            .Length(OrganizationConsts.UlidLength).WithMessage($"所属系统标识必须为 {OrganizationConsts.UlidLength} 位 ULID。");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("组织机构编码不能为空。")
            .MaximumLength(OrganizationConsts.MaxCodeLength).WithMessage($"组织机构编码最多允许 {OrganizationConsts.MaxCodeLength} 个字符。");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("组织机构主名称不能为空。")
            .MaximumLength(OrganizationConsts.MaxNameLength).WithMessage($"组织机构主名称最多允许 {OrganizationConsts.MaxNameLength} 个字符。");
    }
}

/// <summary>
/// 创建组织机构端点。
/// </summary>
public sealed class CreateOrganizationEndpoint(
    IOrganizationRepository organizationRepository,
    IOrganizationQuery organizationQuery,
    ISystemRepository systemRepository)
    : Endpoint<CreateOrganizationRequest, CreateOrganizationResponse>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Post("/api/organization/create");
        Description(d => d.WithTags("组织机构管理"));
        Summary(s =>
        {
            s.Summary = "创建组织机构";
            s.Description = "在指定系统下创建组织机构或子级部门。";
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CreateOrganizationRequest req, CancellationToken ct)
    {
        var system = await systemRepository.GetAsync(req.SystemId, true, ct);

        var exists = await organizationQuery.ExistsBySystemAndCodeAsync(req.SystemId, req.Code, ct);
        if (exists)
        {
            throw new BusinessException(
                "DedsiNative:Organization:CodeAlreadyExists",
                $"系统「{system.Name}」下已存在编码为「{req.Code}」的组织机构。");
        }

        var level = 1;
        if (!string.IsNullOrWhiteSpace(req.ParentId))
        {
            var parent = await organizationRepository.GetAsync(req.ParentId, true, ct);
            if (parent.SystemId != req.SystemId)
            {
                throw new BusinessException(
                    "DedsiNative:Organization:SystemMismatch",
                    "父级组织机构必须属于同一系统。");
            }
            level = parent.Level + 1;
        }

        var id = Ulid.NewUlid().ToString();
        var organization = new Organization(
            id,
            system.Id,
            system.Name,
            req.Code,
            req.Name,
            req.Name1,
            req.Name2,
            req.Name3,
            req.Name4,
            req.ParentId,
            req.Sort,
            level,
            req.Description);

        await organizationRepository.InsertAsync(organization, true, ct);
        await Send.OkAsync(new CreateOrganizationResponse(id), ct);
    }
}
