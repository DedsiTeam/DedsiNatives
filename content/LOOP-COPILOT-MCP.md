# GitHub Copilot Work Item Loop 与 Azure DevOps MCP

本项目使用 VS Code GitHub Copilot Agent mode 作为工作项 Loop 的执行入口。Loop 直接调用 Azure DevOps MCP，并在当前 checkout 中一次处理一个工作项。

## 1. Copilot 原生目录

- `.github/copilot-instructions.md`：仓库级常驻指令。
- `.github/instructions/*.instructions.md`：按路径应用的后端与前端规则。
- `.github/agents/*.agent.md`：backend、frontend、documentation、logic 自定义代理。
- `.github/skills/*/SKILL.md`：可自动匹配或通过 `/skill-name` 调用的项目 Skills。
- `.vscode/mcp.json`：VS Code 工作区 MCP 配置。

## 2. Azure DevOps MCP

生成项目后，在 VS Code 中打开项目根目录，通过 “MCP: List Servers” 启动 `ado`。VS Code 会连接 `https://mcp.dev.azure.com/{{ADO_ORG}}`，并提示使用 Microsoft Entra 账号登录；项目不保存本地认证秘密。

登录账号必须有权访问模板参数指定的 Organization 和 Project。认证完成后，在 Copilot Chat 切换到 Agent mode 并选择所需 `ado` 工具。MCP 不可用或写入无法重读确认时，Loop 必须停止。

## 3. 队列标签

只有同时包含 `copilot-loop` 与下列状态之一的工作项会进入候选队列：

- `copilot-ready`：已评审，可首次领取。
- `copilot-in-progress`：已领取，仅在分支和运行信息可安全确认时恢复。
- `copilot-failed`：上次可重试失败，未达尝试上限时重试。

`copilot-blocked`、`copilot-pr`、`copilot-completed` 和 `copilot-cancelled` 不会自动领取。完整规则见 [工作项协议](.github/skills/work-item-loop/references/work-item-protocol.md)。

## 4. 使用方式

在 VS Code Copilot Agent mode 中输入：

```text
/work-item-loop 预览当前 Azure DevOps Project 的可领取工作项，只读。
```

或：

```text
/work-item-loop 串行处理队列，最多完成 10 项，每项最多尝试 3 次。
```

Loop 会查询并领取一个工作项，创建或恢复 `copilot/wi-<id>-<slug>` 分支，完成实现和验证，push 并创建 Azure DevOps PR，然后回写状态。只有当前项回写完成且工作区干净后才会领取下一项。`copilot-exclusive` 项完成 PR 后，本次 Loop 立即结束。

## 5. 完成条件

验证通过但 PR 尚未合并时写为 `copilot-pr`。只有 PR 按策略合并且 Pipeline 成功后，才能写为 `copilot-completed` 和 `copilot-stage-done`。Loop 不绕过 reviewer、build validation 或其他分支策略。
