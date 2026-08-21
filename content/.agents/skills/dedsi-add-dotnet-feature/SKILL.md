---
name: dedsi-add-dotnet-feature
description: 按 DedsiNative 的 .NET、ABP、Clean Architecture 和 DDD 约定实现跨 Core、Infrastructure、Endpoints、Host 的纵向业务功能。用于新增聚合、完整 CRUD、领域事件或同时影响多个后端层的能力；单独 Endpoint 或持久化修改使用对应专项 Skill。
---

# 新增 Dedsi .NET 业务功能

负责纵向功能的边界、顺序和整体验收；Endpoint 与持久化细节由专项 Skill 提供，避免在本 Skill 重复维护。

## 准备

1. 将包含 `DedsiNative.slnx` 的目录作为 .NET 根，读取内容根 `AGENTS.md`、`.agents/rules/dotnet.md`、[架构约定](references/architecture.md)、目标领域文档和相邻业务模块。
2. 新建聚合或从零创建跨层模块时完整读取 [完整功能示例](references/complete-feature-example.md)；修改现有功能时优先以相邻实现为结构基准，无需加载整份示例。
3. HTTP 契约变化时使用 `dedsi-build-fastendpoint`；映射、DbContext、Repository/Query 实现或迁移变化时使用 `dedsi-efcore-persistence`。只读取实际选中 Skill 要求的 references。

## 架构约束

- 保持 `Core -> Infrastructure -> Endpoints -> Host` 的依赖方向，业务不变量只能由聚合方法维护。
- 领域事件定义与发布位于 Core；处理器位于 `Endpoints/Applications/{Feature}/EventHandlers/`，只依赖 Core 契约。
- 创建、修改、删除和完整聚合加载使用 Repository；列表、分页、统计、导出和 DTO 投影使用 Query。
- Query 契约位于 Core、实现在 Infrastructure，不返回实体、聚合或 `IQueryable`；Endpoint 和处理器不得直接操作 DbContext。
- 可复用字段约束位于聚合同目录 `{Aggregate}Consts`；一对多集合直接使用 `ICollection<T>` 属性，不建立第二套私有集合视图。
- 公共类型与成员遵循 `.agents/rules/dotnet.md` 的中文 XML 文档规则，异步调用贯穿 `CancellationToken`。
- 不手工编辑迁移和 ModelSnapshot；只有持久化形状变化时才生成迁移，未经明确要求不更新数据库。

## 工作流程

1. 确认聚合边界、主键、业务不变量、权限、查询需求、接口契约和领域事件。
2. 按 Core → Infrastructure → Endpoints → Host → tests 的顺序实现，只创建当前能力需要的抽象。
3. 在实现对应层时执行已选择的 Endpoint 或持久化 Skill；不要同时复制其示例和本 Skill 示例中的同类代码。
4. 使用 [功能完成清单](references/feature-checklist.md) 检查跨层遗漏。
5. 从内容根运行 `node .agents/skills/dedsi-add-dotnet-feature/scripts/inspect-architecture.mjs`，再运行 `dotnet build src/dotnet/DedsiNative.slnx`；存在相关测试时运行聚焦 `dotnet test`。
6. 模型变化时按持久化 Skill 生成并检查迁移，再次构建。

## 交付

说明涉及的层、接口契约、领域与持久化行为、迁移情况、验证结果和仍需确认的风险；不顺手重构无关模块。
