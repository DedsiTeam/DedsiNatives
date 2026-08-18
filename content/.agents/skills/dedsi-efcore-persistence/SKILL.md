---
name: dedsi-efcore-persistence
description: 按 DedsiNative 项目规范实现和修改 EF Core PostgreSQL 持久化，包括实体映射、DbContext/DbSet、Dedsi 仓储、查询服务、UTC 审计字段、并发控制和数据库迁移。用于新增实体、修改字段、实现 Repository/Query 或生成检查迁移。
---

# 实现 Dedsi EF Core 持久化

## 强制规则

- 将包含 `DedsiNative.slnx` 的目录作为 .NET 根；先读取内容根的 `AGENTS.md`、`.agents/rules/dotnet.md`、Core 聚合和相邻 EF Core 实现。
- 把实体映射放入 `DedsiNative.Infrastructure/EntityFrameworkCore/Configurations/`。
- 在 `IDedsiNativeDbContext` 与 `DedsiNativeDbContext` 中同步维护 DbSet。
- 在 Core 定义仓储；列表、分页、统计、导出或 DTO 投影需要专用查询时，在 Core 定义 Query 契约并在 Infrastructure 实现。
- Query 实现类必须通过主构造函数注入对应的 DbContext 接口（本项目为 `IDedsiNativeDbContext`），禁止注入具体的 `DedsiNativeDbContext`；Query 不得返回实体、聚合根或 `IQueryable`。
- Repository 实现类必须通过主构造函数注入 `IDbContextProvider<DedsiNativeDbContext> dbContextProvider`。
- 禁止 Endpoint、应用服务和事件处理器直接依赖或操作 DbContext。
- 为新增公共类、接口、属性、构造参数和方法编写中文 XML 文档注释。
- `<summary>` 必须使用多行格式，禁止将标签与正文写在同一行。
- 为 UTC、审计字段、并发令牌、索引选择和数据库特有限制补充中文说明。
- EF Core 映射中的字符串长度必须引用领域同目录的 `{Aggregate}Consts`，不得引用聚合实体上的 `MaxNameLength` 等常量，也不得在映射中重复硬编码约束值。
- 使用 EF CLI 生成迁移，不手写迁移或 ModelSnapshot。
- 一对多导航集合应直接映射聚合根上的 `ICollection<T>` 属性；不要为同一导航集合创建额外私有字段，也不要配置字段访问模式来绕过该属性。集合变更必须由聚合领域方法调用属性上的 `Add`、`Remove`、`Clear` 完成。
- 在 `DedsiNativeInfrastructureModule.ConfigureServices` 中补充 `Configure<AbpEntityOptions>`，为每个聚合根配置 `DefaultWithDetailsFunc`。查询完整领域模型或聚合明细时必须通过 `Repository.GetAsync(id, true, cancellationToken)` 查询，使仓储按聚合根的默认明细配置加载完整聚合。
- `DefaultWithDetailsFunc` 必须包含聚合根详情所需的全部子实体导航集合。例如用户加载 `LoginInfo`、`Positions`，岗位加载 `Permissions`、`Organizations`；没有导航明细的聚合（如系统、权限）也要显式配置为 `query => query`，保持所有聚合的详情查询行为一致。

具体配置方式、聚合明细选择和常见错误见 [AbpEntityOptions 配置说明](references/abp-entity-options.md)。

配置 `AbpEntityOptions` 时应同步核对聚合导航属性、EF Core 映射和完整聚合响应 DTO，禁止在 Query、Endpoint、应用服务或事件处理器中另写 `GetWithRelationsAsync` 等重复的完整聚合加载方法。

开始编码前必须完整读取 [持久化示例代码](references/persistence-examples.md)。按照示例输出目录、类型结构、Dedsi 基类、中文注释、映射配置和查询实现；只根据实际聚合调整字段、约束和查询条件。

详细映射规则见 [持久化约定](references/persistence-conventions.md)，`AbpEntityOptions` 见 [AbpEntityOptions 配置说明](references/abp-entity-options.md)，迁移命令见 [迁移流程](references/migrations.md)。

## 工作流程

1. 读取 [持久化示例代码](references/persistence-examples.md) 和目标聚合根，列出主键、字段、可空性、长度、唯一性、关系、审计和并发要求。
2. 创建 `IEntityTypeConfiguration<T>`，明确配置表、Schema、主键和全部持久化字段。
3. 同步更新 DbContext 接口与实现的 DbSet。
4. 实现仓储和所需 Query；创建、修改、删除及完整聚合查询使用 Repository，列表、分页、统计、导出和 DTO 投影使用 Query。
5. 从内容根运行 `node .agents/skills/dedsi-efcore-persistence/scripts/inspect-persistence.mjs` 做跨平台静态检查。
6. 从 .NET 根运行 `dotnet build DedsiNative.slnx`。
7. 模型变化时按迁移流程生成迁移，阅读生成内容，确认没有无关删除、重命名或类型变化。
8. 再次运行构建。除非用户明确要求，不执行 `database update`。

## 完成检查

- 映射字段与聚合属性逐项对应。
- 字符串字段均明确最大长度和必填性。
- ULID 字符串主键长度为 26。
- `ConcurrencyStamp` 明确配置并发令牌。
- PostgreSQL 时间字段使用 UTC 约定。
- Query 默认使用 `AsNoTracking()`，并在数据库端完成筛选、排序、分页、统计和 DTO 投影。
- 分页查询使用 `WhereIf` 逐项组合可选筛选条件。
- 仓储和查询实现具有中文 XML 文档。
- Query 主构造函数注入对应的 DbContext 接口，Repository 主构造函数注入 `IDbContextProvider<DedsiNativeDbContext> dbContextProvider`。
- Query 不返回实体、聚合根或 `IQueryable`。
- Endpoint、应用服务和事件处理器中不存在 DbContext 直接引用或操作。
- 迁移名称准确表达业务变更。
