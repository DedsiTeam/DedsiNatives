# DedsiNative.WorkItemListener 无人值守工作项执行

`DedsiNative.WorkItemListener` 持续轮询 Azure DevOps Project，以 revision CAS 原子领取工作项，并在独立 Git worktree 中调用 GitHub Copilot CLI。Listener 是唯一调度者；不需要聊天主会话、Coordinator Agent 或 Loop Skill。

## 运行模型

```text
Azure DevOps Work Item Queue
    │ WIQL 查询 + revision CAS + 租约
    ▼
DedsiNative.WorkItemListener
    │ 创建独立 worktree
    ▼
GitHub Copilot CLI
    │ 非交互 Autopilot：实现 + 构建 + 测试
    ▼
Listener commit + push + PR + autocomplete
    │
    ▼
Azure Repos 分支策略与 Pipeline
    │ PR 完成后由 Listener 对账
    ▼
copilot-completed
```

职责边界：

- Listener 负责队列、CAS、attempt、租约、worktree、Git、PR、autocomplete、重试和状态回写。
- Copilot CLI 每次只处理一个工作项，只修改和验证当前 worktree；可以按范围使用 Backend/Frontend 研发子智能体，但不连接 Azure DevOps，不 commit、不 push、不创建 PR。
- Azure Repos reviewer、build validation 和其他分支策略是合并门禁；Listener 不绕过策略。
- 对 Listener 而言，Azure DevOps Work Item 是需求、验收标准、执行状态和证据的唯一事实来源；Listener 只消费已有工作项，不创建、拆分或上传需求。
- 产品经理可以在 Listener 之前按 [产研一体需求流程](docs/product/README.md) 通过 VS Code Copilot 和 Azure DevOps MCP 发布需求；Listener 本身仍不创建、拆分或上传需求。

## 队列和状态

上游需求和编码工作项统一使用 [Azure DevOps 无人值守编码工作项模板](docs/work-item-templates/azure-devops-coding-work-item.md)。完整父需求不进入队列；需要新页面或关键交互时，先按 [前端静态原型晋级流程](docs/work-item-templates/frontend-static-prototype-workflow.md) 完成产品评审。只有通过原型门禁和 Ready 检查的编码子工作项才添加 `copilot-ready`。

`prototype-ready`、`prototype-review` 和 `prototype-approved` 属于 Listener 之前的产品流程，Listener 不查询、领取或更新这些标签。Prototype Agent 也不属于 Listener 可委派的研发子智能体。

上游流程创建并评审完成的工作项使用以下标签进入编码队列：

```text
copilot-loop; copilot-ready; copilot-stage-backlog; copilot-attempt-0
```

Listener 不参与需求准备，只领取以下队列项：

- `copilot-ready`：首次执行。
- `copilot-failed`：attempt 未达到上限时自动重试。
- `copilot-in-progress`：仅用于租约检查；租约过期后先转换为 failed，再进入下一轮重试。
- `copilot-pr`：仅对账 PR 状态，不再次运行 Copilot。

Listener 维护以下控制标签：

- `copilot-attempt-N`
- `copilot-run-<runId>`
- `copilot-lease-<unixSeconds>`
- `copilot-pull-request-<id>`
- `copilot-exclusive`：Migration、公共配置等高冲突工作项独占一轮全部并发容量。

终态：

- `copilot-pr`：变更已推送并创建 PR，等待策略与合并。
- `copilot-failed`：CLI、Git、PR 或其他可重试步骤失败。
- `copilot-blocked`：Copilot 判断关键业务规则、契约、权限或秘密缺失。
- `copilot-completed`：PR 已按分支策略完成。

领取使用 Azure DevOps JSON Patch 的 `test /rev`，因此多个 Listener 同时看到同一工作项时只有一个能成功写入。活跃运行按配置定期续租；进程异常终止后，其他 Listener 可在租约过期后恢复队列。

正常部署时每个 Azure DevOps Project 只运行一个 Listener 实例，这样并发上限和 `copilot-exclusive` 才是全局约束。revision CAS 能避免意外双实例重复领取同一个工作项，但不会让不同实例共享全局并发计数。续租失败时，Listener 会立即取消该次 Copilot/Git/PR 流程，避免失去所有权后继续交付。

## 配置

生成项目根目录的 `.env.local` 已被 Git 忽略。最小配置：

```dotenv
ADO_PAT=<原始 Azure DevOps PAT>
COPILOT_GITHUB_TOKEN=<支持 Copilot Requests 的 fine-grained token>
```

模板已经把 `ADO_ORG` 和 `ADO_PROJECT` 固化为生成参数；如需覆盖可在环境或 `.env.local` 设置：

```dotenv
ADO_ORG=YourOrg
ADO_PROJECT=YourProject
ADO_REPOSITORY=YourRepository

WORK_ITEM_LISTENER_TARGET_BRANCH=main
WORK_ITEM_LISTENER_MAX_PARALLELISM=2
WORK_ITEM_LISTENER_MAX_ATTEMPTS=3
WORK_ITEM_LISTENER_POLL_SECONDS=60
WORK_ITEM_LISTENER_RETRY_SECONDS=300
WORK_ITEM_LISTENER_LEASE_MINUTES=30
WORK_ITEM_LISTENER_HEARTBEAT_MINUTES=5
WORK_ITEM_LISTENER_COPILOT_TIMEOUT_MINUTES=120
WORK_ITEM_LISTENER_MAX_AUTOPILOT_CONTINUES=10
WORK_ITEM_LISTENER_MAX_SUBAGENTS=2
WORK_ITEM_LISTENER_USE_SANDBOX=true
WORK_ITEM_LISTENER_AUTO_COMPLETE=true
WORK_ITEM_LISTENER_CLEANUP_MERGED_WORKTREES=true
```

队列只由 Project 和 `copilot-*` 标签驱动，不读取 `Assigned To`，因此无需把工作项人工分配给某个账号或 Agent。

`ADO_REPOSITORY` 为空时从 Git `origin` 推导。交互式机器可以让 Copilot CLI 使用已保存的 OAuth 登录；CI、容器或 headless 服务建议使用 `COPILOT_GITHUB_TOKEN`。

Azure DevOps PAT 至少需要读取/更新工作项、读取/推送代码、创建/更新 PR 的权限。不要把任何 Token 提交到 Git、工作项评论、PR 描述或服务日志。

## Copilot CLI 执行策略

Listener 使用非交互参数：

```text
<工作项提示> | copilot --autopilot --no-ask-user --allow-all-tools \
  --max-autopilot-continues=10 --experimental --sandbox
```

同时禁用远程会话、workspace MCP、仓库 Hooks、扩展和内置 GitHub MCP。默认允许 Backend/Frontend 两个研发子智能体并发，嵌套深度固定为 1；它们只开放读、编辑、搜索和本地命令工具。Listener 仍拒绝 `git commit`、`git push`、`git reset`、`git clean`、分支切换、worktree 和 `az` 命令，并只根据主 Copilot 最终响应中的机器标记继续：

```text
DEDSI_RESULT=completed
DEDSI_RESULT=blocked
DEDSI_RESULT=failed
```

缺少标记、退出码非零或超时一律按失败处理；报告 completed 但没有 Git 变更也按失败处理，防止错误完成。

本地 sandbox 仍属于 Copilot CLI 的预览能力。无人值守生产环境应再使用专用低权限系统账号、容器或 VM 隔离，不要让 Listener 以管理员身份运行。

## 启动与部署

首次使用先安装并验证 GitHub Copilot CLI，然后验证 Listener 配置：

```bash
copilot --version
dotnet run --project src/dotnet/tools/DedsiNative.WorkItemListener -- --validate
```

只执行一轮：

```bash
dotnet run --project src/dotnet/tools/DedsiNative.WorkItemListener -- --once
```

持续监听：

```bash
dotnet run --project src/dotnet/tools/DedsiNative.WorkItemListener
```

长期运行时使用 systemd、launchd、Windows Service 包装器或容器编排器负责进程重启和日志收集。Listener 本身不在仓库保存队列副本或执行日志。

## 故障恢复

- Copilot、Git 或 REST API 失败：工作项写为 `copilot-failed`，保留 worktree，未达上限时自动重试。
- 业务或契约不明确：写为 `copilot-blocked`，不会自动重试。
- Listener 崩溃：租约到期后由下一轮轮询回收。
- PR 活跃：保持 `copilot-pr`；Listener 重启后继续对账。
- PR abandoned：写为 `copilot-failed`。
- PR completed：写为 `copilot-completed`，可配置清理已合并本地 worktree。

失败或阻塞的 worktree 默认保留在主仓库相邻的 `<repository>.worktrees/`，便于诊断。只有 PR 已合并的 worktree 才会自动删除。

## 验证

```bash
dotnet test src/dotnet/tests/DedsiNative.WorkItemListener.Tests/DedsiNative.WorkItemListener.Tests.csproj
dotnet build src/dotnet/DedsiNative.slnx
```
