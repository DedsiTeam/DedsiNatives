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
- **领域常量约定**：领域实体中不得声明 `MaxNameLength`、`MaxDescriptionLength`、`MaxEmailLength` 等字段约束常量。请在对应领域目录创建 `{Aggregate}Consts`（例如 `Users/UserConsts.cs`），由聚合、基础设施映射和测试统一引用。
- **主键约定**：领域模型主键默认使用 26 位 ULID；只有存在明确的系统兼容性、外部接口或数据库约束等特殊情况时，才使用 Guid 或其他类型，并记录特殊原因。
- **一对多集合约定**：聚合根的一对多子实体集合直接定义为公开集合属性（例如 `ICollection<T> Items { get; private set; } = [];`），领域方法直接对该属性执行 `Add`、`Remove`、`Clear`。不要再创建与集合属性对应的 `_items` 私有字段，也不要通过 `AsReadOnly()` 生成第二个集合视图。
- **领域文档约定**：创建或完善 `docs/domains/*.md` 时，必须使用 `.agents/skills/create-domain-doc/SKILL.md`（`create-domain-doc`），并按照 `docs/domains/用户.md` 的结构生成和校验文档。不得绕过该 Skill 直接创建领域文档。
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

## 6. 自动代理委派与模型路由

本仓库启用 `.codex/agents/` 下的项目级自定义代理。主代理负责理解用户目标、读取并执行适用 Skill、维护计划与工作项状态、整合结果、完成最终验证和回复；满足以下场景时，必须主动委派匹配的独立子任务，无需等待用户再次要求“使用子代理”。

### 6.1 场景路由

| 场景 | 自定义代理 | 模型角色 | 自动委派规则 |
|---|---|---|---|
| 范围清晰的功能实现、缺陷修复、局部重构、测试补充或独立模块验证 | `coding` | Terra / medium | 子任务边界、负责文件和验收命令可以明确时自动委派 |
| 后端领域能力、接口、持久化与 FastEndpoints 实现 | `backend` | Terra / high | 变更仅属于 `src/dotnet`，或 Loop 进入后端编程阶段时委派 |
| 前端页面、API 消费、路由与 UI 交互实现 | `frontend` | Terra / high | 变更仅属于 `src/react-admin`，或 Loop 进入前端编程阶段时委派 |
| 格式明确的文档整理、结构化摘要、说明文档、执行日志或事实材料归档 | `documentation` | Luna / medium | 不涉及关键领域决策，且主代理已读取适用 Skill 和事实来源时自动委派 |
| 复杂领域建模、架构权衡、疑难根因分析、关键代码审查或高风险方案评估 | `logic` | Sol / high / read-only | 需要深度推理或独立复核时自动委派；该代理只读，不承担代码修改 |

未指定自定义代理的普通子任务使用 `.codex/config.toml` 中的默认子代理配置。主代理始终保留最终决策权，不因委派改变当前任务的授权边界。

### 6.2 主动并行条件

- 存在两个或以上互不依赖、不会修改同一批文件的有价值子任务时，主代理应主动并行委派，最多同时使用项目配置允许的 3 个子代理。
- 单个任务同时包含后端、前端、测试、文档或独立审查时，优先按边界拆给匹配代理；存在先后依赖的部分保持顺序执行。
- 委派实现任务时，必须明确指定代理负责的文件或模块、预期结果、已确认契约、适用 Skill 和验证命令，并说明它不是工作区中唯一的执行者，不得撤销或覆盖他人修改。
- `logic` 可与实现工作并行执行只读审查；审查结果由主代理判断并整合，不得让只读代理直接修改文件。
- 主代理必须等待所有必要子代理完成，检查共享工作区中的最终结果并执行整体验证后，才能给出最终回复。

### 6.3 不应委派的情况

- 一步即可完成的微小修改、简单查询或委派成本高于收益的任务。
- 子任务高度耦合、必须连续修改同一文件，或并行执行会造成迁移、快照、锁文件等冲突。
- 读取、解释或决定如何执行 Skill、`AGENTS.md`、工作项协议及权限边界；这些责任始终属于主代理。主代理读取完成后，可在 Skill 允许的范围内委派具体执行工作。
- 破坏性数据库操作、Git 提交/推送、外部消息或其他需要新增授权的动作；委派不能扩大用户已经授予的权限。

### 6.4 Work Item Loop 路由

- 完整 Loop 始终由 Sol 主代理持续统筹，并负责工作项领取、阶段推进、终态判断和状态回写，不得把整个 Loop 交给单个子代理后提前结束主任务。
- 领域模型或架构存在复杂不变量时，自动使用 `logic` 做只读分析或复核。
- 后端编程阶段必须委派 `backend`，前端编程阶段必须委派 `frontend`；主代理不得直接落地这两个阶段的代码，也不得用通用 `coding` 代替专职代理。
- 普通任务无法委派时可由主代理在原授权范围内完成；显式 Work Item Loop 无法执行上述强制委派时，必须将工作项写为 `blocked` 并记录原因。
- 文档格式化、验证日志整理等工作可使用 `documentation`，但领域事实、验收结论和终态必须由主代理核验。
- 只要工作项尚未进入 `completed`、`failed` 或满足协议要求的 `blocked`，主代理不得因某个子代理或某个阶段完成而输出最终回复。

### 6.5 前后端契约与联调

- 进入后端或前端编码前，主代理必须核验工作项中的最小契约，并将采用版本写入执行日志。最小契约包含接口路径、HTTP 方法、鉴权、请求/响应字段、分页字段、状态码和错误结构。
- `backend` 只按该契约实现服务端行为，`frontend` 只按同一契约消费接口。契约缺失、冲突或存在会改变业务语义的歧义时，不得开始相关编码阶段。
- 契约发生变化时，主代理先回写工作项执行日志，再协调受影响的子代理更新实现。
- 交付前由主代理执行后端构建、前端构建以及工作项要求的关键接口或行为验证。

## 7. 任务协作协议

1. 先理解目标、范围、约束和验收条件，读取适用的 `AGENTS.md`、Skill、工作项与现有实现；明确且可执行的部分直接推进。
2. 不确定项必须说明问题、影响、推荐方案及理由；存在实质差异时再列出选项和取舍。
3. 会改变核心业务语义、公开契约、数据结构、权限/安全边界，或需要破坏性操作和新增授权的问题属于阻塞项，等待用户或外部条件解决。
4. 不改变核心语义、风险较低且容易回退的问题属于非阻塞项，采用与现有代码一致的保守方案继续，并在交付结果中说明假设。
5. 用户确认的结论应回写到需求、领域文档、工作项或代码事实来源，并清理冲突的旧规则。
