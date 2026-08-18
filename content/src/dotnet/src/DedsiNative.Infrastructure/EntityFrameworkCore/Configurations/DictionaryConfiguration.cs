using DedsiNative.Dictionaries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SystemEntity = DedsiNative.Systems.System;

namespace DedsiNative.EntityFrameworkCore.Configurations;

/// <summary>
/// 字典聚合根的 EF Core 映射配置。
/// </summary>
public sealed class DictionaryConfiguration : IEntityTypeConfiguration<Dictionary>
{
    /// <summary>
    /// 配置字典分组字段、唯一索引、系统关系和字典项集合。
    /// </summary>
    /// <param name="builder">字典实体类型构建器。</param>
    public void Configure(EntityTypeBuilder<Dictionary> builder)
    {
        builder.ToTable("Dictionaries", DedsiNativeCoreConsts.DbSchemaName);
        builder.HasKey(dictionary => dictionary.Id);
        builder.Property(dictionary => dictionary.Id)
            .HasMaxLength(DictionaryConsts.UlidLength)
            .IsRequired();
        builder.Property(dictionary => dictionary.SystemId)
            .HasMaxLength(DictionaryConsts.UlidLength)
            .IsRequired();
        builder.Property(dictionary => dictionary.SystemName)
            .HasMaxLength(DictionaryConsts.MaxSystemNameLength)
            .IsRequired();
        builder.Property(dictionary => dictionary.Name)
            .HasMaxLength(DictionaryConsts.MaxNameLength)
            .IsRequired();

        builder.HasIndex(dictionary => new { dictionary.SystemId, dictionary.Name })
            .IsUnique();

        builder.HasOne<SystemEntity>()
            .WithMany()
            .HasForeignKey(dictionary => dictionary.SystemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(dictionary => dictionary.Items)
            .WithOne()
            .HasForeignKey(item => item.DictionaryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(dictionary => dictionary.CreationTime).IsRequired();
        builder.Property(dictionary => dictionary.CreatorId).IsRequired();
        builder.Property(dictionary => dictionary.CreatorName)
            .HasMaxLength(64)
            .IsRequired(false);
        builder.Property(dictionary => dictionary.ConcurrencyStamp)
            .HasMaxLength(40)
            .IsRequired(false)
            .IsConcurrencyToken();
    }
}

/// <summary>
/// 字典项子实体的 EF Core 映射配置。
/// </summary>
public sealed class DictionaryItemConfiguration : IEntityTypeConfiguration<DictionaryItem>
{
    /// <summary>
    /// 配置字典项字段、唯一索引和父项关系。
    /// </summary>
    /// <param name="builder">字典项实体类型构建器。</param>
    public void Configure(EntityTypeBuilder<DictionaryItem> builder)
    {
        builder.ToTable("DictionaryItems", DedsiNativeCoreConsts.DbSchemaName);
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id)
            .HasMaxLength(DictionaryConsts.UlidLength)
            .IsRequired();
        builder.Property(item => item.DictionaryId)
            .HasMaxLength(DictionaryConsts.UlidLength)
            .IsRequired();
        builder.Property(item => item.Code)
            .HasMaxLength(DictionaryConsts.MaxCodeLength)
            .IsRequired();
        builder.Property(item => item.Name)
            .HasMaxLength(DictionaryConsts.MaxItemNameLength)
            .IsRequired();
        builder.Property(item => item.Description)
            .HasMaxLength(DictionaryConsts.MaxDescriptionLength)
            .IsRequired(false);
        builder.Property(item => item.Sort).IsRequired();
        builder.Property(item => item.IsEnabled).IsRequired();
        builder.Property(item => item.IsDefault).IsRequired();
        builder.Property(item => item.ParentId)
            .HasMaxLength(DictionaryConsts.UlidLength)
            .IsRequired(false);

        builder.HasIndex(item => new { item.DictionaryId, item.Code })
            .IsUnique();
        builder.HasIndex(item => new { item.DictionaryId, item.ParentId });

        // 父项只用于层级导航，跨分组和环校验由字典聚合维护。
        builder.HasOne<DictionaryItem>()
            .WithMany()
            .HasForeignKey(item => item.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
