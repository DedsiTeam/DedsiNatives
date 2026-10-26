/// <summary>
/// 校验请求的必填引用及嵌套对象结构；业务规则由领域实现。
/// </summary>
private sealed class {{Type}}RequestValidator : AbstractValidator<{{Type}}Request>
{
    /// <summary>
    /// 注册结构校验规则。
    /// </summary>
    public {{Type}}RequestValidator()
    {
{{Rules}}
    }
}
