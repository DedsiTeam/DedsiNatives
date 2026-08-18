using DedsiNative.Dictionaries;
using FastEndpoints;

namespace DedsiNative.Endpoints.DictionaryEndpoints;

/// <summary>
/// 获取字典项详情端点。
/// </summary>
/// <param name="dictionaryRepository">字典聚合仓储。</param>
public sealed class GetDictionaryItemEndpoint(IDictionaryRepository dictionaryRepository)
    : EndpointWithoutRequest<DictionaryItemResponse>
{
    /// <summary>
    /// 配置字典项详情接口。
    /// </summary>
    public override void Configure()
    {
        Get("/api/dictionary/{dictionaryId}/item/{itemId}");
        Description(description => description.WithTags("字典管理"));
        Summary(summary => summary.Summary = "获取字典项详情");
    }

    /// <summary>
    /// 从完整字典聚合中返回指定字典项。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var dictionaryId = Route<string>("dictionaryId")!;
        var itemId = Route<string>("itemId")!;
        var dictionary = await dictionaryRepository.GetAsync(dictionaryId, true, ct);
        var item = dictionary.Items.FirstOrDefault(candidate => candidate.Id == itemId);
        if (item is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(DictionaryEndpointMappings.ToResponse(item), ct);
    }
}
