using System.Net.Http.Json;
using DedsiNative.Exceptions;
using DedsiNative.Endpoints.AuthEndpoints;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Volo.Abp;
using Xunit;

namespace DedsiNative.Host.Tests.Exceptions;

/// <summary>
/// FastEndpoints 自定义异常处理中间件响应与格式测试。
/// </summary>
public sealed class CustomExceptionHandlerTests
{
    [Fact]
    public async Task CustomExceptionHandler_Should_Return_UserFriendly_Message_When_UserFriendlyException_Thrown()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddFastEndpoints(options =>
            options.Assemblies = [typeof(LoginEndpoint).Assembly]);
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();
        app.UseCustomExceptionHandler(useGenericReason: true);
        app.Run(_ => throw new UserFriendlyException("未找到对应登录账号。"));
        await app.StartAsync();

        using var client = app.GetTestClient();
        using var response = await client.GetAsync("/login-test");
        var errorResponse = await response.Content.ReadFromJsonAsync<CustomInternalErrorResponse>();

        Assert.Equal(500, (int)response.StatusCode);
        Assert.NotNull(errorResponse);
        Assert.Equal(500, errorResponse.Code);
        Assert.Equal("未找到对应登录账号。", errorResponse.Message);
        Assert.True(errorResponse.ServiceTime <= DateTime.Now);
    }

    [Fact]
    public async Task CustomExceptionHandler_Should_Return_Password_Validation_Message_When_Thrown()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddFastEndpoints(options =>
            options.Assemblies = [typeof(LoginEndpoint).Assembly]);
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();
        app.UseCustomExceptionHandler(useGenericReason: true);
        app.Run(_ => throw new UserFriendlyException("密码校验失败。"));
        await app.StartAsync();

        using var client = app.GetTestClient();
        using var response = await client.GetAsync("/login-test");
        var errorResponse = await response.Content.ReadFromJsonAsync<CustomInternalErrorResponse>();

        Assert.Equal(500, (int)response.StatusCode);
        Assert.NotNull(errorResponse);
        Assert.Equal(500, errorResponse.Code);
        Assert.Equal("密码校验失败。", errorResponse.Message);
        Assert.True(errorResponse.ServiceTime <= DateTime.Now);
    }

    [Fact]
    public async Task CustomExceptionHandler_Should_Return_FastEndpoints_Error_Structure_With_Generic_Reason_For_Unhandled_Exceptions()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddFastEndpoints(options =>
            options.Assemblies = [typeof(LoginEndpoint).Assembly]);
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();
        app.UseCustomExceptionHandler(useGenericReason: true);
        app.Run(_ => throw new InvalidOperationException("Sensitive DB Connection Failed!"));
        await app.StartAsync();

        using var client = app.GetTestClient();
        using var response = await client.GetAsync("/test-error");
        var errorResponse = await response.Content.ReadFromJsonAsync<CustomInternalErrorResponse>();

        Assert.Equal(500, (int)response.StatusCode);
        Assert.NotNull(errorResponse);
        Assert.Equal(500, errorResponse.Code);
        Assert.Equal("发生未知系统异常，请稍后重试。", errorResponse.Message);
        Assert.True(errorResponse.ServiceTime <= DateTime.Now);
    }

    [Fact]
    public async Task CustomExceptionHandler_Should_Return_Actual_Message_When_Generic_Reason_Is_False()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddFastEndpoints(options =>
            options.Assemblies = [typeof(LoginEndpoint).Assembly]);
        builder.WebHost.UseTestServer();
        await using var app = builder.Build();
        app.UseCustomExceptionHandler(useGenericReason: false);
        app.Run(_ => throw new InvalidOperationException("Detailed custom exception reason."));
        await app.StartAsync();

        using var client = app.GetTestClient();
        using var response = await client.GetAsync("/test-error");
        var errorResponse = await response.Content.ReadFromJsonAsync<CustomInternalErrorResponse>();

        Assert.Equal(500, (int)response.StatusCode);
        Assert.NotNull(errorResponse);
        Assert.Equal(500, errorResponse.Code);
        Assert.Equal("Detailed custom exception reason.", errorResponse.Message);
        Assert.True(errorResponse.ServiceTime <= DateTime.Now);
    }
}
