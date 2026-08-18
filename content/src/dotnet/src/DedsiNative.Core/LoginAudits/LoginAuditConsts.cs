namespace DedsiNative.LoginAudits;

/// <summary>
/// 登录审计聚合使用的字段长度约束。
/// </summary>
public static class LoginAuditConsts
{
    /// <summary>
    /// 登录账号最大长度。
    /// </summary>
    public const int MaxAccountLength = 50;

    /// <summary>
    /// 用户名称最大长度。
    /// </summary>
    public const int MaxUserNameLength = 20;

    /// <summary>
    /// 客户端 IP 地址最大长度。
    /// </summary>
    public const int MaxClientIpLength = 64;

    /// <summary>
    /// 失败说明最大长度。
    /// </summary>
    public const int MaxFailureDescriptionLength = 100;

    /// <summary>
    /// User-Agent 最大长度。
    /// </summary>
    public const int MaxUserAgentLength = 200;
}
