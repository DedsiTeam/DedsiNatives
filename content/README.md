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
└── docs/                                     # 工作项与领域 Markdown 文档
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

`docs/` 目录使用 Markdown（`.md`）维护工作项和领域文档。

前端 UI 规范请参考：[dedsi-style-react-admin-ui](.agents/skills/dedsi-style-react-admin-ui/SKILL.md)。

## 工作项 Agent Loop

项目支持以 `docs/workItems` Markdown 文件作为持久化队列，依次完成领域模型、.NET 后端、React 前端和验证闭环。

- 使用说明：[LOOP.md](LOOP.md)
- 工作项模板：[docs/workItems/_template.md](docs/workItems/_template.md)
- 单项执行 Skill：[.agents/skills/work-item-loop/SKILL.md](.agents/skills/work-item-loop/SKILL.md)

预览下一项但不启动 Agent：

```bash
node agent-loop.mjs --dry-run
```

连续处理最多三个已就绪工作项：

```bash
node agent-loop.mjs --max-items 3 --max-retries 3
```
