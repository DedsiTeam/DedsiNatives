using Dedsi.Ddd.Domain.Entities;

namespace {{Namespace}};

/// <summary>
/// {{Description}}
/// </summary>
public class {{Name}} : DedsiAggregateRoot<{{KeyType}}>
{
    /// <summary>
    /// 供 EF Core 实例化使用。
    /// </summary>
    protected {{Name}}()
    {
    }

    /// <summary>
    /// 创建聚合；具体业务约束应在领域方法中补充。
    /// </summary>
{{ParameterComments}}    public {{Name}}({{ConstructorParameters}})
        : base(id)
    {
{{IdValidation}}
{{Assignments}}
    }

{{UpdateMethod}}
{{Properties}}
}
