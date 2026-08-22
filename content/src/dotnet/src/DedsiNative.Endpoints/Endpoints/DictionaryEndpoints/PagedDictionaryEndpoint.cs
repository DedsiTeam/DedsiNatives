using Dedsi.Ddd.Application.Contracts.Dtos;
using DedsiNative.Dictionaries;
using FastEndpoints;

namespace DedsiNative.Endpoints.DictionaryEndpoints;

/// <summary>
/// 字典分页查询请求。
/// </summary>
public sealed class PagedDictionaryRequest : DedsiPagedRequestDto
{
    /// <summary>
    /// 所属系统筛选条件。
    /// </summary>
    public string? SystemId { get; set; }

    /// <summary>
    /// 字典分组名称筛选条件。
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// 字典分页列表行响应。
/// </summary>
/// <param name="Id">字典分组标识。</param>
/// <param name="SystemId">所属系统标识。</param>
/// <param name="SystemName">所属系统名称快照。</param>
/// <param name="Name">字典分组名称。</param>
/// <param name="ItemCount">字典项数量。</param>
public sealed record DictionaryRowResponse(
    string Id,
    string SystemId,
    string SystemName,
    string Name,
    int ItemCount);

/// <summary>
/// 字典分页查询响应。
/// </summary>
public sealed class PagedDictionaryResponse : DedsiPagedResultDto<DictionaryRowResponse>;

/// <summary>
/// 字典分页查询端点。
/// </summary>
/// <param name="dictionaryQuery">字典只读查询服务。</param>
public sealed class PagedDictionaryEndpoint(IDictionaryQuery dictionaryQuery)
    : Endpoint<PagedDictionaryRequest, PagedDictionaryResponse>
{
    /// <summary>
    /// 配置字典分页查询接口。
    /// </summary>
    public override void Configure()
    {
        Post("/api/dictionary/pagedQuery");
        Policies(ManagementPermissions.Dictionaries.View);
        Description(description => description.WithTags("字典管理"));
        Summary(summary =>
        {
            summary.Summary = "分页查询字典分组";
            summary.Description = "按系统和名称筛选字典分组，并返回字典项数量。";
        });
    }

    /// <summary>
    /// 查询并返回分页字典分组。
    /// </summary>
    /// <param name="req">分页查询请求。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(PagedDictionaryRequest req, CancellationToken ct)
    {
        var result = await dictionaryQuery.GetPagedAsync(new DictionaryPagedQuery(
            req.SystemId,
            req.Name,
            req.GetSkipCount(),
            req.PageSize,
            req.IsExport), ct);

        await Send.OkAsync(new PagedDictionaryResponse
        {
            TotalCount = result.TotalCount,
            Items = result.Items.Select(item => new DictionaryRowResponse(
                item.Id,
                item.SystemId,
                item.SystemName,
                item.Name,
                item.ItemCount)).ToList()
        }, ct);
    }
}
