using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using {{Namespace}};

namespace DedsiNative.EntityFrameworkCore.Configurations;

/// <summary>
/// {{Name}} 的字段与显式关系映射；业务索引、长度和精度需按业务补充。
/// </summary>
public sealed class {{Name}}Configuration : IEntityTypeConfiguration<{{Name}}>
{
    /// <summary>
    /// 配置主键、字段可空性与已有框架约定。
    /// </summary>
    /// <param name="builder">当前实体类型的 EF 映射构建器。</param>
    public void Configure(EntityTypeBuilder<{{Name}}> builder)
    {
        builder.ToTable("{{Name}}", DedsiNativeCoreConsts.DbSchemaName);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id){{KeyLength}}.ValueGeneratedNever().IsRequired();
{{FieldConfigurations}}
{{AuditConfiguration}}
    }
}
