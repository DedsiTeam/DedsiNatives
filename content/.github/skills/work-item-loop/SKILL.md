---
name: work-item-loop
description: 通过 Azure DevOps MCP 预览、领取或串行执行 DedsiNative 远程工作项，并完成独立分支、全栈实现、验证、PR 和状态回写闭环。
---

# Work Item Loop

Azure DevOps 是工作项、状态、验收标准和执行日志的唯一事实来源。本地不得创建需求、工作项副本或 Loop 日志。Loop 在当前 GitHub Copilot Agent 会话中运行；为避免同一 checkout 上的分支和文件冲突，同一时刻只执行一个工作项，不启动后台 `copilot` 进程，也不让多个可写代理并行实现。

## 准备

1. 完整读取根 `.github/copilot-instructions.md` 与 [Azure DevOps 工作项协议](references/work-item-protocol.md)。
2. 确认 `ado` MCP 提供查询、读取、更新、评论、Repository、PR 和 Pipeline 所需工具。MCP 不可用、权限不足或写入无法重读确认时立即停止，不降级成本地文件。
3. 确认远程 MCP 连接的 Organization 为 `{{ADO_ORG}}`，所有工具调用显式使用 Project `{{ADO_PROJECT}}`，并从当前 Git `origin` 识别 Repository 和目标分支；不得枚举、猜测或回退到其他 Project。
4. 确认工作区没有会被切分支影响的未提交改动。存在用户改动时停止，不暂存、不提交、不隐藏、不覆盖。
5. 用户未指定时，单次最多完成 10 项，每项最多尝试 3 次；执行始终串行。

## 预览与领取

1. 使用 `wit_query` 的 `wiql` 动作查询 Project 中带 `copilot-loop`，且处于 `copilot-ready`、`copilot-in-progress` 或 `copilot-failed` 的工作项，再批量读取标题、标签、状态、优先级和 revision；不得依赖保存查询。
2. 用户要求预览或只读验证时，只返回候选摘要，不领取、不写入、不切换分支。
3. 排除达到重试上限、已有活动 PR、无法安全恢复分支或与当前运行标识冲突的工作项。按协议优先级只选择一个工作项。
4. 领取时保留无关标签，替换唯一状态、阶段、attempt 与 run 标签，写入 `copilot-in-progress`、`copilot-stage-domain`、新的 `copilot-attempt-N` 和 `copilot-run-<runId>`。
5. 添加包含 ISO 8601 时间、run ID、attempt 和目标分支的 Markdown 评论；更新后重新读取并确认 revision、状态和标签。确认失败不得开始编码。

## 执行一个工作项

1. 只处理已领取的工作项 ID，读取需求、业务规则、排除项、验收标准和评论；不得在实现期间另领工作项。
2. 从最新目标分支创建或安全恢复该项的 `copilot/wi-<id>-<slug>` 分支。仅在工作区干净且远程状态明确时切换分支；不强推、不重置、不覆盖已有分支。
3. 编码前确认接口路径、HTTP 方法、鉴权、请求/响应、分页、状态码和错误结构，并将采用的契约写入评论。关键契约缺失或冲突时写为 `copilot-blocked`，不要猜测。
4. 领域阶段读取适用的 `docs/domains`；需要更新领域事实时使用 `/create-domain-doc`。按范围加载 `.github/instructions/` 和 `.github/skills/` 中的后端、前端 Skill。
5. 可调用 `.github/agents/` 中的专职代理做只读分析或边界清晰的辅助工作，但所有可写实现必须串行，并由当前会话检查共享工作区后整合。
6. 每次阶段变化替换唯一的 `copilot-stage-*` 标签并添加评论；不适用阶段也要说明原因。
7. 运行仓库指令、工作项和所选 Skill 要求的构建与聚焦测试，逐条形成可复现验收证据。
8. 验证通过后 commit 并 push 当前工作项分支。通过 `ado` MCP 创建或复用同一 source/target 的 PR，关联工作项并启用 autocomplete；不得绕过 reviewer、build validation 或其他分支策略。
9. 通过 `ado` MCP 检查 PR、Pipeline 与合并状态，回写证据和终态。只有当前项完成回写且工作区恢复为干净状态后，才可查询和领取下一项。

## 终态与停止

- 验证通过但 PR 尚未合并：写为 `copilot-pr`，保留分支、commit、PR 和验证证据。
- 可重试实现或验证失败：写为 `copilot-failed` 并评论证据。
- 需要业务决定、权限、秘密、危险操作或人工解决冲突：写为 `copilot-blocked`。
- 只有 PR 已按分支策略合并且 Pipeline 成功后，才能写为 `copilot-completed`、`copilot-stage-done`。
- `copilot-exclusive` 项处理并创建 PR 后结束本次 Loop，不继续领取其他项。
- 达到最大完成数、队列为空、用户中止、工作区不干净、MCP/Project/Git/权限异常或协议冲突时停止。

结束时返回每个已处理工作项的 ID、终态、变更摘要、验证命令、分支、commit、PR/Pipeline 链接和阻塞项。
