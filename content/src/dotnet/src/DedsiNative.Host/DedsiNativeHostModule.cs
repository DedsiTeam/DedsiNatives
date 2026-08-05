using System.Text;
using System.Net;
using Dedsi.CleanArchitecture.HttpApi;
using FastEndpoints;
using FastEndpoints.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace DedsiNative;

/// <summary>
/// DedsiNative 宿主层模块，整合核心层、基础设施层及第三方框架（FastEndpoints、PostgreSQL、Autofac 等）的总入口模块。
/// </summary>
[DependsOn(
    typeof(DedsiNativeCoreModule),
    typeof(DedsiNativeInfrastructureModule),
    
    typeof(DedsiCleanArchitectureHttpApiModule),
    typeof(AbpAutofacModule)
)]
public class DedsiNativeHostModule : AbpModule
{
    /// <summary>
    /// 配置宿主层所需的所有服务，包括数据库、审计日志、时钟、JSON 序列化及跨域策略。
    /// </summary>
    /// <param name="context">服务配置上下文，提供服务注册和配置能力。</param>
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        context.Services
            .AddFastEndpoints()
            .OpenApiDocument(options =>
            {
                options.DocumentName = "v1";
                options.Title = "DedsiNative API";
                options.Version = "v1";
                options.EnableJWTBearerAuth = true;
            });

        // JWT 认证
        var jwtSection = configuration.GetSection("Jwt");
        context.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidIssuer              = jwtSection["Issuer"],
                    ValidateAudience         = true,
                    ValidAudience            = jwtSection["Audience"],
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSection["Secret"]!))
                };
            });

        // 只有显式配置的可信代理才能影响 RemoteIpAddress，避免信任伪造的 X-Forwarded-For。
        context.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();

            foreach (var trustedProxy in configuration
                         .GetSection("ForwardedHeaders:KnownProxies")
                         .Get<string[]>() ?? [])
            {
                if (IPAddress.TryParse(trustedProxy, out var trustedAddress))
                {
                    options.KnownProxies.Add(trustedAddress);
                }
            }
        });

        // PostConfigure 在所有模块 Configure 之后执行，确保 ABP/Dedsi 设置的
        // FallbackPolicy 被清除，使端点级别的 AllowAnonymous() 能正确生效。
        context.Services.PostConfigure<AuthorizationOptions>(options =>
        {
            options.FallbackPolicy = null;
        });
        
        // 日志
        Configure<AbpAuditingOptions>(options =>
        {
            options.ApplicationName = DedsiNativeCoreConsts.ApplicationName;
            options.IsEnabledForGetRequests = false;
        });

        // 跨域
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]?
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.RemovePostFix("/"))
                            .ToArray() ?? []
                    )
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    /// <summary>
    /// 在应用程序初始化阶段配置 ASP.NET Core 中间件管道，包括异常处理、路由、跨域、认证及 FastEndpoints 等中间件的注册顺序。
    /// </summary>
    /// <param name="context">应用程序初始化上下文，提供对 <see cref="IApplicationBuilder"/> 和 <see cref="IWebHostEnvironment"/> 的访问。</param>
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        // 使用通用错误原因，避免将内部异常细节暴露给调用方。
        app.UseDefaultExceptionHandler(useGenericReason: true);

        app.UseForwardedHeaders();
        app.UseHttpsRedirection();
        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseSerilogRequestLogging();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAuditing();
        
        app.UseUnitOfWork();
        
        app.UseConfiguredEndpoints(endpoints =>
        {
            endpoints.MapFastEndpoints();
        });
    }
}
