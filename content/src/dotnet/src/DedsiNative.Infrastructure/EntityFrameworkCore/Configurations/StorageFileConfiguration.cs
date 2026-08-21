using DedsiNative.StorageFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace DedsiNative.EntityFrameworkCore.Configurations;

/// <summary>
/// 文件与对象存储聚合根 EF Core 实体映射配置。
/// </summary>
public sealed class StorageFileConfiguration : IEntityTypeConfiguration<StorageFile>
{
    /// <summary>
    /// 配置文件实体在 PostgreSQL 中的表结构、字段约束及索引。
    /// </summary>
    /// <param name="builder">实体类型构建器。</param>
    public void Configure(EntityTypeBuilder<StorageFile> builder)
    {
        builder.ToTable("StorageFiles", DedsiNativeCoreConsts.DbSchemaName);
        builder.ConfigureByConvention();

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasMaxLength(StorageFileConsts.UlidLength)
            .IsRequired();

        builder.Property(x => x.FileName)
            .HasMaxLength(StorageFileConsts.MaxFileNameLength)
            .IsRequired();

        builder.Property(x => x.StorageName)
            .HasMaxLength(StorageFileConsts.MaxStorageNameLength)
            .IsRequired();

        builder.Property(x => x.Extension)
            .HasMaxLength(StorageFileConsts.MaxExtensionLength)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasMaxLength(StorageFileConsts.MaxContentTypeLength)
            .IsRequired();

        builder.Property(x => x.SizeBytes)
            .IsRequired();

        builder.Property(x => x.StorageType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.RelativePath)
            .HasMaxLength(StorageFileConsts.MaxRelativePathLength)
            .IsRequired();

        builder.Property(x => x.Url)
            .HasMaxLength(StorageFileConsts.MaxUrlLength);

        builder.Property(x => x.Md5Hash)
            .HasMaxLength(StorageFileConsts.MaxMd5HashLength);

        builder.Property(x => x.Category)
            .HasMaxLength(StorageFileConsts.MaxCategoryLength)
            .IsRequired();

        builder.Property(x => x.IsPublic)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(StorageFileConsts.MaxDescriptionLength);

        builder.Property(x => x.ConcurrencyStamp)
            .HasMaxLength(40)
            .IsConcurrencyToken();

        // 索引
        builder.HasIndex(x => x.Md5Hash);
        builder.HasIndex(x => x.Category);
        builder.HasIndex(x => x.CreationTime);
    }
}
