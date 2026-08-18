using DedsiNative.Dictionaries;

namespace DedsiNative.Endpoints.DictionaryEndpoints;

/// <summary>
/// 字典项响应。
/// </summary>
/// <param name="Id">字典项标识。</param>
/// <param name="DictionaryId">所属字典分组标识。</param>
/// <param name="Code">业务编码。</param>
/// <param name="Name">显示名称。</param>
/// <param name="Description">说明。</param>
/// <param name="Sort">展示排序。</param>
/// <param name="IsEnabled">是否启用。</param>
/// <param name="IsDefault">是否为默认项。</param>
/// <param name="ParentId">父字典项标识。</param>
public sealed record DictionaryItemResponse(
    string Id,
    string DictionaryId,
    string Code,
    string Name,
    string? Description,
    int Sort,
    bool IsEnabled,
    bool IsDefault,
    string? ParentId);

/// <summary>
/// 字典端点响应映射。
/// </summary>
internal static class DictionaryEndpointMappings
{
    /// <summary>
    /// 将领域字典项映射为安全响应 DTO。
    /// </summary>
    /// <param name="item">领域字典项。</param>
    /// <returns>字典项响应。</returns>
    internal static DictionaryItemResponse ToResponse(DictionaryItem item)
    {
        return new DictionaryItemResponse(
            item.Id,
            item.DictionaryId,
            item.Code,
            item.Name,
            item.Description,
            item.Sort,
            item.IsEnabled,
            item.IsDefault,
            item.ParentId);
    }
}
