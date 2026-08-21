# .NET 项目开发规范

本规则适用于内容根下的 `src/dotnet/`。

## C# 中文注释规范

- 新增或修改的 C# 业务代码应提供清晰、准确的中文注释。注释说明业务含义、设计意图或约束原因，不得只把标识符机械翻译成中文。
- 对外可见或承载业务契约的类、接口、记录、枚举及公共、受保护成员使用中文 XML 文档注释。DTO、请求/响应模型、领域实体、仓储、服务及 Endpoint 的契约成员同样适用。
- 已编写 XML 注释的方法和构造函数使用 `<param>` 说明非显然的格式、取值范围或特殊约束；有业务含义的返回值使用 `<returns>`，按契约抛出异常时使用 `<exception>`。
- 名称和类型已经能够完整表达含义的私有字段、简单自动属性、框架样板构造函数及显而易见的实现细节无需补充注释，避免制造重复信息。
- 非公共类型或成员只要承载业务规则、领域约束、数据转换、状态流转或其他不直观逻辑，也必须提供中文 XML 文档注释。
- 在关键代码位置添加必要的中文行内注释，重点解释“为什么这样处理”，例如复杂条件、边界处理、幂等与并发控制、权限校验、事务边界、状态转换和兼容性处理。不得为显而易见的赋值、循环或语法逐行添加无信息量注释。
- 重写或实现框架成员时，如果继承的注释不能完整表达当前实现的业务语义，应补充中文 XML 文档注释，不得仅使用 `<inheritdoc />` 掩盖项目特有行为。
- 自动生成文件、第三方代码和 EF Core Migration Designer 等生成产物不要求手工补充注释；不得为了满足注释要求修改生成代码。
- `<summary>`、`<param>`、`<returns>` 和 `<exception>` 注释必须使用多行格式，禁止将内容写在标签同一行。

正确示例：

```csharp
/// <summary>
/// 用户邮箱最大长度。
/// </summary>
public const int MaxEmailLength = 256;
```

方法示例：

```csharp
/// <summary>
/// 根据当前状态确认用户启用操作。
/// </summary>
/// <param name="operatorId">
/// 执行启用操作的管理员标识。
/// </param>
/// <returns>
/// 启用后的用户状态。
/// </returns>
public UserStatus Enable(Ulid operatorId)
{
    // 已启用的用户直接返回，保证重复请求不会产生额外状态变更。
    if (Status == UserStatus.Enabled)
    {
        return Status;
    }

    Status = UserStatus.Enabled;
    return Status;
}
```

错误示例：

```csharp
/// <summary>用户邮箱最大长度。</summary>
```
