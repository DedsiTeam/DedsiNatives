using FastEndpoints;
using FastEndpoints.OpenApi;
using Volo.Abp.Modularity;

namespace DedsiNative;

/// <summary>
/// DedsiNative 接口层模块，负责注册 FastEndpoints 端点发现和 OpenAPI 文档服务。
/// </summary>
[DependsOn(typeof(DedsiNativeInfrastructureModule))]
public class DedsiNativeEndpointsModule : AbpModule
{
    /// <summary>
    /// 注册接口层服务，并限定 FastEndpoints 只扫描当前接口程序集。
    /// </summary>
    /// <param name="context">
    /// 服务配置上下文，提供接口层依赖注入注册能力。
    /// </param>
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services
            .AddFastEndpoints(options =>
            {
                // Endpoint 已拆分到独立类库，显式指定程序集可避免宿主默认扫描遗漏接口。
                options.Assemblies = [typeof(DedsiNativeEndpointsModule).Assembly];
                options.DisableAutoDiscovery = true;
            })
            .OpenApiDocument(options =>
            {
                options.DocumentName = "v1";
                options.Title = "DedsiNative API";
                options.Version = "v1";
                options.EnableJWTBearerAuth = true;
            });
    }
}
