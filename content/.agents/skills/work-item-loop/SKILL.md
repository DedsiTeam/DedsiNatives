---
name: work-item-loop
description: 只读检查或验证 DedsiNative 的 docs/workItems 队列，或执行、继续、恢复一个工作项并完成领域、后端、前端、验证和状态回写闭环。
---

# Work Item Loop

预览、列表和验证模式只读；执行、继续和恢复模式每次只处理一个工作项。连续处理由外层 runner 启动新的短生命周期调用。

## 准备与选择

1. 将包含 `docs/`、`src/` 和 `AGENTS.md` 的目录作为内容根，完整读取根 `AGENTS.md` 与 [工作项状态协议](references/work-item-protocol.md)。
2. 检查 `git status --short`，保留无关和用户已有改动。
3. 先判断模式：预览、列表或验证只运行对应选择器并返回，不领取、不改状态、不写日志。
4. 执行模式优先使用调用方指定的工作项；未指定时运行：

   ```bash
   node .agents/skills/work-item-loop/scripts/get-work-item.mjs \
     --work-items-path docs/workItems --mode next --json
   ```

5. 结果为 `null` 时报告队列为空并成功结束。领取、恢复、尝试次数、状态转换和停止条件完全按 protocol 执行，不在本文件另建一套规则。

## 执行阶段

1. **分析与契约**
   - 核对目标、业务规则、范围、排除项和每条验收标准，明确不适用阶段及原因。
   - 在编码前确认路径、HTTP 方法、鉴权、请求/响应、分页、状态码和错误结构；把采用版本写入执行日志。
   - 契约缺失、冲突或需要实质业务决策时，按 protocol 写为 `blocked`，不要猜测。
2. **领域**
   - 读取匹配的 `docs/domains` 文档，将验收标准落实为聚合、不变量、领域事件及 Repository/Query 契约。
   - 领域文档缺失或需要实质更新时使用 `create-domain-doc`；未经确认的规则保持待确认。
3. **实现路由**
   - 后端完整纵向能力使用 `dedsi-add-dotnet-feature`；仅 Endpoint 或持久化变更分别使用 `dedsi-build-fastendpoint`、`dedsi-efcore-persistence`。只选择实际需要的 Skill。
   - 前端完整业务页面使用 `dedsi-add-react-admin-feature`；仅 API 或 UI 变更分别使用 `dedsi-build-react-admin-api`、`dedsi-style-react-admin-ui`。只选择实际需要的 Skill。
   - 修改后端或前端前读取对应 `.agents/rules/`，并在日志记录实际使用的 Skill。
   - 用户明确要求委派，或存在真正独立且能明显提升速度或质量的子任务时使用 `.codex/agents/` 专职代理；小改动、强顺序依赖和共享文件重叠时由当前代理直接实施。
   - 委派时提供已确认契约、负责路径、适用 Skill、预期结果和验证命令；委派不扩大授权，当前代理仍负责整合和验收。
4. **验证**
   - 运行工作项、所选 Skill 和 `AGENTS.md` 要求的构建与聚焦测试。
   - 对照同一契约核对前后端，检查 diff 是否越界，并以可复现证据逐条更新验收状态。

阶段开始时按 protocol 更新 `work-item-stage`。不适用阶段必须写明原因，不得静默跳过。

## 结束

- 只有全部验收标准具备证据且必要构建/测试通过时才写为 `completed`；可重试实现失败写为 `failed`；需要业务决定、权限、秘密或外部条件时写为 `blocked`。
- 在 `LOOP_LOG_START` / `LOOP_LOG_END` 之间记录执行者、所选 Skill、采用契约、修改路径、验证命令、验收证据和剩余问题；不得记录秘密。
- 不提交、推送、创建 PR、重置 Git、删除迁移或覆盖无关改动，除非用户明确授权。
- 结束时返回工作项 ID、终态、验证证据和阻塞项或下一步。
