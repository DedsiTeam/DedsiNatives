# GitHub Copilot 项目指令

开始处理任务前，完整读取并遵守仓库根目录的 `AGENTS.md`。该文件是项目结构、架构边界、Azure DevOps 项目边界、无人值守工作项边界以及验证要求的唯一事实来源。

让 `.github/instructions/*.instructions.md` 按 `applyTo` 自动提供当前路径规则。除非工作项明确需要某个专项编码流程，否则不要加载 Skill；不要使用 Skill 领取、分配或调度工作项。

产品经理在 VS Code 中手动选择 `.github/agents/product-manager.agent.md` 时，可以在 `docs/product/` 讨论和编写需求，并在每次远程写入前获得人工明确确认后，通过 `.vscode/mcp.json` 中的 `ado` MCP 发布到固定 Project。发布需求和标记 `copilot-ready` 必须分别确认。

由 `DedsiNative.WorkItemListener` 启动的 Copilot CLI 任务只负责当前 worktree 中的实现和验证；Listener 负责队列、租约、Git、PR 和 Azure DevOps 状态，Copilot CLI 不得接管这些职责，不得连接 `ado` MCP，也不得创建、整理或上传需求。

按变更范围使用 `.github/agents/backend.agent.md` 和 `.github/agents/frontend.agent.md`。全栈工作项可以并行委派两者，但先固定最小接口契约，由主智能体负责集成、最终验证和唯一的 `DEDSI_RESULT` 终态；子智能体不得嵌套委派或输出该终态。
