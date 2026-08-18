using FastEndpoints;
using DedsiNative.Applications.Auth;
using DedsiNative.Endpoints.AuthEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Volo.Abp;
using Xunit;

namespace DedsiNative.Host.Tests.Auth;

/// <summary>
/// 登录失败异常经过实际 ASP.NET Core 中间件后的响应测试。
/// </summary>
public sealed class LoginFailureExceptionHandlingTests
{
    /// <summary>
    /// 空消息用户友好异常应由 FastEndpoints 通用异常处理中间件转换为不泄露原因的 HTTP 500。
    /// </summary>
    [Fact]
    public async Task Empty_UserFriendlyException_Should_Return_Generic_Http_500()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddFastEndpoints(options =>
            options.Assemblies = [typeof(LoginEndpoint).Assembly]);
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();
        app.UseLoginFailureExceptionHandler();
        app.UseDefaultExceptionHandler(useGenericReason: true);
        app.Run(_ => Task.FromException(new UserFriendlyException("")));
        await app.StartAsync();

        using var client = app.GetTestClient();

        using var response = await client.GetAsync("/login-failure");
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.Equal(500, (int)response.StatusCode);
        Assert.DoesNotContain("AccountNotFound", responseBody, StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidPassword", responseBody, StringComparison.Ordinal);
    }
}
