---
name: create-requirement-work-item
description: 根据用户直接需求、现有代码与 Azure DevOps 队列，通过 MCP 创建或整理可独立验收的远程全栈工作项。
---

# 创建需求工作项

Azure DevOps 是工作项唯一事实来源；不得创建 `docs/workItems`、需求 Markdown 或本地工作项副本。

## 准备

1. 完整读取根 `AGENTS.md` 与 `work-item-loop/references/work-item-protocol.md`。
2. 取得 Azure DevOps Project，通过 MCP 确认需求工作项类型、现有 backlog、重复项与依赖。类型或 Project 不明确时停止，不得猜测。
3. 检查相关代码与已有接口/页面契约；用户明确表达的领域、字段、流程、范围和排除项是事实，推断内容必须标记待领域阶段确认。

## 边界

- 按可独立验收的业务能力拆分，每项覆盖所需领域、后端、前端和验证。
- 不按技术层、页面局部或聚合内部实体拆分；不同聚合或独立交付能力可以拆分。
- 不把推断的唯一性、长度、删除策略、权限、状态流转或聚合结构写成已确认事实。

## MCP 创建流程

1. 使用 `wit_work_item_write` 的 `create` 创建团队实际采用的需求工作项类型。
2. 填写标题、Description、Acceptance Criteria、用户已确认规则、范围、排除项和待决事项。
3. 添加 `codex-loop; codex-draft; codex-stage-backlog; codex-attempt-0`。
4. 添加 Markdown 评论，记录来源、拆分原因、依赖和待确认事项；不得记录秘密。
5. 重新读取确认字段与标签，返回工作项 ID 和 URL。

除非用户明确批准实施，新项保持 `codex-draft`，不得自动改为 `codex-ready` 或启动 Loop。只通过 MCP 修改 Azure DevOps。
