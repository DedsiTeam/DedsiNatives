using Dedsi.Ddd.Application.Contracts.Dtos;
using DedsiNative.EntityFrameworkCore.Queries;
using DedsiNative.OpenIddict;
using FastEndpoints;

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
public class PagedOpenIddictScopeEndpoint(OpenIddictScopeQuery query)
    : Endpoint<PagedOpenIddictScopeRequest, PagedOpenIddictScopeResponse>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Post("openiddict/scopes/pagedQuery");
        Policies(OpenIddict.OpenIddictPermissions.View);
        Description(x => x.WithTags("SSO 作用域管理"));
        Summary(s =>
        {
            s.Summary = "分页查询 SSO 作用域";
            s.Description = "按 Scope 名称或显示名称过滤查询 OpenIddict 作用域列表。";
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(PagedOpenIddictScopeRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(new OpenIddictScopePagedQueryInput(
            req.Name,
            req.DisplayName,
            req.GetSkipCount(),
            req.PageSize), ct);

        await Send.OkAsync(new PagedOpenIddictScopeResponse
        {
            TotalCount = result.TotalCount,
            Items = result.Items.Select(item => new PagedOpenIddictScopeRowDto(
                item.Id,
                item.Name,
                item.DisplayName,
                item.Description,
                item.Resources)).ToList()
        }, ct);
    }
}
