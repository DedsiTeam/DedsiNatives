using System.Security.Cryptography.X509Certificates;
using DedsiNative.AuthServer.Data;
using DedsiNative.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Quartz;
using Serilog;
using Volo.Abp;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace DedsiNative.AuthServer;

/// <summary>
/// DedsiNative 认证服务宿主模块，提供 OIDC/OAuth 2.0 授权服务器与基于 Razor Pages 的交互页面。
/// </summary>
[DependsOn(
    typeof(DedsiNativeInfrastructureModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreSerilogModule)
)]
public class DedsiNativeAuthServerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetAbpHostEnvironment();
        var configuration = context.Services.GetConfiguration();

        // 1. 注册 MVC 与 Razor Pages
        context.Services.AddControllersWithViews();
        context.Services.AddRazorPages();

        // 2. Cookie 交互认证（用于前端登录态与 Razor 页面鉴权）
        context.Services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Error";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
                options.Cookie.Name = ".DedsiNative.Auth.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

        // 3. Quartz 定时任务调度器配置（用于 OpenIddict 自动修剪与后台任务）
        context.Services.AddQuartz(options =>
        {
            options.UseSimpleTypeLoader();
            options.UseInMemoryStore();
        });
        context.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        // 4. OpenIddict 配置
        context.Services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<DedsiNativeDbContext>();
                options.UseQuartz(); // 启用 Quartz 自动修剪过期的 Token 与授权记录
            })
            .AddServer(options =>
            {
                // 启用端点
                options.SetAuthorizationEndpointUris("/connect/authorize")
                    .SetTokenEndpointUris("/connect/token")
                    .SetEndSessionEndpointUris("/connect/logout")
                    .SetUserInfoEndpointUris("/connect/userinfo")
                    .SetIntrospectionEndpointUris("/connect/introspect")
                    .SetRevocationEndpointUris("/connect/revoke");

                // 启用授权流程
                options.AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange()
                    .AllowClientCredentialsFlow()
                    .AllowRefreshTokenFlow();

                // 禁用内部 JWE 加密，生成可供 Resource Server 验签的标准 JWT 访问令牌
                options.DisableAccessTokenEncryption();

                // 注册作用域
                options.RegisterScopes(
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles,
                    "dedsinative_api");

                // 加密与签名证书（开发环境下使用临时/自签名秘钥）
                if (hostingEnvironment.IsDevelopment())
                {
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                }
                else
                {
                    // 生产环境中可通过证书存储或临时秘钥注入
                    options.AddEphemeralEncryptionKey()
                        .AddEphemeralSigningKey();
                }

                // 集成 ASP.NET Core 并关闭 HTTPS 传输安全强制要求
                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .EnableStatusCodePagesIntegration()
                    .DisableTransportSecurityRequirement();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        // 4. 代理与跨域（完全开放所有跨域来源）
        context.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        // 5. 注册数据播种器
        context.Services.AddTransient<OpenIddictDataSeeder>();
    }

    public override async Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        // 自动执行数据库播种
        var seeder = context.ServiceProvider.GetRequiredService<OpenIddictDataSeeder>();
        await seeder.SeedAsync();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseForwardedHeaders();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseSerilogRequestLogging();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseUnitOfWork();

        app.UseConfiguredEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapRazorPages();
            endpoints.MapDefaultControllerRoute();
        });
    }
}
