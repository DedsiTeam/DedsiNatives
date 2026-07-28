---
name: dedsi-add-dotnet-feature
description: 按 DedsiNatives 的 .NET 10、ABP、Clean Architecture、FastEndpoints 与 EF Core PostgreSQL 约定新增完整业务功能。用于新增聚合根、业务模块、CRUD、查询、领域事件，或需要同时修改 Core、Infrastructure、Host 三层的纵向功能开发。
---

# 新增 Dedsi .NET 业务功能

## 执行原则

先读取仓库中的 `AGENTS.md`、解决方案配置和相邻业务模块，再设计和修改代码。以现有实现确认框架 API，以本 skill 的规则纠正临时代码或分层冲突。

开始编码前必须完整读取 [完整功能示例](references/complete-feature-example.md)。以示例的目录、命名、中文注释、依赖方向和代码组织为输出基准，只替换实际业务概念与字段；如用户要求与示例冲突，以用户要求为准并说明差异。

必须满足以下要求：

- 保持 `Core -> Infrastructure -> Host` 的依赖方向，禁止 Core 引用 EF Core、FastEndpoints 或 Host。
- 禁止 Endpoint 直接操作 DbContext；创建、详情、更新、删除使用领域仓储，列表、分页和导出通过 Core 查询契约及 Infrastructure 查询实现。
- 为新增公共类、接口、方法、属性和 DTO 编写清晰的中文 XML 文档注释。
- 为复杂分支、框架约束、事务意图和非显然实现补充中文行内注释；禁止只把代码翻译成注释。
- 将同一 Endpoint 的 Request、Response、Validator（如有）和 Endpoint 放在同一个功能目录；默认共置在同一个 `.cs` 文件。
- 使用 `CancellationToken` 贯穿异步调用。
- 不手工编辑 EF Core 迁移和 ModelSnapshot。
- 修改完成后至少运行后端构建；涉及模型变化时还要生成并检查迁移。

## 工作流程

1. 定位解决方案根目录、三个项目及相邻功能，读取 [完整功能示例](references/complete-feature-example.md) 和 [架构约定](references/architecture.md)。
2. 明确聚合边界、主键类型、业务不变量、查询需求、权限要求和是否需要领域事件。
3. 在 Core 创建聚合根、领域方法和仓储接口；存在列表、分页或导出需求时再创建查询接口。通过私有设置器保护状态，通过领域方法执行校验和变更。
4. 在 Infrastructure 创建实体映射、DbSet、仓储实现和查询实现。涉及持久化时同时遵循 `$dedsi-efcore-persistence`。
5. 在 Host 创建 FastEndpoints 端点。涉及 API 时同时遵循 `$dedsi-build-fastendpoint`。
6. 如有副作用，在聚合中注册本地领域事件；在 Core 定义外部能力接口，在 Infrastructure 实现该接口。
7. 按 [功能完成清单](references/feature-checklist.md) 检查三层文件、中文注释、依赖方向和遗漏项。
8. 运行 `dotnet build`。模型有变化时生成迁移，再次构建并检查迁移只包含预期变更。

## 变更边界

- 优先扩展已有抽象，不在 Endpoint 中复制领域规则。
- 详情 Endpoint 使用仓储 `GetAsync` 加载完整聚合；不要为单条聚合详情在 Query 中增加投影方法。
- 分页 Query 使用 `WhereIf` 逐项组合可选筛选条件，确保每个条件对应正确的实体属性。
- 不把当前硬编码登录、未使用变量、直接 DbContext 查询等临时实现当作模板。
- 不顺手重构无关模块；若相邻代码存在问题，只在它阻塞当前功能时做最小修正并说明。
- 无明确要求时不执行数据库更新，只生成迁移并验证。

## 交付

说明新增了哪些层和能力、是否生成迁移、执行了哪些验证，以及仍未覆盖的测试或外部依赖。
