using Dedsi.CleanArchitecture.Infrastructure;
using DedsiNative.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DistributedEvents;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Modularity;
using Volo.Abp.RabbitMQ;
using Volo.Abp.Timing;

namespace DedsiNative;

/// <summary>
/// DedsiNative 基础设施层模块，负责注册 EntityFrameworkCore 数据库上下文及仓储实现。
/// </summary>
[DependsOn(
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpEventBusRabbitMqModule),
    
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
        var configuration = context.Services.GetConfiguration();
        
        // EntityFrameworkCore
        context.Services.AddAbpDbContext<DedsiNativeDbContext>(options =>
        {
            options.AddDefaultRepositories(true);
        });

        Configure<AbpDistributedEventBusOptions>(options =>
        {
            // ABP 将聚合登记的分布式事件与业务数据写入同一个 DbContext，
            // 从而保证 Outbox 记录和业务事务只能一同提交或回滚。
            options.Outboxes.Configure(config =>
            {
                config.UseDbContext<DedsiNativeDbContext>();
            });

            // Inbox 将事件处理结果与完成标记纳入同一工作单元；数据库中的
            // MessageId 唯一索引负责在并发重复投递时提供原子幂等约束。
            options.Inboxes.Configure(config =>
            {
                config.UseDbContext<DedsiNativeDbContext>();
            });
        });

        Configure<AbpRabbitMqEventBusOptions>(options =>
        {
            // 不使用 RabbitMQ 的空名默认交换机，并按应用名隔离不同宿主的队列。
            options.ExchangeName = DedsiNativeCoreConsts.ApplicationName;
            options.ClientName = DedsiNativeCoreConsts.ApplicationName;
        });

        Configure<AbpRabbitMqOptions>(options =>
        {
            var rabbitMqConnectionString = configuration["ConnectionStrings:DedsiNativeRabbitMQ"];
            if (!string.IsNullOrWhiteSpace(rabbitMqConnectionString))
            {
                options.Connections.Default.Uri = new Uri(rabbitMqConnectionString);
            }
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
