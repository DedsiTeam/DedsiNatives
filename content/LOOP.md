# DedsiNative 工作项 Agent Loop

这个 Loop 以 `docs/workItems` 及其领域子目录中的 Markdown 工作项为持久化队列，按“领域模型 → .NET 后端 → React 前端 → 验证 → 回写状态”完成闭环。

## 组成

- `AGENTS.md`：项目架构、质量和安全边界。
- `.agents/skills/work-item-loop/SKILL.md`：单个工作项的执行流程。
- `docs/workItems/**/*.md`：工作项状态和验收证据。
- `agent-loop.ps1`：重复启动短生命周期 Codex Agent。

每个 Agent 调用只处理一个工作项。连续处理由外层脚本负责，避免一个上下文无限增长。

## 模块 Skill 路由

Loop 从 `content` 启动，不会依赖 Codex 向下自动发现子目录 Skill。进入实现阶段后，`work-item-loop` 会通过明确路径加载模块工作流：

- .NET 阶段：按变更范围使用 `src/dotnet/.agents/skills/` 下的完整功能、FastEndpoint、EF Core Skill。
- React 阶段：按变更范围使用 `src/react-admin/.agents/skills/` 下的完整功能、API、UI Skill。

一个阶段可以叠加多个 Skill。例如全栈用户管理功能的后端通常同时应用完整功能、FastEndpoint 和 EF Core Skill；前端通常同时应用完整功能、API 和 UI Skill。实际使用的 Skill 必须写入工作项执行日志。

## 准备工作项

1. 复制 `docs/workItems/_template.md`。
2. 使用唯一名称，例如 `WI-0002-订单审核.md`。
3. 填写目标、业务规则、范围和可验证的验收标准。
4. 完成评审后，将元数据从：

   ```html
   <meta name="work-item-status" content="draft">
   ```

   改为：

   ```html
   <meta name="work-item-status" content="ready">
   ```

`draft`、`blocked`、`completed` 和 `cancelled` 不会被自动领取。

## 检查队列

验证全部工作项元数据：

```powershell
pwsh -NoProfile -File .agents/skills/work-item-loop/scripts/Get-WorkItem.ps1 `
  -WorkItemsPath docs/workItems -Mode Validate
```

查看完整队列：

```powershell
pwsh -NoProfile -File .agents/skills/work-item-loop/scripts/Get-WorkItem.ps1 `
  -WorkItemsPath docs/workItems -Mode List
```

查看下一项：

```powershell
pwsh -NoProfile -File .agents/skills/work-item-loop/scripts/Get-WorkItem.ps1 `
  -WorkItemsPath docs/workItems -Mode Next
```

## 在 Codex 中受控执行

在 `content` 目录打开 Codex，然后输入：

```text
使用 $work-item-loop 执行下一个工作项。
```

也可以指定工作项：

```text
使用 $work-item-loop 处理 docs/workItems/WI-0002-订单审核.md。
```

## 用 PowerShell 驱动

先预览选择结果和 Prompt，不启动 Agent：

```powershell
./agent-loop.ps1 -DryRun
```

处理一个工作项：

```powershell
./agent-loop.ps1 -MaxItems 1 -MaxRetries 3
```

连续处理最多三个工作项：

```powershell
./agent-loop.ps1 -MaxItems 3 -MaxRetries 3
```

执行器使用：

```text
codex exec --cd <content目录> --sandbox workspace-write <prompt>
```

运行前确保 `codex login status` 成功。不要使用管理员权限运行 Loop。脚本不会自动提交、推送或重置 Git。

## 停止与恢复

Loop 在以下情况停止：

- 队列为空
- 工作项进入 `blocked`
- `failed` 达到最大重试次数
- 工作项元数据非法或 ID 重复
- 同时出现多个 `in-progress`
- Agent 返回后没有写入终态
- Codex 进程异常退出

再次运行脚本时，唯一的 `in-progress` 工作项会优先恢复。解除 `blocked`、重开 `completed` 或取消工作项必须由人工修改状态。
