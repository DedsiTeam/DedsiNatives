# Azure DevOps 工作项状态协议

## 1. 唯一事实来源

- Azure DevOps Work Item 是需求、状态、验收标准和执行日志的唯一事实来源。
- 本地仓库不得保存工作项副本、YAML 元数据、需求 Markdown 或 Loop 日志。
- `System.Id` 是不可变身份；`System.Title`、`System.Description` 与流程对应的 Acceptance Criteria 字段承载工作内容。
- GitHub Copilot Agent 通过 WIQL 查询整个 Project；只有包含 `copilot-loop` 和可领取状态标签的工作项属于队列。
- 一个 Copilot Agent 会话同一时刻只领取和实现一个工作项。

## 2. 标签

每项最多存在一个 Copilot 状态标签：

| 标签 | 含义 | 可自动领取 |
|---|---|---|
| `copilot-draft` | 尚未准备实施 | 否 |
| `copilot-ready` | 已评审且允许实施 | 是 |
| `copilot-in-progress` | 已领取 | 是，仅安全恢复 |
| `copilot-failed` | 上一轮可重试失败 | 是 |
| `copilot-blocked` | 需要人工或外部条件 | 否 |
| `copilot-pr` | 已创建 PR，等待策略与合并 | 否 |
| `copilot-completed` | PR 已合并且验收完成 | 否 |
| `copilot-cancelled` | 已取消 | 否 |

阶段标签同样必须唯一：`copilot-stage-backlog`、`copilot-stage-domain`、`copilot-stage-backend`、`copilot-stage-frontend`、`copilot-stage-verifying`、`copilot-stage-done`。

控制标签：

- `copilot-attempt-N`：当前尝试次数，N 为非负整数。
- `copilot-run-<runId>`：本次 Copilot Agent Loop 的运行标识。
- `copilot-exclusive`：涉及 Migration、公共配置或高冲突改造；完成该项 PR 后必须结束本次 Loop。

更新 Copilot 标签时必须保留业务、Area、Iteration 等无关标签。

## 3. 领取与恢复

- 同一 Project 同一时间只应有一个 Copilot Agent 会话执行 Loop。MCP 更新不保证跨会话 CAS，因此领取后必须重读并校验 revision 与 run ID。
- 每次只领取一个工作项。选择顺序为：可安全恢复的 `copilot-in-progress`、未达上限的 `copilot-failed`、`copilot-ready`；同状态按 Priority/Stack Rank 和 ID 排序。
- 从 ready/failed 领取时 attempt 加一；恢复中断项时沿用或按明确恢复策略递增，并在评论说明。
- 恢复前必须确认对应分支、commit、工作区和远程状态；无法无损确认时写为 `copilot-blocked`。
- 领取更新后必须重读确认 ID、revision、状态、attempt 与 run 标签；确认失败不得切分支或编码。

## 4. 状态转换

```text
copilot-draft -> copilot-ready              # 仅人工批准
copilot-ready -> copilot-in-progress        # Copilot Agent
copilot-failed -> copilot-in-progress       # Copilot Agent，未达重试上限
copilot-in-progress -> copilot-failed       # 实现或验证可重试失败
copilot-in-progress -> copilot-blocked      # 业务、权限或集成阻塞
copilot-in-progress -> copilot-pr           # 分支已 push 且 PR 已创建
copilot-pr -> copilot-completed             # PR 已合并且 Pipeline 成功
copilot-blocked -> copilot-ready            # 仅人工解除
copilot-completed -> copilot-ready          # 仅人工重开
任意非 completed -> copilot-cancelled       # 仅人工取消
```

不得仅因本地代码完成就写 `copilot-completed`。

## 5. 阶段门禁

- `domain`：领域规则无关键歧义，领域事实与计划实现一致。
- `backend`：公共契约已确认，后端实现、构建和相关测试通过。
- `frontend`：采用同一契约，类型安全 API 接入、页面行为和前端构建通过。
- `verifying`：所有适用构建与测试通过，验收标准逐条具备证据。
- `done`：PR 已合并，Pipeline 成功，评论包含最终证据。

不适用阶段必须写入评论，不得静默跳过。

## 6. 评论日志

每次领取、阶段切换、失败、阻塞、PR 创建和完成都追加 Markdown 评论，至少包含：ISO 8601 时间、run ID、attempt、状态、阶段、已确认契约、修改路径、使用的 Skill、验证结果、分支、commit、PR/Pipeline 链接和剩余风险。评论不得包含秘密。

## 7. Git 与集成

- 每项使用独立的 `copilot/wi-<id>-<slug>` 分支；同一 checkout 串行处理，分支切换前后都必须保持工作区干净。
- 工作项分支从最新目标分支创建，commit 和 push 只作用于当前分支。不得强推、重置目标分支或覆盖用户改动。
- 使用 Azure DevOps PR 关联工作项并启用 autocomplete；不得绕过 reviewer、build validation 或其他分支策略。
- PR 合并冲突不得猜测解决；写 `copilot-blocked` 并保留分支和冲突证据，等待人工决定后重新进入 `copilot-ready`。

## 8. 停止条件

满足任一条件就停止领取新项：MCP 或认证不可用、Project/Repository 无法确认、工作区不干净、run ID/revision 冲突、达到上限、`copilot-exclusive` 项已处理、需要新权限或秘密、存在破坏性操作或人工业务决定。
