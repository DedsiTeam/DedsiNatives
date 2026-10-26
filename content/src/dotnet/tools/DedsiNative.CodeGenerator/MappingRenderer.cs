namespace DedsiNative.CodeGenerator;

/// <summary>
/// 把显式领域引用渲染成 owned 值对象或一对多关系；不从说明文字推断映射。
/// </summary>
public sealed class MappingRenderer(DomainElement[] elements, TemplateRenderer renderer)
{
    /// <summary>
    /// 递归渲染标量字段及值对象字段；循环引用已在输入校验阶段拒绝。
    /// </summary>
    public string Fields(DomainElement element, string builder, int depth = 0)
    {
        var fragments = new List<string>();
        foreach (var field in element.Fields.Where(f => element.Kind == "value_object" || f.Name != "Id"))
        {
            var type = DomainReader.Resolve(field.DataType, elements, element.Name + "." + field.Name);
            var values = new Dictionary<string, string>
            {
                ["Builder"] = builder, ["Name"] = field.Name,
                ["Required"] = type.Nullable ? "false" : "true",
                ["ColumnType"] = field.DataType.TrimEnd('?') is "DateTime" or "DateTimeOffset" ? ".HasColumnType(\"timestamp without time zone\")" : ""
            };
            if (type.Target is null) fragments.Add(renderer.Render("FieldConfiguration", values));
            else if (type.Collection)
            {
                values["ForeignKey"] = element.Name + "Id";
                fragments.Add(renderer.Render("CollectionConfiguration", values));
            }
            else
            {
                values["OwnedBuilder"] = "owned" + depth;
                values["Fields"] = Fields(type.Target, "owned" + depth, depth + 1);
                fragments.Add(renderer.Render("OwnedConfiguration", values));
            }
        }
        return string.Join("\n", fragments);
    }

    /// <summary>
    /// 标记当前实体是否有唯一的持久化父级。
    /// </summary>
    public bool HasOwner(DomainElement target) => FindOwner(target) is not null;

    /// <summary>
    /// 明确影子外键的类型和必填约束；已有匹配字段时复用该字段。
    /// </summary>
    public string ForeignKey(DomainElement target)
    {
        var owner = FindOwner(target);
        if (owner is null) return "";
        var key = DomainReader.KeyType(owner);
        return renderer.Render("ForeignKeyConfiguration", new()
        {
            ["ForeignKey"] = owner.Name + "Id", ["KeyType"] = DomainReader.TypeName(key, owner.Name + ".Id"),
            ["KeyLength"] = key == "string" ? ".HasMaxLength(26)" : ""
        });
    }

    private DomainElement? FindOwner(DomainElement target) => elements.FirstOrDefault(e =>
        e.Kind is "aggregate_root" or "entity" && e.Fields.Any(f =>
        {
            var type = DomainReader.Resolve(f.DataType, elements, f.Name);
            return type.Collection && type.Target == target;
        }));
}
