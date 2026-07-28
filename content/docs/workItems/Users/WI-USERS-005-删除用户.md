---
work-item-id: WI-USERS-005
work-item-title: 删除用户
work-item-status: draft
work-item-stage: backlog
work-item-priority: high
work-item-domain: 用户
work-item-scope: full-stack
work-item-attempt: 0
work-item-updated-at: "2026-07-28T18:00:00+08:00"
---

# WI-USERS-005 · 删除用户

由 DeleteUserEndpoint 和前端删除确认交互反向整理。

## 目标与用户故事

作为用户管理员，我希望在明确确认后删除指定用户，以便移除不再需要的用户记录，同时避免误删。

## API 契约

| 项目 | 契约 |
|---|---|
| 方法与路由 | `POST /api/user/delete/{id}` |
| 路径参数 | `id: string`，用户 ULID |
| 请求体 | 无 |
| 成功响应 | `true` |

## 业务规则

1. 删除前必须根据 ID 找到目标用户。
2. 通过 `IUserRepository` 删除聚合，不从 Endpoint 直接操作 DbContext。
3. 不存在或非法 ID 产生一致错误，不把未执行的删除报告为成功。
4. 删除属于破坏性动作，前端必须要求用户明确确认。

## 实现范围

### .NET

- 维护删除路由、仓储加载与删除、取消令牌和错误响应。
- 确认物理删除、软删除以及关联数据行为符合领域决策。

### React

- 通过确认框调用 `UserApiService.deleteUser`，取消时不得发送请求。
- 成功后刷新列表；删除末页最后一条时回退到有效页；失败时展示错误。

## 验收标准

- [ ] AC-001 [pending]: 确认后，有效 ID 对应用户被删除且接口返回 true。
- [ ] AC-002 [pending]: 取消确认不会发出删除请求，数据保持不变。
- [ ] AC-003 [pending]: 不存在或非法 ID 返回一致错误，不报告成功。
- [ ] AC-004 [pending]: 前端删除成功后刷新到有效分页，失败时给出可见反馈。
- [ ] AC-005 [pending]: 后端与前端构建通过，并具有删除与取消行为验证证据。

## 来源与风险

来源：`DeleteUserEndpoint.cs`、`IUserRepository.cs`、UserApiService 和用户列表删除确认框。

当前代码未明确软删除/物理删除、关联数据限制及操作者是否允许删除自身；这些属于破坏性业务决策，工作项转为 ready 前必须确认。

## Loop 执行日志

<!-- LOOP_LOG_START -->
<!-- LOOP_LOG_END -->
