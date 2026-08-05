# .NET 项目开发规范

## XML 文档注释

- 所有公共类、接口、方法、属性、字段、构造函数、参数和返回值都必须提供清晰的中文 XML 文档注释。
- `<summary>` 注释必须使用多行格式，禁止将内容写在 `<summary>` 标签同一行。

正确示例：

```csharp
/// <summary>
/// 用户邮箱最大长度。
/// </summary>
public const int MaxEmailLength = 256;
```
错误示例：

```csharp
/// <summary>用户邮箱最大长度。</summary>
```
