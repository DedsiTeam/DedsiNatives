---
name: work-item-loop
description: 按 DedsiNative Loop 执行一个工作项并完成状态回写
---

你负责在 DedsiNative 中执行单个 Markdown 工作项。开始前：

1. 读取根 [`AGENTS.md`](../../AGENTS.md)，检查 `git status --short`，保留他人和用户已有改动。
2. 完整读取 [`.agents/skills/work-item-loop/SKILL.md`](../../.agents/skills/work-item-loop/SKILL.md) 与其 `references/work-item-protocol.md`；它们定义领取、阶段门禁、日志和终态规则。
3. 从 `docs/workItems/**/*.md` 选择调用者指定的工作项，或使用 Loop 脚本选择下一项。只处理 `ready`、`failed` 或唯一的 `in-progress` 项，不自动重开其他状态。
4. 根据实际变更范围，完整读取并应用 `src/dotnet/.agents/skills/` 与 `src/react-admin/.agents/skills/` 中对应模块 Skill，并在执行日志中写明所用 Skill。

按 Loop 顺序完成领域模型、后端、前端、验证和状态回写；不跳过领域层，不臆测缺失业务决策。至少执行仓库要求的后端构建与前端构建/类型检查（若阶段适用），并将命令、结果、剩余问题写入工作项日志。仅在所有验收标准和验证通过时标记 `completed`；实现或验证失败标记 `failed`；需要业务决定、外部状态或新增授权时标记 `blocked`。

不要提交、推送、创建 PR、删除迁移、重置 Git 或覆盖无关文件。最终报告工作项 ID、终态、验证证据及下一步/阻塞原因。
