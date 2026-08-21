namespace DedsiNative.Users;

/// <summary>
/// 用户组织机构关联子实体，记录用户所属的组织机构及名称快照。
/// </summary>
public sealed class UserOrganization
{
    /// <summary>
    /// 供 ORM 框架反射实例化的私有构造函数。
    /// </summary>
    private UserOrganization()
    {
    }

    /// <summary>
    /// 创建用户组织机构关联。
    /// </summary>
    /// <param name="userId">所属用户标识。</param>
    /// <param name="organizationId">组织机构标识（26 位 ULID）。</param>
    /// <param name="organizationName">组织机构名称快照。</param>
    public UserOrganization(Guid userId, string organizationId, string organizationName)
    {
        UserId = userId;
        OrganizationId = organizationId;
        OrganizationName = Volo.Abp.Check.NotNullOrWhiteSpace(organizationName, nameof(organizationName), 256);
    }

    /// <summary>所属用户标识。</summary>
    public Guid UserId { get; private set; }

    /// <summary>组织机构标识，使用 26 位 ULID 字符串。</summary>
    public string OrganizationId { get; private set; } = string.Empty;

    /// <summary>组织机构名称快照。</summary>
    public string OrganizationName { get; private set; } = string.Empty;
}
