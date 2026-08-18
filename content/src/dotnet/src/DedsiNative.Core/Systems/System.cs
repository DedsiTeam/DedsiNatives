using Dedsi.Ddd.Domain.Entities;
using Volo.Abp;

namespace DedsiNative.Systems;

/// <summary>
/// 系统聚合根，负责维护系统名称、说明和展示排序。
/// </summary>
public class System : DedsiAggregateRoot<string>
{
    /// <summary>供 ORM 框架反射创建实体的受保护构造函数。</summary>
    protected System()
    {
    }

    /// <summary>创建系统聚合根。</summary>
    /// <param name="id">系统唯一标识，必须是 26 位 ULID 字符串。</param>
    /// <param name="name">系统名称，不能为空。</param>
    /// <param name="description">系统说明，可为空。</param>
    /// <param name="sort">展示排序，数值越小越靠前。</param>
    public System(string id, string name, string? description = null, int sort = 0) : base(ValidateId(id))
    {
        ChangeName(name);
        ChangeDescription(description);
        ChangeSort(sort);
    }

    /// <summary>系统名称。</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>系统说明。</summary>
    public string? Description { get; private set; }

    /// <summary>系统展示排序，数值越小越靠前。</summary>
    public int Sort { get; private set; }

    /// <summary>修改系统名称。</summary>
    /// <param name="name">新的系统名称，不能为空。</param>
    /// <returns>当前系统聚合根。</returns>
    public System ChangeName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), SystemConsts.MaxNameLength);
        return this;
    }

    /// <summary>修改系统说明。</summary>
    /// <param name="description">新的系统说明，可为空。</param>
    /// <returns>当前系统聚合根。</returns>
    public System ChangeDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : Check.NotNullOrWhiteSpace(description, nameof(description), SystemConsts.MaxDescriptionLength);
        return this;
    }

    /// <summary>修改系统展示排序。</summary>
    /// <param name="sort">新的排序值。</param>
    /// <returns>当前系统聚合根。</returns>
    public System ChangeSort(int sort)
    {
        Sort = sort;
        return this;
    }

    private static string ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Length != 26 || !Ulid.TryParse(id, out _))
        {
            throw new ArgumentException("系统 ID 必须是合法的 26 位 ULID。", nameof(id));
        }

        return id;
    }
}
