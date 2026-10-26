namespace {{Namespace}};

/// <summary>
/// {{Description}}
/// </summary>
public sealed record {{Name}}
{
{{OrmConstructor}}
    /// <summary>
    /// 创建不可变的领域数据。
    /// </summary>
{{ParameterComments}}    public {{Name}}({{Parameters}})
    {
{{Assignments}}
    }

{{Properties}}
}
