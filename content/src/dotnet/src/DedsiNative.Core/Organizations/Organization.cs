using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace DedsiNative.Organizations;

/// <summary>
/// 组织机构/部门聚合根，维护系统内树形多级部门与组织架构体系。
/// </summary>
public class Organization : FullAuditedAggregateRoot<string>
{
    /// <summary>
    /// EF Core 所需的无参构造函数。
    /// </summary>
    protected Organization()
    {
    }

    /// <summary>
    /// 初始化组织机构聚合根实例。
    /// </summary>
    /// <param name="id">
    /// 组织机构唯一标识，26 位有序 ULID 字符串。
    /// </param>
    /// <param name="systemId">
    /// 所属系统唯一标识，26 位有序 ULID 字符串。
    /// </param>
    /// <param name="systemName">
    /// 所属系统名称快照。
    /// </param>
    /// <param name="code">
    /// 组织机构编码，系统内唯一。
    /// </param>
    /// <param name="name">
    /// 组织机构名称。
    /// </param>
    /// <param name="name1">
    /// 组织机构名称 1（可选）。
    /// </param>
    /// <param name="name2">
    /// 组织机构名称 2（可选）。
    /// </param>
    /// <param name="name3">
    /// 组织机构名称 3（可选）。
    /// </param>
    /// <param name="name4">
    /// 组织机构名称 4（可选）。
    /// </param>
    /// <param name="parentId">
    /// 父级组织标识，顶级组织为 null。
    /// </param>
    /// <param name="sort">
    /// 同级排序权重序号。
    /// </param>
    /// <param name="level">
    /// 组织层级深度（顶级为 1）。
    /// </param>
    /// <param name="description">
    /// 组织机构职能说明或备注。
    /// </param>
    public Organization(
        string id,
        string systemId,
        string systemName,
        string code,
        string name,
        string? name1 = null,
        string? name2 = null,
        string? name3 = null,
        string? name4 = null,
        string? parentId = null,
        int sort = 0,
        int level = 1,
        string? description = null)
        : base(ValidateUlid(id, nameof(id)))
    {
        SystemId = ValidateUlid(systemId, nameof(systemId));
        SystemName = Check.NotNullOrWhiteSpace(systemName, nameof(systemName), OrganizationConsts.MaxSystemNameLength);
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), OrganizationConsts.MaxCodeLength);
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), OrganizationConsts.MaxNameLength);
        Name1 = Check.Length(name1, nameof(name1), OrganizationConsts.MaxNameLength);
        Name2 = Check.Length(name2, nameof(name2), OrganizationConsts.MaxNameLength);
        Name3 = Check.Length(name3, nameof(name3), OrganizationConsts.MaxNameLength);
        Name4 = Check.Length(name4, nameof(name4), OrganizationConsts.MaxNameLength);
        ParentId = string.IsNullOrWhiteSpace(parentId) ? null : ValidateUlid(parentId, nameof(parentId));
        Sort = sort;
        Level = Math.Max(1, level);
        IsEnabled = true;
        Description = Check.Length(description, nameof(description), OrganizationConsts.MaxDescriptionLength);
    }

    /// <summary>
    /// 所属系统标识，26 位有序 ULID 字符串。
    /// </summary>
    public string SystemId { get; private set; } = string.Empty;

    /// <summary>
    /// 所属系统名称快照。
    /// </summary>
    public string SystemName { get; private set; } = string.Empty;

    /// <summary>
    /// 组织机构编码，同一系统下唯一。
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// 组织机构主名称。
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 组织机构名称 1（可选）。
    /// </summary>
    public string? Name1 { get; private set; }

    /// <summary>
    /// 组织机构名称 2（可选）。
    /// </summary>
    public string? Name2 { get; private set; }

    /// <summary>
    /// 组织机构名称 3（可选）。
    /// </summary>
    public string? Name3 { get; private set; }

    /// <summary>
    /// 组织机构名称 4（可选）。
    /// </summary>
    public string? Name4 { get; private set; }

    /// <summary>
    /// 父级组织机构标识，顶级组织为 null。
    /// </summary>
    public string? ParentId { get; private set; }

    /// <summary>
    /// 同级展示排序权重序号，越小越靠前。
    /// </summary>
    public int Sort { get; private set; }

    /// <summary>
    /// 组织层级深度（顶级节点为 1）。
    /// </summary>
    public int Level { get; private set; }

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; private set; }

    /// <summary>
    /// 组织机构职责说明或备注。
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 更新组织机构的基本业务信息。
    /// </summary>
    /// <param name="name">
    /// 新的组织机构主名称。
    /// </param>
    /// <param name="name1">
    /// 组织机构名称 1。
    /// </param>
    /// <param name="name2">
    /// 组织机构名称 2。
    /// </param>
    /// <param name="name3">
    /// 组织机构名称 3。
    /// </param>
    /// <param name="name4">
    /// 组织机构名称 4。
    /// </param>
    /// <param name="sort">
    /// 同级排序序号。
    /// </param>
    /// <param name="description">
    /// 组织职责描述。
    /// </param>
    /// <returns>
    /// 当前组织机构聚合根实例。
    /// </returns>
    public Organization UpdateInfo(
        string name,
        string? name1,
        string? name2,
        string? name3,
        string? name4,
        int sort,
        string? description)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), OrganizationConsts.MaxNameLength);
        Name1 = Check.Length(name1, nameof(name1), OrganizationConsts.MaxNameLength);
        Name2 = Check.Length(name2, nameof(name2), OrganizationConsts.MaxNameLength);
        Name3 = Check.Length(name3, nameof(name3), OrganizationConsts.MaxNameLength);
        Name4 = Check.Length(name4, nameof(name4), OrganizationConsts.MaxNameLength);
        Sort = sort;
        Description = Check.Length(description, nameof(description), OrganizationConsts.MaxDescriptionLength);
        return this;
    }

    /// <summary>
    /// 调整组织机构的父级节点归属与层级深度。
    /// </summary>
    /// <param name="parentId">
    /// 新的父级组织标识（顶级为 null）。
    /// </param>
    /// <param name="level">
    /// 计算后的层级深度。
    /// </param>
    /// <returns>
    /// 当前组织机构聚合根实例。
    /// </returns>
    public Organization ChangeParent(string? parentId, int level)
    {
        if (parentId == Id)
        {
            throw new BusinessException(
                "DedsiNative:Organization:CannotBeParentOfSelf",
                "组织机构不能将自身设置为上级组织。");
        }

        ParentId = string.IsNullOrWhiteSpace(parentId) ? null : ValidateUlid(parentId, nameof(parentId));
        Level = Math.Max(1, level);
        return this;
    }

    /// <summary>
    /// 设置组织机构启用/停用状态。
    /// </summary>
    /// <param name="isEnabled">
    /// 是否启用。
    /// </param>
    /// <returns>
    /// 当前组织机构聚合根实例。
    /// </returns>
    public Organization SetStatus(bool isEnabled)
    {
        IsEnabled = isEnabled;
        return this;
    }

    /// <summary>
    /// 同步所属系统名称快照。
    /// </summary>
    /// <param name="systemName">
    /// 所属系统名称快照。
    /// </param>
    /// <returns>
    /// 当前组织机构聚合根实例。
    /// </returns>
    public Organization UpdateSystemInfo(string systemName)
    {
        SystemName = Check.NotNullOrWhiteSpace(systemName, nameof(systemName), OrganizationConsts.MaxSystemNameLength);
        return this;
    }

    private static string ValidateUlid(string value, string paramName)
    {
        Check.NotNullOrWhiteSpace(value, paramName);
        if (value.Length != OrganizationConsts.UlidLength)
        {
            throw new ArgumentException($"'{paramName}' 必须是长度为 26 位的 ULID 格式字符串。", paramName);
        }

        return value;
    }
}
