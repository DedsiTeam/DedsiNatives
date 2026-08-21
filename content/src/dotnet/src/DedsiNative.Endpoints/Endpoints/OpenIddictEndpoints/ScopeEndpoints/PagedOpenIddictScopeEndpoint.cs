using Dedsi.Ddd.Application.Contracts.Dtos;
using DedsiNative.EntityFrameworkCore;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace DedsiNative.Endpoints.OpenIddictEndpoints.ScopeEndpoints;

/// <summary>
/// OpenIddict 作用域分页查询请求。
/// </summary>
public class PagedOpenIddictScopeRequest : DedsiPagedRequestDto
{
    /// <summary>
    /// 作用域名称模糊搜索。
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 显示名称模糊搜索。
    /// </summary>
    public string? DisplayName { get; set; }
}

/// <summary>
/// OpenIddict 作用域列表单行数据。
/// </summary>
public record PagedOpenIddictScopeRowDto(
    string Id,
    string? Name,
    string? DisplayName,
    string? Description,
    string[] Resources);

/// <summary>
/// OpenIddict 作用域分页查询响应。
/// </summary>
public class PagedOpenIddictScopeResponse : DedsiPagedResultDto<PagedOpenIddictScopeRowDto>;

/// <summary>
/// 分页查询 OpenIddict 作用域端点。
/// </summary>
public class PagedOpenIddictScopeEndpoint(DedsiNativeDbContext dbContext)
    : Endpoint<PagedOpenIddictScopeRequest, PagedOpenIddictScopeResponse>
{
    public override void Configure()
    {
        Post("/api/openiddict/scopes/pagedQuery");
        Policies(OpenIddict.OpenIddictPermissions.View);
        Description(x => x.WithTags("SSO 作用域管理"));
        Summary(s =>
        {
            s.Summary = "分页查询 SSO 作用域";
            s.Description = "按 Scope 名称或显示名称过滤查询 OpenIddict 作用域列表。";
        });
    }

    public override async Task HandleAsync(PagedOpenIddictScopeRequest req, CancellationToken ct)
    {
        var query = dbContext.Set<OpenIddictEntityFrameworkCoreScope>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.Name))
        {
            query = query.Where(s => s.Name != null && s.Name.Contains(req.Name.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(req.DisplayName))
        {
            query = query.Where(s => s.DisplayName != null && s.DisplayName.Contains(req.DisplayName.Trim()));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(s => s.Name)
            .Skip(req.GetSkipCount())
            .Take(req.PageSize)
            .ToListAsync(ct);

        var rows = items.Select(item => new PagedOpenIddictScopeRowDto(
            item.Id ?? string.Empty,
            item.Name,
            item.DisplayName,
            item.Description,
            string.IsNullOrEmpty(item.Resources) ? [] : System.Text.Json.JsonSerializer.Deserialize<string[]>(item.Resources) ?? []
        )).ToList();

        await Send.OkAsync(new PagedOpenIddictScopeResponse
        {
            TotalCount = totalCount,
            Items = rows
        }, ct);
    }
}
