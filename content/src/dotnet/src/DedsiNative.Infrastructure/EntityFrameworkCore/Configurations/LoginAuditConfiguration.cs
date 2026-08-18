using DedsiNative.LoginAudits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DedsiNative.EntityFrameworkCore.Configurations;

/// <summary>
/// 登录审计聚合根的 EF Core 映射配置。
/// </summary>
public sealed class LoginAuditConfiguration : IEntityTypeConfiguration<LoginAudit>
{
    /// <summary>
    /// 配置登录审计表、索引和字段长度。
    /// </summary>
    /// <param name="builder">登录审计实体类型构建器。</param>
    public void Configure(EntityTypeBuilder<LoginAudit> builder)
    {
        builder.ToTable("LoginAudits", DedsiNativeCoreConsts.DbSchemaName);
        builder.HasKey(audit => audit.Id);
        builder.Property(audit => audit.Id)
            .HasMaxLength(26)
            .IsRequired();

        // 认证时间按 UTC 保存，并作为审计列表默认倒序字段。
        builder.Property(audit => audit.LoginTimeUtc)
            .IsRequired();
        builder.Property(audit => audit.Result)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(audit => audit.Reason)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(audit => audit.Account)
            .HasMaxLength(LoginAuditConsts.MaxAccountLength)
            .IsRequired();
        builder.Property(audit => audit.UserName)
            .HasMaxLength(LoginAuditConsts.MaxUserNameLength)
            .IsRequired(false);
        builder.Property(audit => audit.UserId)
            .IsRequired(false);
        builder.Property(audit => audit.ClientIp)
            .HasMaxLength(LoginAuditConsts.MaxClientIpLength)
            .IsRequired(false);
        builder.Property(audit => audit.FailureDescription)
            .HasMaxLength(LoginAuditConsts.MaxFailureDescriptionLength)
            .IsRequired(false);
        builder.Property(audit => audit.UserAgent)
            .HasMaxLength(LoginAuditConsts.MaxUserAgentLength)
            .IsRequired(false);

        // 审计调查的主要访问路径按时间、账号和用户标识建立索引。
        builder.HasIndex(audit => audit.LoginTimeUtc);
        builder.HasIndex(audit => new { audit.Account, audit.LoginTimeUtc });
        builder.HasIndex(audit => new { audit.UserId, audit.LoginTimeUtc });

    }
}
