using DedsiNative.Organizations;
using FastEndpoints;

namespace DedsiNative.Endpoints.OrganizationEndpoints;

/// <summary>
/// 用户表单组织机构下拉选项树节点模型。
/// </summary>
/// <param name="Value">组织机构唯一标识（ULID）。</param>
/// <param name="Title">组织机构主名称。</param>
/// <param name="Children">子级选项列表，无子级时为空。</param>
public sealed record UserOrganizationOptionNodeResponse(
    string Value,
    string Title,
    UserOrganizationOptionNodeResponse[]? Children);

/// <summary>
/// 为创建/编辑用户选择组织机构提供专用下拉树数据的端点。
/// </summary>
public sealed class GetUserOrganizationOptionsEndpoint(IOrganizationQuery organizationQuery)
    : EndpointWithoutRequest<UserOrganizationOptionNodeResponse[]>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Get("/api/organization/user-options");
        Description(d => d.WithTags("组织机构管理"));
        Summary(s =>
        {
            s.Summary = "获取用户选择组织机构选项树";
            s.Description = "提供仅包含已启用组织机构的精简树形结构，专用于创建/编辑用户时的多选组织下拉组件。";
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        // 仅查询已启用的组织机构
        var list = await organizationQuery.GetTreeListAsync(new OrganizationTreeQuery(null, IsEnabled: true), ct);

        var tree = BuildOptionTree(list, null);
        await Send.OkAsync(tree, ct);
    }

    private static UserOrganizationOptionNodeResponse[] BuildOptionTree(
        OrganizationQueryItem[] items,
        string? parentId)
    {
        var result = new List<UserOrganizationOptionNodeResponse>();
        var children = items.Where(x => x.ParentId == parentId).OrderBy(x => x.Sort).ThenBy(x => x.Id);

        foreach (var child in children)
        {
            var grandChildren = BuildOptionTree(items, child.Id);
            result.Add(new UserOrganizationOptionNodeResponse(
                child.Id,
                child.Name,
                grandChildren.Length > 0 ? grandChildren : null));
        }

        return result.ToArray();
    }
}
