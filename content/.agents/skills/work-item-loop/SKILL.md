---
name: work-item-loop
description: 通过 Azure DevOps MCP 预览、领取或执行 DedsiNative 远程工作项，并完成隔离分支、全栈实现、验证、PR 和状态回写闭环。
---

# Work Item Loop

Azure DevOps 是工作项、状态、验收标准和执行日志的唯一事实来源。本地不得创建需求或工作项文档。一个 Worker 只处理 Dispatcher 明确分配的一个工作项；连续与并发处理由 `agent-loop.mjs` 管理。

## 共同准备

1. 完整读取根 `AGENTS.md` 与 [Azure DevOps 工作项协议](references/work-item-protocol.md)。
2. 检查 `git status --short`，保留无关和用户已有改动。
3. 确认 `ado` MCP 可用，并且至少提供工作项查询、读取、更新和评论工具。MCP 不可用、缺少写权限或远程写入无法确认时立即返回错误；禁止降级成本地文件。
4. 从调用方取得 Azure DevOps Project、当前 Git remote 自动识别的 Repository、run ID、并发容量和最大尝试次数；Project 必须与 MCP 的 `ado_mcp_project` 默认值一致，不得枚举、猜测或回退到其他 Project。

## Dispatcher 模式

Dispatcher 只操作 Azure DevOps，不修改本地文件：

1. 通过 `wit_query` 的 `wiql` 动作查询整个 Project 中带 `codex-loop` 且处于 `codex-ready`、`codex-in-progress` 或 `codex-failed` 的工作项，再批量读取候选项的标题、标签、状态、优先级和 revision；不得依赖保存查询。
2. 排除调用方列出的运行中 ID、达到重试上限的失败项，以及不满足并发条件的 `codex-exclusive` 项。
3. 按协议优先级选择不超过空闲容量的不同工作项。只有 Dispatcher 可以领取；Worker 禁止自行选项。
4. 领取时保留无关标签，替换 Codex 状态、阶段、attempt 与 run 标签，写入 `codex-in-progress`、`codex-stage-domain`、新的 `codex-attempt-N` 和 `codex-run-<runId>`。
5. 添加包含时间、run ID、attempt 的 Markdown 评论；更新后重新读取每个工作项确认。
6. 返回调用方 JSON Schema 要求的 `claimed`、`empty`、`blocked` 或 `error`，不得附加自然语言。

## Worker 模式

1. 只读取调用方指定的工作项 ID，核对 `codex-in-progress`、attempt、需求、业务规则、排除项和验收标准。
2. 编码前确认接口路径、HTTP 方法、鉴权、请求/响应、分页、状态码和错误结构；将采用契约写入工作项评论。关键契约缺失或冲突时置为 `codex-blocked`，不要猜测。
3. 领域阶段读取适用的 `docs/domains` 文档；需要更新领域事实时使用 `create-domain-doc`。本地领域文档可以存在，但不得包含工作项队列或执行状态。
4. 按范围加载 `.agents/rules/` 和后端/前端模块 Skill。一个工作项在同一分支完成所需的领域、后端、前端和验证，不拆成前后端两个工作项分支。
5. 每次阶段变化替换唯一的 `codex-stage-*` 标签并添加评论。不适用阶段必须在评论说明原因。
6. 运行 `AGENTS.md`、工作项和所选 Skill 要求的构建与聚焦测试，逐条形成可复现验收证据。
7. Worker 不 commit、push、创建 PR 或合并；外层 Dispatcher 在收到 `verified` 后处理 Git 与 PR。

## 终态

- Worker 验证通过：保持 `codex-in-progress`，返回 `verified` 与验证证据，等待外层集成。
- 可重试实现或验证失败：写为 `codex-failed` 并评论证据。
- 需要业务决定、权限、秘密、危险操作或人工解决冲突：写为 `codex-blocked`。
- 只有 PR 已按分支策略合并且 Pipeline 成功后，集成阶段才能写为 `codex-completed`、`codex-stage-done`。
- 不提交秘密，不绕过分支策略，不强推、不重置主分支、不执行破坏性数据库操作。
