using DedsiNative.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemEntity = DedsiNative.Systems.System;

namespace DedsiNative.EntityFrameworkCore.Configurations;

/// <summary>系统聚合根的 EF Core 数据库映射配置。</summary>
public sealed class SystemConfiguration : IEntityTypeConfiguration<SystemEntity>
{
    /// <summary>配置系统表、字段约束、审计字段和并发令牌。</summary>
    /// <param name="builder">系统实体类型构建器。</param>
    public void Configure(EntityTypeBuilder<SystemEntity> builder)
    {
        builder.ToTable("Systems", DedsiNativeCoreConsts.DbSchemaName);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(26).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(SystemConsts.MaxNameLength).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(SystemConsts.MaxDescriptionLength).IsRequired(false);
        builder.Property(x => x.Sort).IsRequired().HasDefaultValue(0);

        builder.Property(x => x.CreationTime).IsRequired();
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.CreatorName).HasMaxLength(64).IsRequired(false);
        builder.Property(x => x.ConcurrencyStamp)
            .HasMaxLength(40)
            .IsRequired(false)
            .IsConcurrencyToken();

        builder.HasData(new
        {
            Id = "01ARZ3NDEKTSV4RRFFQ69G5FAV",
            Name = "身份管理系统",
            Description = "DedsiNative 基础身份与授权管理。",
            Sort = 0,
            ExtraProperties = new Volo.Abp.Data.ExtraPropertyDictionary(),
            ConcurrencyStamp = (string?)null,
            CreatorId = Guid.Empty,
            CreatorName = "system",
            CreationTime = new DateTime(2026, 8, 2, 0, 0, 0)
        });
    }
}
