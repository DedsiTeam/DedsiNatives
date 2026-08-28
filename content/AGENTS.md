# DedsiNative 项目与 GitHub Copilot 约束

`content/` 是 NuGet 模板展开后的真实项目根，包含 `docs/`、`src/`、`.agents/`、`.github/` 和 `.vscode/`。项目 Skill 位于 Copilot 支持的 `.agents/skills/`，路径规则位于 `.github/instructions/`，自定义智能体位于 `.github/agents/`，VS Code Azure DevOps MCP 配置位于 `.vscode/mcp.json`。`src/dotnet/tools/DedsiNative.WorkItemListener` 是 Azure DevOps 工作项的唯一自动领取和交付编排入口。

## 架构边界

- `src/dotnet/src/DedsiNative.Core`：领域模型、值对象、领域事件及 Repository/Query 契约；不得依赖 EF Core、FastEndpoints、Endpoints 或 Host。
- `src/dotnet/src/DedsiNative.Infrastructure`：EF Core 映射、DbContext、Repository/Query 实现及外部服务。
- `src/dotnet/src/DedsiNative.Endpoints`：FastEndpoints 与应用编排；事件处理器放在 `Applications/{Feature}/EventHandlers/`。
- `src/dotnet/host/DedsiNative.Host`：启动、认证授权、跨域、审计、日志与中间件。
- `src/dotnet/asipres`：.NET Aspire 服务编排与遥测。
- `src/react-admin`：React Admin 前端；DTO 与 API Service 统一放在 `src/apiServices/`。

后端读写边界：完整聚合及创建、修改、删除使用 Repository；列表、分页、统计、导出和 DTO 投影使用 Query。Endpoint、应用服务和事件处理器不得直接操作 DbContext。

## 始终适用的规则

- 只修改用户任务范围内的文件，保留工作区中无关和用户已有改动；不得擅自提交、推送、重置 Git 或执行破坏性数据库操作。
- 不在源码、模板、文档、日志或回复中写入生产环境或其他真实系统的密码、令牌、连接字符串、私钥等秘密；允许在配置中保存明确用于本地开发且不具备生产访问权限的测试凭据。
- 先以现有代码、真实 Endpoint/OpenAPI、已发布 Azure DevOps 工作项和可追溯产品快照确认事实；不得为了完成页面或示例臆造业务规则、接口字段或响应包装。
- 会改变领域语义、公开契约、数据结构、权限或安全边界的歧义属于阻塞项；低风险且易回退的实现细节采用与现有代码一致的保守方案。
- 不手工编辑 EF Core Migration、Designer 或 ModelSnapshot；模型变化时使用项目约定工具生成并检查迁移，未经明确要求不执行 `database update`。

## Azure DevOps 项目边界

- 产品编写和研发交付都限制在 Azure DevOps Project `{{ADO_PROJECT}}`；不枚举、猜测或回退到其他 Project。
- 产品经理可以在 VS Code 中手动选择 `.github/agents/product-manager.agent.md`，在 `docs/product/` 讨论和编写需求，并在每次远程写入前获得人工确认后，使用 `.vscode/mcp.json` 配置的 `ado` MCP 创建或更新工作项。
- 需求首次发布和进入 `copilot-ready` 队列是两次独立操作，必须分别预览和获得人工确认。
- `DedsiNative.WorkItemListener` 通过 Azure DevOps REST API 读取队列、回写执行状态并管理关联 PR；它不使用 MCP，不读取或发布本地需求文件。
- Project 占位符未替换、目标 Project 不存在或当前 MCP/Listener 身份无权访问时属于阻塞项；不得枚举、猜测或回退到其他 Project。
- GitHub Copilot CLI 不连接 Azure DevOps，也不得创建或修改工作项；工作项内容只能作为当前编码任务的只读输入。

## 无人值守工作项边界

- 只有 `DedsiNative.WorkItemListener` 可以查询自动化队列、使用 revision CAS 领取工作项、维护租约、创建 worktree、commit、push、创建 PR 和回写自动化状态。
- GitHub Copilot CLI 每次只实现 Listener 传入的一个工作项；不得查询或领取队列，不得自行修改 Azure DevOps，不得 commit、push、切换分支或创建 PR。
- 自动队列只接受上游已经准备好、同时包含 `copilot-loop` 和 `copilot-ready`，或处于可重试 `copilot-failed` 状态的工作项。
- Copilot CLI 完成实现和本地验证后只留下 worktree 变更并返回机器终态；Listener 负责剩余 Git 与 Azure DevOps 操作。
- PR 的 reviewer、build validation 和其他分支策略是 Pipeline 成功门禁。只有 PR 按策略完成后，Listener 才能把工作项写为 `copilot-completed`。
- 工作项进入 `copilot-ready`、`copilot-in-progress`、`copilot-pr` 或终态后，产品经理 Agent 不再修改其需求正文、标签或自动化状态。

## Copilot 自定义智能体

- 产品经理优先使用 GPT-5.4 进行需求分析和拆分，静态原型优先使用 Claude Sonnet 4.6 进行页面与交互设计；二者仅在 VS Code 中由用户手动选择，并配置另一模型作为不可用时的备用。
- Backend 使用 Copilot CLI 的 `gpt-5.3-codex` 执行 .NET、领域模型和复杂代码变更，Frontend 使用 `claude-sonnet-4.6` 执行 React、TypeScript 和交互实现。Agent 中的模型配置优先于 Listener 外层会话模型；模型受订阅或组织策略限制而不可用时，由 Copilot 回退到可用的会话模型。
- `.github/agents/product-manager.agent.md` 仅供产品经理在 VS Code 中手动选择，只写 `docs/product/**`，并仅在人工明确确认后通过 `ado` MCP 发布工作项。
- `.github/agents/prototype.agent.md` 仅供产品经理在工作项进入 `copilot-ready` 前使用 VS Code 交互调用，只负责可晋级的 React 静态页面，不调用后端且不使用 Mock 数据。
- `.github/agents/backend.agent.md` 只负责当前工作项的 `src/dotnet/**` 后端实现与验证。
- `.github/agents/frontend.agent.md` 只负责当前工作项的 `src/react-admin/**` 前端实现与验证。
- Copilot 主智能体可以按工作项范围自动委派 Backend、Frontend；全栈工作项可并行委派两者，但必须先明确最小接口契约，并由主智能体负责最终集成和统一验证。
- Listener 启动的 Copilot 不委派产品经理或 Prototype Agent；它只在已发布工作项和已批准原型上委派 Backend/Frontend 进行正式研发。
- 子智能体共享 Listener 创建的当前 worktree，只能修改各自路径，不得访问 Azure DevOps、领取工作项、创建需求、commit、push、创建 PR 或继续嵌套其他子智能体。
- 简单或单层任务可以由主智能体直接完成；不为了使用子智能体而制造无必要的拆分。

## 按范围加载最小规则

- 修改 `src/dotnet/`：自动应用 `.github/instructions/dotnet.instructions.md`；只有工作项明确需要框架专项流程时，才按需读取一个直接相关的实现 Skill 及其必要 references。
- 修改 `src/react-admin/`：自动应用 `.github/instructions/react-admin.instructions.md`；只有工作项明确涉及专项 UI 或 API 约定时，才按需读取一个直接相关的实现 Skill。
- 修改 `docs/product/`：自动应用 `.github/instructions/product-requirements.instructions.md`；只有手动选择的产品经理 Agent 可以在人工确认后使用 `ado` MCP 发布。
- Azure DevOps 工作项无人值守执行由 `DedsiNative.WorkItemListener` 完成，不使用聊天主会话、Agent 或 Skill 领取和调度；Copilot CLI 只可在已领取的当前工作项内部委派 Backend/Frontend 编码，不得读取或发布本地需求。

仓库只保留直接服务于代码实现的 Skill。Listener 启动的 Copilot CLI 默认只加载根规则和一个适用的范围规则；Skill 是专项编码参考，不是领取或执行工作项的前置编排层。

## 验证与交付

- 只修改后端时至少运行 `dotnet build src/dotnet/DedsiNative.slnx`；存在相关测试时运行聚焦 `dotnet test`。
- 只修改前端时从 `src/react-admin` 运行 `bun run build`；存在适用命令时运行聚焦 lint 或测试。
- 全栈契约变更同时验证后端、前端及关键接口行为；文档或配置任务运行对应静态检查和 `git diff --check`。
- 修改 `DedsiNative.WorkItemListener` 时运行 `dotnet test src/dotnet/tests/DedsiNative.WorkItemListener.Tests/DedsiNative.WorkItemListener.Tests.csproj`。
- 无法完成验证时说明具体阻塞，不修复任务范围外的问题来制造“全绿”。
- 交付时说明变更范围、关键行为、验证结果、假设和剩余风险；不要重复工具日志或无关实现过程。
