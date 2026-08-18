using DedsiNative.Dictionaries;
using FastEndpoints;

namespace DedsiNative.Endpoints.DictionaryEndpoints;

/// <summary>
/// 字典分组详情响应。
/// </summary>
/// <param name="Id">字典分组标识。</param>
/// <param name="SystemId">所属系统标识。</param>
/// <param name="SystemName">所属系统名称快照。</param>
/// <param name="Name">字典分组名称。</param>
/// <param name="Items">按层级和排序返回的字典项。</param>
public sealed record GetDictionaryResponse(
    string Id,
    string SystemId,
    string SystemName,
    string Name,
    IReadOnlyList<DictionaryItemResponse> Items);

/// <summary>
/// 获取字典分组详情端点。
/// </summary>
/// <param name="dictionaryRepository">字典聚合仓储。</param>
public sealed class GetDictionaryEndpoint(IDictionaryRepository dictionaryRepository)
    : EndpointWithoutRequest<GetDictionaryResponse>
{
    /// <summary>
    /// 配置字典分组详情接口。
    /// </summary>
    public override void Configure()
    {
        Get("/api/dictionary/{id}");
        Description(description => description.WithTags("字典管理"));
        Summary(summary =>
        {
            summary.Summary = "获取字典分组详情";
            summary.Description = "返回字典分组及其全部字典项。";
        });
    }

    /// <summary>
    /// 通过仓储加载完整字典聚合并返回详情。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id")!;
        var dictionary = await dictionaryRepository.GetAsync(id, true, ct);
        await Send.OkAsync(new GetDictionaryResponse(
            dictionary.Id,
            dictionary.SystemId,
            dictionary.SystemName,
            dictionary.Name,
            dictionary.Items
                .OrderBy(item => item.ParentId)
                .ThenBy(item => item.Sort)
                .ThenBy(item => item.Id)
                .Select(DictionaryEndpointMappings.ToResponse)
                .ToList()), ct);
    }
}
