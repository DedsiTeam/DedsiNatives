---
work-item-id: WI-USERS-004
work-item-title: 更新用户
work-item-status: draft
work-item-stage: backlog
work-item-priority: medium
work-item-domain: 用户
work-item-scope: full-stack
work-item-attempt: 0
work-item-updated-at: "2026-07-28T18:00:00+08:00"
---

# WI-USERS-004 · 更新用户

由 UpdateUserEndpoint、User 聚合行为和前端编辑表单反向整理。

## 目标与用户故事

作为用户管理员，我希望修改指定用户的名称和邮箱，以便保持用户资料准确并由聚合统一维护业务不变量。

## API 契约

| 项目 | 契约 |
|---|---|
| 方法与路由 | `POST /api/user/update/{id}` |
| 路径参数 | `id: string`，用户 ULID |
| 请求 | `{ name: string, email: string }` |
| 成功响应 | `true` |

## 业务规则

1. 必须找到目标用户后才能修改。
2. 名称和邮箱通过 `User.ChangeName` 与 `User.ChangeEmail` 修改，禁止绕过聚合设置属性。
3. 名称和邮箱不得为空或纯空白。
4. 通过 `IUserRepository` 持久化，遵守聚合并发控制。

## 实现范围

### .NET

- 维护更新请求 DTO、路由参数、聚合调用、仓储更新和一致错误响应。

### React

- 编辑弹窗预填当前名称和邮箱，通过 `UserApiService.updateUser` 提交。
- 提供必填、邮箱格式、提交中、失败提示和成功刷新。

## 验收标准

- [ ] AC-001 [pending]: 合法请求通过聚合方法更新目标用户名称和邮箱，并返回 true。
- [ ] AC-002 [pending]: 空白名称或邮箱被拒绝，原用户数据保持不变。
- [ ] AC-003 [pending]: 不存在或非法 ID 返回一致错误，不创建新用户。
- [ ] AC-004 [pending]: 前端编辑表单正确预填、校验、禁用重复提交，并在成功后刷新列表。
- [ ] AC-005 [pending]: 后端与前端构建通过，并具有更新行为验证证据。

## 来源与风险

来源：`UpdateUserEndpoint.cs`、`User.ChangeName`、`User.ChangeEmail`、UserApiService 和用户编辑 Modal。

邮箱格式、邮箱唯一性以及并发冲突的外部响应尚未形成明确业务契约；工作项转为 ready 前应确认这些边界。

## Loop 执行日志

<!-- LOOP_LOG_START -->
<!-- LOOP_LOG_END -->
