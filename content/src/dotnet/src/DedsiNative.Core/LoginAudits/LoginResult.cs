namespace DedsiNative.LoginAudits;

/// <summary>
/// 一次登录尝试的认证结果。
/// </summary>
public enum LoginResult
{
    /// <summary>
    /// 账号密码认证成功。
    /// </summary>
    Success = 1,

    /// <summary>
    /// 账号密码认证失败。
    /// </summary>
    Failure = 2
}
