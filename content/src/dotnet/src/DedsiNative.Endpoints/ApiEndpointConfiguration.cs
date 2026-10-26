using DedsiNative.Serialization;
using FastEndpoints;

namespace DedsiNative;

/// <summary>
/// 统一配置业务 API 的路由前缀与序列化行为。
/// </summary>
public static class ApiEndpointConfiguration
{
    /// <summary>
    /// 业务 API 路由前缀；模板创建项目时替换应用名称的小写形式。
    /// </summary>
    public const string RoutePrefix = "dedsinative-api";

    /// <summary>
    /// 应用业务 API 的全局配置，保留单个 Endpoint 覆盖前缀的能力。
    /// </summary>
    /// <param name="options">
    /// FastEndpoints 全局配置。
    /// </param>
    public static void Configure(Config options)
    {
        options.Endpoints.RoutePrefix = RoutePrefix;
        ApiDateTimeConfiguration.Configure(options);
    }
}
