namespace DedsiNative.Users;

/// <summary>用户账户状态。</summary>
public enum AccountStatus
{
    /// <summary>账户正常。</summary>
    Normal = 1,
    /// <summary>账户被禁用。</summary>
    Disabled = 2,
    /// <summary>账户被锁定。</summary>
    Locked = 3,
    /// <summary>账户已注销。</summary>
    Cancelled = 4
}

/// <summary>用户登录信息子实体。密码字段保存密码哈希，不保存明文密码。</summary>
public sealed class UserLoginInfo
{
    private UserLoginInfo() { }

    /// <summary>创建用户登录信息。</summary>
    public UserLoginInfo(Guid userId, string account, string passwordHash, string passwordSalt,
        AccountStatus status = AccountStatus.Normal)
    {
        UserId = userId;
        Account = string.IsNullOrWhiteSpace(account) ? throw new ArgumentException("账号不能为空。", nameof(account)) : account.Trim();
        PasswordHash = string.IsNullOrWhiteSpace(passwordHash) ? throw new ArgumentException("密码哈希不能为空。", nameof(passwordHash)) : passwordHash;
        PasswordSalt = string.IsNullOrWhiteSpace(passwordSalt) ? throw new ArgumentException("密码盐值不能为空。", nameof(passwordSalt)) : passwordSalt;
        Status = status;
    }

    /// <summary>关联用户 ID。</summary>
    public Guid UserId { get; private set; }
    /// <summary>登录账号。</summary>
    public string Account { get; private set; } = string.Empty;
    /// <summary>密码哈希。</summary>
    public string PasswordHash { get; private set; } = string.Empty;
    /// <summary>密码盐值。</summary>
    public string PasswordSalt { get; private set; } = string.Empty;
    /// <summary>账户状态。</summary>
    public AccountStatus Status { get; private set; }

    /// <summary>
    /// 使用新的密码哈希和盐值更新登录凭据。
    /// </summary>
    /// <param name="passwordHash">新的密码哈希。</param>
    /// <param name="passwordSalt">新的密码盐值。</param>
    public void ResetPassword(string passwordHash, string passwordSalt)
    {
        PasswordHash = string.IsNullOrWhiteSpace(passwordHash)
            ? throw new ArgumentException("密码哈希不能为空。", nameof(passwordHash))
            : passwordHash;
        PasswordSalt = string.IsNullOrWhiteSpace(passwordSalt)
            ? throw new ArgumentException("密码盐值不能为空。", nameof(passwordSalt))
            : passwordSalt;
    }
}
