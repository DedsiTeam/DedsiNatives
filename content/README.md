# DedsiNative 解决方案

基于 .NET 10、ABP Framework、FastEndpoints 和 PostgreSQL 的 Clean Architecture 解决方案模板。

## 技术栈

- **后端**：.NET 10、ABP Framework、FastEndpoints、EF Core、PostgreSQL
- **服务编排**：.NET Aspire (`src/dotnet/asipres/DedsiNative.AppHost`)
- **前端**：React 19、TypeScript、Vite、Ant Design (`src/react-admin/`)

## 目录结构

```text
├── src/
│   ├── dotnet/
│   │   ├── src/
│   │   │   ├── DedsiNative.Core/            # 领域层：实体、领域事件、仓储接口
│   │   │   ├── DedsiNative.Infrastructure/  # 基础设施层：EF Core 与仓储实现
│   │   │   └── DedsiNative.Endpoints/       # FastEndpoints API 接口层
│   │   ├── host/
│   │   │   └── DedsiNative.Host/            # 应用启动与中间件宿主
│   │   ├── asipres/
│   │   │   ├── DedsiNative.AppHost/         # .NET Aspire 编排项目
│   │   │   └── DedsiNative.ServiceDefaults/ # Aspire 服务默认配置
│   │   └── DedsiNative.slnx
│   └── react-admin/                          # React 前端管理后台
└── docs/                                     # 领域 Markdown 文档；工作项存放于 Azure DevOps
```

## 快速开始

### 1. 数据库配置与迁移

1. 检查并修改 `src/dotnet/host/DedsiNative.Host/appsettings.json` 中的 PostgreSQL 数据库连接字符串。
2. 执行 EF Core 迁移创建数据库表：
   ```bash
   dotnet ef database update --project src/dotnet/src/DedsiNative.Infrastructure --startup-project src/dotnet/host/DedsiNative.Host
   ```

### 2. 启动应用

#### 方式一：使用 .NET Aspire 启动（推荐）
运行 Aspire AppHost 启动全套服务（包含后端 API 与 Aspire Dashboard）：
```bash
dotnet run --project src/dotnet/asipres/DedsiNative.AppHost
```

#### 方式二：独立启动后端 API
```bash
dotnet run --project src/dotnet/host/DedsiNative.Host
```

#### 方式三：启动前端项目
```bash
cd src/react-admin
bun install   # 或 npm install
bun dev       # 或 npm run dev
```

## 研发约束与 Agent 规范

本项目已配置通用开发规范，详见 [AGENTS.md](AGENTS.md)。

项目 Skills、后端/前端规则与专项参考资料统一存放在 [`.agents/`](.agents/)；`src/` 目录只包含产品源代码。

模板配置只包含 `CHANGE_ME` 占位符。运行宿主前请通过环境变量提供实际敏感配置：`ConnectionStrings__DedsiNativeDB`、`ConnectionStrings__DedsiNativeRabbitMQ` 和 `Jwt__Secret`；通过 Aspire 启动时还需配置 `Parameters__PostgresPassword`、`Parameters__RabbitMqUserName` 和 `Parameters__RabbitMqPassword`。不要把真实值提交到仓库。

## 文档存放规范

`docs/` 目录只维护领域文档。需求、工作项、验收标准、状态和执行日志统一存放于 Azure DevOps，不在本地创建副本。

前端 UI 规范请参考：[dedsi-style-react-admin-ui](.agents/skills/dedsi-style-react-admin-ui/SKILL.md)。

## Azure DevOps MCP 与 Codex 原生 Work Item Loop

项目通过 [`.codex/config.toml`](.codex/config.toml) 为 Codex 桌面端、CLI 和 IDE 配置 Azure DevOps Local MCP。Codex 桌面端直接启动该 MCP；Azure DevOps 是工作项、验收标准、状态、评论日志、分支、PR 和 Pipeline 状态的唯一远程事实来源。

每个生成项目只绑定一个 Azure DevOps Project。模板参数 `--AdoProject` 会同时写入 Codex 项目规则和 MCP 的 `ado_mcp_project` 默认值；除非用户明确要求，Codex 不枚举或操作其他 Project。

创建模板时提供组织和 Project：

```bash
dotnet new dedsi-native -n YourProject \
  --HttpPort 12256 \
  --AdoOrg YourOrg \
  --AdoProject YourProject
```

首次运行前，直接在生成项目根目录创建 `.env.local`。在本模板仓库中，对应路径是 `content/.env.local`：

```dotenv
ADO_PAT=<原始 Azure DevOps PAT>
```

`.env.local` 已被 Git 忽略且不会进入 NuGet 模板包。Codex 启动 MCP 时才在内存中把 `ADO_PAT` 转换为 `<任意非空值>:<PAT>` 的 Base64；用户不需要执行配置命令、手工编码或设置系统环境变量。主任务创建的 Codex worktree 会自动复用主项目的 `.env.local`，不会复制 PAT。

保存文件后重启 Codex，信任项目，并在对话中使用 `/mcp` 确认 `ado` 已连接。不要把 `.env.local` 的内容复制到配置、评论、日志或 Git。

Loop 通过 WIQL 直接查询整个 Azure DevOps Project，不需要创建保存查询。需要进入队列的工作项添加 `codex-loop`，并设置 `codex-ready`、`codex-in-progress` 或 `codex-failed` 状态标签。

在 Codex 项目的主任务中说明要运行 `$work-item-loop`。主任务直接通过 `ado` MCP 领取工作项，并为每个工作项创建一个用户可见的独立 Codex 新任务。每个新任务在独立 Git worktree 中实现、验证、commit 和 push，并直接通过同一 `ado` MCP 创建 PR、检查 Pipeline/合并和回写工作项终态。主任务持续等待、核对并补充新任务，因此整个 loop 始终留在 Codex UI 内。

例如：

```text
使用 $work-item-loop 预览当前 Azure DevOps Project 的可领取工作项，只读，不创建新任务。

使用 $work-item-loop 处理队列，最多完成 10 项，并发 2 个独立 Codex 任务。
```

完整运行前必须满足：Codex 已信任项目且 `ado` MCP 可用、项目根目录已生成 `.env.local`、`origin/main` 可访问、Git 已配置提交身份与 Azure Repos push 权限，并且 PAT 具有工作项/评论/代码/PR/Pipeline 所需权限。Loop 不绕过 reviewer、build validation 或其他分支策略。

- Loop Skill：[.agents/skills/work-item-loop/SKILL.md](.agents/skills/work-item-loop/SKILL.md)
- 状态协议：[.agents/skills/work-item-loop/references/work-item-protocol.md](.agents/skills/work-item-loop/references/work-item-protocol.md)
- Loop、Codex 与 MCP 使用说明：[LOOP-CODEX-MCP.md](LOOP-CODEX-MCP.md)
- Azure DevOps MCP 文档：[Microsoft Azure DevOps MCP](https://github.com/microsoft/azure-devops-mcp/blob/main/docs/GETTINGSTARTED.md#codex)
