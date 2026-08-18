using Dedsi.CleanArchitecture.Infrastructure;
using DedsiNative.EntityFrameworkCore;
using DedsiNative.Permissions;
using DedsiNative.Positions;
using DedsiNative.Menus;
using DedsiNative.Users;
using DedsiNative.Dictionaries;
using DedsiNative.LoginAudits;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DependencyInjection;
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
    static DedsiNativeInfrastructureModule()
    {
        // 允许直接以本地时间（北京时间）读写 PostgreSQL timestamp without time zone。
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

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
        
        // 审计与实体时间字段使用本地时间（北京时间）。
        Configure<AbpClockOptions>(options =>
        {
            options.Kind = DateTimeKind.Local;
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

        Configure<AbpEntityOptions>(options =>
        {
            // User
            options.Entity<User>(userOptions =>
            {
                userOptions.DefaultWithDetailsFunc = query => query
                    .Include(u => u.Positions)
                    .Include(u => u.LoginInfo);
            });

            // System
            options.Entity<DedsiNative.Systems.System>(systemOptions =>
            {
                systemOptions.DefaultWithDetailsFunc = query => query;
            });

            // Permission
            options.Entity<Permission>(permissionOptions =>
            {
                permissionOptions.DefaultWithDetailsFunc = query => query;
            });

            // Position
            options.Entity<Position>(positionOptions =>
            {
                positionOptions.DefaultWithDetailsFunc = query => query
                    .Include(position => position.Permissions)
                    .Include(position => position.Organizations);
            });

            options.Entity<Menu>(menuOptions =>
            {
                menuOptions.DefaultWithDetailsFunc = query => query;
            });

            options.Entity<Dictionary>(dictionaryOptions =>
            {
                dictionaryOptions.DefaultWithDetailsFunc = query => query
                    .Include(dictionary => dictionary.Items);
            });

            // LoginAudit 没有聚合内部导航，但仍显式声明详情查询行为。
            options.Entity<LoginAudit>(loginAuditOptions =>
            {
                loginAuditOptions.DefaultWithDetailsFunc = query => query;
            });

        });
    }
}
