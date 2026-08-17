---
name: dedsi-add-dotnet-feature
description: 按 DedsiNative 的 .NET 10、ABP、Clean Architecture、FastEndpoints 与 EF Core PostgreSQL 约定新增完整业务功能。用于新增聚合根、业务模块、CRUD、查询、领域事件，或需要同时修改 Core、Infrastructure、Endpoints、Host 四层的纵向功能开发。
---

# 新增 Dedsi .NET 业务功能

## 执行原则

先读取仓库中的 `AGENTS.md`、解决方案配置和相邻业务模块，再设计和修改代码。以现有实现确认框架 API，以本 skill 的规则纠正临时代码或分层冲突。

开始编码前必须完整读取 [完整功能示例](references/complete-feature-example.md)。以示例的目录、命名、中文注释、依赖方向和代码组织为输出基准，只替换实际业务概念与字段；如用户要求与示例冲突，以用户要求为准并说明差异。

必须满足以下要求：

- 保持 `Core -> Infrastructure -> Endpoints -> Host` 的依赖方向，禁止 Core 引用 EF Core、FastEndpoints、Endpoints 或 Host。
- 领域事件定义与聚合发布放在 Core；所有 `EventHandler` 放在 `DedsiNative.Endpoints/Applications/{Feature}/EventHandlers/`，通过 Core 契约协调跨聚合副作用，禁止直接操作 `DbContext`。
- 禁止 Endpoint、应用服务和事件处理器直接操作 DbContext；创建、修改、删除以及完整领域模型或聚合明细查询使用领域仓储，列表、分页、统计、导出和 DTO 投影通过 Core 查询契约及 Infrastructure 查询实现。
- Query 接口定义在 Core，实现在 Infrastructure；实现类必须通过主构造函数注入对应的 DbContext 接口（本项目为 `IDedsiNativeDbContext`），禁止注入具体的 `DedsiNativeDbContext`，且不得返回实体、聚合根或 `IQueryable`。
- Repository 实现类必须通过主构造函数注入 `IDbContextProvider<DedsiNativeDbContext> dbContextProvider`；完整聚合查询必须调用 `Repository.GetAsync(id, true, cancellationToken)`。
- 为新增公共类、接口、方法、属性和 DTO 编写清晰的中文 XML 文档注释。
- `<summary>` 必须使用多行格式，标签和注释正文不得写在同一行；该规则同样适用于公共字段、构造函数参数和返回值说明。
- 为复杂分支、框架约束、事务意图和非显然实现补充中文行内注释；禁止只把代码翻译成注释。
- 领域字段约束常量必须放在聚合同目录的 `{Aggregate}Consts` 类中，实体本身不得声明 `MaxNameLength` 等长度常量；Infrastructure 映射和测试也统一引用该常量类。
- 将同一 Endpoint 的 Request、Response、Validator（如有）和 Endpoint 放在同一个功能目录；默认共置在同一个 `.cs` 文件。
- 使用 `CancellationToken` 贯穿异步调用。
- 聚合根的一对多子实体集合直接定义为 `ICollection<T>` 属性并使用 `private set` 初始化；领域方法直接对该属性执行 `Add`、`Remove`、`Clear`，禁止为同一集合额外创建 `_items` 私有字段或只读包装视图。
- 不手工编辑 EF Core 迁移和 ModelSnapshot。
- 修改完成后至少运行后端构建；涉及模型变化时还要生成并检查迁移。

## 工作流程

1. 定位解决方案根目录、四个项目及相邻功能，读取 [完整功能示例](references/complete-feature-example.md) 和 [架构约定](references/architecture.md)。
2. 明确聚合边界、主键类型、业务不变量、查询需求、权限要求和是否需要领域事件。
3. 在 Core 创建聚合根、领域方法和仓储接口；存在列表、分页、统计、导出或 DTO 投影需求时再创建查询接口。通过私有设置器保护状态，通过领域方法执行校验和变更。
4. 在 Infrastructure 创建实体映射、DbSet、仓储实现和查询实现。涉及持久化时同时遵循 `$dedsi-efcore-persistence`。
5. 在 Endpoints 创建 FastEndpoints 端点。涉及 API 时同时遵循 `$dedsi-build-fastendpoint`。
6. 如有副作用，在 Core 聚合中注册领域事件，并在 Endpoints 的 `Applications/{Feature}/EventHandlers/` 实现处理器；外部能力接口定义在 Core，具体实现位于 Infrastructure。
7. 按 [功能完成清单](references/feature-checklist.md) 检查四层文件、中文注释、依赖方向和遗漏项。
8. 运行 `dotnet build`。模型有变化时生成迁移，再次构建并检查迁移只包含预期变更。

## 变更边界

- 优先扩展已有抽象，不在 Endpoint 中复制领域规则。
- 查询完整领域模型或聚合明细时使用仓储 `GetAsync(id, true, cancellationToken)`；不要为完整聚合查询在 Query 中增加重复加载方法。
- 创建、修改和删除必须通过 Repository；修改或删除前可由 Repository 加载完整聚合，再调用领域方法改变状态。
- Query 默认使用 `AsNoTracking()`，并在数据库端完成筛选、排序、分页、统计和 DTO 投影。
- 分页 Query 使用 `WhereIf` 逐项组合可选筛选条件，确保每个条件对应正确的实体属性。
- 不把当前硬编码登录、未使用变量、直接 DbContext 查询等临时实现当作模板。
- 不顺手重构无关模块；若相邻代码存在问题，只在它阻塞当前功能时做最小修正并说明。
- 无明确要求时不执行数据库更新，只生成迁移并验证。

## 交付

说明新增了哪些层和能力、是否生成迁移、执行了哪些验证，以及仍未覆盖的测试或外部依赖。
