using DedsiNative.Permissions;
using DedsiNative.LoginAudits;
using DedsiNative.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.Data;
using SystemEntity = DedsiNative.Systems.System;

namespace DedsiNative.EntityFrameworkCore.Configurations;

/// <summary>权限聚合根的 EF Core 数据库映射配置。</summary>
public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    private const string IdentitySystemId = "01ARZ3NDEKTSV4RRFFQ69G5FAV";
    private const string LoginAuditViewPermissionId = "01ARZ3NDEKTSV4RRFFQ69G5FB2";

    /// <summary>配置权限表、系统关系、字段约束和并发令牌。</summary>
    /// <param name="builder">权限实体类型构建器。</param>
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions", DedsiNativeCoreConsts.DbSchemaName);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(26).IsRequired();
        builder.Property(x => x.SystemId).HasMaxLength(26).IsRequired();
        builder.Property(x => x.SystemName).HasMaxLength(PermissionConsts.MaxNameLength).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(PermissionConsts.MaxNameLength).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(PermissionConsts.MaxDescriptionLength).IsRequired(false);
        builder.Property(x => x.IsEnabled).IsRequired().HasDefaultValue(true);

        // 权限依赖系统存在，但系统删除策略由应用层先行检查，避免误删权限配置。
        builder.HasOne<SystemEntity>()
            .WithMany()
            .HasForeignKey(x => x.SystemId)
            .HasPrincipalKey(x => x.Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CreationTime).IsRequired();
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.CreatorName).HasMaxLength(64).IsRequired(false);
        builder.Property(x => x.ConcurrencyStamp)
            .HasMaxLength(40)
            .IsRequired(false)
            .IsConcurrencyToken();

        // 登录审计查询必须有可分配的权限数据，默认只授予既有系统管理员岗位。
        builder.HasData(new
        {
            Id = LoginAuditViewPermissionId,
            SystemId = IdentitySystemId,
            SystemName = "身份管理系统",
            Name = LoginAuditPermissions.View,
            Description = "查看登录审计列表和详情。",
            IsEnabled = true,
            ExtraProperties = new ExtraPropertyDictionary(),
            ConcurrencyStamp = (string?)null,
            CreatorId = Guid.Empty,
            CreatorName = "system",
            CreationTime = new DateTime(2026, 8, 4, 10, 30, 0)
        });
    }
}
