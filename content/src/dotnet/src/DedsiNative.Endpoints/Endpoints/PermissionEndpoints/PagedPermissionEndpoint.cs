using Dedsi.Ddd.Application.Contracts.Dtos;
using DedsiNative.Permissions;
using FastEndpoints;

namespace DedsiNative.Endpoints.PermissionEndpoints;

/// <summary>权限分页查询请求。</summary>
public sealed class PagedPermissionRequest : DedsiPagedRequestDto
{
    /// <summary>系统 ID 筛选条件。</summary>
    public string? SystemId { get; set; }

    /// <summary>权限名称筛选条件。</summary>
    public string? Name { get; set; }

    /// <summary>启用状态筛选；为空时查询全部状态。</summary>
    public bool? IsEnabled { get; set; }
}

/// <summary>权限分页结果中的单行数据。</summary>
/// <param name="Id">权限唯一标识。</param>
/// <param name="SystemId">所属系统 ID。</param>
/// <param name="SystemName">所属系统名称。</param>
/// <param name="Name">权限名称。</param>
/// <param name="Description">权限说明。</param>
/// <param name="IsEnabled">是否启用。</param>
public sealed record PagedPermissionRowResponse(
    string Id,
    string SystemId,
    string SystemName,
    string Name,
    string? Description,
    bool IsEnabled);

/// <summary>权限分页查询响应。</summary>
public sealed class PagedPermissionResponse : DedsiPagedResultDto<PagedPermissionRowResponse>;

/// <summary>权限分页查询端点，通过查询契约隔离 Host 与 EF Core。</summary>
/// <param name="permissionQuery">权限只读查询服务。</param>
public sealed class PagedPermissionEndpoint(IPermissionQuery permissionQuery)
    : Endpoint<PagedPermissionRequest, PagedPermissionResponse>
{
    /// <summary>配置权限分页查询接口的路由和 HTTP 方法。</summary>
    public override void Configure()
    {
        Post("/api/permission/pagedQuery");
        Policies(ManagementPermissions.Permissions.View);
        Description(x => x.WithTags("权限管理"));
        Summary(s =>
        {
            s.Summary = "分页查询权限";
            s.Description = "按系统、权限名称和启用状态查询权限列表，支持分页和导出。";
        });
    }

    /// <summary>按系统、名称和状态筛选权限并返回分页结果。</summary>
    /// <param name="req">权限分页查询请求。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(PagedPermissionRequest req, CancellationToken ct)
    {
        var query = new PermissionPagedQuery(
            req.SystemId,
            req.Name,
            req.IsEnabled,
            req.GetSkipCount(),
            req.PageSize,
            req.IsExport);
        var result = await permissionQuery.GetPagedAsync(query, ct);

        await Send.OkAsync(new PagedPermissionResponse
        {
            TotalCount = result.TotalCount,
            Items = result.Items
                .Select(item => new PagedPermissionRowResponse(
                    item.Id,
                    item.SystemId,
                    item.SystemName,
                    item.Name,
                    item.Description,
                    item.IsEnabled))
                .ToList()
        }, ct);
    }
}
