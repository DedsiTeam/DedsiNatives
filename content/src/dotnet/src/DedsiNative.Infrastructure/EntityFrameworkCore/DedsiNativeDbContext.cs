using Dedsi.EntityFrameworkCore;
using DedsiNative.Users;
using DedsiNative.Systems;
using DedsiNative.Permissions;
using DedsiNative.Positions;
using DedsiNative.Menus;
using DedsiNative.Dictionaries;
using DedsiNative.LoginAudits;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using SystemEntity = DedsiNative.Systems.System;

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

    /// <summary>系统聚合根对应的数据集。</summary>
    DbSet<SystemEntity> Systems { get; }

    /// <summary>权限聚合根对应的数据集。</summary>
    DbSet<Permission> Permissions { get; }

    /// <summary>岗位聚合根对应的数据集。</summary>
    DbSet<Position> Positions { get; }
    /// <summary>岗位权限子实体数据集。</summary>
    DbSet<PositionPermission> PositionPermissions { get; }
    /// <summary>岗位组织机构子实体数据集。</summary>
    DbSet<PositionOrganization> PositionOrganizations { get; }
    DbSet<Menu> Menus { get; }
    /// <summary>字典聚合根数据集。</summary>
    DbSet<Dictionary> Dictionaries { get; }

    /// <summary>字典项子实体数据集。</summary>
    DbSet<DictionaryItem> DictionaryItems { get; }

    /// <summary>
    /// 登录审计聚合根对应的数据集。
    /// </summary>
    DbSet<LoginAudit> LoginAudits { get; }

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

    /// <summary>系统聚合根对应的数据集。</summary>
    public DbSet<SystemEntity> Systems { get; set; }

    /// <summary>权限聚合根对应的数据集。</summary>
    public DbSet<Permission> Permissions { get; set; }

    /// <summary>岗位聚合根对应的数据集。</summary>
    public DbSet<Position> Positions { get; set; }
    /// <summary>岗位权限子实体数据集。</summary>
    public DbSet<PositionPermission> PositionPermissions { get; set; }
    /// <summary>岗位组织机构子实体数据集。</summary>
    public DbSet<PositionOrganization> PositionOrganizations { get; set; }
    public DbSet<Menu> Menus { get; set; }
    /// <summary>字典聚合根数据集。</summary>
    public DbSet<Dictionary> Dictionaries { get; set; }

    /// <summary>字典项子实体数据集。</summary>
    public DbSet<DictionaryItem> DictionaryItems { get; set; }

    /// <summary>
    /// 登录审计聚合根对应的数据集。
    /// </summary>
    public DbSet<LoginAudit> LoginAudits { get; set; }

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
