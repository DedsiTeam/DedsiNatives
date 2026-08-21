using DedsiNative.Dictionaries;
using FastEndpoints;

namespace DedsiNative.Endpoints.DictionaryEndpoints;

/// <summary>
/// 获取字典项列表端点。
/// </summary>
/// <param name="dictionaryRepository">字典聚合仓储。</param>
public sealed class GetDictionaryItemsEndpoint(IDictionaryRepository dictionaryRepository)
    : EndpointWithoutRequest<DictionaryItemResponse[]>
{
    /// <summary>
    /// 配置字典项列表接口。
    /// </summary>
    public override void Configure()
    {
        Get("/api/dictionary/{dictionaryId}/items");
        Description(description => description.WithTags("字典管理"));
        Summary(summary => summary.Summary = "获取字典项列表");
    }

    /// <summary>
    /// 加载完整聚合并按层级、排序返回字典项。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var dictionaryId = Route<string>("dictionaryId")!;
        var dictionary = await dictionaryRepository.GetAsync(dictionaryId, true, ct);
        var items = dictionary.Items
            .OrderBy(item => item.ParentId)
            .ThenBy(item => item.Sort)
            .ThenBy(item => item.Id)
            .Select(DictionaryEndpointMappings.ToResponse)
            .ToArray();
        await Send.OkAsync(items, ct);
    }
}
