namespace DedsiNative.LoginAudits;

/// <summary>
/// 登录认证的固定原因码。
/// </summary>
public enum LoginReason
{
    /// <summary>
    /// 成功完成账号密码认证。
    /// </summary>
    SuccessfulAuthentication = 1,

    /// <summary>
    /// 未找到对应的登录账号。
    /// </summary>
    AccountNotFound = 2,

    /// <summary>
    /// 提交的密码校验失败。
    /// </summary>
    InvalidPassword = 3,

    /// <summary>
    /// 用户已被软删除。
    /// </summary>
    UserSoftDeleted = 4,

    /// <summary>
    /// 账户已被禁用。
    /// </summary>
    AccountDisabled = 5,

    /// <summary>
    /// 账户已被锁定。
    /// </summary>
    AccountLocked = 6,

    /// <summary>
    /// 账户已被注销。
    /// </summary>
    AccountCancelled = 7,

    /// <summary>
    /// 认证过程中发生系统异常。
    /// </summary>
    SystemError = 8
}
