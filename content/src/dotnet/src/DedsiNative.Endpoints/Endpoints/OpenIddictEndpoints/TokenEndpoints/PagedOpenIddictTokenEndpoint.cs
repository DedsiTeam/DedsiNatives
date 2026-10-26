using Dedsi.Ddd.Application.Contracts.Dtos;
using DedsiNative.EntityFrameworkCore.Queries;
using DedsiNative.OpenIddict;
using FastEndpoints;

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
public class PagedOpenIddictTokenEndpoint(OpenIddictTokenQuery query)
    : Endpoint<PagedOpenIddictTokenRequest, PagedOpenIddictTokenResponse>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Post("openiddict/tokens/pagedQuery");
        Policies(OpenIddict.OpenIddictPermissions.View);
        Description(x => x.WithTags("SSO 授权与令牌查看"));
        Summary(s =>
        {
            s.Summary = "分页查询 SSO 令牌列表";
            s.Description = "查看各客户端签发的 AccessToken、RefreshToken 及授权码等令牌状态与过期时间。";
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(PagedOpenIddictTokenRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(new OpenIddictTokenPagedQueryInput(
            req.Subject,
            req.ApplicationId,
            req.Type,
            req.Status,
            req.GetSkipCount(),
            req.PageSize), ct);

        await Send.OkAsync(new PagedOpenIddictTokenResponse
        {
            TotalCount = result.TotalCount,
            Items = result.Items.Select(item => new PagedOpenIddictTokenRowDto(
                item.Id,
                item.ApplicationId,
                item.ClientId,
                item.Subject,
                item.Status,
                item.Type,
                item.CreationDate,
                item.ExpirationDate,
                item.RedemptionDate)).ToList()
        }, ct);
    }
}
