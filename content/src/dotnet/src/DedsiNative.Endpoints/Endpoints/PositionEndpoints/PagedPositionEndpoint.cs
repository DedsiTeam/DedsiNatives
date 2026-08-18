using Dedsi.Ddd.Application.Contracts.Dtos;
using DedsiNative.Positions;
using FastEndpoints;

namespace DedsiNative.Endpoints.PositionEndpoints;

/// <summary>岗位分页查询请求。</summary>
public sealed class PagedPositionRequest : DedsiPagedRequestDto
{
    /// <summary>系统 ID 筛选条件。</summary>
    public string? SystemId { get; set; }
    /// <summary>岗位名称筛选条件。</summary>
    public string? Name { get; set; }
    /// <summary>启用状态筛选；为空时查询全部状态。</summary>
    public bool? IsEnabled { get; set; }
}

/// <summary>岗位分页结果中的单行数据。</summary>
/// <param name="Id">岗位的 26 位 ULID。</param>
/// <param name="Name">岗位名称。</param>
/// <param name="SystemId">所属系统的 26 位 ULID。</param>
/// <param name="SystemName">所属系统名称快照。</param>
/// <param name="Description">岗位说明。</param>
/// <param name="IsEnabled">是否启用。</param>
/// <param name="PermissionCount">岗位关联的权限数量。</param>
/// <param name="OrganizationCount">岗位关联的组织机构数量。</param>
public sealed record PagedPositionRowResponse(
    string Id,
    string Name,
    string SystemId,
    string SystemName,
    string? Description,
    bool IsEnabled,
    int PermissionCount,
    int OrganizationCount);

/// <summary>岗位分页查询响应。</summary>
public sealed class PagedPositionResponse : DedsiPagedResultDto<PagedPositionRowResponse>;

/// <summary>岗位分页查询端点，通过查询契约隔离 Host 与 EF Core。</summary>
public sealed class PagedPositionEndpoint(IPositionQuery positionQuery)
    : Endpoint<PagedPositionRequest, PagedPositionResponse>
{
    /// <summary>配置岗位分页查询接口。</summary>
    public override void Configure()
    {
        Post("/api/position/pagedQuery");
        Description(x => x.WithTags("岗位管理"));
        Summary(s =>
        {
            s.Summary = "分页查询岗位";
            s.Description = "按系统、岗位名称和启用状态查询岗位列表，返回权限和组织机构数量。";
        });
    }

    /// <summary>按系统、名称和状态筛选岗位并返回关联数量。</summary>
    public override async Task HandleAsync(PagedPositionRequest req, CancellationToken ct)
    {
        var query = new PositionPagedQuery(
            req.SystemId,
            req.Name,
            req.IsEnabled,
            req.GetSkipCount(),
            req.PageSize,
            req.IsExport);
        var result = await positionQuery.GetPagedAsync(query, ct);

        await Send.OkAsync(new PagedPositionResponse
        {
            TotalCount = result.TotalCount,
            Items = result.Items.Select(item => new PagedPositionRowResponse(
                item.Id, item.Name, item.SystemId, item.SystemName, item.Description,
                item.IsEnabled, item.PermissionCount, item.OrganizationCount)).ToList()
        }, ct);
    }
}
