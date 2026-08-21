# DedsiNative 工作项 Agent Loop

这个 Loop 以 `docs/workItems` 及其领域子目录中的 Markdown 工作项为持久化队列，按“领域模型 → .NET 后端 → React 前端 → 验证 → 回写状态”完成闭环。

## 组成

- `AGENTS.md`：项目架构、质量和安全边界。
- `.codex/agents/work-item-loop.toml`：单项闭环编排子智能体。
- `.codex/agents/backend.toml` 与 `.codex/agents/frontend.toml`：后端和前端专职子智能体。
- `.codex/agents/logic.toml` 与 `.codex/agents/documentation.toml`：只读深度分析与事实文档整理子智能体。
- `.agents/skills/work-item-loop/SKILL.md`：单个工作项的执行流程。
- `docs/workItems/**/*.md`：工作项状态和验收证据。
- `agent-loop.mjs`：跨平台重复启动短生命周期 Codex Agent，仅依赖 Node.js 内置模块。

每个 Agent 调用只处理一个工作项。连续处理由外层脚本负责，避免一个上下文无限增长。

## 模块 Skill 路由

Loop 从 `content` 启动，不会依赖 Codex 向下自动发现子目录 Skill。进入实现阶段后，`work-item-loop` 会通过明确路径加载模块工作流：

- .NET 阶段：读取 `.agents/rules/dotnet.md`，并按变更范围使用 `.agents/skills/` 下的完整功能、FastEndpoint、EF Core Skill。
- React 阶段：读取 `.agents/rules/react-admin.md`，并按变更范围使用 `.agents/skills/` 下的完整功能、API、UI Skill。

一个阶段可以叠加多个 Skill。例如全栈用户管理功能的后端通常同时应用完整功能、FastEndpoint 和 EF Core Skill；前端通常同时应用完整功能、API 和 UI Skill。实际使用的 Skill 必须写入工作项执行日志。

## 子智能体路由

- Loop 主智能体负责工作项领取、契约确认、阶段推进、结果整合、最终验证和状态回写。
- 用户明确要求委派，或后端、前端、测试等存在真正独立且能明显提升速度或质量的子任务时，使用项目级专职子智能体。
- `backend` 只修改分配的 `src/dotnet` 范围；`frontend` 只修改分配的 `src/react-admin` 范围。
- `logic` 只用于复杂领域或架构的只读分析；`documentation` 只整理经过主智能体核验的事实。
- 小改动、强顺序依赖或共享文件高度重叠时由 Loop 主智能体直接实施，避免委派成本和写入冲突。

## 准备工作项

1. 复制 `docs/workItems/_template.md`。
2. 使用唯一名称，例如 `WI-0002-订单审核.md`。
3. 填写目标、业务规则、范围和可验证的验收标准。
4. 涉及 API 或跨端协作时，填写接口、后端和前端契约；不适用的字段明确说明原因。
5. 契约未完整填写、存在冲突或包含关键业务歧义时，不得进入实现阶段。
6. 完成评审后，将元数据从：

   ```yaml
   work-item-status: draft
   ```

   改为：

   ```yaml
   work-item-status: ready
   ```

`draft`、`blocked`、`completed` 和 `cancelled` 不会被自动领取。

## 开发前契约检查

主智能体在直接实现或委派后端、前端之前检查：接口路径、HTTP 方法、认证方式、请求/响应字段、分页、状态码、错误结构，以及两端实现约束。确认后的版本写入工作项执行日志；发生委派时再提供给对应专职子智能体。

契约发生变化时，先回写执行日志，再协调受影响的子智能体更新；不得让任一子智能体自行猜测或改变公共契约。

## 检查队列

验证全部工作项元数据：

```bash
node .agents/skills/work-item-loop/scripts/get-work-item.mjs \
  --work-items-path docs/workItems --mode validate
```

查看完整队列：

```bash
node .agents/skills/work-item-loop/scripts/get-work-item.mjs \
  --work-items-path docs/workItems --mode list
```

查看下一项：

```bash
node .agents/skills/work-item-loop/scripts/get-work-item.mjs \
  --work-items-path docs/workItems --mode next
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

预览、列表和验证命令只读，不会领取工作项、增加尝试次数或写入执行日志。

## 用 Node.js 驱动

先预览选择结果和 Prompt，不启动 Agent：

```bash
node agent-loop.mjs --dry-run
```

处理一个工作项：

```bash
node agent-loop.mjs --max-items 1 --max-retries 3
```

连续处理最多三个工作项：

```bash
node agent-loop.mjs --max-items 3 --max-retries 3
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
- 没有可执行工作项；达到重试上限的 `failed` 项会被跳过，不会阻塞后续 `ready` 项
- 工作项元数据非法或 ID 重复
- 同时出现多个 `in-progress`
- Agent 返回后没有写入终态
- Codex 进程异常退出

再次运行脚本时，唯一的 `in-progress` 工作项会优先恢复。解除 `blocked`、重开 `completed` 或取消工作项必须由人工修改状态。
