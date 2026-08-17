using Dedsi.EntityFrameworkCore;
using DedsiNative.Users;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore.DistributedEvents;
using Volo.Abp.EventBus.Distributed;

namespace DedsiNative.EntityFrameworkCore;

/// <summary>
/// DedsiNative 数据库上下文接口，定义当前模块所有聚合根对应的 DbSet。
/// </summary>
[ConnectionStringName(DedsiNativeCoreConsts.ConnectionStringName)]
public interface IDedsiNativeDbContext : IDedsiEfCoreDbContext, IHasEventOutbox, IHasEventInbox
{
    /// <summary>
    /// 用户聚合根对应的数据集。
    /// </summary>
    DbSet<User> Users { get; }
}

/// <summary>
/// DedsiNative 数据库上下文，负责配置实体映射并管理与数据库的交互。
/// </summary>
/// <param name="options">EF Core 数据库上下文配置选项。</param>
[ConnectionStringName(DedsiNativeCoreConsts.ConnectionStringName)]
public class DedsiNativeDbContext(DbContextOptions<DedsiNativeDbContext> options) 
    : DedsiEfCoreDbContext<DedsiNativeDbContext>(options), IDedsiNativeDbContext
{
    /// <summary>
    /// 用户聚合根对应的数据集。
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// 等待发送的分布式事件数据集，由 ABP Outbox 在业务事务内写入。
    /// </summary>
    public DbSet<OutgoingEventRecord> OutgoingEvents { get; set; }

    /// <summary>
    /// 已接收分布式事件的数据集，由 ABP Inbox 按消息标识提供幂等消费。
    /// </summary>
    public DbSet<IncomingEventRecord> IncomingEvents { get; set; }
    
    /// <summary>
    /// 配置数据库模型，从当前程序集中自动加载所有 <see cref="IEntityTypeConfiguration{TEntity}"/> 实现。
    /// </summary>
    /// <param name="modelBuilder">EF Core 模型构建器。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="modelBuilder"/> 为 null 时抛出。</exception>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DedsiNativeDbContext).Assembly);

        // 使用 ABP 原生模型保存 Outbox/Inbox；Outbox 与业务写侧共享当前事务，
        // Inbox 则通过消息标识和处理状态记录消费结果。
        modelBuilder.ConfigureEventOutbox();
        modelBuilder.ConfigureEventInbox();

        // ABP 默认的 MessageId 索引不是唯一索引，无法原子阻止并发重复投递；
        // 唯一约束将 Inbox 防重门禁下沉到数据库层。
        modelBuilder.Entity<IncomingEventRecord>()
            .HasIndex(incomingEvent => incomingEvent.MessageId)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}
