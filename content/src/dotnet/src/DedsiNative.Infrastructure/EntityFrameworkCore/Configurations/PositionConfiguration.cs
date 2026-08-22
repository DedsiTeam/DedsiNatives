using DedsiNative.Positions;
using DedsiNative.Permissions;
using DedsiNative.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemEntity = DedsiNative.Systems.System;

namespace DedsiNative.EntityFrameworkCore.Configurations;

/// <summary>岗位聚合根的 EF Core 映射配置。</summary>
public sealed class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    /// <summary>配置岗位、子实体关系、字段约束和并发令牌。</summary>
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("Positions", DedsiNativeCoreConsts.DbSchemaName);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(26).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(PositionConsts.MaxNameLength).IsRequired();
        builder.Property(x => x.SystemId).HasMaxLength(26).IsRequired();
        builder.Property(x => x.SystemName).HasMaxLength(PositionConsts.MaxNameLength).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(PositionConsts.MaxDescriptionLength).IsRequired(false);
        builder.Property(x => x.IsEnabled).IsRequired().HasDefaultValue(true);

        builder.HasOne<SystemEntity>()
            .WithMany()
            .HasForeignKey(x => x.SystemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Permissions)
            .WithOne()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Organizations)
            .WithOne()
            .HasForeignKey(x => x.PositionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreationTime).IsRequired();
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.CreatorName).HasMaxLength(64).IsRequired(false);
        builder.Property(x => x.ConcurrencyStamp)
            .HasMaxLength(40)
            .IsRequired(false)
            .IsConcurrencyToken();

        builder.HasData(new
        {
            Id = "01ARZ3NDEKTSV4RRFFQ69G5FB0",
            Name = "系统管理员",
            SystemId = "01ARZ3NDEKTSV4RRFFQ69G5FAV",
            SystemName = "身份管理系统",
            Description = "拥有身份管理系统的基础管理权限。",
            IsEnabled = true,
            ExtraProperties = new Volo.Abp.Data.ExtraPropertyDictionary(),
            ConcurrencyStamp = (string?)null,
            CreatorId = Guid.Empty,
            CreatorName = "system",
            CreationTime = new DateTime(2026, 8, 2, 0, 0, 0)
        });
    }
}

/// <summary>岗位权限子实体的 EF Core 映射配置。</summary>
public sealed class PositionPermissionConfiguration : IEntityTypeConfiguration<PositionPermission>
{
    /// <summary>配置岗位权限复合主键和字段长度。</summary>
    public void Configure(EntityTypeBuilder<PositionPermission> builder)
    {
        builder.ToTable("PositionPermissions", DedsiNativeCoreConsts.DbSchemaName);
        builder.HasKey(x => new { x.PositionId, x.PermissionId });
        builder.Property(x => x.PositionId).HasMaxLength(26).IsRequired();
        builder.Property(x => x.PermissionId).HasMaxLength(26).IsRequired();
        builder.Property(x => x.PermissionName).HasMaxLength(PermissionConsts.MaxNameLength).IsRequired();
        builder.Property(x => x.SystemId).HasMaxLength(26).IsRequired();
        builder.Property(x => x.SystemName).HasMaxLength(PermissionConsts.MaxNameLength).IsRequired();

        // 升级模板后保留默认管理员的完整管理能力，其他岗位仍按显式分配结果授权。
        builder.HasData(BuiltInPermissionSeedCatalog.All.Select(permission => new
        {
            PositionId = BuiltInPermissionSeedCatalog.AdministratorPositionId,
            PermissionId = permission.Id,
            PermissionName = permission.Name,
            SystemId = BuiltInPermissionSeedCatalog.IdentitySystemId,
            SystemName = "身份管理系统"
        }));
    }
}

/// <summary>岗位组织机构子实体的 EF Core 映射配置。</summary>
public sealed class PositionOrganizationConfiguration : IEntityTypeConfiguration<PositionOrganization>
{
    /// <summary>配置岗位组织机构复合主键和字段长度。</summary>
    public void Configure(EntityTypeBuilder<PositionOrganization> builder)
    {
        builder.ToTable("PositionOrganizations", DedsiNativeCoreConsts.DbSchemaName);
        builder.HasKey(x => new { x.PositionId, x.OrganizationId });
        builder.Property(x => x.PositionId).HasMaxLength(26).IsRequired();
        builder.Property(x => x.OrganizationId).HasMaxLength(26).IsRequired();
        builder.Property(x => x.OrganizationName).HasMaxLength(128).IsRequired();
    }
}
