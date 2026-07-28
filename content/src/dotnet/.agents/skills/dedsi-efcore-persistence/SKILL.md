---
name: dedsi-efcore-persistence
description: 按 DedsiNatives 项目规范实现和修改 EF Core PostgreSQL 持久化，包括实体映射、DbContext/DbSet、Dedsi 仓储、查询服务、UTC 审计字段、并发控制和数据库迁移。用于新增实体、修改字段、实现 Repository/Query 或生成检查迁移。
---

# 实现 Dedsi EF Core 持久化

## 强制规则

- 将包含 `DedsiNative.slnx` 的目录作为 .NET 根；先读取内容根的 `AGENTS.md`、Core 聚合和相邻 EF Core 实现。
- 把实体映射放入 `DedsiNative.Infrastructure/EntityFrameworkCore/Configurations/`。
- 在 `IDedsiNativeDbContext` 与 `DedsiNativeDbContext` 中同步维护 DbSet。
- 在 Core 定义仓储；列表、分页或导出需要专用投影时定义查询契约并在 Infrastructure 实现。
- 禁止 Host Endpoint 直接依赖 DbContext。
- 为新增公共类、接口、属性、构造参数和方法编写中文 XML 文档注释。
- 为 UTC、审计字段、并发令牌、索引选择和数据库特有限制补充中文说明。
- 使用 EF CLI 生成迁移，不手写迁移或 ModelSnapshot。

开始编码前必须完整读取 [持久化示例代码](references/persistence-examples.md)。按照示例输出目录、类型结构、Dedsi 基类、中文注释、映射配置和查询实现；只根据实际聚合调整字段、约束和查询条件。

详细映射规则见 [持久化约定](references/persistence-conventions.md)，迁移命令见 [迁移流程](references/migrations.md)。

## 工作流程

1. 读取 [持久化示例代码](references/persistence-examples.md) 和目标聚合根，列出主键、字段、可空性、长度、唯一性、关系、审计和并发要求。
2. 创建 `IEntityTypeConfiguration<T>`，明确配置表、Schema、主键和全部持久化字段。
3. 同步更新 DbContext 接口与实现的 DbSet。
4. 实现仓储；列表、分页或导出使用 Query，单条详情由 Endpoint 通过仓储 `GetAsync` 加载完整聚合。
5. 运行 `scripts/inspect-persistence.ps1` 做静态检查。
6. 从 .NET 根运行 `dotnet build DedsiNative.slnx`。
7. 模型变化时按迁移流程生成迁移，阅读生成内容，确认没有无关删除、重命名或类型变化。
8. 再次运行构建。除非用户明确要求，不执行 `database update`。

## 完成检查

- 映射字段与聚合属性逐项对应。
- 字符串字段均明确最大长度和必填性。
- ULID 字符串主键长度为 26。
- `ConcurrencyStamp` 明确配置并发令牌。
- PostgreSQL 时间字段使用 UTC 约定。
- 查询使用 `AsNoTracking()` 并在数据库端投影。
- 分页查询使用 `WhereIf` 逐项组合可选筛选条件。
- 仓储和查询实现具有中文 XML 文档。
- Host Endpoints 中不存在 DbContext 直接引用。
- 迁移名称准确表达业务变更。
