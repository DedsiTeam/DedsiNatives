/// <summary>
/// 校验{{Operation}}请求。
/// </summary>
public sealed class {{Operation}}{{Name}}Validator : Validator<{{Operation}}{{Name}}Request>
{
    /// <summary>
    /// 注册当前操作的独立请求校验规则。
    /// </summary>
    public {{Operation}}{{Name}}Validator()
    {
{{Rules}}
    }

{{NestedValidators}}
}
