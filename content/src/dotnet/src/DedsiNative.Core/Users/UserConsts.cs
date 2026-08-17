namespace DedsiNative.Users;

/// <summary>
/// 用户聚合的字段约束常量。
/// </summary>
public static class UserConsts
{
    /// <summary>
    /// 用户名称最大长度。
    /// </summary>
    public const int MaxNameLength = 64;

    /// <summary>
    /// 用户邮箱最大长度。
    /// </summary>
    public const int MaxEmailLength = 256;

    /// <summary>
    /// 登录账号最大长度。
    /// </summary>
    public const int MaxAccountLength = 128;

    /// <summary>
    /// 密码哈希最大长度。
    /// </summary>
    public const int MaxPasswordHashLength = 512;

    /// <summary>
    /// 密码盐值最大长度。
    /// </summary>
    public const int MaxPasswordSaltLength = 256;
}
