using Dedsi.Ddd.Domain.Entities;
using Volo.Abp;

namespace DedsiNative.Dictionaries;

/// <summary>
/// 字典聚合根，负责维护系统归属、分组名称和内部字典项。
/// </summary>
public class Dictionary : DedsiAggregateRoot<string>
{
    /// <summary>
    /// 供 ORM 反射实例化的受保护构造函数。
    /// </summary>
    protected Dictionary()
    {
    }

    /// <summary>
    /// 创建字典分组
    /// </summary>
    /// <param name="id">字典分组唯一标识。</param>
    /// <param name="systemId">所属系统标识。</param>
    /// <param name="systemName">所属系统名称快照。</param>
    /// <param name="name">字典名称。</param>
    public Dictionary(
        string id,
        string systemId,
        string systemName,
        string name)
        : base(id)
    {
        ChangeSystem(systemId, systemName);
        ChangeName(name);
    }

    /// <summary>
    /// 所属系统标识。
    /// </summary>
    public string SystemId { get; private set; } = string.Empty;

    /// <summary>
    /// 所属系统名称快照。
    /// </summary>
    public string SystemName { get; private set; } = string.Empty;

    /// <summary>
    /// 字典分组名称。
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 字典项集合。
    /// </summary>
    public ICollection<DictionaryItem> Items { get; private set; } = [];

    /// <summary>
    /// 修改字典分组名称。
    /// </summary>
    /// <param name="name">新的分组名称。</param>
    /// <returns>当前字典聚合根。</returns>
    public Dictionary ChangeName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(
            name,
            nameof(name),
            DictionaryConsts.MaxNameLength).Trim();
        return this;
    }

    /// <summary>
    /// 修改所属系统和系统名称快照。
    /// </summary>
    /// <param name="systemId">新的系统标识。</param>
    /// <param name="systemName">新的系统名称快照。</param>
    /// <returns>当前字典聚合根。</returns>
    public Dictionary ChangeSystem(
        string systemId,
        string systemName)
    {
        SystemId = systemId;
        SystemName = Check.NotNullOrWhiteSpace(
            systemName,
            nameof(systemName),
            DictionaryConsts.MaxSystemNameLength).Trim();
        return this;
    }

    /// <summary>
    /// 添加字典项。
    /// </summary>
    /// <param name="itemId">字典项的 26 位 ULID。</param>
    /// <param name="code">业务编码。</param>
    /// <param name="name">显示名称。</param>
    /// <param name="description">说明。</param>
    /// <param name="sort">展示排序。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="isDefault">是否为默认项。</param>
    /// <param name="parentId">父字典项标识。</param>
    /// <returns>当前字典聚合根。</returns>
    public Dictionary AddItem(
        string itemId,
        string code,
        string name,
        string? description,
        int sort,
        bool isEnabled,
        bool isDefault,
        string? parentId)
    {
        var normalizedItemId = itemId;
        EnsureCodeAvailable(code, null);
        ValidateParent(normalizedItemId, parentId);
        if (isEnabled && isDefault)
        {
            ClearDefaultItem();
        }
        Items.Add(new DictionaryItem(
            normalizedItemId,
            Id,
            code,
            name,
            description,
            sort,
            isEnabled,
            isDefault,
            parentId));
        return this;
    }

    /// <summary>
    /// 修改指定字典项。
    /// </summary>
    /// <param name="itemId">待修改的字典项标识。</param>
    /// <param name="code">业务编码。</param>
    /// <param name="name">显示名称。</param>
    /// <param name="description">说明。</param>
    /// <param name="sort">展示排序。</param>
    /// <param name="isEnabled">是否启用。</param>
    /// <param name="isDefault">是否为默认项。</param>
    /// <param name="parentId">父字典项标识。</param>
    /// <returns>当前字典聚合根。</returns>
    public Dictionary ChangeItem(
        string itemId,
        string code,
        string name,
        string? description,
        int sort,
        bool isEnabled,
        bool isDefault,
        string? parentId)
    {
        var item = GetItem(itemId);
        EnsureCodeAvailable(code, item.Id);
        ValidateParent(item.Id, parentId);
        if (isEnabled && isDefault)
        {
            ClearDefaultItem(item.Id);
        }
        item.Change(
            code,
            name,
            description,
            sort,
            isEnabled,
            isDefault,
            parentId);
        return this;
    }

    /// <summary>
    /// 取得当前字典分组中的指定字典项；不存在时拒绝操作。
    /// </summary>
    /// <param name="itemId">当前字典分组内的字典项标识。</param>
    /// <returns>属于当前字典分组的字典项。</returns>
    private DictionaryItem GetItem(string itemId)
    {
        var normalizedItemId = itemId;
        return Items.FirstOrDefault(item => item.Id == normalizedItemId)
            ?? throw new ArgumentException("字典项不属于当前字典分组。", nameof(itemId));
    }

    /// <summary>
    /// 校验去除首尾空白后的编码在当前字典分组内唯一。
    /// </summary>
    /// <param name="code">字典项编码，同一分组内唯一。</param>
    /// <param name="excludedItemId">需要排除的字典项标识；为空时不排除任何项。</param>
    private void EnsureCodeAvailable(
        string code,
        string? excludedItemId)
    {
        var normalizedCode = Check.NotNullOrWhiteSpace(
            code,
            nameof(code),
            DictionaryConsts.MaxCodeLength).Trim();
        if (Items.Any(item => item.Id != excludedItemId && item.Code == normalizedCode))
        {
            throw new ArgumentException("同一字典分组内的字典项编码不能重复。", nameof(code));
        }
    }

    /// <summary>
    /// 校验父项属于当前字典分组，并阻止自引用和层级循环。
    /// </summary>
    /// <param name="itemId">当前字典分组内的字典项标识。</param>
    /// <param name="parentId">父级标识；为空表示根节点。</param>
    private void ValidateParent(
        string itemId,
        string? parentId)
    {
        if (parentId is null)
        {
            return;
        }
        var normalizedParentId = parentId;
        if (normalizedParentId == itemId)
        {
            throw new ArgumentException("字典项不能将自身设为父项。", nameof(parentId));
        }
        var parent = Items.FirstOrDefault(item => item.Id == normalizedParentId)
            ?? throw new ArgumentException("父字典项必须属于当前字典分组。", nameof(parentId));
        var visited = new HashSet<string>(StringComparer.Ordinal) { itemId };
        while (parent is not null)
        {
            if (!visited.Add(parent.Id))
            {
                throw new ArgumentException("字典项层级不能形成环。", nameof(parentId));
            }
            parent = parent.ParentId is null
                ? null
                : Items.FirstOrDefault(item => item.Id == parent.ParentId);
        }
    }

    /// <summary>
    /// 清除其他字典项的默认标记，可保留指定项。
    /// </summary>
    /// <param name="excludedItemId">需要排除的字典项标识；为空时不排除任何项。</param>
    private void ClearDefaultItem(string? excludedItemId = null)
    {
        foreach (var item in Items.Where(item => item.Id != excludedItemId && item.IsDefault))
        {
            item.SetDefault(false);
        }
    }

}
