using Volo.Abp;

namespace DedsiNative.Dictionaries;

/// <summary>
/// 字典项子实体，只能由所属字典聚合维护。
/// </summary>
public sealed class DictionaryItem
{
    /// <summary>
    /// 供 ORM 实例化使用。
    /// </summary>
    private DictionaryItem()
    {
    }

    /// <summary>
    /// 创建字典项。
    /// </summary>
    /// <param name="id">字典项的 26 位 ULID。</param>
    /// <param name="dictionaryId">所属字典分组的 26 位 ULID。</param>
    /// <param name="code">业务编码。</param>
    /// <param name="name">显示名称。</param>
    /// <param name="description">说明，可为空。</param>
    /// <param name="sort">展示排序。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="isDefault">是否为默认项。</param>
    /// <param name="parentId">父字典项 ID，可为空。</param>
    internal DictionaryItem(
        string id,
        string dictionaryId,
        string code,
        string name,
        string? description,
        int sort,
        bool isEnabled,
        bool isDefault,
        string? parentId)
    {
        Id = id;
        DictionaryId = dictionaryId;
        Change(
            code,
            name,
            description,
            sort,
            isEnabled,
            isDefault,
            parentId);
    }

    /// <summary>
    /// 字典项唯一标识。
    /// </summary>
    public string Id { get; private set; } = string.Empty;

    /// <summary>
    /// 所属字典分组标识。
    /// </summary>
    public string DictionaryId { get; private set; } = string.Empty;

    /// <summary>
    /// 字典项业务编码。
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// 字典项显示名称。
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 字典项说明。
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 字典项展示排序，数值越小越靠前。
    /// </summary>
    public int Sort { get; private set; }

    /// <summary>
    /// 字典项是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; }

    /// <summary>
    /// 字典项是否为所属分组的默认项。
    /// </summary>
    public bool IsDefault { get; private set; }

    /// <summary>
    /// 父字典项标识，为空表示顶级项。
    /// </summary>
    public string? ParentId { get; private set; }

    /// <summary>
    /// 修改字典项资料。
    /// </summary>
    /// <param name="code">字典项编码，同一分组内唯一。</param>
    /// <param name="name">字典项名称。</param>
    /// <param name="description">补充说明，可为空。</param>
    /// <param name="sort">显示排序值。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="isDefault">是否设为默认项；停用项不会保留默认标记。</param>
    /// <param name="parentId">父级标识；为空表示根节点。</param>
    internal void Change(
        string code,
        string name,
        string? description,
        int sort,
        bool isEnabled,
        bool isDefault,
        string? parentId)
    {
        Code = Check.NotNullOrWhiteSpace(
            code,
            nameof(code),
            DictionaryConsts.MaxCodeLength).Trim();
        Name = Check.NotNullOrWhiteSpace(
            name,
            nameof(name),
            DictionaryConsts.MaxItemNameLength).Trim();
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : Check.NotNullOrWhiteSpace(
            description,
            nameof(description),
            DictionaryConsts.MaxDescriptionLength).Trim();
        Sort = sort;
        IsEnabled = isEnabled;
        IsDefault = isEnabled && isDefault;
        ParentId = parentId is null ? null : parentId;
    }

    /// <summary>
    /// 设置默认标记；停用项不会成为默认项。
    /// </summary>
    /// <param name="isDefault">是否为默认项。</param>
    internal void SetDefault(bool isDefault)
    {
        IsDefault = IsEnabled && isDefault;
    }

}
