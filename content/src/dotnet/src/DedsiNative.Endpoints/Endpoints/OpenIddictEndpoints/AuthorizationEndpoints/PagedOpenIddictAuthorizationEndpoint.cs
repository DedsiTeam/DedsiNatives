using Dedsi.Ddd.Application.Contracts.Dtos;
using DedsiNative.EntityFrameworkCore.Queries;
using DedsiNative.OpenIddict;
using FastEndpoints;

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
public class PagedOpenIddictAuthorizationEndpoint(OpenIddictAuthorizationQuery query)
    : Endpoint<PagedOpenIddictAuthorizationRequest, PagedOpenIddictAuthorizationResponse>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Post("openiddict/authorizations/pagedQuery");
        Policies(OpenIddict.OpenIddictPermissions.View);
        Description(x => x.WithTags("SSO 授权与令牌查看"));
        Summary(s =>
        {
            s.Summary = "分页查询用户应用授权记录";
            s.Description = "查看用户与各客户端应用的 Consent 授权绑定记录及作用域范围。";
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(PagedOpenIddictAuthorizationRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(new OpenIddictAuthorizationPagedQueryInput(
            req.Subject,
            req.ApplicationId,
            req.Status,
            req.GetSkipCount(),
            req.PageSize), ct);

        await Send.OkAsync(new PagedOpenIddictAuthorizationResponse
        {
            TotalCount = result.TotalCount,
            Items = result.Items.Select(item => new PagedOpenIddictAuthorizationRowDto(
                item.Id,
                item.ApplicationId,
                item.ClientId,
                item.ApplicationDisplayName,
                item.Subject,
                item.Status,
                item.Type,
                item.Scopes,
                item.CreationDate)).ToList()
        }, ct);
    }
}
