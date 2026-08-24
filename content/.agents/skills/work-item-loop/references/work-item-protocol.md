# Azure DevOps 工作项状态协议

## 1. 唯一事实来源

- Azure DevOps Work Item 是需求、状态、验收标准和执行日志的唯一事实来源。
- 本地仓库不得保存工作项副本、YAML 元数据、需求 Markdown 或 Loop 日志。
- `System.Id` 是不可变身份；`System.Title`、`System.Description` 与流程对应的 Acceptance Criteria 字段承载工作内容。
- Codex 主任务通过 WIQL 查询整个 Project；只有包含 `codex-loop` 和可领取状态标签的工作项属于队列。
- Loop 运行在 Codex 主任务；一个用户可见的独立 Codex 任务只处理一个由主任务明确分配的 ID。

## 2. 标签

每项最多存在一个 Codex 状态标签：

| 标签 | 含义 | 可自动领取 |
|---|---|---|
| `codex-draft` | 尚未准备实施 | 否 |
| `codex-ready` | 已评审且允许实施 | 是 |
| `codex-in-progress` | 已领取 | 是，仅恢复 |
| `codex-failed` | 上一轮可重试失败 | 是 |
| `codex-blocked` | 需要人工或外部条件 | 否 |
| `codex-pr` | 已创建 PR，等待策略与合并 | 否 |
| `codex-completed` | PR 已合并且验收完成 | 否 |
| `codex-cancelled` | 已取消 | 否 |

阶段标签同样必须唯一：`codex-stage-backlog`、`codex-stage-domain`、`codex-stage-backend`、`codex-stage-frontend`、`codex-stage-verifying`、`codex-stage-done`。

控制标签：

- `codex-attempt-N`：当前尝试次数，N 为正整数。
- `codex-run-<runId>`：本次 Codex 主任务 Loop 的运行标识。
- `codex-exclusive`：必须独占全部并发槽位，适用于 Migration、公共配置或高冲突改造。

更新 Codex 标签时必须保留业务、Area、Iteration 等无关标签。

## 3. 领取与并发

- 同一项目同一时间只应有一个 Codex 主任务执行 Loop。当前 MCP 更新不保证跨主任务 CAS，因此主任务必须在领取后重读并校验 run ID。
- 只有主任务可以查询和领取；独立工作项任务只能处理传入 ID。
- 主任务每次领取不超过当前空闲槽位，并排除已创建或正在等待的 ID。
- 选择顺序：可恢复的 `codex-in-progress`、未达上限的 `codex-failed`、`codex-ready`；同状态按 Azure DevOps Priority/Stack Rank 和 ID 排序。
- 从 ready/failed 领取时 attempt 加一；恢复已中断项时沿用或按明确恢复策略递增，必须在评论说明。
- `codex-exclusive` 只能在没有其他运行项时领取，且领取后占满全部槽位。
- 当前 Azure DevOps MCP 更新不保证跨主任务或跨机器 compare-and-swap；多消费者模式必须增加 revision CAS 或外部锁。
- 领取更新后必须重新读取并确认 ID、状态、attempt 与 run 标签；确认失败不得创建独立工作项任务。

## 4. 状态转换

```text
codex-draft -> codex-ready                  # 仅人工批准
codex-ready -> codex-in-progress            # Codex 主任务
codex-failed -> codex-in-progress           # Codex 主任务，未达重试上限
codex-in-progress -> codex-failed           # 独立工作项任务/编排失败
codex-in-progress -> codex-blocked          # 独立工作项任务/集成阻塞
codex-in-progress -> codex-pr               # 分支已 push 且 PR 已创建
codex-pr -> codex-completed                 # PR 与 Pipeline 成功
codex-blocked -> codex-ready                # 仅人工解除
codex-completed -> codex-ready              # 仅人工重开
任意非 completed -> codex-cancelled         # 仅人工取消
```

不得仅因本地代码完成就写 `codex-completed`。

## 5. 阶段门禁

- `domain`：领域规则无关键歧义，领域事实与计划实现一致。
- `backend`：公共契约已确认，后端实现、构建和相关测试通过。
- `frontend`：采用同一契约，类型安全 API 接入、页面行为和前端构建通过。
- `verifying`：所有适用构建与测试通过，验收标准逐条具备证据。
- `done`：PR 已合并，Pipeline 成功，评论包含最终证据。

不适用阶段必须写入评论，不得静默跳过。

## 6. 评论日志

每次领取、阶段切换、失败、阻塞、PR 创建和完成都追加 Markdown 评论，至少包含：

- ISO 8601 时间、run ID、attempt、状态和阶段；
- 已确认契约及变更；
- 实际使用的 Skill 与执行者；
- 修改路径摘要；
- 验证命令、退出结果与验收证据；
- 分支、commit、PR 和 Pipeline 链接；
- 错误、阻塞或剩余风险。

评论不得包含令牌、密码、连接字符串或其他秘密。

## 7. Git 与集成

- 每项使用 Codex 为新任务创建的独立 Git worktree；独立任务不得访问其他工作项 worktree。
- 独立任务在自己的 worktree 和当前分支中完成 commit 和 push，不得修改主任务的 checkout。
- 使用 Azure DevOps PR 关联工作项并启用 autocomplete；不得绕过 reviewer、build validation 或其他分支策略。
- PR 合并冲突不得由集成步骤猜测解决；写 `codex-blocked`、保留 worktree 和冲突证据，等待人工决定后重新进入 `codex-ready`。
- PR 未合并前不得删除失败证据或写完成状态。

## 8. 停止条件

满足任一条件停止领取新项，并等待已运行 Worker 安全结束：

- MCP 不可用、认证失效或缺少必要读写工具；
- Project 配置无效，或无法从当前 Git remote 识别 Repository；
- 主任务发现编排阻塞、重复 ID 或 run ID 被覆盖；
- 达到本次最大完成数或每项最大尝试次数；
- 无法为新 Codex 任务创建隔离 worktree；
- 需要新的权限、秘密、破坏性操作或人工业务决定。
