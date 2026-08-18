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
    /// 注册接口层服务，并限定 FastEndpoints 扫描当前接口程序集与 OpenAPI 文档描述。
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
            })
            .OpenApiDocument(options =>
            {
                options.DocumentName = "v1";
                options.Title = "DedsiNative API";
                options.Version = "v1";
                options.EnableJWTBearerAuth = true;
                options.TagDescriptions = tags =>
                {
                    tags["认证管理"] = "用户登录和访问令牌相关接口。";
                    tags["系统管理"] = "系统基础信息维护接口。";
                    tags["权限管理"] = "系统权限及权限状态维护接口。";
                    tags["岗位管理"] = "岗位及其权限、组织机构关联维护接口。";
                    tags["用户管理"] = "用户资料、登录信息和岗位关联维护接口。";
                    tags["个人中心"] = "当前登录用户的个人资料与密码维护接口。";
                    tags["登录审计"] = "受权人员查询账号密码登录审计记录。";
                };
            });
    }
}
