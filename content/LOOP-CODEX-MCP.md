# Codex 原生 Work Item Loop 与 Azure DevOps MCP

本项目的 Loop 以 Codex 桌面端主任务为编排中心。它不启动 `codex exec` 子进程，也不用 Node 程序替代 Codex 主页面。

## 1. 运行模型

```text
Codex 主任务（Loop Coordinator）
    │ 直接调用 ado MCP 查询、领取、核对状态
    │
    ├─ Codex 新任务：WI #101（独立 worktree）
    │      └─ 直接调用 ado MCP 读取需求、写评论、创建 PR、检查 Pipeline
    │
    └─ Codex 新任务：WI #102（独立 worktree）
           └─ 直接调用 ado MCP 读取需求、写评论、创建 PR、检查 Pipeline

Azure DevOps Project
```

- 主任务保留用户当前对话，通过 MCP 查询队列和领取工作项，创建、等待、继续和核对独立 Codex 任务。
- 工作项任务只处理一个指定 ID，在自己的 worktree 内实现、验证、commit、push，并直接通过 MCP 完成 PR 和状态回写。
- Azure DevOps MCP 是 Codex 与 Azure DevOps 之间的工具通道，不自行领取或实现工作项。

## 2. MCP 直连

`.codex/config.toml` 中的 `[mcp_servers.ado]` 是项目级 MCP 配置。信任项目后，Codex 桌面端、CLI 和 IDE 可共享该配置。

PAT 只需要在文件中设置一次。生成项目使用根目录 `.env.local`；本模板仓库使用 `content/.env.local`：

```dotenv
ADO_PAT=<原始 Azure DevOps PAT>
```

`.env.local` 已被 Git 忽略且不会进入 NuGet 模板包。`.codex/ado-mcp-pat.mjs` 在 Codex 启动 MCP 时读取 PAT，在内存中生成 `<任意非空值>:<PAT>` 的 Base64 并传给 Azure DevOps MCP。用户不需要执行配置命令、手工编码、修改 `config.toml` 或配置系统环境变量。

每个 Codex 新任务的 worktree 都通过 Git worktree 注册信息找到主项目的 `.env.local`，因此 PAT 不会被复制到各个 worktree。如果 Codex 宿主已经提供 `PERSONAL_ACCESS_TOKEN`，启动器仍优先使用该环境变量，便于 CI 或集中秘密管理。

设置完成后重启 Codex，打开并信任生成项目，然后用 `/mcp` 确认 `ado` 已连接。MCP 不可用或写入结果无法重读确认时，Loop 必须停止，不得降级到本地工作项文件。

## 3. 队列和状态

主任务通过 `wit_query` 的 `wiql` 动作查询当前 Project。只有同时包含 `codex-loop` 与下列可领取状态之一的工作项属于队列：

- `codex-ready`：已评审，可以首次领取。
- `codex-in-progress`：已领取，可按协议恢复。
- `codex-failed`：上一次可重试失败，未达尝试上限时可重新领取。

`codex-blocked`、`codex-pr`、`codex-completed` 和 `codex-cancelled` 不会被自动领取。完整标签、阶段、评论和转换规则以 [work-item-protocol.md](.agents/skills/work-item-loop/references/work-item-protocol.md) 为准。

## 4. 启动 Loop

不再有单独命令行入口。在生成项目的 Codex 主任务中直接发出请求：

```text
使用 $work-item-loop 预览可处理的 Azure DevOps 工作项，只读。
```

或者：

```text
使用 $work-item-loop 开始处理队列，最多完成 10 项，并发 2 个独立 Codex 任务，每项最多尝试 3 次。
```

用户没有指定时，默认上限就是完成 10 项、并发 2 项、每项尝试 3 次。主任务会保持在当前页面，每个工作项会出现为一个可单独打开、跟进和审查的 Codex 任务。

## 5. 完整时序

1. 主任务验证 Project、Git remote 和 `ado` MCP。
2. 主任务用 WIQL 查询队列，按协议领取不超过空闲槽位的工作项，写入 attempt、run ID 和评论后重读确认。
3. 主任务为每个已领取 ID 创建独立 Codex 新任务和 Git worktree。
4. 工作项任务直接通过 MCP 读取需求、验收标准和评论，核对 attempt 与 run ID。
5. 工作项任务在自己的 worktree 内完成实现和验证，然后 commit 并 push 当前分支。
6. 工作项任务直接通过 MCP 创建或复用 PR，关联工作项、启用 autocomplete，并检查 Pipeline、分支策略和合并状态。
7. 只有 PR 已合并且 Pipeline 成功后，工作项任务才回写 `codex-completed` 和 `codex-stage-done`。
8. 主任务等待任务结果，核对 ID、PR 和终态，再填充空闲槽位，直到达到上限、队列为空或遇到停止条件。

## 6. 停止条件

- `ado` MCP 不可用、认证失效、缺少必要工具或写入无法确认。
- Project 配置无效，或 Git remote 不是目标 Azure Repos 仓库。
- 无法创建、等待或继续独立 Codex 任务/worktree。
- 发现重复领取、run ID 被覆盖或另一个主任务正在消费同一队列。
- 需要新的权限、秘密、业务决定、破坏性操作或人工解决合并冲突。
- 达到最大完成数、重试上限，或队列已空。

任务失败时写入 `codex-failed`；需要人工时写入 `codex-blocked` 并保留证据。主任务不会在背景 CLI 中继续隐藏运行。
