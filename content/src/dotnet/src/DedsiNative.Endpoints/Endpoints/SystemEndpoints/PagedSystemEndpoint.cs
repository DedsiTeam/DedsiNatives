using Dedsi.Ddd.Application.Contracts.Dtos;
using DedsiNative.Systems;
using FastEndpoints;

namespace DedsiNative.Endpoints.SystemEndpoints;

/// <summary>系统分页查询请求。</summary>
public sealed class PagedSystemRequest : DedsiPagedRequestDto
{
    /// <summary>系统名称筛选条件，为空时不筛选。</summary>
    public string? Name { get; set; }
}

/// <summary>系统分页结果中的单行数据。</summary>
/// <param name="Id">系统唯一标识。</param>
/// <param name="Name">系统名称。</param>
/// <param name="Description">系统说明。</param>
/// <param name="Sort">展示排序。</param>
public sealed record PagedSystemRowResponse(string Id, string Name, string? Description, int Sort);

/// <summary>系统分页查询响应。</summary>
public sealed class PagedSystemResponse : DedsiPagedResultDto<PagedSystemRowResponse>;

/// <summary>系统分页查询端点，通过查询契约隔离 Host 与 EF Core。</summary>
/// <param name="systemQuery">系统只读查询服务。</param>
public sealed class PagedSystemEndpoint(ISystemQuery systemQuery)
    : Endpoint<PagedSystemRequest, PagedSystemResponse>
{
    /// <summary>配置系统分页查询接口的路由和 HTTP 方法。</summary>
    public override void Configure()
    {
        Post("/api/system/pagedQuery");
        Description(x => x.WithTags("系统管理"));
        Summary(s =>
        {
            s.Summary = "分页查询系统";
            s.Description = "按系统名称查询系统列表，支持分页和导出，并按排序值返回稳定结果。";
        });
    }

    /// <summary>按名称查询系统并按排序值、ID 返回稳定结果。</summary>
    /// <param name="req">分页查询请求。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(PagedSystemRequest req, CancellationToken ct)
    {
        var query = new SystemPagedQuery(req.Name, req.GetSkipCount(), req.PageSize, req.IsExport);
        var result = await systemQuery.GetPagedAsync(query, ct);

        await Send.OkAsync(new PagedSystemResponse
        {
            TotalCount = result.TotalCount,
            Items = result.Items
                .Select(item => new PagedSystemRowResponse(item.Id, item.Name, item.Description, item.Sort))
                .ToList()
        }, ct);
    }
}
