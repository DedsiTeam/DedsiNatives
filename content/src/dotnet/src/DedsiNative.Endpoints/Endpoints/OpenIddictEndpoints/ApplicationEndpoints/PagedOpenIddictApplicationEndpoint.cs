using Dedsi.Ddd.Application.Contracts.Dtos;
using DedsiNative.EntityFrameworkCore.Queries;
using DedsiNative.OpenIddict;
using FastEndpoints;

namespace DedsiNative.Endpoints.OpenIddictEndpoints.ApplicationEndpoints;

/// <summary>
/// OpenIddict 客户端分页查询请求。
/// </summary>
public class PagedOpenIddictApplicationRequest : DedsiPagedRequestDto
{
    /// <summary>
    /// 客户端唯一标识模糊搜索。
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// 显示名称模糊搜索。
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 客户端类型（confidential / public）。
    /// </summary>
    public string? ClientType { get; set; }
}

/// <summary>
/// OpenIddict 客户端列表单行数据。
/// </summary>
public record PagedOpenIddictApplicationRowDto(
    string Id,
    string? ClientId,
    string? DisplayName,
    string? ClientType,
    string? ConsentType,
    string[] RedirectUris,
    string[] PostLogoutRedirectUris,
    string[] Permissions);

/// <summary>
/// OpenIddict 客户端分页查询响应。
/// </summary>
public class PagedOpenIddictApplicationResponse : DedsiPagedResultDto<PagedOpenIddictApplicationRowDto>;

/// <summary>
/// 分页查询 OpenIddict 客户端应用端点。
/// </summary>
public class PagedOpenIddictApplicationEndpoint(OpenIddictApplicationQuery query)
    : Endpoint<PagedOpenIddictApplicationRequest, PagedOpenIddictApplicationResponse>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Post("openiddict/applications/pagedQuery");
        Policies(OpenIddict.OpenIddictPermissions.View);
        Description(x => x.WithTags("SSO 客户端管理"));
        Summary(s =>
        {
            s.Summary = "分页查询 SSO 客户端应用";
            s.Description = "按 ClientId、显示名称或客户端类型过滤查询 OpenIddict 客户端应用列表。";
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(PagedOpenIddictApplicationRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(new OpenIddictApplicationPagedQueryInput(
            req.ClientId,
            req.DisplayName,
            req.ClientType,
            req.GetSkipCount(),
            req.PageSize), ct);

        await Send.OkAsync(new PagedOpenIddictApplicationResponse
        {
            TotalCount = result.TotalCount,
            Items = result.Items.Select(item => new PagedOpenIddictApplicationRowDto(
                item.Id,
                item.ClientId,
                item.DisplayName,
                item.ClientType,
                item.ConsentType,
                item.RedirectUris,
                item.PostLogoutRedirectUris,
                item.Permissions)).ToList()
        }, ct);
    }
}
