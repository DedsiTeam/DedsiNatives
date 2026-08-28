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
└── docs/                                     # 产品需求草稿、发布快照与可复用流程模板
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

#### 静态原型评审

产品经理可以在 VS Code 中使用 Prototype Agent，把静态页面直接绘制在最终生产目录。原型不调用后端，不使用 Mock 数据：

```bash
cd src/react-admin
bun run dev:prototype
```

通过 `http://localhost:11026/__prototype/WI-<id>` 评审登记的页面。详细流程参考 [前端静态原型晋级流程](docs/work-item-templates/frontend-static-prototype-workflow.md)。

## 研发约束与 GitHub Copilot 规范

本项目已配置通用开发规范，详见 [AGENTS.md](AGENTS.md)。

项目 Skills 位于 Copilot 官方支持的 [`.agents/skills/`](.agents/skills/)；路径规则位于 [`.github/instructions/`](.github/instructions/)，产品经理、Prototype 交互智能体和 Backend/Frontend 研发子智能体位于 [`.github/agents/`](.github/agents/)；VS Code Azure DevOps MCP 配置位于 [`.vscode/mcp.json`](.vscode/mcp.json)。

模板配置只包含 `CHANGE_ME` 占位符。运行宿主前请通过环境变量提供实际敏感配置：`ConnectionStrings__DedsiNativeDB`、`ConnectionStrings__DedsiNativeRabbitMQ` 和 `Jwt__Secret`；通过 Aspire 启动时还需配置 `Parameters__PostgresPassword`、`Parameters__RabbitMqUserName` 和 `Parameters__RabbitMqPassword`。不要把真实值提交到仓库。

## 文档存放规范

`docs/product/` 保存产品经理与 VS Code Copilot 讨论形成的本地需求草稿和发布快照；`docs/work-item-templates/` 保存可复用流程模板。需求发布后，Azure DevOps 是工作项、验收标准、状态和执行记录的远程事实来源；Listener 不读取本地需求文件作为编码输入。

产研一体流程请参考：[产研一体需求流程](docs/product/README.md)。
无人值守编码工作项请使用：[Azure DevOps 无人值守编码工作项模板](docs/work-item-templates/azure-devops-coding-work-item.md)。

前端 UI 规范请参考：[dedsi-style-react-admin-ui](.agents/skills/dedsi-style-react-admin-ui/SKILL.md)。

## Azure DevOps 与无人值守 Work Item Listener

Azure DevOps 是已发布工作项、验收标准、状态、评论日志、分支、PR 和 Pipeline 状态的唯一远程事实来源。产品经理在 VS Code 中手动使用产品经理 Agent，经发布预览和人工确认后通过 Azure DevOps MCP 创建或更新需求；无人值守执行只由 `DedsiNative.WorkItemListener` 通过 REST API 消费已准备工作项。

每个生成项目只绑定一个 Azure DevOps Project。模板参数 `--AdoProject` 会写入项目规则和 Listener 默认值；Listener 不枚举或操作其他 Project。

创建模板时提供组织和 Project：

```bash
dotnet new dedsi-native -n YourProject \
  --HttpPort 12256 \
  --AdoOrg YourOrg \
  --AdoProject YourProject
```

产品经理首次使用时，在 VS Code 打开 `.vscode/mcp.json`，启动 `ado` Server 并通过浏览器完成交互登录。该配置只启用 `core` 和 `work-items` 域，不在仓库保存凭据。

首次运行前，直接在生成项目根目录创建 `.env.local`。在本模板仓库中，对应路径是 `content/.env.local`：

```dotenv
ADO_PAT=<原始 Azure DevOps PAT>
COPILOT_GITHUB_TOKEN=<支持 Copilot Requests 的 fine-grained token>
```

`.env.local` 已被 Git 忽略且不会进入 NuGet 模板包。Listener 直接读取原始 PAT，但不会把 Azure DevOps Token 传给 Copilot CLI、工作项、Git 或 PR。Headless 环境使用 `COPILOT_GITHUB_TOKEN`，交互式机器也可以复用 Copilot CLI 已保存的 OAuth 登录。

安装并登录 GitHub Copilot CLI，然后验证 Listener 配置：

```bash
copilot --version
dotnet run --project src/dotnet/tools/DedsiNative.WorkItemListener -- --validate
```

Listener 通过 WIQL 查询整个 Project，使用 revision CAS 原子领取队列项，维护租约，并为每项创建独立 Git worktree。Copilot CLI 以非交互 Autopilot 模式完成实现和验证，并可在同一工作项内使用 Backend/Frontend 研发子智能体；Listener 再负责 commit、push、创建关联 PR、启用 autocomplete，并在 PR 按分支策略完成后回写工作项终态。

上游流程创建并评审工作项后，使用以下标签把它放入编码队列：

```text
copilot-loop; copilot-ready; copilot-stage-backlog; copilot-attempt-0
```

Listener 不创建需求，也不需要把工作项分配给人员、聊天会话或 Agent：

```bash
dotnet run --project src/dotnet/tools/DedsiNative.WorkItemListener
```

完整运行前必须满足：`origin/main` 可访问、Git 已配置提交身份与 Azure Repos push 权限、Copilot CLI 可以非交互认证、PAT 具有工作项/代码/PR 所需权限，并且目标分支已配置 reviewer、build validation 等策略。Listener 不绕过任何分支策略。

- 无人值守 Listener 说明：[WORK-ITEM-LISTENER.md](WORK-ITEM-LISTENER.md)
- 产研一体需求流程：[docs/product/README.md](docs/product/README.md)
- 产品需求编写模板：[docs/product/templates/product-requirement.md](docs/product/templates/product-requirement.md)
- Azure DevOps 编码工作项模板：[docs/work-item-templates/azure-devops-coding-work-item.md](docs/work-item-templates/azure-devops-coding-work-item.md)
- 前端静态原型晋级流程：[docs/work-item-templates/frontend-static-prototype-workflow.md](docs/work-item-templates/frontend-static-prototype-workflow.md)
- Listener 源码：[src/dotnet/tools/DedsiNative.WorkItemListener](src/dotnet/tools/DedsiNative.WorkItemListener)
- GitHub Copilot CLI 自动化文档：[Running GitHub Copilot CLI programmatically](https://docs.github.com/en/copilot/how-tos/copilot-cli/automate-copilot-cli/run-cli-programmatically)
