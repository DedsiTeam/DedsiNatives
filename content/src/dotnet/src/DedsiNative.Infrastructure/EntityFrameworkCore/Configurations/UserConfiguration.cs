using DedsiNative.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DedsiNative.EntityFrameworkCore.Configurations;

/// <summary>
/// 用户实体的 EF Core 数据库映射配置，定义表名、主键及所有字段的列约束。
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// 配置 <see cref="User"/> 实体到数据库的映射规则，包括表名、主键和每个字段的约束。
    /// </summary>
    /// <param name="builder">实体类型构建器，用于配置表结构和约束。</param>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // 映射到指定 Schema 下的 Users 表
        builder.ToTable("Users", DedsiNativeCoreConsts.DbSchemaName);

        // ── 主键 ──────────────────────────────────────────────────
        // Id：用户唯一标识，使用 ULID 字符串，最大长度 26 位
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasMaxLength(26)
            .IsRequired();

        // ── User 自有字段 ─────────────────────────────────────────
        // Name：用户名称，必填，最大长度 64
        builder.Property(x => x.Name)
            .HasMaxLength(UserConsts.MaxNameLength)
            .IsRequired();

        // Email：用户邮箱地址，必填，最大长度 256
        builder.Property(x => x.Email)
            .HasMaxLength(UserConsts.MaxEmailLength)
            .IsRequired();

        builder.Property(x => x.Account)
            .HasMaxLength(UserConsts.MaxAccountLength)
            .IsRequired(false);

        // 登录账号必须全局唯一；PostgreSQL 唯一索引允许多个尚未开通登录的空账号。
        builder.HasIndex(x => x.Account)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(UserConsts.MaxPasswordHashLength)
            .IsRequired(false);

        builder.Property(x => x.PasswordSalt)
            .HasMaxLength(UserConsts.MaxPasswordSaltLength)
            .IsRequired(false);

        // ── 继承自 DedsiAggregateRoot 的审计字段 ──────────────────
        // CreationTime：记录创建时间，统一存储为 UTC
        builder.Property(x => x.CreationTime)
            .IsRequired();

        // CreatorId：创建者的用户 ID（Guid 值类型，不可为 null，由框架必填约束管理）
        builder.Property(x => x.CreatorId)
            .IsRequired();

        // CreatorName：创建者的用户名称（可为空），最大长度 64
        builder.Property(x => x.CreatorName)
            .HasMaxLength(64)
            .IsRequired(false);

        // ── 继承自 AggregateRoot 的并发控制字段 ───────────────────
        // ConcurrencyStamp：乐观并发令牌，每次更新时刷新，最大长度 40
        builder.Property(x => x.ConcurrencyStamp)
            .HasMaxLength(40)
            .IsRequired(false)
            .IsConcurrencyToken();

        var creationTime = new DateTime(2026, 8, 17, 0, 0, 0, DateTimeKind.Utc);

        // 种子使用确定性的密码材料，避免模型快照因随机盐变化而产生重复迁移。
        builder.HasData(new
        {
            Id = "01ARZ3NDEKTSV4RRFFQ69G5FAV",
            Name = "超级管理员",
            Email = "admin@dedsinative.local",
            Account = "15833084138",
            PasswordHash = "DqpyFntIjpkXAwEXsqcW5PDBfi27fXEnDcuC4v4f3/Q=",
            PasswordSalt = "XMTFCyq7q+8jOGe5ihk1eA==",
            ExtraProperties = new Volo.Abp.Data.ExtraPropertyDictionary(),
            ConcurrencyStamp = (string?)null,
            CreatorId = Guid.Empty,
            CreatorName = "system",
            CreationTime = creationTime
        });
    }
}
