namespace DedsiNative.Users;

/// <summary>
/// 用户岗位关联子实体，记录用户关联的岗位及岗位名称快照。
/// </summary>
public sealed class UserPosition
{
    /// <summary>
    /// 供 ORM 框架反射实例化的私有构造函数。
    /// </summary>
    private UserPosition()
    {
    }

    /// <summary>
    /// 创建用户岗位关联。
    /// </summary>
    /// <param name="userId">所属用户标识。</param>
    /// <param name="positionId">岗位标识。</param>
    /// <param name="positionName">岗位名称快照。</param>
    public UserPosition(Guid userId, string positionId, string positionName)
    {
        UserId = userId;
        PositionId = positionId;
        PositionName = Volo.Abp.Check.NotNullOrWhiteSpace(positionName, nameof(positionName), 128);
    }

    /// <summary>所属用户标识。</summary>
    public Guid UserId { get; private set; }

    /// <summary>岗位标识，使用 26 位 ULID 字符串。</summary>
    public string PositionId { get; private set; } = string.Empty;

    /// <summary>岗位名称快照。</summary>
    public string PositionName { get; private set; } = string.Empty;
}
