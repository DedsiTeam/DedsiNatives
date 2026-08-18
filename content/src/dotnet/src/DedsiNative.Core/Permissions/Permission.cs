using Dedsi.Ddd.Domain.Entities;
using DedsiNative.Permissions.Events;
using Volo.Abp;

namespace DedsiNative.Permissions;

/// <summary>权限聚合根，负责维护系统归属、权限名称、说明和启用状态。</summary>
public class Permission : DedsiAggregateRoot<string>
{
    /// <summary>供 ORM 框架反射创建实体的受保护构造函数。</summary>
    protected Permission()
    {
    }

    /// <summary>创建权限聚合根。</summary>
    /// <param name="id">权限唯一标识，必须是 26 位 ULID。</param>
    /// <param name="systemId">所属系统 ID，必须是 26 位 ULID。</param>
    /// <param name="systemName">所属系统名称快照。</param>
    /// <param name="name">权限名称，不能为空。</param>
    /// <param name="description">权限说明，可为空。</param>
    /// <param name="isEnabled">是否启用，默认启用。</param>
    public Permission(
        string id,
        string systemId,
        string systemName,
        string name,
        string? description = null,
        bool isEnabled = true) : base(ValidateUlid(id, nameof(id)))
    {
        ChangeSystem(systemId, systemName);
        ChangeName(name);
        ChangeDescription(description);
        IsEnabled = isEnabled;
    }

    /// <summary>所属系统 ID。</summary>
    public string SystemId { get; private set; } = string.Empty;

    /// <summary>所属系统名称快照。</summary>
    public string SystemName { get; private set; } = string.Empty;

    /// <summary>权限名称。</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>权限说明。</summary>
    public string? Description { get; private set; }

    /// <summary>权限是否启用。</summary>
    public bool IsEnabled { get; private set; }

    /// <summary>修改权限所属系统和系统名称快照。</summary>
    /// <param name="systemId">新的系统 ID。</param>
    /// <param name="systemName">新的系统名称。</param>
    /// <returns>当前权限聚合根。</returns>
    public Permission ChangeSystem(string systemId, string systemName)
    {
        SystemId = ValidateUlid(systemId, nameof(systemId));
        SystemName = Check.NotNullOrWhiteSpace(systemName, nameof(systemName), PermissionConsts.MaxNameLength);
        return this;
    }

    /// <summary>
    /// 修改权限名称，并在已有权限实际变更时发布名称变更事件。
    /// </summary>
    /// <param name="name">新的权限名称，不能为空。</param>
    /// <returns>当前权限聚合根。</returns>
    public Permission ChangeName(string name)
    {
        var oldName = Name;
        var newName = Check.NotNullOrWhiteSpace(name, nameof(name), PermissionConsts.MaxNameLength);
        Name = newName;
        if (!string.IsNullOrEmpty(oldName)
            && !string.Equals(oldName, newName, StringComparison.Ordinal))
        {
            AddLocalEvent(new PermissionNameChangedEvent(Id, oldName, newName, SystemId));
        }

        return this;
    }

    /// <summary>修改权限说明。</summary>
    /// <param name="description">新的权限说明，可为空。</param>
    /// <returns>当前权限聚合根。</returns>
    public Permission ChangeDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : Check.NotNullOrWhiteSpace(description, nameof(description), PermissionConsts.MaxDescriptionLength);
        return this;
    }

    /// <summary>启用权限。</summary>
    /// <returns>当前权限聚合根。</returns>
    public Permission Enable()
    {
        IsEnabled = true;
        return this;
    }

    /// <summary>停用权限。</summary>
    /// <returns>当前权限聚合根。</returns>
    public Permission Disable()
    {
        IsEnabled = false;
        return this;
    }

    private static string ValidateUlid(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 26 || !Ulid.TryParse(value, out _))
        {
            throw new ArgumentException("标识必须是合法的 26 位 ULID。", parameterName);
        }

        return value;
    }
}
