---
name: dedsi-build-fastendpoint
description: 按 DedsiNative 项目约定创建、修改和审查 FastEndpoints API，包括 Request、Response、Validator、Endpoint、路由、认证、分页和查询。用于在 DedsiNative.Endpoints 项目新增 CRUD、详情、分页、导出或匿名接口。
---

# 开发 Dedsi FastEndpoint

## 强制规则

- 将包含 `DedsiNative.slnx` 的目录作为 .NET 根；先读取内容根的 `AGENTS.md`、Endpoints 模块、Host 模块和同功能相邻 Endpoint。
- 将端点放入 `DedsiNative.Endpoints/{Feature}Endpoints/`。
- 每个 Endpoint 使用独立 `.cs` 文件。
- 将该 Endpoint 的 Request、Response 和 Endpoint 默认定义在同一个 `.cs` 文件；需要独立 Validator 时，也必须放在同一个功能目录。
- 为 Request、Response、Validator、Endpoint、公共属性和重写方法编写清晰的中文 XML 文档注释。
- `<summary>` 必须使用多行格式，禁止使用 `/// <summary>说明。</summary>` 单行写法。
- 为授权例外、特殊状态码、复杂过滤和框架限制编写中文行内注释。
- 禁止 Endpoint、应用服务和事件处理器注入或操作 `IDedsiNativeDbContext` 或具体 DbContext。
- 创建、修改、删除及完整领域模型或聚合明细查询使用领域仓储；列表、分页、统计、导出和 DTO 投影使用 Core 查询契约。
- 所有异步数据库和响应调用传递 `CancellationToken`。

开始编码前必须完整读取 [Endpoint 示例代码](references/endpoint-examples.md)。按照示例输出文件结构、类型顺序、命名、依赖注入、中文注释和异步调用；只根据实际业务调整路由、DTO 字段和领域调用。

详细文件布局和通用规则见 [Endpoint 约定](references/endpoint-conventions.md)。

## 开发流程

1. 读取 [Endpoint 示例代码](references/endpoint-examples.md)，再确认功能目录、路由、HTTP 方法、权限、输入、输出和错误状态。
2. 选择 `Endpoint<TRequest,TResponse>`、`EndpointWithoutRequest<TResponse>` 或相邻代码使用的其他基类。
3. 在一个端点文件内依次定义 Request、Response 和 Endpoint；没有请求或响应时省略对应 DTO。
4. 使用结构化 DTO，不直接返回领域实体。
5. 在 `Configure()` 中声明路由和认证；只有明确的公开接口才调用 `AllowAnonymous()`。
6. 在 `HandleAsync()` 中完成编排，把业务规则留在聚合；完整领域模型或聚合明细通过仓储 `GetAsync(id, true, cancellationToken)` 加载，列表、分页、统计、导出和 DTO 投影留在 Query 实现。
7. 检查路由参数、筛选字段、排序、分页、空值和状态码。
8. 运行 `dotnet build`，并说明尚未执行的集成测试。

## 查询与命令

- 创建、修改、删除：注入 `I{Aggregate}Repository`；修改或删除前可由 Repository 加载完整聚合，再调用领域方法改变状态。
- 完整领域模型或聚合明细：注入 `I{Aggregate}Repository`，调用 `GetAsync(id, true, cancellationToken)`；不得在 Query 中重复实现完整聚合加载。
- 列表、分页、统计、导出和 DTO 投影：注入 `I{Aggregate}Query`，由 Infrastructure 使用 DbContext 接口完成数据库端筛选、排序、分页、统计和投影。
- 分页 Query 使用 `WhereIf` 逐项组合可选筛选条件。
- Query 默认使用 `AsNoTracking()`，不返回实体、聚合根或 `IQueryable`。
- 不跨过 Core 的 Query/Repository 契约直接使用 DbContext。
- 不在 Endpoint 重复字段长度、不变量或状态转换等领域规则。

## 完成检查

- 检查 Request、Response、Endpoint 是否在同一文件或至少同一功能目录。
- 检查所有公共类型和成员是否有准确中文注释。
- 检查邮箱等筛选条件是否使用正确属性，避免复制条件后忘记修改字段。
- 检查受保护端点没有误加 `AllowAnonymous()`。
- 检查没有硬编码账号、密码、JWT Secret 或连接字符串。
- 检查没有未使用的依赖和临时变量。
