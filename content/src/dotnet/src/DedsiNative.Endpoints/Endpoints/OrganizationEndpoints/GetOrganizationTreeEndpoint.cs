using DedsiNative.Organizations;
using FastEndpoints;

namespace DedsiNative.Endpoints.OrganizationEndpoints;

/// <summary>
/// 组织机构树节点响应模型。
/// </summary>
/// <param name="Id">组织唯一标识，26 位 ULID。</param>
/// <param name="SystemId">所属系统标识。</param>
/// <param name="SystemName">所属系统名称。</param>
/// <param name="Code">组织机构编码。</param>
/// <param name="Name">组织机构主名称。</param>
/// <param name="Name1">组织机构名称 1。</param>
/// <param name="Name2">组织机构名称 2。</param>
/// <param name="Name3">组织机构名称 3。</param>
/// <param name="Name4">组织机构名称 4。</param>
/// <param name="ParentId">父级组织标识。</param>
/// <param name="Sort">同级排序序号。</param>
/// <param name="Level">组织层级深度。</param>
/// <param name="IsEnabled">是否启用。</param>
/// <param name="Description">组织说明。</param>
/// <param name="Children">下级子组织列表。</param>
public sealed record OrganizationTreeNodeResponse(
    string Id,
    string SystemId,
    string SystemName,
    string Code,
    string Name,
    string? Name1,
    string? Name2,
    string? Name3,
    string? Name4,
    string? ParentId,
    int Sort,
    int Level,
    bool IsEnabled,
    string? Description,
    IReadOnlyList<OrganizationTreeNodeResponse>? Children);

/// <summary>
/// 查询指定系统下的多级组织机构树端点。
/// </summary>
public sealed class GetOrganizationTreeEndpoint(IOrganizationQuery organizationQuery)
    : EndpointWithoutRequest<IReadOnlyList<OrganizationTreeNodeResponse>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Get("/api/organization/tree/{systemId}");
        Description(d => d.WithTags("组织机构管理"));
        Summary(s =>
        {
            s.Summary = "获取组织机构树";
            s.Description = "获取指定系统下的所有多级组织机构树结构，按层级与同级排序严格升序排列。";
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        var systemId = Route<string>("systemId");
        var list = await organizationQuery.GetTreeListAsync(new OrganizationTreeQuery(systemId), ct);

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
