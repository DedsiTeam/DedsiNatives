# .NET 项目开发规范

本规则适用于 `src/dotnet/`，并叠加仓库根 `AGENTS.md`。

## Query 契约与 DTO 命名规范

- 新增或修改 Query 契约时，统一使用下表中的命名，不再引入无后缀的查询入参、`QueryResult` 出参或无 `Dto` 后缀的查询明细类型。

| 用途 | 命名规则 | 示例 |
| --- | --- | --- |
| 查询接口 | `I{领域名}Query` | `IDictionaryQuery` |
| 查询实现 | `{领域名}Query` | `DictionaryQuery` |
| 查询入参 | `{领域名}{查询用途}QueryInput` | `DictionaryPagedQueryInput`、`OrganizationTreeQueryInput` |
| 查询出参包装 | `{领域名}{查询用途}QueryOutput` | `DictionaryPagedQueryOutput` |
| 查询结果明细 DTO | `{领域名}QueryItemDto` | `DictionaryQueryItemDto` |
| HTTP 请求 / 响应 | `XxxRequest` / `XxxResponse` | `PagedDictionaryRequest`、`PagedDictionaryResponse` |

- `Input`、`Output` 已表达契约用途，不再追加 `Dto`；DTO 后缀统一拼写为 `Dto`，不使用 `DTO`。
- Query 接口及其入参、出参和明细 DTO 保留在 `DedsiNative.Core` 对应领域目录，由 Infrastructure 实现查询，Endpoints 使用这些契约编排 HTTP 请求。
- HTTP 请求 / 响应模型与内部 Query 契约按职责区分；需要转换时由 Endpoint 完成，不为减少字段重复而强行合并。
- 分页查询使用 `XxxPagedQueryOutput` 包装总数与明细数组；只返回集合的查询可直接返回 `XxxQueryItemDto[]`，不为统一后缀而新增无意义的 Output 包装。标量参数、布尔返回值及已有领域实体类型也无需为命名规则额外包装。
- 重命名时同步更新接口、实现、Endpoint、测试及其他引用，检查旧名称无残留。

## Query 条件筛选规范

- 简单的可选筛选统一使用链式 `WhereIf(condition, predicate)`，不使用重复的 `if` 加 `query = query.Where(...)`。固定必选条件仍使用 `Where`。
- 字符串筛选参数先在查询表达式外通过 `input.Xxx?.Trim()` 归一化，再使用 `!string.IsNullOrEmpty(xxx)` 判断是否启用条件；空值、空字符串及纯空白字符串均不参与筛选。业务明确需要保留空白或匹配空值时，遵循该字段的业务语义。
- 保留原有精确匹配、模糊匹配和数据库字段空值保护。只有启用条件已保证参数非空时，才在 `Contains(xxx!)` 等表达式中使用空值抑制符。
- 条件筛选保持在 `IQueryable` 上执行，在计数、排序、分页及物化之前完成，不为使用 `WhereIf` 提前调用 `ToArray` 或 `AsEnumerable`。
- 分支涉及多步操作、不同查询结构或其他业务流程时，保留清晰的 `if`，不强行改写为 `WhereIf`。

## namespace 规范

- namespace 统一为项目的有效 `RootNamespace` 加文件所在的项目内相对目录，各目录层级以 `.` 连接；项目根目录的类型直接使用 `RootNamespace`。
- 优先使用项目或共享构建配置中声明的 `RootNamespace`；未显式配置时使用 SDK 默认值（通常为项目名）。不要直接把程序集名或项目文件夹名当作 namespace 前缀。
- 例如 `DedsiNative.Infrastructure` 的 `RootNamespace` 为 `DedsiNative`：`StorageFiles/MinioStorageProvider.cs` 使用 `DedsiNative.StorageFiles`，`EntityFrameworkCore/Queries/DictionaryQuery.cs` 使用 `DedsiNative.EntityFrameworkCore.Queries`，不得额外插入 `Infrastructure`。
- `DedsiNative.Endpoints` 的 `Endpoints/DictionaryEndpoints/` 使用 `DedsiNative.Endpoints.DictionaryEndpoints`，其中 `Endpoints` 来自项目内目录；`DedsiNative.AuthServer` 的根命名空间为 `DedsiNative.AuthServer`，须保留该前缀。
- 新建或移动文件时检查 namespace，同步更新 `using`、类型全限定名及其他引用；`RootNamespace` 不会自动改写已有 C# 源码中的 namespace。
- 自动生成文件和第三方代码遵循其生成工具或来源约定，不为调整 namespace 手工修改 EF Core Migration、Designer 或 ModelSnapshot。

## C# 中文注释规范

- 领域模型的多参数构造函数、方法及较长的调用参数列表按每个参数一行排版；方法体和构造函数体展开，不将多条语句挤在同一行。该规则针对代码参数列表，不改变下述 XML 注释格式。

- 新增或修改的 C# 业务代码应提供清晰、准确的中文注释。注释说明业务含义、设计意图或约束原因，不得只把标识符机械翻译成中文。
- 对外可见或承载业务契约的类、接口、记录、枚举及公共、受保护成员使用中文 XML 文档注释。DTO、请求/响应模型、领域实体、仓储、服务及 Endpoint 的契约成员同样适用。
- 已编写 XML 注释的方法和构造函数使用 `<param>` 说明非显然的格式、取值范围或特殊约束；有业务含义的返回值使用 `<returns>`，按契约抛出异常时使用 `<exception>`。
- 名称和类型已经能够完整表达含义的私有字段、简单自动属性、框架样板构造函数及显而易见的实现细节无需补充注释，避免制造重复信息。
- 非公共类型或成员只要承载业务规则、领域约束、数据转换、状态流转或其他不直观逻辑，也必须提供中文 XML 文档注释。
- 在关键代码位置添加必要的中文行内注释，重点解释“为什么这样处理”，例如复杂条件、边界处理、幂等与并发控制、权限校验、事务边界、状态转换和兼容性处理。不得为显而易见的赋值、循环或语法逐行添加无信息量注释。
- 重写或实现框架成员时，如果继承的注释不能完整表达当前实现的业务语义，应补充中文 XML 文档注释，不得仅使用 `<inheritdoc />` 掩盖项目特有行为。
- 自动生成文件、第三方代码和 EF Core Migration Designer 等生成产物不要求手工补充注释；不得为了满足注释要求修改生成代码。
- `<summary>` 统一使用多行格式，字段、属性和枚举成员的说明必须换行。方法和构造函数的 `<param>`、`<returns>`、`<exception>` 使用单行格式；record 的类型级参数说明同样使用单行 <param>；位置参数列表本身每个参数一行。注释内容必须完整，不保留空标签。

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
/// <param name="operatorId">执行启用操作的管理员标识。</param>
/// <returns>启用后的用户状态。</returns>
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

## 数据与集合返回规范

- 当方法、服务或 Endpoint 需要返回一组/多项数据（集合、列表等）时，统一使用**数组**（如 `T[]`）返回。
- 避免使用 `List<T>`、`IEnumerable<T>` 或 `IReadOnlyList<T>` 等作为返回类型契约。
- 在数据查询、排序或投影处理末尾，统一调用 `.ToArray()` 或以数组形式返回。

示例：

```csharp
// 正确示例：返回数组
public sealed class GetDictionaryItemsEndpoint(IDictionaryRepository dictionaryRepository)
    : EndpointWithoutRequest<DictionaryItemResponse[]>
{
    public override async Task HandleAsync(CancellationToken ct)
    {
        var dictionaryId = Route<string>("dictionaryId")!;
        var dictionary = await dictionaryRepository.GetAsync(dictionaryId, true, ct);
        var items = dictionary.Items
            .OrderBy(item => item.ParentId)
            .ThenBy(item => item.Sort)
            .ThenBy(item => item.Id)
            .Select(DictionaryEndpointMappings.ToResponse)
            .ToArray();

        await Send.OkAsync(items, ct);
    }
}
```

## 时间与 HTTP 契约

- 业务 API 通过 `ApiEndpointConfiguration.Configure` 统一设置 `dedsinative-api` 路由前缀，并保留时间序列化配置。Endpoint 只声明业务相对路径，例如 `Post("dictionary/pagedQuery")`，不得重复写 `/api` 或 `/dedsinative-api`。
- 特殊接口可在 Endpoint 中用 `RoutePrefixOverride("特殊前缀")` 覆盖，或用 `RoutePrefixOverride(string.Empty)` 取消前缀；不要在全局 Configurator 中覆盖所有 Endpoint 的前缀。
- 后端生成业务 API 链接时使用 `ApiEndpointConfiguration.RoutePrefix`；调整前缀时同步更新前端 API Service、开发代理和路由测试。模板通过 `sourceName: "DedsiNative"` 的小写形式替换 `dedsinative`。
- 业务日期时间从源头使用北京时间（Asia/Shanghai、UTC+08:00）：`DateTime.Now`、PostgreSQL `timestamp without time zone`、接口 `yyyy-MM-dd HH:mm:ss.FFFFFFF`。运行环境不在北京时间时阻止业务服务启动；不得把业务时间转成 UTC 或兼容 ISO/offset 输入。
- JWT、OAuth 和外部协议的数值时间按协议处理，在业务响应边界转换成北京时间，不将协议 UTC 值直接作为业务时间返回。
- HTTP 方法按现有接口和业务语义选择，不限制为 GET/POST；变更接口前核对调用方及公开契约。
