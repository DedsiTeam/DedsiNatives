# 工作项状态协议

## 文档导航

- [文件与身份](#1-文件与身份)
- [必需元数据](#2-必需元数据)
- [尝试次数](#3-尝试次数)
- [验收标准](#4-验收标准)
- [状态转换](#5-状态转换)
- [阶段门禁](#6-阶段门禁)
- [执行日志](#7-执行日志)
- [停止条件](#8-停止条件)

## 1. 文件与身份

- 队列目录：`docs/workItems`
- 工作项格式：`.md`
- 允许按领域使用子目录，例如 `docs/workItems/Users`；选择器会递归扫描。
- `_` 开头的文件是模板或说明，不进入队列。
- `work-item-id` 在目录内必须唯一，创建后不得更改。
- 一个 Agent 调用最多处理一个工作项。

## 2. 必需元数据

每个可入队 Markdown 文件必须以 YAML Front Matter 声明：

```yaml
---
work-item-id: WI-0001
work-item-title: 工作项标题
work-item-status: ready
work-item-stage: backlog
work-item-priority: medium
work-item-domain: 用户
work-item-scope: full-stack
work-item-attempt: 0
work-item-updated-at: "2026-07-28T16:00:00+08:00"
---
```

合法状态：

| 状态 | 含义 | 可自动选择 |
|---|---|---|
| `draft` | 需求草拟中 | 否 |
| `ready` | 已满足实施条件 | 是 |
| `in-progress` | 已由 Loop 领取 | 是，优先恢复 |
| `failed` | 上一轮可重试失败 | 是 |
| `blocked` | 需要人工或外部条件 | 否 |
| `completed` | 验收完成 | 否 |
| `cancelled` | 已取消 | 否 |

合法阶段：`backlog`、`domain`、`backend`、`frontend`、`verifying`、`done`。

优先级：`critical`、`high`、`medium`、`low`。选择顺序为：

1. 唯一的 `in-progress`
2. `failed`
3. `ready`
4. 同状态按优先级排序
5. 同优先级按工作项 ID 排序

如果同时出现多个 `in-progress`，队列无效，必须停止并让用户处理。

## 3. 尝试次数

- 从 `ready` 或 `failed` 领取时，将 `work-item-attempt` 加一。
- 恢复 `in-progress` 时不增加。
- 外层执行器通过 `--max-retries` 限制最大尝试次数。
- 达到上限的 `failed` 工作项不得继续自动处理；选择器会跳过它并继续选择后续符合条件的工作项。

## 4. 验收标准

每项验收标准采用稳定 ID 和显式状态：

```md
- [ ] AC-001 [pending]: 可验证行为
```

合法验收状态为 `pending`、`passed`、`failed`。`passed` 使用 `[x]`，其余状态使用 `[ ]`，并始终保留方括号内的显式状态，例如：

```md
- [x] AC-001 [passed]: 已通过且具有证据
- [ ] AC-002 [failed]: 验证失败
```

只有具备构建、测试、接口响应、页面行为或代码检查证据时才能改为 `passed`。不得仅凭“代码已编写”判定通过。

## 5. 状态转换

```text
draft -> ready
ready -> in-progress
failed -> in-progress
in-progress -> completed
in-progress -> failed
in-progress -> blocked
blocked -> ready        # 仅人工解除
completed -> ready      # 仅人工重开
任意非 completed -> cancelled  # 仅人工取消
```

Agent 不得自动执行标注为“仅人工”的转换。

预览、列表和验证操作是只读的，不属于状态转换，不得领取工作项、增加尝试次数或写入执行日志。

## 6. 阶段门禁

- `domain`：领域规则无关键歧义，领域文档与计划实现一致。
- `backend`：前后端契约已确认并记录，已按变更范围加载对应 `.NET` 模块 Skill，由 `backend` 子智能体完成实现，后端构建和相关测试通过。
- `frontend`：使用同一份已确认契约，已按变更范围加载对应 React 模块 Skill，由 `frontend` 子智能体完成类型安全的 API 接入与页面行为。
- `verifying`：后端构建、前端构建及相关测试通过；逐条验收。
- `done`：所有验收项均为 `passed`，执行日志包含证据。

即使工作项只涉及部分层，也要显式记录某阶段为“不适用”及原因，然后继续门禁，不得静默跳过。

## 7. 执行日志

在工作项中保留以下标记，并在其间追加 Markdown 日志条目：

```md
<!-- LOOP_LOG_START -->
<!-- LOOP_LOG_END -->
```

每条记录至少包含：

- ISO 8601 时间
- attempt 与结束状态
- 本轮采用的前后端契约摘要及其变更记录
- 后端和前端阶段实际委派的子智能体名称；不适用时记录原因
- 后端和前端阶段实际加载的 Skill 名称；不适用时记录原因
- 修改的路径摘要
- 执行的验证命令及退出结果
- 验收标准证据
- 错误或阻塞问题

日志不得记录密码、令牌、连接字符串或其他秘密。

## 8. 停止条件

满足任一条件立即结束当前 Agent 调用：

- 工作项完成、失败或阻塞
- 队列为空
- 发现多个 `in-progress`
- 元数据非法或 ID 重复
- 需要破坏性操作或新的用户授权
- 发现无法安全保留的用户改动
