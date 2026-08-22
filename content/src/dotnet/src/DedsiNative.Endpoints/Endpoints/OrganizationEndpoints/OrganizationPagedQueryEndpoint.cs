using DedsiNative.Organizations;
using FastEndpoints;

namespace DedsiNative.Endpoints.OrganizationEndpoints;

/// <summary>
/// 组织机构分页查询请求模型。
/// </summary>
/// <param name="SystemId">所属系统标识（可选）。</param>
/// <param name="Keyword">组织名称或编码关键字（可选）。</param>
/// <param name="ParentId">父级组织标识（可选）。</param>
/// <param name="IsEnabled">启用状态（可选）。</param>
/// <param name="PageIndex">页码（默认 1）。</param>
/// <param name="PageSize">每页条数（默认 10）。</param>
public sealed record OrganizationPagedRequest(
    string? SystemId,
    string? Keyword,
    string? ParentId,
    bool? IsEnabled,
    int PageIndex = 1,
    int PageSize = 10);

/// <summary>
/// 组织机构分页查询响应模型。
/// </summary>
/// <param name="TotalCount">总记录数。</param>
/// <param name="Items">组织记录列表。</param>
public sealed record OrganizationPagedResponse(
    long TotalCount,
    OrganizationQueryItem[] Items);

/// <summary>
/// 分页查询组织机构列表端点。
/// </summary>
public sealed class OrganizationPagedQueryEndpoint(IOrganizationQuery organizationQuery)
    : Endpoint<OrganizationPagedRequest, OrganizationPagedResponse>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Post("/api/organization/pagedQuery");
        Policies(ManagementPermissions.Organizations.View);
        Description(d => d.WithTags("组织机构管理"));
        Summary(s =>
        {
            s.Summary = "分页查询组织机构";
            s.Description = "按系统、关键字、父级和状态分页检索组织机构列表。";
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(OrganizationPagedRequest req, CancellationToken ct)
    {
        var pageIndex = Math.Max(1, req.PageIndex);
        var pageSize = Math.Clamp(req.PageSize, 1, 1000);
        var skipCount = (pageIndex - 1) * pageSize;

        var result = await organizationQuery.GetPagedAsync(
            new OrganizationPagedQuery(
                req.SystemId,
                req.Keyword,
                req.ParentId,
                req.IsEnabled,
                skipCount,
                pageSize,
                false),
            ct);

        await Send.OkAsync(new OrganizationPagedResponse(result.TotalCount, result.Items), ct);
    }
}
