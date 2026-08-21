using DedsiNative.Endpoints.AuthEndpoints;
using Xunit;

namespace DedsiNative.Host.Tests.Auth;

/// <summary>
/// 登录响应结构及岗位权限数据模型测试。
/// </summary>
public sealed class LoginResponseTests
{
    [Fact]
    public void LoginUserResponse_Should_Contain_Positions_And_Deduplicated_Permissions()
    {
        var positionId = "01ARZ3NDEKTSV4RRFFQ69G5FB1";
        var userId = Guid.NewGuid();

        var positionResponse = new LoginUserPositionResponse(
            positionId,
            "系统管理员");

        var userResponse = new LoginUserResponse(
            userId,
            "张三",
            "zhangsan@example.com",
            "admin",
            ["system:users:create", "system:users:view"],
            [positionResponse]);

        var loginResponse = new LoginResponse(
            "mock-jwt-token",
            DateTime.UtcNow.AddHours(2),
            userResponse);

        Assert.Equal("mock-jwt-token", loginResponse.Token);
        Assert.Equal(userId, loginResponse.User.Id);
        Assert.Equal("张三", loginResponse.User.Name);
        Assert.Equal("admin", loginResponse.User.Account);
        Assert.Equal(2, loginResponse.User.Permissions.Length);
        Assert.Contains("system:users:create", loginResponse.User.Permissions);
        Assert.Contains("system:users:view", loginResponse.User.Permissions);

        Assert.Single(loginResponse.User.Positions);
        var firstPosition = loginResponse.User.Positions[0];
        Assert.Equal(positionId, firstPosition.PositionId);
        Assert.Equal("系统管理员", firstPosition.PositionName);
    }
}
