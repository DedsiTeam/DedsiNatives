using DedsiNative.Dictionaries;
using FastEndpoints;

namespace DedsiNative.Endpoints.DictionaryEndpoints;

/// <summary>
/// 创建字典项请求。
/// </summary>
public sealed class CreateDictionaryItemRequest
{
    /// <summary>业务编码。</summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>显示名称。</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>说明。</summary>
    public string? Description { get; set; }
    /// <summary>展示排序。</summary>
    public int Sort { get; set; }
    /// <summary>是否启用。</summary>
    public bool IsEnabled { get; set; } = true;
    /// <summary>是否为默认项。</summary>
    public bool IsDefault { get; set; }
    /// <summary>父字典项标识。</summary>
    public string? ParentId { get; set; }
}

/// <summary>
/// 创建字典项响应。
/// </summary>
/// <param name="Id">新字典项标识。</param>
public sealed record CreateDictionaryItemResponse(string Id);

/// <summary>
/// 创建字典项端点。
/// </summary>
/// <param name="dictionaryRepository">字典聚合仓储。</param>
public sealed class CreateDictionaryItemEndpoint(IDictionaryRepository dictionaryRepository)
    : Endpoint<CreateDictionaryItemRequest, CreateDictionaryItemResponse>
{
    /// <summary>
    /// 配置创建字典项接口。
    /// </summary>
    public override void Configure()
    {
        Post("/api/dictionary/{dictionaryId}/item/create");
        Description(description => description.WithTags("字典管理"));
        Summary(summary => summary.Summary = "创建字典项");
    }

    /// <summary>
    /// 加载完整字典聚合并添加字典项。
    /// </summary>
    /// <param name="req">创建请求。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(CreateDictionaryItemRequest req, CancellationToken ct)
    {
        var dictionaryId = Route<string>("dictionaryId")!;
        var dictionary = await dictionaryRepository.GetAsync(dictionaryId, true, ct);
        var itemId = Ulid.NewUlid().ToString();
        dictionary.AddItem(
            itemId,
            req.Code,
            req.Name,
            req.Description,
            req.Sort,
            req.IsEnabled,
            req.IsDefault,
            req.ParentId);

        await dictionaryRepository.UpdateAsync(dictionary, true, ct);
        await Send.OkAsync(new CreateDictionaryItemResponse(itemId), ct);
    }
}
