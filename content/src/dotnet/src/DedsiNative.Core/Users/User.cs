using Dedsi.Ddd.Domain.Entities;
using Volo.Abp;

namespace DedsiNative.Users;

/// <summary>
/// 用户聚合根实体，包含用户基本信息及相关业务操作。
/// </summary>
public class User : DedsiAggregateRoot<Guid>
{
    /// <summary>
    /// 受保护的无参构造函数，供 ORM 框架反射实例化使用，禁止业务代码直接调用。
    /// </summary>
    protected User()
    {
    }

    /// <summary>
    /// 创建用户实体的业务构造函数。
    /// </summary>
    /// <param name="id">用户唯一标识（ULID 字符串）。</param>
    /// <param name="name">用户名称，不能为空或纯空白字符。</param>
    /// <param name="email">用户邮箱地址，不能为空或纯空白字符。</param>
    public User(Guid id, string name, string email) : base(id)
    {
        ChangeName(name);
        ChangeEmail(email);
        LastUpdatedAt = DateTime.Now;
    }

    /// <summary>
    /// 用户名称。
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 用户邮箱地址。
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>用户联系电话。</summary>
    public string? Phone { get; private set; }

    /// <summary>用户身份证号码。</summary>
    public string? IdCardNumber { get; private set; }

    /// <summary>最后更新时间。</summary>
    public DateTime LastUpdatedAt { get; private set; }

    /// <summary>最后登录时间。</summary>
    public DateTime? LastLoginTime { get; private set; }

    /// <summary>最后登录 IP 地址。</summary>
    public string? LastLoginIp { get; private set; }

    /// <summary>软删除时间；为空表示未删除。</summary>
    public DateTime? SoftDeletedAt { get; private set; }

    /// <summary>用户登录信息。</summary>
    public UserLoginInfo? LoginInfo { get; private set; }

    /// <summary>用户关联的岗位集合。</summary>
    public ICollection<UserPosition> Positions { get; private set; } = [];

    /// <summary>用户关联的组织机构集合。</summary>
    public ICollection<UserOrganization> Organizations { get; private set; } = [];

    /// <summary>设置用户联系电话。</summary>
    public User ChangePhone(string? phone) { Phone = phone?.Trim(); LastUpdatedAt = DateTime.Now; return this; }

    /// <summary>设置用户身份证号码。</summary>
    public User ChangeIdCardNumber(string? idCardNumber) { IdCardNumber = idCardNumber?.Trim(); LastUpdatedAt = DateTime.Now; return this; }

    /// <summary>记录用户最后一次成功登录。</summary>
    public User RecordLogin(DateTime loginTime, string? ipAddress)
    {
        LastLoginTime = loginTime;
        LastLoginIp = ipAddress?.Trim();
        LastUpdatedAt = DateTime.Now;
        return this;
    }

    /// <summary>标记用户为软删除。</summary>
    public User MarkAsSoftDeleted(DateTime? deletedAt = null)
    {
        SoftDeletedAt = deletedAt ?? DateTime.Now;
        LastUpdatedAt = DateTime.Now;
        return this;
    }

    /// <summary>绑定用户登录信息。</summary>
    public User SetLoginInfo(UserLoginInfo loginInfo)
    {
        ArgumentNullException.ThrowIfNull(loginInfo);
        if (loginInfo.UserId != Id) throw new ArgumentException("登录信息必须属于当前用户。", nameof(loginInfo));
        LoginInfo = loginInfo;
        LastUpdatedAt = DateTime.Now;
        return this;
    }

    /// <summary>
    /// 将用户登录密码更新为已经安全处理的密码材料。
    /// </summary>
    /// <param name="passwordHash">新的密码哈希。</param>
    /// <param name="passwordSalt">新的密码盐值。</param>
    /// <returns>当前用户聚合根。</returns>
    public User ResetPassword(string passwordHash, string passwordSalt)
    {
        if (SoftDeletedAt is not null)
        {
            throw new BusinessException("User:CannotResetPasswordForSoftDeletedUser");
        }

        if (LoginInfo is null)
        {
            throw new BusinessException("User:LoginInfoNotFound");
        }

        LoginInfo.ResetPassword(passwordHash, passwordSalt);
        LastUpdatedAt = DateTime.Now;
        return this;
    }

    /// <summary>
    /// 为用户关联岗位。
    /// </summary>
    /// <param name="positionId">岗位唯一标识，必须是 26 位 ULID。</param>
    /// <param name="positionName">岗位名称快照。</param>
    /// <returns>当前用户聚合根。</returns>
    public User AssignPosition(string positionId, string positionName)
    {
        var normalizedId = ValidatePositionId(positionId);
        if (Positions.Any(item => item.PositionId == normalizedId))
        {
            throw new ArgumentException("用户不能重复关联同一岗位。", nameof(positionId));
        }

        Positions.Add(new UserPosition(Id, normalizedId, positionName));
        LastUpdatedAt = DateTime.Now;
        return this;
    }

    /// <summary>
    /// 移除用户与指定岗位的关联。
    /// </summary>
    /// <param name="positionId">岗位唯一标识。</param>
    /// <returns>当前用户聚合根。</returns>
    public User RemovePosition(string positionId)
    {
        var normalizedId = ValidatePositionId(positionId);
        var relation = Positions.SingleOrDefault(item => item.PositionId == normalizedId);
        if (relation is not null)
        {
            Positions.Remove(relation);
            LastUpdatedAt = DateTime.Now;
        }

        return this;
    }

    /// <summary>
    /// 清空用户的全部岗位关联。
    /// </summary>
    /// <returns>当前用户聚合根。</returns>
    public User ClearPositions()
    {
        if (Positions.Count > 0)
        {
            Positions.Clear();
            LastUpdatedAt = DateTime.Now;
        }

        return this;
    }

    /// <summary>
    /// 为用户关联组织机构。
    /// </summary>
    /// <param name="organizationId">组织机构唯一标识，必须是 26 位 ULID。</param>
    /// <param name="organizationName">组织机构名称快照。</param>
    /// <returns>当前用户聚合根。</returns>
    public User AssignOrganization(string organizationId, string organizationName)
    {
        var normalizedId = ValidateOrganizationId(organizationId);
        if (Organizations.Any(item => item.OrganizationId == normalizedId))
        {
            throw new ArgumentException("用户不能重复关联同一组织机构。", nameof(organizationId));
        }

        Organizations.Add(new UserOrganization(Id, normalizedId, organizationName));
        LastUpdatedAt = DateTime.Now;
        return this;
    }

    /// <summary>
    /// 移除用户与指定组织机构的关联。
    /// </summary>
    /// <param name="organizationId">组织机构唯一标识。</param>
    /// <returns>当前用户聚合根。</returns>
    public User RemoveOrganization(string organizationId)
    {
        var normalizedId = ValidateOrganizationId(organizationId);
        var relation = Organizations.SingleOrDefault(item => item.OrganizationId == normalizedId);
        if (relation is not null)
        {
            Organizations.Remove(relation);
            LastUpdatedAt = DateTime.Now;
        }

        return this;
    }

    /// <summary>
    /// 清空用户的全部组织机构关联。
    /// </summary>
    /// <returns>当前用户聚合根。</returns>
    public User ClearOrganizations()
    {
        if (Organizations.Count > 0)
        {
            Organizations.Clear();
            LastUpdatedAt = DateTime.Now;
        }

        return this;
    }

    /// <summary>
    /// 修改用户名称。
    /// </summary>
    /// <param name="name">新的用户名称，不能为空或纯空白字符。</param>
    /// <returns>返回当前用户实体，支持链式调用。</returns>
    public User ChangeName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(
            name,
            nameof(name),
            UserConsts.MaxNameLength);
        LastUpdatedAt = DateTime.Now;
        return this;
    }

    /// <summary>
    /// 修改用户邮箱地址。
    /// </summary>
    /// <param name="email">新的邮箱地址，不能为空或纯空白字符。</param>
    /// <returns>返回当前用户实体，支持链式调用。</returns>
    public User ChangeEmail(string email)
    {
        Email = Check.NotNullOrWhiteSpace(
            email,
            nameof(email),
            UserConsts.MaxEmailLength);
        LastUpdatedAt = DateTime.Now;
        return this;
    }

    private static string ValidatePositionId(string positionId)
    {
        if (string.IsNullOrWhiteSpace(positionId)
            || positionId.Length != 26
            || !Ulid.TryParse(positionId, out _))
        {
            throw new ArgumentException("岗位标识必须是合法的 26 位 ULID。", nameof(positionId));
        }

        return positionId;
    }

    private static string ValidateOrganizationId(string organizationId)
    {
        if (string.IsNullOrWhiteSpace(organizationId)
            || organizationId.Length != 26
            || !Ulid.TryParse(organizationId, out _))
        {
            throw new ArgumentException("组织机构标识必须是合法的 26 位 ULID。", nameof(organizationId));
        }

        return organizationId;
    }
}
