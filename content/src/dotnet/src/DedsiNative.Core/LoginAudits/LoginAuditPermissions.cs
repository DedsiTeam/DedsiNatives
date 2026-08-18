namespace DedsiNative.LoginAudits;

/// <summary>
/// 登录审计模块使用的权限名称。
/// </summary>
public static class LoginAuditPermissions
{
    /// <summary>
    /// JWT 中保存权限名称的声明类型。
    /// </summary>
    public const string ClaimType = "permission";

    /// <summary>
    /// 查看登录审计列表和详情的权限。
    /// </summary>
    public const string View = "LoginAudits.View";
}
