using DedsiNative.Organizations;
using FastEndpoints;

namespace DedsiNative.Endpoints.OrganizationEndpoints;

/// <summary>
/// 查询所有组织机构构成的完整组织机构树端点。
/// </summary>
public sealed class GetAllOrganizationTreeEndpoint(IOrganizationQuery organizationQuery)
    : EndpointWithoutRequest<IReadOnlyList<OrganizationTreeNodeResponse>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Get("/api/organization/all-tree", "/api/organization/tree");
        Description(d => d.WithTags("组织机构管理"));
        Summary(s =>
        {
            s.Summary = "获取全部组织机构树";
            s.Description = "获取系统内所有组织机构，并自动组装为多级组织机构树结构返回。";
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        // 传入 null 查询全局所有组织机构
        var list = await organizationQuery.GetTreeListAsync(new OrganizationTreeQuery(null), ct);

        var tree = BuildTree(list, null);
        await Send.OkAsync(tree, ct);
    }

    private static List<OrganizationTreeNodeResponse> BuildTree(
        IReadOnlyList<OrganizationQueryItem> items,
        string? parentId)
    {
        var result = new List<OrganizationTreeNodeResponse>();
        var children = items.Where(x => x.ParentId == parentId).OrderBy(x => x.Sort).ThenBy(x => x.Id);

        foreach (var child in children)
        {
            var grandChildren = BuildTree(items, child.Id);
            result.Add(new OrganizationTreeNodeResponse(
                child.Id,
                child.SystemId,
                child.SystemName,
                child.Code,
                child.Name,
                child.Name1,
                child.Name2,
                child.Name3,
                child.Name4,
                child.ParentId,
                child.Sort,
                child.Level,
                child.IsEnabled,
                child.Description,
                grandChildren.Count > 0 ? grandChildren : null));
        }

        return result;
    }
}
