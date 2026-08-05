# DedsiNative Copilot 指引

开始任何修改前，先读取仓库根目录的 [`AGENTS.md`](../AGENTS.md)，并把它作为架构、质量和权限的最高优先级事实来源。不要在 Copilot 文件中重新定义或复制该文件的完整规则。

## 工作项与事实来源

- 工作项队列和验收标准以 [`docs/workItems/`](../docs/workItems/) 中对应 Markdown 为准；一次只处理一个工作项。
- 当任务涉及执行、继续、恢复、验证或检查工作项 Loop 时，先完整读取 [`.agents/skills/work-item-loop/SKILL.md`](../.agents/skills/work-item-loop/SKILL.md) 及其引用的 [`references/work-item-protocol.md`](../.agents/skills/work-item-loop/references/work-item-protocol.md)。
- 进入实现阶段，再按变更范围读取 `src/dotnet/.agents/skills/` 或 `src/react-admin/.agents/skills/` 下的模块 Skill；未读取适用 Skill 前不要编码。
- 代理分工和模型路由以 [`.codex/agents/`](../.codex/agents/) 及根 `AGENTS.md` 的场景说明为准；Copilot 不应绕过这些边界。

## 实现边界

遵循 Clean Architecture、DDD、ABP、FastEndpoints 和 React 目录约定。保持领域层独立，API 不得直接操作 `DbContext`；前端保持严格 TypeScript 类型，禁止 `any`。保留无关或用户已有改动，不执行提交、推送、重置或破坏性数据库操作，除非用户明确授权。

## 验证与交付

修改后按根 `AGENTS.md` 和工作项要求运行匹配的构建、测试或类型检查，并在工作项执行日志中记录命令、结果和变更路径。完成 Loop 时必须将工作项写入规定的终态并提供可核验的证据。
