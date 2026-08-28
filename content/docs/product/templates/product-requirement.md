---
schema: dedsi-product-requirement/v1
status: draft
kind: feature
title: "{{需求标题}}"
created: YYYY-MM-DD
updated: YYYY-MM-DD
owner: "{{产品负责人}}"
ado:
  organization: "{{ADO_ORG}}"
  project: "{{ADO_PROJECT}}"
  parent_id: null
  parent_url: null
  parent_revision: null
  child_ids: []
  published_at: null
prototype:
  required: undecided
  status: undecided
  work_item_id: null
  pull_request_id: null
  approved_commit: null
---

# {{需求标题}}

## 业务问题

{{当前谁在什么场景中遇到什么问题；为什么值得解决。}}

## 目标用户与场景

- 用户/角色：{{}}
- 使用场景：{{}}
- 当前替代方式：{{}}

## 目标与成功指标

- 产品目标：{{}}
- 可观测结果：{{}}
- 成功指标：{{无法量化时说明为何。}}

## 排除范围

- {{明确不在本次完成的内容。}}

## 领域语言和已确认规则

### 领域语言

| 术语 | 业务含义 | 不表示 |
|---|---|---|
| {{}} | {{}} | {{}} |

### 已确认规则

- {{权限、状态、不变量、计算或流程规则。}}

### 异常和边界场景

- {{空数据、无权限、重复操作、并发或失败场景。}}

## 用户流程

1. {{前置条件。}}
2. {{用户操作。}}
3. {{系统可观察结果。}}

## 静态原型决策

- 是否需要：{{Yes/NotRequired/Undecided}}
- 原因：{{新页面或关键交互通常需要；Bug、纯后端或明确小改动可跳过。}}
- 页面和交互范围：{{}}
- 已批准页面路径：{{未批准时写“无”。}}

## 总体验收标准

- [ ] {{给定条件、操作与可观察结果。}}
- [ ] {{权限、边界或异常验收。}}
- [ ] {{排除范围未被引入。}}

## 依赖、风险和发布约束

- 前置依赖：{{无时写“无”。}}
- 外部依赖：{{无时写“无”。}}
- 数据/兼容/安全风险：{{无时写“无”。}}
- 发布约束：{{无时写“无”。}}

## 待决事项

- [ ] {{尚未确认的业务、权限、契约或交互决策。}}

## Azure DevOps 发布计划

### 父需求

- 工作项类型：{{Epic/Feature/User Story/项目实际类型}}
- 标题：{{}}
- 初始标签：`product-requirement; product-draft`

### 编码子工作项 1

- 工作项类型：{{Product Backlog Item/User Story/Bug/Task/项目实际类型}}
- 标题：{{一个可独立验收的业务能力}}
- 初始标签：`product-work-item; product-draft`
- 原型：{{Required/NotRequired}}
- 依赖：{{无时写“无”。}}

#### Description

```markdown
## 业务目标

{{}}

## 当前上下文和已确认规则

- {{}}

## 实现范围

- {{}}

## 排除范围

- {{}}

## 接口与数据契约

{{无接口时写“无”；否则写明方法、路径、鉴权、请求、响应和错误。}}

## 静态原型

- 状态：{{Required/NotRequired/Approved}}
- 页面路径：{{无时写“无”。}}
- 必须保留：{{无时写“无”。}}

## 依赖和风险

- {{}}
```

#### Acceptance Criteria

```markdown
- [ ] {{业务可观察结果。}}
- [ ] {{权限、边界或失败场景。}}
- [ ] {{适用的构建、测试和前后端集成验证。}}
```

#### Ready 状态

- [ ] 关键规则和契约已确认。
- [ ] 前置依赖已合并。
- [ ] 原型为 `NotRequired` 或已 `Approved`。
- [ ] 可以通过一个 PR 独立交付。
- [ ] 工作项正文自包含，不依赖本地文件或父工作项正文。
- [ ] `Assigned To` 保持为空。

## 发布记录

{{Agent 在 MCP 发布和回读核对后写入；草稿阶段保留为空。}}
