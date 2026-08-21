using DedsiNative.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DedsiNative.EntityFrameworkCore.Configurations;

/// <summary>
/// 用户实体的 EF Core 数据库映射配置，定义表名、主键及所有字段的列约束。
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
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
        builder.Property(x => x.Id).IsRequired();

        // ── User 自有字段 ─────────────────────────────────────────
        // Name：用户名称，必填，最大长度 64
        builder.Property(x => x.Name)
            .HasMaxLength(UserConsts.MaxNameLength)
            .IsRequired();

        // Email：用户邮箱地址，必填，最大长度 256
        builder.Property(x => x.Email)
            .HasMaxLength(UserConsts.MaxEmailLength)
            .IsRequired();

        builder.Property(x => x.Phone).HasMaxLength(32).IsRequired(false);
        builder.Property(x => x.IdCardNumber).HasMaxLength(32).IsRequired(false);
        builder.Property(x => x.LastUpdatedAt).IsRequired();
        builder.Property(x => x.LastLoginTime).IsRequired(false);
        builder.Property(x => x.LastLoginIp).HasMaxLength(64).IsRequired(false);
        builder.Property(x => x.SoftDeletedAt).IsRequired(false);

        var defaultUserId = Guid.Parse("01951500-0000-7000-8000-000000000001");

        // 登录信息是用户聚合内的一对一子实体，随用户聚合统一跟踪和保存。
        builder.OwnsOne(user => user.LoginInfo, loginInfoBuilder =>
        {
            loginInfoBuilder.ToTable("UserLoginInfos", DedsiNativeCoreConsts.DbSchemaName);
            loginInfoBuilder.WithOwner().HasForeignKey(loginInfo => loginInfo.UserId);
            loginInfoBuilder.HasKey(loginInfo => loginInfo.UserId);
            loginInfoBuilder.Property(loginInfo => loginInfo.Account).HasMaxLength(128).IsRequired();
            loginInfoBuilder.HasIndex(loginInfo => loginInfo.Account).IsUnique();
            loginInfoBuilder.Property(loginInfo => loginInfo.PasswordHash).HasMaxLength(512).IsRequired();
            loginInfoBuilder.Property(loginInfo => loginInfo.PasswordSalt).HasMaxLength(256).IsRequired();
            loginInfoBuilder.Property(loginInfo => loginInfo.Status).HasConversion<string>().HasMaxLength(16).IsRequired();

            loginInfoBuilder.HasData(new
            {
                UserId = defaultUserId,
                Account = "CohenWang",
                PasswordHash = "W/2szOheNj12boeq2Lb+T8mtJsWknqskgTxfEcbPV68=",
                PasswordSalt = "AQIDBAUGBwgJCgsMDQ4PEA==",
                Status = AccountStatus.Normal
            });
        });

        // 岗位关联是用户聚合内的集合子实体，以用户和岗位标识组成复合主键。
        builder.OwnsMany(user => user.Positions, positionBuilder =>
        {
            positionBuilder.ToTable("UserPositions", DedsiNativeCoreConsts.DbSchemaName);
            positionBuilder.WithOwner().HasForeignKey(position => position.UserId);
            positionBuilder.HasKey(position => new { position.UserId, position.PositionId });
            positionBuilder.Property(position => position.PositionId).HasMaxLength(26).IsRequired();
            positionBuilder.Property(position => position.PositionName).HasMaxLength(128).IsRequired();

            positionBuilder.HasData(new
            {
                UserId = defaultUserId,
                PositionId = "01ARZ3NDEKTSV4RRFFQ69G5FB0",
                PositionName = "系统管理员"
            });
        });

        // 组织机构关联是用户聚合内的集合子实体，以用户和组织机构标识组成复合主键。
        builder.OwnsMany(user => user.Organizations, orgBuilder =>
        {
            orgBuilder.ToTable("UserOrganizations", DedsiNativeCoreConsts.DbSchemaName);
            orgBuilder.WithOwner().HasForeignKey(org => org.UserId);
            orgBuilder.HasKey(org => new { org.UserId, org.OrganizationId });
            orgBuilder.Property(org => org.OrganizationId).HasMaxLength(26).IsRequired();
            orgBuilder.Property(org => org.OrganizationName).HasMaxLength(256).IsRequired();
        });
        
        builder.Navigation(user => user.LoginInfo).AutoInclude();
        builder.Navigation(user => user.Positions).AutoInclude();
        builder.Navigation(user => user.Organizations).AutoInclude();

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

        builder.HasData(new
        {
            Id = defaultUserId,
            Name = "CohenWang",
            Email = "cohenwang@example.com",
            LastUpdatedAt = new DateTime(2026, 8, 18, 0, 0, 0),
            CreationTime = new DateTime(2026, 8, 18, 0, 0, 0),
            CreatorId = Guid.Empty,
            CreatorName = "system",
            ExtraProperties = new Volo.Abp.Data.ExtraPropertyDictionary(),
            ConcurrencyStamp = (string?)null
        });
    }
}
