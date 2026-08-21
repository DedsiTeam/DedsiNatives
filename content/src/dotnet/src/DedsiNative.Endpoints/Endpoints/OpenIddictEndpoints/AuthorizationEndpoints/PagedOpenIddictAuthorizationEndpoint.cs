using Dedsi.Ddd.Application.Contracts.Dtos;
using DedsiNative.EntityFrameworkCore;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace DedsiNative.Endpoints.OpenIddictEndpoints.AuthorizationEndpoints;

/// <summary>
/// OpenIddict 授权记录分页查询请求。
/// </summary>
public class PagedOpenIddictAuthorizationRequest : DedsiPagedRequestDto
{
    /// <summary>
    /// 按用户主体标识 (Subject/UserId) 过滤。
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// 按关联应用 ID 过滤。
    /// </summary>
    public string? ApplicationId { get; set; }

    /// <summary>
    /// 状态过滤 (valid / revoked)。
    /// </summary>
    public string? Status { get; set; }
}

/// <summary>
/// OpenIddict 授权列表单行数据。
/// </summary>
public record PagedOpenIddictAuthorizationRowDto(
    string Id,
    string? ApplicationId,
    string? ClientId,
    string? ApplicationDisplayName,
    string? Subject,
    string? Status,
    string? Type,
    string[] Scopes,
    DateTime? CreationDate);

/// <summary>
/// OpenIddict 授权记录分页查询响应。
/// </summary>
public class PagedOpenIddictAuthorizationResponse : DedsiPagedResultDto<PagedOpenIddictAuthorizationRowDto>;

/// <summary>
/// 分页查询用户应用授权记录端点。
/// </summary>
public class PagedOpenIddictAuthorizationEndpoint(DedsiNativeDbContext dbContext)
    : Endpoint<PagedOpenIddictAuthorizationRequest, PagedOpenIddictAuthorizationResponse>
{
    public override void Configure()
    {
        Post("/api/openiddict/authorizations/pagedQuery");
        Policies(OpenIddict.OpenIddictPermissions.View);
        Description(x => x.WithTags("SSO 授权与令牌查看"));
        Summary(s =>
        {
            s.Summary = "分页查询用户应用授权记录";
            s.Description = "查看用户与各客户端应用的 Consent 授权绑定记录及作用域范围。";
        });
    }

    public override async Task HandleAsync(PagedOpenIddictAuthorizationRequest req, CancellationToken ct)
    {
        var query = dbContext.Set<OpenIddictEntityFrameworkCoreAuthorization>()
            .Include(a => a.Application)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.Subject))
        {
            query = query.Where(a => a.Subject == req.Subject.Trim());
        }

        if (!string.IsNullOrWhiteSpace(req.ApplicationId))
        {
            query = query.Where(a => a.Application != null && a.Application.Id == req.ApplicationId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(req.Status))
        {
            query = query.Where(a => a.Status == req.Status.Trim());
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.CreationDate)
            .Skip(req.GetSkipCount())
            .Take(req.PageSize)
            .ToListAsync(ct);

        var rows = items.Select(item => new PagedOpenIddictAuthorizationRowDto(
            item.Id ?? string.Empty,
            item.Application?.Id,
            item.Application?.ClientId,
            item.Application?.DisplayName,
            item.Subject,
            item.Status,
            item.Type,
            string.IsNullOrEmpty(item.Scopes) ? [] : System.Text.Json.JsonSerializer.Deserialize<string[]>(item.Scopes) ?? [],
            item.CreationDate
        )).ToList();

        await Send.OkAsync(new PagedOpenIddictAuthorizationResponse
        {
            TotalCount = totalCount,
            Items = rows
        }, ct);
    }
}
