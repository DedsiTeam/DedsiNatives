---
name: work-item-loop
description: 通过 Azure DevOps MCP 预览、领取或执行 DedsiNative 远程工作项，并完成隔离分支、全栈实现、验证、PR 和状态回写闭环。
---

# Work Item Loop

Azure DevOps 是工作项、状态、验收标准和执行日志的唯一事实来源。本地不得创建需求或工作项文档。Loop 必须保持在当前 Codex 主任务中；每个工作项使用一个用户可见的独立 Codex 新任务，由该任务直接调用项目的 `ado` MCP。不得通过 `codex exec`、Node 编排器或隐藏子代理代替新任务。

## 共同准备

1. 完整读取根 `AGENTS.md` 与 [Azure DevOps 工作项协议](references/work-item-protocol.md)。
2. 先判定角色：初始提示同时给出唯一工作项 ID、attempt 和 run ID 时，当前是“独立工作项任务”；否则是“主任务”。独立任务不得再创建工作项任务，避免递归编排。
3. 确认 `ado` MCP 可用，且提供当前角色需要的查询、读取、更新、评论、Repository、PR 和 Pipeline 工具。MCP 不可用、缺少必要权限或远程写入无法确认时立即停止；禁止降级成本地文件。
4. 确认 Azure DevOps Project 与 MCP 的 `ado_mcp_project` 默认值一致，从当前 Git `origin` 识别 Repository 和目标分支；不得枚举、猜测或回退到其他 Project。

## 主任务（Loop Coordinator）

主任务保持为用户的工作台，只编排队列和独立任务，不实现工作项代码：

1. 确认当前客户端可以创建、等待和继续用户可见的独立 Codex 任务。如果没有这些能力，说明限制并停止，不回退到 CLI 子进程或隐藏子代理。
2. 用户未指定时，单次最多完成 10 项、最多并发 2 个工作项任务、每项最多尝试 3 次。用户明确给出的限制优先。
3. 通过 `wit_query` 的 `wiql` 动作查询整个 Project 中带 `codex-loop` 且处于 `codex-ready`、`codex-in-progress` 或 `codex-failed` 的工作项，再批量读取候选项的标题、标签、状态、优先级和 revision；不得依赖保存查询。用户要求预览或只读验证时，在此返回候选摘要，不领取、不写入、不创建新任务。
4. 排除主任务已创建或正在等待的工作项 ID、达到重试上限的失败项，以及不满足并发条件的 `codex-exclusive` 项。
5. 按协议优先级选择不超过空闲容量的不同工作项。只有主任务可以查询队列和领取；工作项任务禁止自行选项。
6. 领取时保留无关标签，替换 Codex 状态、阶段、attempt 与 run 标签，写入 `codex-in-progress`、`codex-stage-domain`、新的 `codex-attempt-N` 和 `codex-run-<runId>`。
7. 添加包含时间、run ID、attempt 的 Markdown 评论；更新后重新读取每个工作项确认。
8. 对每个已确认领取的工作项，在当前已保存的 Codex 项目中创建一个独立、用户可见的新任务；Git 项目默认使用 Codex worktree。标题使用 `WI #<id> - <title>`。
9. 新任务的初始提示必须包含：Project、Repository、target branch、工作项 ID、attempt、run ID，要求完整读取本 Skill 与 protocol，且由该任务直接使用 `ado` MCP 完成代码、验证、commit、push、PR、Pipeline/合并检查和终态回写。
10. 主任务记录新任务的 thread ID 与工作项 ID 映射，使用 Codex 任务等待能力监控不超过并发上限的任务。完成一项后核对返回的工作项 ID、PR 和终态，再领取下一项。
11. 新任务要求补充信息或需要人工审批时，主任务将该工作项置为 `codex-blocked`、回写证据并停止领取新项；不在主任务中接管实现。
12. 达到最大完成数、队列为空、出现协议停止条件或用户中止时结束 loop，并在主任务汇总每个独立任务的可见结果。

## 独立工作项任务

1. 只读取主任务指定的工作项 ID，核对 `codex-in-progress`、attempt、run ID、需求、业务规则、排除项和验收标准；禁止查询或领取队列中的其他工作项。
2. 编码前确认接口路径、HTTP 方法、鉴权、请求/响应、分页、状态码和错误结构；将采用契约写入工作项评论。关键契约缺失或冲突时置为 `codex-blocked`，不要猜测。
3. 领域阶段读取适用的 `docs/domains` 文档；需要更新领域事实时使用 `create-domain-doc`。本地领域文档可以存在，但不得包含工作项队列或执行状态。
4. 按范围加载 `.agents/rules/` 和后端/前端模块 Skill。一个工作项在同一分支完成所需的领域、后端、前端和验证，不拆成前后端两个工作项分支。
5. 每次阶段变化替换唯一的 `codex-stage-*` 标签并添加评论。不适用阶段必须在评论说明原因。
6. 运行 `AGENTS.md`、工作项和所选 Skill 要求的构建与聚焦测试，逐条形成可复现验收证据。
7. 验证通过后在当前 Codex worktree 的当前分支 commit 并 push，不要切换、重置或修改主工作区。通过 `ado` MCP 创建或复用该 source/target 的 PR，关联工作项并启用 autocomplete；不得绕过分支策略。
8. 直接通过 `ado` MCP 检查 PR、Pipeline 与合并状态，并回写评论和终态。新任务不得要求主任务代为调用 MCP 或执行集成步骤。

## 终态

- 验证通过但 PR 尚未合并：写为 `codex-pr`，保留 PR、commit 和验证证据。
- 可重试实现或验证失败：写为 `codex-failed` 并评论证据。
- 需要业务决定、权限、秘密、危险操作或人工解决冲突：写为 `codex-blocked`。
- 只有 PR 已按分支策略合并且 Pipeline 成功后，集成阶段才能写为 `codex-completed`、`codex-stage-done`。
- 不提交秘密，不绕过分支策略，不强推、不重置主分支、不执行破坏性数据库操作。
- 独立任务最终返回工作项 ID、终态、变更摘要、验证命令、分支、commit、PR/Pipeline 链接及阻塞项，供主任务核对。
