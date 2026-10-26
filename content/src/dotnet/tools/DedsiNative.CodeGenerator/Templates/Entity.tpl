namespace {{Namespace}};

/// <summary>
/// {{Description}}
/// </summary>
public sealed class {{Name}}
{
    /// <summary>
    /// 供 ORM 实例化使用。
    /// </summary>
    private {{Name}}()
    {
    }

    /// <summary>
    /// 创建实体；由聚合统一管理其持久化生命周期。
    /// </summary>
{{ParameterComments}}    public {{Name}}({{ConstructorParameters}})
    {
{{IdValidation}}
        Id = id;
{{Assignments}}
    }

    /// <summary>
    /// 业务实体主键，与设计文档 ID 无关。
    /// </summary>
    public {{KeyType}} Id { get; private set; } = default!;

{{Properties}}
}
