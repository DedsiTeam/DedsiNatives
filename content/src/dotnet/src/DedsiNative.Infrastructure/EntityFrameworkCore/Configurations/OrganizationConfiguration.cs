using DedsiNative.Organizations;
using DedsiNative.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using SystemEntity = DedsiNative.Systems.System;

namespace DedsiNative.EntityFrameworkCore.Configurations;

/// <summary>
/// 组织机构聚合根 EF Core 实体映射配置。
/// </summary>
public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    /// <summary>
    /// 配置组织机构实体在 PostgreSQL 中的表结构、字段约束及索引。
    /// </summary>
    /// <param name="builder">实体类型构建器。</param>
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations", DedsiNativeCoreConsts.DbSchemaName);
        builder.ConfigureByConvention();

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasMaxLength(OrganizationConsts.UlidLength)
            .IsRequired();

        builder.Property(x => x.SystemId)
            .HasMaxLength(OrganizationConsts.UlidLength)
            .IsRequired();

        builder.Property(x => x.SystemName)
            .HasMaxLength(OrganizationConsts.MaxSystemNameLength)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(OrganizationConsts.MaxCodeLength)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(OrganizationConsts.MaxNameLength)
            .IsRequired();

        builder.Property(x => x.Name1)
            .HasMaxLength(OrganizationConsts.MaxNameLength);

        builder.Property(x => x.Name2)
            .HasMaxLength(OrganizationConsts.MaxNameLength);

        builder.Property(x => x.Name3)
            .HasMaxLength(OrganizationConsts.MaxNameLength);

        builder.Property(x => x.Name4)
            .HasMaxLength(OrganizationConsts.MaxNameLength);

        builder.Property(x => x.ParentId)
            .HasMaxLength(OrganizationConsts.UlidLength);

        builder.Property(x => x.Sort)
            .IsRequired();

        builder.Property(x => x.Level)
            .IsRequired();

        builder.Property(x => x.IsEnabled)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(OrganizationConsts.MaxDescriptionLength);

        builder.Property(x => x.ConcurrencyStamp)
            .HasMaxLength(40)
            .IsConcurrencyToken();

        // 索引
        builder.HasIndex(x => new { x.SystemId, x.Code }).IsUnique();
        builder.HasIndex(x => new { x.SystemId, x.ParentId });
        builder.HasIndex(x => x.Sort);

        // 外键约束
        builder.HasOne<SystemEntity>()
            .WithMany()
            .HasForeignKey(x => x.SystemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
