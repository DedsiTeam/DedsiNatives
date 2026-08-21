using DedsiNative.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace DedsiNative.AuthServer.Data;

/// <summary>
/// OpenIddict 默认客户端与权限作用域数据初始化播种器。
/// </summary>
public class OpenIddictDataSeeder(
    IServiceProvider serviceProvider,
    ILogger<OpenIddictDataSeeder> logger) : ITransientDependency
{
    /// <summary>
    /// 初始化默认 Scope 与 Client 应用程序配置。
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DedsiNativeDbContext>();
        
        // 确保数据库表已迁移（在开发环境下自动迁移）
        await context.Database.MigrateAsync(cancellationToken);

        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();
        var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        // 1. 播种标准与自定义 Scopes
        await SeedScopesAsync(scopeManager, cancellationToken);

        // 2. 播种默认客户端应用
        await SeedApplicationsAsync(applicationManager, cancellationToken);
    }

    private async Task SeedScopesAsync(IOpenIddictScopeManager scopeManager, CancellationToken cancellationToken)
    {
        var scopes = new[]
        {
            (Name: OpenIddictConstants.Permissions.Scopes.Email, DisplayName: "电子邮箱", Description: "访问您的电子邮箱地址"),
            (Name: OpenIddictConstants.Permissions.Scopes.Profile, DisplayName: "基本资料", Description: "访问您的用户基本资料（姓名、账号等）"),
            (Name: OpenIddictConstants.Permissions.Scopes.Roles, DisplayName: "用户角色与岗位", Description: "访问您被分配的角色与岗位信息"),
            (Name: "dedsinative_api", DisplayName: "DedsiNative 业务 API 访问权限", Description: "调用 DedsiNative 后端业务接口")
        };

        foreach (var (name, displayName, description) in scopes)
        {
            if (await scopeManager.FindByNameAsync(name, cancellationToken) is null)
            {
                logger.LogInformation("正在创建 OpenIddict Scope: {ScopeName}", name);
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = name,
                    DisplayName = displayName,
                    Description = description,
                    Resources = { "dedsinative_api" }
                }, cancellationToken);
            }
        }
    }

    private async Task SeedApplicationsAsync(IOpenIddictApplicationManager applicationManager, CancellationToken cancellationToken)
    {
        // 客户端 1：Web 前端 SPA 客户端（授权码模式 + PKCE）
        const string webClientId = "dedsinative-web";
        if (await applicationManager.FindByClientIdAsync(webClientId, cancellationToken) is null)
        {
            logger.LogInformation("正在创建 OpenIddict 客户端: {ClientId}", webClientId);
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = webClientId,
                DisplayName = "DedsiNative Web 前端应用",
                ClientType = OpenIddictConstants.ClientTypes.Public,
                ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
                RedirectUris =
                {
                    new Uri("http://localhost:11026/signin-oidc"),
                    new Uri("http://localhost:11026/callback"),
                    new Uri("http://localhost:12256/swagger/oauth2-redirect.html"),
                    new Uri("http://localhost:12256/scalar/v1")
                },
                PostLogoutRedirectUris =
                {
                    new Uri("http://localhost:11026/signout-callback-oidc"),
                    new Uri("http://localhost:11026/")
                },
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.EndSession,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "dedsinative_api"
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                }
            }, cancellationToken);
        }

        // 客户端 2：后端服务间调用客户端（客户端凭据模式 Client Credentials）
        const string apiClientId = "dedsinative-m2m";
        if (await applicationManager.FindByClientIdAsync(apiClientId, cancellationToken) is null)
        {
            logger.LogInformation("正在创建 OpenIddict 客户端: {ClientId}", apiClientId);
            await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = apiClientId,
                ClientSecret = "DedsiNativeM2MSecret2026!",
                DisplayName = "DedsiNative 机器通信客户端 (M2M)",
                ClientType = OpenIddictConstants.ClientTypes.Confidential,
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "dedsinative_api"
                }
            }, cancellationToken);
        }
    }
}
