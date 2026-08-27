# DedsiNative 解决方案

基于 .NET 10、ABP Framework、FastEndpoints 和 PostgreSQL 的 Clean Architecture 解决方案模板。

## 技术栈

- **后端**：.NET 10、ABP Framework、FastEndpoints、EF Core、PostgreSQL
- **服务编排**：.NET Aspire (`src/dotnet/asipres/DedsiNative.AppHost`)
- **前端**：React 19、TypeScript、Vite、Ant Design (`src/react-admin/`)

## 目录结构

```text
├── .github/                                # GitHub Copilot 指令、代理、Skills 与 MCP 配置
├── .vscode/mcp.json                        # VS Code GitHub Copilot MCP 配置
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

本项目已配置通用开发规范，详见 [.github/copilot-instructions.md](.github/copilot-instructions.md)。

项目 Skills、后端/前端规则与专项参考资料统一存放在 [`.github/`](.github/)；`src/` 目录只包含产品源代码。

模板配置只包含 `CHANGE_ME` 占位符。运行宿主前请通过环境变量提供实际敏感配置：`ConnectionStrings__DedsiNativeDB`、`ConnectionStrings__DedsiNativeRabbitMQ` 和 `Jwt__Secret`；通过 Aspire 启动时还需配置 `Parameters__PostgresPassword`、`Parameters__RabbitMqUserName` 和 `Parameters__RabbitMqPassword`。不要把真实值提交到仓库。

## 文档存放规范

`docs/` 目录只维护领域文档。需求、工作项、验收标准、状态和执行日志统一存放于 Azure DevOps，不在本地创建副本。

前端 UI 规范请参考：[dedsi-style-react-admin-ui](.github/skills/dedsi-style-react-admin-ui/SKILL.md)。

## GitHub Copilot 与 Azure DevOps Work Item Loop

项目以 VS Code GitHub Copilot 为主要 AI 开发入口：仓库指令、路径指令、自定义代理与 Skills 均位于 [`.github/`](.github/)，并通过 [`.vscode/mcp.json`](.vscode/mcp.json) 连接 Azure DevOps Remote MCP。Azure DevOps 是工作项、验收标准、状态、评论、分支、PR 和 Pipeline 状态的唯一远程事实来源。

每个生成项目只绑定一个 Azure DevOps Organization 和一个 Project。模板参数 `--AdoOrg` 写入远程 MCP URL，`--AdoProject` 写入 Copilot 仓库指令；除非用户明确要求，Copilot 不枚举或操作其他 Project。

创建模板时提供组织和 Project：

```bash
dotnet new dedsi-native -n YourProject \
  --HttpPort 12256 \
  --AdoOrg YourOrg \
  --AdoProject YourProject
```

生成项目后，在 VS Code 中通过 “MCP: List Servers” 启动 `ado`。首次连接 `https://mcp.dev.azure.com/{{ADO_ORG}}` 时，VS Code 会提示使用有权访问 `{{ADO_ORG}}` 与 `{{ADO_PROJECT}}` 的 Microsoft Entra 账号登录。然后在 GitHub Copilot Chat 中切换到 Agent mode，并从工具列表选择需要的 `ado` 工具。项目不保存本地认证秘密。

Loop 通过 WIQL 直接查询整个 Azure DevOps Project，不需要创建保存查询。需要进入队列的工作项添加 `copilot-loop`，并设置 `copilot-ready`、`copilot-in-progress` 或 `copilot-failed` 状态标签。

在 VS Code Copilot Agent mode 中调用 `/work-item-loop`。当前会话通过 `ado` MCP 串行领取工作项，每项使用独立的 `copilot/wi-<id>-<slug>` 分支完成实现、验证、commit、push、PR 和状态回写。只有当前项回写完成且工作区干净后才领取下一项，避免多个可写代理在同一 checkout 中产生冲突。

例如：

```text
使用 /work-item-loop 预览当前 Azure DevOps Project 的可领取工作项，只读。

使用 /work-item-loop 串行处理队列，最多完成 10 项，每项最多尝试 3 次。
```

完整运行前必须满足：VS Code Copilot Agent mode 可用、`ado` MCP 已通过 Microsoft 账号完成登录、工作区干净、目标分支可访问、Git 已配置提交身份与 Azure Repos push 权限，并且登录账号具有工作项、评论、代码、PR 和 Pipeline 所需权限。Loop 不绕过 reviewer、build validation 或其他分支策略。

- Loop Skill：[.github/skills/work-item-loop/SKILL.md](.github/skills/work-item-loop/SKILL.md)
- 状态协议：[.github/skills/work-item-loop/references/work-item-protocol.md](.github/skills/work-item-loop/references/work-item-protocol.md)
- Copilot 与 MCP 使用说明：[LOOP-COPILOT-MCP.md](LOOP-COPILOT-MCP.md)
- Azure DevOps Remote MCP 文档：[Microsoft Azure DevOps Remote MCP](https://github.com/MicrosoftDocs/azure-devops-docs/blob/main/docs/mcp-server/remote-mcp-server.md)
