---
name: run-work-item-loop
description: 使用仓库事实来源执行一个 DedsiNative 工作项
agent: work-item-loop
---

使用 `.github/agents/work-item-loop.agent.md` 的流程执行一个工作项。

- 若用户给出路径，只处理该 `docs/workItems/**/*.md` 文件。
- 若未给出路径，按 [`.agents/skills/work-item-loop/SKILL.md`](../../.agents/skills/work-item-loop/SKILL.md) 的脚本和状态规则选择下一项。
- 不要复制或替代根 [`AGENTS.md`](../../AGENTS.md)、Loop protocol 或模块 Skill；先读取它们，再依实际变更执行。
- 结束时回写工作项终态和验证日志，并报告结果；不要进行 Git 提交、推送或破坏性操作。
