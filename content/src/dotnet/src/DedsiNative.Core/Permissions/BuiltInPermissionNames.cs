using DedsiNative.LoginAudits;
using DedsiNative.OpenIddict;

namespace DedsiNative.Permissions;

/// <summary>
/// 平台内置权限编码集合，用于保护授权体系自身不被重命名、停用或删除。
/// </summary>
public static class BuiltInPermissionNames
{
    /// <summary>
    /// 返回平台全部内置权限编码。
    /// </summary>
    public static string[] All =>
    [
        .. ManagementPermissions.All,
        LoginAuditPermissions.View,
        OpenIddictPermissions.View,
        OpenIddictPermissions.Manage
    ];

    /// <summary>
    /// 判断权限编码是否由平台内置。
    /// </summary>
    /// <param name="permissionName">
    /// 待检查的权限编码。
    /// </param>
    /// <returns>
    /// 内置权限返回 <see langword="true"/>，否则返回 <see langword="false"/>。
    /// </returns>
    public static bool Contains(string permissionName) =>
        All.Contains(permissionName, StringComparer.Ordinal);
}
