using DedsiNative.Endpoints.AuthEndpoints;
using Xunit;

namespace DedsiNative.Host.Tests.Auth;

/// <summary>
/// 登录响应结构及岗位权限数据模型测试。
/// </summary>
public sealed class LoginResponseTests
{
    [Fact]
    public void LoginUserResponse_Should_Contain_Positions_And_Permissions()
    {
        var permissionId = "01ARZ3NDEKTSV4RRFFQ69G5FA1";
        var positionId = "01ARZ3NDEKTSV4RRFFQ69G5FB1";
        var systemId = "01ARZ3NDEKTSV4RRFFQ69G5FC1";
        var userId = Guid.NewGuid();

        var permissionResponse = new LoginPositionPermissionResponse(
            permissionId,
            "Users.Create",
            systemId,
            "用户中心");

        var positionResponse = new LoginUserPositionResponse(
            positionId,
            "系统管理员",
            [permissionResponse]);

        var userResponse = new LoginUserResponse(
            userId,
            "张三",
            "zhangsan@example.com",
            "admin",
            ["Users.Create"],
            [positionResponse]);

        var loginResponse = new LoginResponse(
            "mock-jwt-token",
            DateTime.UtcNow.AddHours(2),
            userResponse);

        Assert.Equal("mock-jwt-token", loginResponse.Token);
        Assert.Equal(userId, loginResponse.User.Id);
        Assert.Equal("张三", loginResponse.User.Name);
        Assert.Equal("admin", loginResponse.User.Account);
        Assert.Single(loginResponse.User.Permissions);
        Assert.Equal("Users.Create", loginResponse.User.Permissions[0]);

        Assert.Single(loginResponse.User.Positions);
        var firstPosition = loginResponse.User.Positions[0];
        Assert.Equal(positionId, firstPosition.PositionId);
        Assert.Equal("系统管理员", firstPosition.PositionName);

        Assert.Single(firstPosition.Permissions);
        var firstPermission = firstPosition.Permissions[0];
        Assert.Equal(permissionId, firstPermission.PermissionId);
        Assert.Equal("Users.Create", firstPermission.PermissionName);
        Assert.Equal(systemId, firstPermission.SystemId);
        Assert.Equal("用户中心", firstPermission.SystemName);
    }
}
