# DedsiNative 项目与 AI Agent 约束

`content/` 是 NuGet 模板展开后的真实项目根，包含 `docs/`、`src/`、`.agents/` 和 `.codex/`。项目 Skill 统一位于 `.agents/skills/`，Codex 项目配置与自定义代理位于 `.codex/`。

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
- 不在源码、模板、文档、日志或回复中写入真实密码、令牌、连接字符串、私钥等秘密；配置使用环境变量或明确占位符。
- 先以现有代码、真实 Endpoint/OpenAPI、领域文档和工作项确认事实；不得为了完成页面或示例臆造业务规则、接口字段或响应包装。
- 会改变领域语义、公开契约、数据结构、权限或安全边界的歧义属于阻塞项；低风险且易回退的实现细节采用与现有代码一致的保守方案。
- 不手工编辑 EF Core Migration、Designer 或 ModelSnapshot；模型变化时使用项目约定工具生成并检查迁移，未经明确要求不执行 `database update`。

## 按范围加载规则与 Skill

- 修改 `src/dotnet/`：完整读取 `.agents/rules/dotnet.md`，再按任务选择 `dedsi-add-dotnet-feature`、`dedsi-build-fastendpoint`、`dedsi-efcore-persistence`；只读取所选 Skill 要求的 references。
- 修改 `src/react-admin/`：完整读取 `.agents/rules/react-admin.md`；涉及页面、布局或样式时使用 `dedsi-style-react-admin-ui`，完整业务功能使用 `dedsi-add-react-admin-feature`，API 契约使用 `dedsi-build-react-admin-api`。
- 创建或更新 `docs/domains/*.md`：使用 `create-domain-doc`。
- 根据领域文档创建工作项：使用 `create-domain-work-item`；根据直接需求创建工作项：使用 `create-requirement-work-item`。
- 执行、继续、恢复、预览或验证 `docs/workItems` 队列：使用 `work-item-loop`。该 Skill 及其 protocol 是状态、阶段、日志和停止条件的唯一流程来源。

同一规则只保留一个事实来源。Skill 入口负责路由与关键约束，完整示例、模板和条件性细节放在其 `references/`；不要为了“更保险”读取未被当前任务选中的全部 Skill。

## 自定义代理

`.codex/agents/` 提供 `backend`、`frontend`、`documentation`、`logic` 和 `work-item-loop` 专职代理；具体模型、权限和职责以对应 TOML 为准。

- 仅当用户明确要求、适用 Skill 明确要求，或存在两个以上真正独立且能明显提升速度或质量的子任务时委派。
- 一步可完成的小改动、需要连续修改同一文件的任务、Skill/规则解释和权限决策由主代理直接处理。
- 委派不得扩大用户授权；主代理负责契约确认、结果整合、共享工作区检查和最终验证。

## 验证与交付

- 只修改后端时至少运行 `dotnet build src/dotnet/DedsiNative.slnx`；存在相关测试时运行聚焦 `dotnet test`。
- 只修改前端时从 `src/react-admin` 运行 `bun run build`；存在适用命令时运行聚焦 lint 或测试。
- 全栈契约变更同时验证后端、前端及关键接口行为；文档或配置任务运行对应静态检查和 `git diff --check`。
- 无法完成验证时说明具体阻塞，不修复任务范围外的问题来制造“全绿”。
- 交付时说明变更范围、关键行为、验证结果、假设和剩余风险；不要重复工具日志或无关实现过程。
