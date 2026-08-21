namespace DedsiNative.OpenIddict;

/// <summary>
/// OpenIddict SSO 模块权限编码定义。
/// </summary>
public static class OpenIddictPermissions
{
    /// <summary>
    /// JWT 中保存权限名称的声明类型。
    /// </summary>
    public const string ClaimType = "permission";

    /// <summary>
    /// 查看 SSO 客户端应用、作用域、用户授权与活跃令牌的权限。
    /// </summary>
    public const string View = "system:openiddict:view";

    /// <summary>
    /// 管理 SSO 客户端、作用域配置（增删改、重置密钥）及强制吊销令牌与授权的权限。
    /// </summary>
    public const string Manage = "system:openiddict:manage";
}
