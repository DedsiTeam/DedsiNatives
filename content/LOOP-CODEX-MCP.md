# Agent Loop、Codex 与 Azure DevOps MCP 使用说明

本文说明 `agent-loop.mjs`、Codex 和 Azure DevOps MCP 的职责、调用关系、配置来源以及实际运行命令。

## 1. 三者关系

三者是逐层调用关系，不是三个独立运行的队列消费者：

```text
agent-loop.mjs
    │ 启动 codex exec
    ▼
Codex
    │ 根据项目规则调用 ado MCP 工具
    ▼
Azure DevOps Local MCP
    │ 使用 PAT 调用 Azure DevOps API
    ▼
Azure DevOps Project
```

- **Agent Loop**：本地编排器，管理并发槽位、Git worktree、分支、提交、推送和执行顺序。
- **Codex**：分析和执行者，读取工作项、实现代码、运行验证，并决定工作项应该进入下一阶段、失败或阻塞。
- **Azure DevOps MCP**：Codex 与 Azure DevOps 之间的工具通道，提供工作项、评论、仓库、PR 和 Pipeline 操作。

MCP 不会自行领取工作项或编写代码。Loop 也不直接调用 Azure DevOps API；Loop 启动 Codex，Codex 再调用 MCP。

## 2. 当前作用范围

一个生成项目只绑定一个 Azure DevOps Project，由模板参数 `AdoProject` 确定。Loop 通过 WIQL 直接查询整个 Project，不需要配置 Azure DevOps 保存查询 GUID。

“查询整个 Project”不表示自动处理 Project 中的所有工作项。只有同时满足以下条件的工作项才会进入自动队列：

- 包含 `codex-loop` 标签；
- 包含 `codex-ready`、`codex-in-progress` 或 `codex-failed` 中的一个状态标签。

Azure Repos 仓库名不需要配置。Loop 从当前 Git remote（默认为 `origin`）自动提取仓库名，因此本地 `origin` 必须指向需要创建 PR 的 Azure Repos 仓库。

## 3. 各组件职责

### 3.1 Agent Loop

入口文件是 `agent-loop.mjs`，主要负责：

1. 检查项目配置和当前 Git 仓库。
2. 从 `origin` 自动识别 Azure Repos 仓库。
3. 使用 Git common directory 锁，防止本机重复启动 Dispatcher。
4. 启动 Dispatcher Codex 领取工作项。
5. 为每个工作项创建独立 worktree 和 `codex/wi-<ID>-<slug>` 分支。
6. 并发启动 Worker Codex。
7. Worker 验证通过后执行 commit 和 push。
8. 启动 Integration Codex 创建 PR、等待策略检查和合并。
9. 清理成功任务的 worktree，失败或阻塞的 worktree 默认保留。

并发槽位当前固定为 `2`，定义在 `agent-loop.mjs` 的 `CONCURRENT_SLOTS` 常量中，不接受配置文件、环境变量或命令行覆盖。

### 3.2 Codex

Loop 通过 `codex exec` 启动短生命周期 Codex。这里指 Loop 创建的 Codex CLI 子进程，不是当前 Codex Desktop 对话本身。

一次完整处理包含三类 Codex 调用：

#### Dispatcher

- 通过 MCP 的 `wit_query` / `wiql` 查询当前 Project；
- 根据标签、优先级、重试次数和空闲槽位选择候选项；
- 排除本地已经运行的工作项；
- 将领取项更新为 `codex-in-progress`；
- 写入 `codex-attempt-N`、`codex-run-<runId>` 和阶段标签；
- 添加领取评论并重新读取确认更新结果；
- 向 Loop 返回结构化领取结果。

#### Worker

- 只处理 Dispatcher 分配的一个工作项；
- 通过 MCP 读取描述、验收标准和评论；
- 按 `AGENTS.md` 和适用 Skill 完成领域、后端、前端实现；
- 运行构建、测试和验收验证；
- 通过 MCP 更新阶段、失败或阻塞状态；
- 不执行 commit、push、创建 PR 或合并。

#### Integration

- 在 Loop 完成 commit 和 push 后运行；
- 通过 MCP 查找或创建 source/target 对应的 PR；
- 将 PR 与工作项关联并启用 autocomplete；
- 检查 Pipeline、分支策略、冲突和合并状态；
- 只有确认 PR 已合并后，才把工作项更新为 `codex-completed` 和 `codex-stage-done`。

### 3.3 Azure DevOps MCP

项目通过 `.codex/config.toml` 注册名为 `ado` 的 Local MCP Server。Codex 按需启动 MCP，并使用它完成：

- WIQL 查询和工作项读取；
- 工作项标签、字段和评论更新；
- Azure Repos 仓库、分支和 PR 操作；
- Pipeline 与合并状态查询。

MCP 使用环境变量 `PERSONAL_ACCESS_TOKEN` 认证。该值必须是 `<任意非空邮箱>:<PAT>` 的 Base64，而不是直接填写原始 PAT。

## 4. 配置传递关系

```text
.env.local
  └─ PERSONAL_ACCESS_TOKEN
       └─ node agent-loop.mjs
            └─ codex exec
                 └─ .codex/config.toml
                      └─ Azure DevOps MCP

agent-loop.config.json
  └─ Azure DevOps Project、Git 和 Loop 参数

Git origin
  └─ 自动识别 Azure Repos 仓库名
```

主要文件：

- `.env.local`：本机 PAT，仅用于运行，不得提交。
- `.codex/config.toml`：Azure DevOps Local MCP 启动和认证配置。
- `agent-loop.config.json`：Project、最大处理数量、重试次数、Git remote 和目标分支。
- `agent-loop.mjs`：实际编排程序，并发槽位也定义在这里。
- `.agents/skills/work-item-loop/`：Codex 使用的工作项协议、提示规则和结构化结果 Schema。

## 5. 首次创建项目

创建模板项目时只需要指定 HTTP 端口、Azure DevOps 组织和 Project：

```bash
dotnet new dedsi-native -n YourProject \
  --HttpPort 12256 \
  --AdoOrg YourOrg \
  --AdoProject YourProject
```

进入生成项目的根目录，该目录应包含 `agent-loop.mjs`、`agent-loop.config.json` 和 `.codex/config.toml`。

确认 Git remote：

```bash
git remote -v
git remote get-url origin
```

`origin` 应指向当前 `AdoProject` 中需要提交代码和创建 PR 的 Azure Repos 仓库。如果使用其他 remote，可在运行时传入 `--remote`。

## 6. 配置 PAT

在生成项目根目录创建被 Git 忽略的 `.env.local`：

```dotenv
PERSONAL_ACCESS_TOKEN=<Base64 后的邮箱:PAT>
```

不要把原始 PAT 或 `.env.local` 提交到 Git，也不要在工作项评论、日志和截图中公开它。PAT 泄露后应立即在 Azure DevOps 撤销并重新创建。

## 7. 准备工作项

在当前 Azure DevOps Project 中选择允许 Loop 自动处理的工作项，并添加：

```text
codex-loop
codex-ready
```

工作项应提供足够明确的描述和验收标准。以下状态由 Loop/Codex 使用：

- `codex-ready`：允许 Dispatcher 领取；
- `codex-in-progress`：已经领取，支持中断恢复；
- `codex-failed`：本轮失败，在重试上限内可再次领取；
- `codex-blocked`：需要人工决策、权限或外部条件；
- `codex-pr`：PR 已创建，等待策略或合并；
- `codex-completed`：PR 已合并且验证完成。

不要为同一个工作项同时添加多个 Codex 状态标签。

## 8. 运行命令

以下命令都在生成项目根目录执行。

### 查看参数

```bash
node agent-loop.mjs --help
```

### 只检查解析结果

`--dry-run` 不调用 MCP、不领取工作项、不创建 worktree、不提交代码，也不创建 PR：

```bash
node --env-file=.env.local agent-loop.mjs --dry-run
```

### 正式运行

使用 `agent-loop.config.json` 中的默认设置：

```bash
node --env-file=.env.local agent-loop.mjs
```

### 限制本次成功合并数量

下面的命令最多成功合并 10 个工作项，并发槽位仍固定为 2：

```bash
node --env-file=.env.local agent-loop.mjs --max-items 10
```

### 临时指定 Project

```bash
node --env-file=.env.local agent-loop.mjs \
  --ado-project AzureDevOpsMcpTest
```

该参数只覆盖当前运行，不会修改配置文件。它必须与 MCP 允许访问的 Project 一致。

### 使用其他 Git remote 或目标分支

```bash
node --env-file=.env.local agent-loop.mjs \
  --remote azure \
  --base-branch main
```

### 禁用 PR 自动完成

```bash
node --env-file=.env.local agent-loop.mjs --no-auto-complete
```

此模式会创建 PR，但需要人工完成合并，因此对应工作项会保持等待人工处理的状态。

### 失败后删除 worktree

默认保留失败或阻塞的 worktree，便于检查和恢复。如明确不需要保留：

```bash
node --env-file=.env.local agent-loop.mjs --remove-failed-worktrees
```

## 9. 一次完整执行时序

```text
1. 启动 Loop
2. 检查 AdoProject、Git origin、主工作区和本机排他锁
3. fetch origin/main
4. Dispatcher Codex 通过 MCP 执行项目级 WIQL
5. Dispatcher 最多领取两个工作项并回写标签、attempt 和评论
6. Loop 创建两个独立 worktree 和分支
7. 两个 Worker Codex 并发读取需求、实现代码并自测
8. Loop 对 verified 分支执行 commit 和 push
9. Integration Codex 通过 MCP 创建或复用 PR
10. Azure DevOps 执行 reviewer、Pipeline 和分支策略
11. PR 合并后，Codex 通过 MCP 将工作项标记为 completed
12. Loop 清理成功 worktree，并继续领取下一批
```

如果 Worker 失败，工作项进入 `codex-failed`；如果缺少业务决定、权限、秘密或需要人工处理冲突，则进入 `codex-blocked`。

## 10. 运行前检查

正式运行前确认：

- 当前目录是生成项目根目录；
- 已安装并可执行 Node.js 和 Codex CLI；
- `PERSONAL_ACCESS_TOKEN` 可以通过 `.env.local` 注入；
- PAT 具有工作项、评论、代码、PR 和 Pipeline 所需权限；
- `AdoProject` 正确；
- `origin` 指向正确的 Azure Repos 仓库；
- `origin/main` 可以 fetch；
- Git 已配置提交用户名和邮箱；
- 主工作区没有未提交改动；
- 待处理工作项带有 `codex-loop` 和 `codex-ready` 标签。

## 11. 常见停止原因

- MCP 无法启动、PAT 无效或权限不足；
- Project 配置与 MCP 访问范围不一致；
- 无法从 Git remote 识别仓库；
- 主工作区存在未提交改动；
- 无法 fetch、push 或创建 worktree；
- 工作项缺少明确契约或验收标准；
- Pipeline 失败、PR 冲突或分支策略需要人工审批；
- 工作项达到最大重试次数；
- 已经有另一个 Loop 在同一台机器上运行。

当队列中没有符合标签条件的工作项时，Loop 正常结束，不会把 Project 中的普通工作项自动加入队列。
