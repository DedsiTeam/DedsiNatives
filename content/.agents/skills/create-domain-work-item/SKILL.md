---
name: create-domain-work-item
description: 根据 DedsiNative 指定的 docs/domains 中文领域文档，通过 Azure DevOps MCP 创建或整理可独立验收的远程全栈工作项。
---

# 创建领域工作项

Azure DevOps 是工作项唯一事实来源；不得创建 `docs/workItems` 或其他本地需求副本。

## 前置条件

- 调用方必须明确指定存在于 `docs/domains/` 的领域文档，并完整读取。
- 必须取得 Azure DevOps Project；通过 MCP 检查团队使用的需求工作项类型。类型不唯一且无法从现有 backlog 判断时停止询问，不得猜测。
- 完整读取根 `AGENTS.md` 与 `work-item-loop/references/work-item-protocol.md`，确认 ado MCP 具有查询、创建、更新和评论权限。

## 形成工作项

- 按可独立验收的业务能力拆分，不按技术层、页面局部或同一聚合内部实体拆分。
- 聚合根及内部子实体保持同一项；CRUD 只有具备独立业务价值时才拆分。
- 领域文档中的确认事实进入 Description/Acceptance Criteria；建议和待确认内容保持显式待决，不擅自固化。
- 至少覆盖适用的领域不变量、持久化、Endpoint 契约、前端类型安全体验、构建与测试证据。

## MCP 创建流程

1. 使用保存查询、全文搜索或 backlog 工具检查重复项和依赖。
2. 使用 `wit_work_item_write` 的 `create` 创建团队实际使用的需求工作项类型。
3. 填写 `System.Title`、`System.Description`、Acceptance Criteria、Area/Iteration（调用方提供时）和优先级。
4. 添加 `codex-loop; codex-draft; codex-stage-backlog; codex-attempt-0`，保留 Azure DevOps 业务标签约定。
5. 添加 Markdown 评论，记录领域来源、拆分理由、未决事项和进入 `codex-ready` 前的评审条件；不得记录秘密。
6. 重新读取工作项，验证字段、标签、链接和唯一性，返回 ID 与 URL。

除非用户明确批准实施，禁止自动把 `codex-draft` 改成 `codex-ready`，也不启动 Loop。只通过 MCP 修改 Azure DevOps；领域事实本身需要修改时改用 `create-domain-doc`。
