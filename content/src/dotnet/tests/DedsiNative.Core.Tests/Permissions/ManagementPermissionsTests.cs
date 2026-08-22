using DedsiNative.Permissions;
using Xunit;

namespace DedsiNative.Core.Tests.Permissions;

/// <summary>
/// 管理端细粒度权限编码测试。
/// </summary>
public sealed class ManagementPermissionsTests
{
    /// <summary>
    /// 验证所有管理权限编码唯一且遵循 system:module:action 格式。
    /// </summary>
    [Fact]
    public void All_Should_Contain_Unique_WellFormed_Permission_Names()
    {
        var permissions = ManagementPermissions.All;

        Assert.Equal(permissions.Length, permissions.Distinct(StringComparer.Ordinal).Count());
        Assert.All(permissions, permission =>
        {
            Assert.StartsWith("system:", permission, StringComparison.Ordinal);
            Assert.True(permission.Split(':').Length >= 3);
        });
    }

    /// <summary>
    /// 验证内置权限目录覆盖管理、登录审计和 SSO 权限且没有重复项。
    /// </summary>
    [Fact]
    public void BuiltInPermissionNames_Should_Contain_All_Platform_Permissions()
    {
        var permissions = BuiltInPermissionNames.All;

        Assert.Equal(permissions.Length, permissions.Distinct(StringComparer.Ordinal).Count());
        Assert.All(ManagementPermissions.All, permission => Assert.Contains(permission, permissions));
        Assert.Contains("system:login-audits:view", permissions);
        Assert.Contains("system:openiddict:view", permissions);
        Assert.Contains("system:openiddict:manage", permissions);
    }
}
