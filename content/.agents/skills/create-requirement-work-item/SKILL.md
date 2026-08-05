---
name: create-requirement-work-item
description: 根据 DedsiNative 的用户业务需求、现有代码和工作项队列创建或整理 docs/workItems 工作项，无需预先存在 docs/domains 领域文档。用户提出新功能、管理能力、接口或页面需求并要求创建、拆分、合并或整理工作项时使用；默认将一个业务领域作为一个整体全栈工作项。
---

# 创建需求工作项

不依赖领域文档，根据用户直接给出的需求创建可评审的 DedsiNative 工作项。工作项记录已确认事实、范围和待决事项；领域模型设计由后续 `work-item-loop` 的领域阶段完成。

## 准备

1. 将包含 `docs/`、`src/` 和 `AGENTS.md` 的目录作为项目根，完整读取根 `AGENTS.md` 与所有会影响目标路径的更具体 `AGENTS.md`。
2. 完整读取 `docs/workItems/_template.md` 和 `.agents/skills/work-item-loop/references/work-item-protocol.md`。
3. 检查 `docs/workItems/` 中的现有工作项、相关代码和已有 API/页面契约，识别重复、依赖和用户已有改动。
4. 将用户明确表达的领域名称、字段、流程、范围和排除项视为事实来源；不要求、也不创建或修改 `docs/domains/`。

## 形成边界

- 从需求中归纳一个业务领域或明确管理模块，并创建一个整体工作项，覆盖该能力的领域设计、后端、前端和验证。
- 不按 CRUD 操作、技术层、页面局部或同一聚合的内部子实体拆分。例如用户要求“字典管理”时，字典分组和字典项应作为同一工作项处理。
- 只有用户明确要求拆开，或能力属于不同领域、不同聚合边界且可独立验收时，才创建多个工作项；写出拆分原因和依赖。
- 不把从需求中推断的聚合、实体、唯一性、字段长度、删除策略、权限关系或状态流转写成既定事实。将它们标记为“待领域阶段确认”。

## 编写工作项

1. 在 `docs/workItems/{领域英文复数或现有模块名}/` 创建 `WI-{DOMAIN}-001-{中文标题}.md`。扫描同前缀 ID，使用下一个未占用序号。
2. 使用模板完整填写 YAML Front Matter。新工作项默认：

   ```yaml
   work-item-status: draft
   work-item-stage: backlog
   work-item-scope: full-stack
   work-item-attempt: 0
   ```

3. 写明目标与业务价值、用户故事、用户已确认的业务规则、包含范围、不包含范围、可验证验收标准和实现提示。
4. 验收标准至少覆盖：用户可见行为、领域阶段输出、Endpoint 不直接操作 `DbContext`、前端类型安全与状态、后端/前端构建及相关测试。仅为实际适用的层定义验收；不适用层需说明原因。
5. 在“实现提示”或首次 Loop 日志中列出待领域阶段确认的决策，避免把缺少的信息伪装成可立即实现的规则。
6. 保留 `LOOP_LOG_START` 与 `LOOP_LOG_END` 标记。创建日志只记录该项尚未实施及待确认项，禁止记录秘密。

## 状态与校验

- 除非用户明确要求准备执行，否则新项保持 `draft`，不转为 `ready`，不启动 Loop。
- 创建后验证工作项 ID 唯一、YAML 元数据完整、验收标准符合协议、日志标记存在，并执行 `git diff --check`。
- 只修改 `docs/workItems/`。用户后续要求补充或确认领域模型时，改用 `create-domain-doc` Skill；用户要求执行时，改用 `work-item-loop` Skill。
