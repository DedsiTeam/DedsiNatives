# DedsiNative 开发约束与 AI Agent 规范

本项目是基于 Clean Architecture、ABP Framework 和 FastEndpoints 的领域驱动设计 (DDD) 应用。在进行代码新增、修改或重构时，请遵循以下开发规范。

## 1. 架构分层规范

- **领域层 (`src/dotnet/src/DedsiNative.Core`)**
  - 仅包含领域实体 (Entity / AggregateRoot)、值对象 (Value Object)、领域事件 (Domain Event) 与仓储接口 (Repository Interface)。
  - **禁止** 引入任何基础设施或宿主框架的依赖（如 EF Core、FastEndpoints 等）。

- **基础设施层 (`src/dotnet/src/DedsiNative.Infrastructure`)**
  - 实现领域层定义的仓储接口、EF Core `DbContext` 映射以及外部服务（如 EmailSender、文件存储等）。
  - 所有数据库实体映射应当放在 `EntityFrameworkCore/Configurations/` 目录下。

- **宿主与 API 层 (`src/dotnet/src/DedsiNative.Host`)**
  - 使用 FastEndpoints 编写 HTTP 接口，代替传统 ASP.NET Core 控制器 (Controller)。
  - Endpoint 按功能模块归类到 `Endpoints/{Module}/` 目录下。
  - 通过 ABP 模块 (`DedsiNativeHostModule`) 统一注册依赖注入与中间件。

- **编排层 (`src/dotnet/asipres/`)**
  - 使用 .NET Aspire 进行多服务统一编排与遥测监控（OpenTelemetry / HealthCheck）。

- **前端层 (`src/react-admin/`)**
  - 使用 TypeScript + React 构建，API 请求方法与 DTO 按模块存放在 `src/apiServices/` 目录下。

## 2. 后端开发约定

- **代码与文档**：新增公共类和接口需提供清晰的中文注释 / XML 文档。
- **主键约定**：无特殊系统兼容要求时，实体主键建议使用 Guid 或 ULID，保证强类型与无碰撞生成。
- **数据库迁移**：模型新增或更新后，使用 `dotnet-ef` 工具生成与更新迁移：
  ```bash
  dotnet ef migrations add <MigrationName> --project src/dotnet/src/DedsiNative.Infrastructure --startup-project src/dotnet/src/DedsiNative.Host
  dotnet ef database update --project src/dotnet/src/DedsiNative.Infrastructure --startup-project src/dotnet/src/DedsiNative.Host
  ```
- **FastEndpoints 规范**：
  - Endpoint 应当是无状态的、职责单一的类，重写 `Configure()` 和 `HandleAsync()` 方法。
  - 请求与响应定义推荐使用结构化的 DTO。

## 3. 前端开发约定

- **类型安全**：禁止使用 `any`，所有 API 接口输入输出均需定义对应的 TypeScript interface / type。
- **模块化**：页面存放在 `src/pages/`，通用布局存放在 `src/layouts/`。

## 4. 质量与验证要求

- 修改代码后，必须保证后端能够成功构建 (`dotnet build src/dotnet/DedsiNative.slnx`)，前端能够通过类型检查（从 `src/react-admin` 运行 `bun run build` 或 `tsc`）。
- 保持领域概念一致性，禁止跳过领域层直接在 Endpoint 操作 `DbContext`。

## 5. 工作项 Agent Loop

- 当用户要求“执行 Loop”“处理下一个工作项”“继续/恢复工作项”或显式调用 `$work-item-loop` 时，必须使用 `.agents/skills/work-item-loop/SKILL.md`。
- `docs/workItems/**/*.md` 是工作项事实来源；允许按领域建立子目录，`_` 开头的文件不进入队列。
- 单次 Agent 调用只处理一个工作项，严格按“领域模型 → .NET 后端 → React 前端 → 验证 → 状态回写”执行。
- 进入 .NET 阶段时，必须按变更范围加载并应用 `src/dotnet/.agents/skills/` 下对应 Skill；进入 React 阶段时，必须按变更范围加载并应用 `src/react-admin/.agents/skills/` 下对应 Skill。
- 模块 Skill 位于 Loop CWD 的子目录，不依赖 Codex 自动发现；`work-item-loop` 必须通过明确路径完整读取并执行其工作流。
- 仅自动领取 `ready`、`failed` 或唯一的 `in-progress` 工作项。不得自动解除 `blocked`、重开 `completed` 或执行 `draft`。
- 完成实现后必须把工作项写入 `completed`、`failed` 或 `blocked` 终态，并追加不含秘密的验证日志。
- Loop 不授权自动提交、推送、重置 Git、删除迁移或执行破坏性数据库操作。
- 完整协议、使用方法和终止条件见 `LOOP.md`。
