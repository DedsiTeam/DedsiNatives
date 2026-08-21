using System.Collections.Immutable;
using Dedsi.Ddd.Application.Contracts.Dtos;
using DedsiNative.EntityFrameworkCore;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

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
public class PagedOpenIddictApplicationEndpoint(DedsiNativeDbContext dbContext)
    : Endpoint<PagedOpenIddictApplicationRequest, PagedOpenIddictApplicationResponse>
{
    public override void Configure()
    {
        Post("/api/openiddict/applications/pagedQuery");
        Policies(OpenIddict.OpenIddictPermissions.View);
        Description(x => x.WithTags("SSO 客户端管理"));
        Summary(s =>
        {
            s.Summary = "分页查询 SSO 客户端应用";
            s.Description = "按 ClientId、显示名称或客户端类型过滤查询 OpenIddict 客户端应用列表。";
        });
    }

    public override async Task HandleAsync(PagedOpenIddictApplicationRequest req, CancellationToken ct)
    {
        var query = dbContext.Set<OpenIddictEntityFrameworkCoreApplication>().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.ClientId))
        {
            query = query.Where(a => a.ClientId != null && a.ClientId.Contains(req.ClientId.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(req.DisplayName))
        {
            query = query.Where(a => a.DisplayName != null && a.DisplayName.Contains(req.DisplayName.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(req.ClientType))
        {
            query = query.Where(a => a.ClientType == req.ClientType.Trim());
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(a => a.ClientId)
            .Skip(req.GetSkipCount())
            .Take(req.PageSize)
            .ToListAsync(ct);

        var rows = items.Select(item => new PagedOpenIddictApplicationRowDto(
            item.Id ?? string.Empty,
            item.ClientId,
            item.DisplayName,
            item.ClientType,
            item.ConsentType,
            string.IsNullOrEmpty(item.RedirectUris) ? [] : System.Text.Json.JsonSerializer.Deserialize<string[]>(item.RedirectUris) ?? [],
            string.IsNullOrEmpty(item.PostLogoutRedirectUris) ? [] : System.Text.Json.JsonSerializer.Deserialize<string[]>(item.PostLogoutRedirectUris) ?? [],
            string.IsNullOrEmpty(item.Permissions) ? [] : System.Text.Json.JsonSerializer.Deserialize<string[]>(item.Permissions) ?? []
        )).ToList();

        await Send.OkAsync(new PagedOpenIddictApplicationResponse
        {
            TotalCount = totalCount,
            Items = rows
        }, ct);
    }
}
