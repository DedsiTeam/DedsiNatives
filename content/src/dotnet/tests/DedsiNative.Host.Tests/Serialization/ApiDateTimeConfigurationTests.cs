using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
    /// 固定格式按北京时间墙钟解析，拒绝 ISO、UTC 和偏移格式。
    /// </summary>
    [Fact]
    public void TryParseBeijing_Should_Only_Accept_WallClock_Format()
    {
        var success = ApiDateTimeConfiguration.TryParseBeijing(
            "2026-08-06 14:17:49.1234567",
            out var value);

        Assert.True(success);
        Assert.Equal(DateTimeKind.Unspecified, value.Kind);
        Assert.Equal(new DateTime(2026, 8, 6, 14, 17, 49).AddTicks(1234567), value);
        Assert.True(ApiDateTimeConfiguration.TryParseBeijing("2026-08-06 14:17:49", out _));
        Assert.False(ApiDateTimeConfiguration.TryParseBeijing(
            "2026-08-06T06:17:49Z",
            out _));
        Assert.False(ApiDateTimeConfiguration.TryParseBeijing(
            "2026-08-06 14:17:49+08:00",
            out _));
    }

    /// <summary>
    /// UTC 值不得被隐式写成北京时间。
    /// </summary>
    [Fact]
    public void JsonConverter_Should_Reject_Utc_Value()
    {
        var options = new JsonSerializerOptions { Converters = { new ApiDateTimeJsonConverter() } };
        Assert.Throws<JsonException>(() => JsonSerializer.Serialize(DateTime.UtcNow, options));
    }

    /// <summary>
    /// JSON Body 和 Query 中的时间都应按北京时间墙钟原样输出。
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
            new { occurredAt = "2026-08-06 14:17:49" });
        var bodyJson = await bodyResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, bodyResponse.StatusCode);
        Assert.Contains("\"occurredAt\":\"2026-08-06 14:17:49\"", bodyJson, StringComparison.Ordinal);

        using var queryResponse = await client.GetAsync(
            "/test/date-time/query?occurredAt=2026-08-06%2014%3A17%3A49");
        var queryJson = await queryResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, queryResponse.StatusCode);
        Assert.Contains("\"occurredAt\":\"2026-08-06 14:17:49\"", queryJson, StringComparison.Ordinal);

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
