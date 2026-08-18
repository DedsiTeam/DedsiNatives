using System.Net;
using System.Net.Http.Json;
using DedsiNative.Serialization;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace DedsiNative.Host.Tests.Serialization;

/// <summary>
/// API 时间全局配置测试。
/// </summary>
public sealed class ApiDateTimeConfigurationTests
{
    /// <summary>
    /// 固定格式应被解析为 UTC，且不接受其他格式。
    /// </summary>
    [Fact]
    public void TryParseUtc_Should_Only_Accept_Fixed_Format()
    {
        var success = ApiDateTimeConfiguration.TryParseUtc(
            "2026-08-06 06:17:49",
            out var value);

        Assert.True(success);
        Assert.Equal(DateTimeKind.Utc, value.Kind);
        Assert.Equal(new DateTime(2026, 8, 6, 6, 17, 49, DateTimeKind.Utc), value);
        Assert.False(ApiDateTimeConfiguration.TryParseUtc(
            "2026-08-06T06:17:49Z",
            out _));
        Assert.False(ApiDateTimeConfiguration.TryParseUtc(
            "2026-08-06 06:17:49.617575",
            out _));
    }

    /// <summary>
    /// JSON Body 和 Query 中的时间都应使用统一格式，并以 UTC 写入响应。
    /// </summary>
    [Fact]
    public async Task FastEndpoints_Should_Use_Fixed_Format_For_Json_And_Query_DateTimes()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddFastEndpoints(options =>
        {
            options.DisableAutoDiscovery = true;
            options.Assemblies = [typeof(DateTimeEchoEndpoint).Assembly];
        });

        await using var app = builder.Build();
        app.UseFastEndpoints(ApiDateTimeConfiguration.Configure);
        await app.StartAsync();

        using var client = app.GetTestClient();
        using var bodyResponse = await client.PostAsJsonAsync(
            "/test/date-time/body",
            new { occurredAt = "2026-08-06 06:17:49" });
        var bodyJson = await bodyResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, bodyResponse.StatusCode);
        Assert.Contains("\"occurredAt\":\"2026-08-06 06:17:49\"", bodyJson, StringComparison.Ordinal);

        using var queryResponse = await client.GetAsync(
            "/test/date-time/query?occurredAt=2026-08-06%2006%3A17%3A49");
        var queryJson = await queryResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, queryResponse.StatusCode);
        Assert.Contains("\"occurredAt\":\"2026-08-06 06:17:49\"", queryJson, StringComparison.Ordinal);

        using var invalidResponse = await client.PostAsJsonAsync(
            "/test/date-time/body",
            new { occurredAt = "2026-08-06T06:17:49Z" });

        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);
    }
}

/// <summary>
/// 时间 Echo 请求。
/// </summary>
/// <param name="OccurredAt">发生时间。</param>
public sealed record DateTimeEchoRequest(DateTime OccurredAt);

/// <summary>
/// 时间 Echo 响应。
/// </summary>
/// <param name="OccurredAt">发生时间。</param>
public sealed record DateTimeEchoResponse(DateTime OccurredAt);

/// <summary>
/// 用于验证 JSON Body 时间绑定和响应序列化的测试端点。
/// </summary>
public sealed class DateTimeEchoEndpoint
    : Endpoint<DateTimeEchoRequest, DateTimeEchoResponse>
{
    /// <summary>
    /// 配置 JSON Body 时间测试路由。
    /// </summary>
    public override void Configure()
    {
        Post("/test/date-time/body");
        AllowAnonymous();
    }

    /// <summary>
    /// 原样返回已绑定的时间。
    /// </summary>
    /// <param name="req">已绑定的请求。</param>
    /// <param name="ct">用于取消异步操作的令牌。</param>
    public override async Task HandleAsync(DateTimeEchoRequest req, CancellationToken ct)
    {
        await Send.OkAsync(new DateTimeEchoResponse(req.OccurredAt), ct);
    }
}

/// <summary>
/// 查询字符串时间请求。
/// </summary>
public sealed class DateTimeQueryRequest
{
    /// <summary>
    /// 发生时间。
    /// </summary>
    public DateTime OccurredAt { get; set; }
}

/// <summary>
/// 用于验证 Query 时间绑定的测试端点。
/// </summary>
public sealed class DateTimeQueryEndpoint
    : Endpoint<DateTimeQueryRequest, DateTimeEchoResponse>
{
    /// <summary>
    /// 配置 Query 时间测试路由。
    /// </summary>
    public override void Configure()
    {
        Get("/test/date-time/query");
        AllowAnonymous();
    }

    /// <summary>
    /// 原样返回查询字符串中绑定的时间。
    /// </summary>
    /// <param name="req">已绑定的请求。</param>
    /// <param name="ct">用于取消异步操作的令牌。</param>
    public override async Task HandleAsync(DateTimeQueryRequest req, CancellationToken ct)
    {
        await Send.OkAsync(new DateTimeEchoResponse(req.OccurredAt), ct);
    }
}
