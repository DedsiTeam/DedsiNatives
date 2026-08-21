using System.Net;
using System.Text;
using Dedsi.CleanArchitecture.HttpApi;
using DedsiNative.Exceptions;
using DedsiNative.LoginAudits;
using DedsiNative.Serialization;
using FastEndpoints;
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
/// DedsiNative 宿主层模块，整合接口层与第三方中间件（认证、审计、跨域、Autofac 等）的总入口模块。
/// </summary>
[DependsOn(
    typeof(DedsiNativeEndpointsModule),
    typeof(DedsiCleanArchitectureHttpApiModule),
    typeof(AbpAutofacModule)
)]
public class DedsiNativeHostModule : AbpModule
{
    /// <summary>
    /// 配置宿主层所需的认证、审计日志、代理转发、授权策略与跨域服务。
    /// </summary>
    /// <param name="context">
    /// 服务配置上下文，提供服务注册和配置能力。
    /// </param>
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

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

        // 只有显式配置的可信代理才能影响 RemoteIpAddress，避免直接信任伪造 X-Forwarded-For。
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

        context.Services.AddAuthorization(options =>
        {
            options.AddPolicy(LoginAuditPermissions.View, policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(
                    LoginAuditPermissions.ClaimType,
                    LoginAuditPermissions.View));

            options.AddPolicy(DedsiNative.OpenIddict.OpenIddictPermissions.View, policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(
                    DedsiNative.OpenIddict.OpenIddictPermissions.ClaimType,
                    DedsiNative.OpenIddict.OpenIddictPermissions.View));

            options.AddPolicy(DedsiNative.OpenIddict.OpenIddictPermissions.Manage, policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim(
                    DedsiNative.OpenIddict.OpenIddictPermissions.ClaimType,
                    DedsiNative.OpenIddict.OpenIddictPermissions.Manage));
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
    /// <param name="context">
    /// 应用程序初始化上下文，提供对 <see cref="IApplicationBuilder"/> 的访问。
    /// </param>
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();

        // FastEndpoints 自定义通用异常处理程序（返回 { Status, Code, Reason, Note } 结构并记录日志）
        app.UseCustomExceptionHandler();

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
            endpoints.MapFastEndpoints(ApiDateTimeConfiguration.Configure);
        });
    }
}
