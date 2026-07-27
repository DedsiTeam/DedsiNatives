using Dedsi.CleanArchitecture.Infrastructure;
using DedsiNative.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.Modularity;
using Volo.Abp.Timing;

namespace DedsiNative;

/// <summary>
/// DedsiNative 基础设施层模块，负责注册 EntityFrameworkCore 数据库上下文及仓储实现。
/// </summary>
[DependsOn(
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    
    typeof(DedsiNativeCoreModule),
    typeof(DedsiCleanArchitectureInfrastructureModule)
)]
public class DedsiNativeInfrastructureModule : AbpModule
{
    /// <summary>
    /// 配置基础设施层所需的服务，注册 EF Core 数据库上下文并启用默认仓储。
    /// </summary>
    /// <param name="context">服务配置上下文，提供服务注册能力。</param>
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostEnvironment = context.Services.GetAbpHostEnvironment();
        
        // EntityFrameworkCore
        context.Services.AddAbpDbContext<DedsiNativeDbContext>(options =>
        {
            options.AddDefaultRepositories(true);
        });
        
        
        // PostgreSQL 的 timestamp with time zone 仅接受 UTC DateTime，审计字段统一使用 UTC。
        Configure<AbpClockOptions>(options =>
        {
            options.Kind = DateTimeKind.Utc;
        });

        // 数据库
        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(dbConfigContext =>
            {
                // 本地研发环境 - 输出到控制台
                if (hostEnvironment.IsDevelopment())
                {
                    dbConfigContext
                        .DbContextOptions
                        .LogTo(Console.WriteLine, [DbLoggerCategory.Database.Command.Name]).
                        EnableSensitiveDataLogging();
                }
                dbConfigContext.UseNpgsql();
            });
        });
    }
}