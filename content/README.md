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
首次运行前，为 Seq 管理员设置本机 User Secrets：

```bash
dotnet user-secrets set "Parameters:SeqAdminPassword" "<自定义强密码>" \
  --project src/dotnet/asipres/DedsiNative.AppHost
```

运行 Aspire AppHost 启动全套服务（包含后端 API、Aspire Dashboard 与 Seq）：
```bash
dotnet run --project src/dotnet/asipres/DedsiNative.AppHost
```

Seq 页面位于 `http://localhost:15341`，用于持久化查询两项 .NET 服务的结构化日志；Aspire Dashboard 继续用于查看开发期日志、链路和指标。Seq 数据保存在 Docker volume 中。独立启动服务时，未配置 `SEQ_URI` 就不会向 Seq 发送日志。

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

产品经理可以在 Codex 中使用 `prototype` Agent，把静态页面直接绘制在最终生产目录。原型不调用后端，不使用 Mock 数据：

```bash
cd src/react-admin
bun run dev:prototype
```

通过 `http://localhost:11026/__prototype/WI-<id>` 评审登记的页面。详细流程参考 [前端静态原型晋级流程](docs/work-item-templates/frontend-static-prototype-workflow.md)。

## Codex 开发规范

通用规则见 [AGENTS.md](AGENTS.md)，后端、前端和产品文档分别使用各目录的 `AGENTS.md`。专用 Agent 位于 [`.codex/agents/`](.codex/agents/)，技能位于 [`.agents/skills/`](.agents/skills/)。Azure DevOps MCP 配置位于 [`.codex/config.toml`](.codex/config.toml)，产品工作由 `product_manager` Agent 按需启用。

业务时间从写入、存储到显示统一为北京时间，API 使用 `yyyy-MM-dd HH:mm:ss.FFFFFFF`。前端可运行 `bun run build` 和 `bun run test:e2e` 验证。

模板配置只包含 `CHANGE_ME` 占位符。运行宿主前请通过环境变量提供实际敏感配置：`ConnectionStrings__DedsiNativeDB`、`ConnectionStrings__DedsiNativeRabbitMQ` 和 `Jwt__Secret`；通过 Aspire 启动时还需配置 `Parameters__PostgresPassword`、`Parameters__RabbitMqUserName` 和 `Parameters__RabbitMqPassword`。不要把真实值提交到仓库。

## 文档存放规范

`docs/product/` 保存产品经理与 Codex 讨论形成的本地需求草稿和发布快照；`docs/work-item-templates/` 保存可复用流程模板。需求发布后，Azure DevOps 是工作项、验收标准、状态和执行记录的远程事实来源。

产研一体流程请参考：[产研一体需求流程](docs/product/README.md)。

前端 UI 规范请参考：[dedsi-style-react-admin-ui](.agents/skills/dedsi-style-react-admin-ui/SKILL.md)。

## Azure DevOps 产品工作

创建模板时可指定 Azure DevOps 组织和 Project：

```bash
dotnet new dedsi-native -n YourProject \
  --HttpPort 12256 \
  --AdoOrg YourOrg \
  --AdoProject YourProject
```

产品经理在 Codex 中使用 `product_manager` Agent 讨论和整理需求，发布前预览，获得人工确认后通过 Azure DevOps MCP 写入，再回读核对。参见[产研一体需求流程](docs/product/README.md)。
