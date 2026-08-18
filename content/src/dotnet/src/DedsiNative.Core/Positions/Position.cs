using Dedsi.Ddd.Domain.Entities;
using DedsiNative.Permissions;
using Volo.Abp;

namespace DedsiNative.Positions;

/// <summary>岗位聚合根，负责维护岗位资料、状态及权限和组织机构关联。</summary>
public class Position : DedsiAggregateRoot<string>
{
    /// <summary>供 ORM 框架反射创建实体的受保护构造函数。</summary>
    protected Position()
    {
    }

    /// <summary>创建岗位聚合根。</summary>
    /// <param name="id">岗位唯一标识，必须是 26 位 ULID。</param>
    /// <param name="name">岗位名称。</param>
    /// <param name="systemId">所属系统 ID，必须是 26 位 ULID。</param>
    /// <param name="systemName">所属系统名称快照。</param>
    /// <param name="description">岗位说明，可为空。</param>
    /// <param name="isEnabled">是否启用，默认启用。</param>
    public Position(string id, string name, string systemId, string systemName,
        string? description = null, bool isEnabled = true) : base(ValidateUlid(id, nameof(id)))
    {
        ChangeName(name);
        ChangeSystem(systemId, systemName);
        ChangeDescription(description);
        IsEnabled = isEnabled;
    }

    /// <summary>岗位名称。</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>所属系统 ID。</summary>
    public string SystemId { get; private set; } = string.Empty;

    /// <summary>所属系统名称快照。</summary>
    public string SystemName { get; private set; } = string.Empty;

    /// <summary>岗位说明。</summary>
    public string? Description { get; private set; }

    /// <summary>岗位是否启用。</summary>
    public bool IsEnabled { get; private set; }

    /// <summary>岗位权限子实体集合。</summary>
    public ICollection<PositionPermission> Permissions { get; private set; } = [];

    /// <summary>岗位组织机构子实体集合。</summary>
    public ICollection<PositionOrganization> Organizations { get; private set; } = [];

    /// <summary>修改岗位名称。</summary>
    public Position ChangeName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), PositionConsts.MaxNameLength);
        return this;
    }

    /// <summary>修改岗位所属系统和名称快照。</summary>
    public Position ChangeSystem(string systemId, string systemName)
    {
        SystemId = ValidateUlid(systemId, nameof(systemId));
        SystemName = Check.NotNullOrWhiteSpace(systemName, nameof(systemName), PositionConsts.MaxNameLength);
        return this;
    }

    /// <summary>修改岗位说明。</summary>
    public Position ChangeDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : Check.NotNullOrWhiteSpace(description, nameof(description), PositionConsts.MaxDescriptionLength);
        return this;
    }

    /// <summary>启用岗位。</summary>
    public Position Enable() { IsEnabled = true; return this; }

    /// <summary>停用岗位。</summary>
    public Position Disable() { IsEnabled = false; return this; }

    /// <summary>增加岗位权限关联。</summary>
    public Position AddPermission(string permissionId, string permissionName, string systemId, string systemName)
    {
        if (systemId != SystemId) throw new ArgumentException("权限所属系统必须与岗位系统一致。", nameof(systemId));
        var normalizedPermissionId = ValidateUlid(permissionId, nameof(permissionId));
        if (Permissions.Any(item => item.PermissionId == normalizedPermissionId))
        {
            throw new ArgumentException("岗位已关联该权限。", nameof(permissionId));
        }

        Permissions.Add(new PositionPermission(Id, normalizedPermissionId, permissionName, systemId, systemName));
        return this;
    }

    /// <summary>移除岗位权限关联。</summary>
    public Position RemovePermission(string permissionId)
    {
        foreach (var permission in Permissions
                     .Where(item => item.PermissionId == permissionId)
                     .ToList())
        {
            Permissions.Remove(permission);
        }
        return this;
    }

    /// <summary>清空岗位权限关联。</summary>
    public Position ClearPermissions() { Permissions.Clear(); return this; }

    /// <summary>
    /// 更新指定权限在岗位中的名称快照。
    /// </summary>
    /// <param name="permissionId">权限唯一标识。</param>
    /// <param name="permissionName">新的权限名称快照。</param>
    /// <returns>当前岗位聚合根。</returns>
    public Position ChangePermissionName(string permissionId, string permissionName)
    {
        var normalizedPermissionId = ValidateUlid(permissionId, nameof(permissionId));
        foreach (var permission in Permissions.Where(item => item.PermissionId == normalizedPermissionId))
        {
            permission.ChangeName(permissionName);
        }

        return this;
    }

    /// <summary>增加岗位组织机构关联。</summary>
    public Position AddOrganization(string organizationId, string organizationName)
    {
        var normalizedOrganizationId = ValidateUlid(organizationId, nameof(organizationId));
        if (Organizations.Any(item => item.OrganizationId == normalizedOrganizationId))
        {
            throw new ArgumentException("岗位已关联该组织机构。", nameof(organizationId));
        }

        Organizations.Add(new PositionOrganization(Id, normalizedOrganizationId, organizationName));
        return this;
    }

    /// <summary>移除岗位组织机构关联。</summary>
    public Position RemoveOrganization(string organizationId)
    {
        Organizations.RemoveAll(item => item.OrganizationId == organizationId);
        return this;
    }

    /// <summary>清空岗位组织机构关联。</summary>
    public Position ClearOrganizations() { Organizations.Clear(); return this; }

    private static string ValidateUlid(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 26 || !Ulid.TryParse(value, out _))
        {
            throw new ArgumentException("标识必须是合法的 26 位 ULID。", parameterName);
        }

        return value;
    }
}

/// <summary>岗位权限子实体。</summary>
public sealed class PositionPermission
{
    private PositionPermission() { }

    /// <summary>创建岗位权限关联。</summary>
    public PositionPermission(string positionId, string permissionId, string permissionName, string systemId, string systemName)
    {
        PositionId = positionId;
        PermissionId = permissionId;
        PermissionName = Check.NotNullOrWhiteSpace(permissionName, nameof(permissionName), PermissionConsts.MaxNameLength);
        SystemId = systemId;
        SystemName = Check.NotNullOrWhiteSpace(systemName, nameof(systemName), PermissionConsts.MaxNameLength);
    }

    /// <summary>所属岗位 ID。</summary>
    public string PositionId { get; private set; } = string.Empty;
    /// <summary>权限 ID。</summary>
    public string PermissionId { get; private set; } = string.Empty;
    /// <summary>权限名称快照。</summary>
    public string PermissionName { get; private set; } = string.Empty;
    /// <summary>权限所属系统 ID。</summary>
    public string SystemId { get; private set; } = string.Empty;
    /// <summary>权限所属系统名称快照。</summary>
    public string SystemName { get; private set; } = string.Empty;

    /// <summary>
    /// 更新权限名称快照，仅供所属岗位聚合行为调用。
    /// </summary>
    /// <param name="permissionName">新的权限名称。</param>
    internal void ChangeName(string permissionName)
    {
        PermissionName = Check.NotNullOrWhiteSpace(
            permissionName,
            nameof(permissionName),
            PermissionConsts.MaxNameLength);
    }
}

/// <summary>岗位组织机构子实体。</summary>
public sealed class PositionOrganization
{
    private PositionOrganization() { }

    /// <summary>创建岗位组织机构关联。</summary>
    public PositionOrganization(string positionId, string organizationId, string organizationName)
    {
        PositionId = positionId;
        OrganizationId = organizationId;
        OrganizationName = Check.NotNullOrWhiteSpace(organizationName, nameof(organizationName), 128);
    }

    /// <summary>所属岗位 ID。</summary>
    public string PositionId { get; private set; } = string.Empty;
    /// <summary>组织机构 ID。</summary>
    public string OrganizationId { get; private set; } = string.Empty;
    /// <summary>组织机构名称快照。</summary>
    public string OrganizationName { get; private set; } = string.Empty;
}
