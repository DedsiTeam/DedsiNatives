using Dedsi.EntityFrameworkCore;
using DedsiNative.Users;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;

namespace DedsiNative.EntityFrameworkCore;

/// <summary>
/// DedsiNative 数据库上下文接口，定义当前模块所有聚合根对应的 DbSet。
/// </summary>
[ConnectionStringName(DedsiNativeCoreConsts.ConnectionStringName)]
public interface IDedsiNativeDbContext : IDedsiEfCoreDbContext
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

        base.OnModelCreating(modelBuilder);
    }
}