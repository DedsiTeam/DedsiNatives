using Dedsi.Ddd.Application.Contracts.Dtos;
using DedsiNative.EntityFrameworkCore;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace DedsiNative.Endpoints.OpenIddictEndpoints.TokenEndpoints;

/// <summary>
/// OpenIddict 令牌分页查询请求。
/// </summary>
public class PagedOpenIddictTokenRequest : DedsiPagedRequestDto
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
    /// 按令牌类型过滤 (access_token / refresh_token / authorization_code 等)。
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// 按状态过滤 (valid / revoked / redeemed 等)。
    /// </summary>
    public string? Status { get; set; }
}

/// <summary>
/// OpenIddict 令牌列表单行数据。
/// </summary>
public record PagedOpenIddictTokenRowDto(
    string Id,
    string? ApplicationId,
    string? ClientId,
    string? Subject,
    string? Status,
    string? Type,
    DateTime? CreationDate,
    DateTime? ExpirationDate,
    DateTime? RedemptionDate);

/// <summary>
/// OpenIddict 令牌分页查询响应。
/// </summary>
public class PagedOpenIddictTokenResponse : DedsiPagedResultDto<PagedOpenIddictTokenRowDto>;

/// <summary>
/// 分页查询活跃与历史令牌端点。
/// </summary>
public class PagedOpenIddictTokenEndpoint(DedsiNativeDbContext dbContext)
    : Endpoint<PagedOpenIddictTokenRequest, PagedOpenIddictTokenResponse>
{
    public override void Configure()
    {
        Post("/api/openiddict/tokens/pagedQuery");
        Policies(OpenIddict.OpenIddictPermissions.View);
        Description(x => x.WithTags("SSO 授权与令牌查看"));
        Summary(s =>
        {
            s.Summary = "分页查询 SSO 令牌列表";
            s.Description = "查看各客户端签发的 AccessToken、RefreshToken 及授权码等令牌状态与过期时间。";
        });
    }

    public override async Task HandleAsync(PagedOpenIddictTokenRequest req, CancellationToken ct)
    {
        var query = dbContext.Set<OpenIddictEntityFrameworkCoreToken>()
            .Include(t => t.Application)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(req.Subject))
        {
            query = query.Where(t => t.Subject == req.Subject.Trim());
        }

        if (!string.IsNullOrWhiteSpace(req.ApplicationId))
        {
            query = query.Where(t => t.Application != null && t.Application.Id == req.ApplicationId.Trim());
        }

        if (!string.IsNullOrWhiteSpace(req.Type))
        {
            query = query.Where(t => t.Type == req.Type.Trim());
        }

        if (!string.IsNullOrWhiteSpace(req.Status))
        {
            query = query.Where(t => t.Status == req.Status.Trim());
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.CreationDate)
            .Skip(req.GetSkipCount())
            .Take(req.PageSize)
            .ToListAsync(ct);

        var rows = items.Select(item => new PagedOpenIddictTokenRowDto(
            item.Id ?? string.Empty,
            item.Application?.Id,
            item.Application?.ClientId,
            item.Subject,
            item.Status,
            item.Type,
            item.CreationDate,
            item.ExpirationDate,
            item.RedemptionDate
        )).ToList();

        await Send.OkAsync(new PagedOpenIddictTokenResponse
        {
            TotalCount = totalCount,
            Items = rows
        }, ct);
    }
}
