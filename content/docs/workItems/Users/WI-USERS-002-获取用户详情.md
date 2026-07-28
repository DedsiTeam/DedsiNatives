---
work-item-id: WI-USERS-002
work-item-title: 获取用户详情
work-item-status: draft
work-item-stage: backlog
work-item-priority: medium
work-item-domain: 用户
work-item-scope: full-stack
work-item-attempt: 0
work-item-updated-at: "2026-07-28T18:00:00+08:00"
---

# WI-USERS-002 · 获取用户详情

由 GetUserEndpoint 和前端详情弹窗反向整理。

## 目标与用户故事

作为用户管理员，我希望通过用户 ID 获取最新详情，以便在不依赖列表缓存的情况下查看准确的名称和邮箱。

## API 契约

| 项目 | 契约 |
|---|---|
| 方法与路由 | `GET /api/user/{id}` |
| 路径参数 | `id: string`，用户 ULID |
| 成功响应 | `{ id, name, email }` |
| 认证 | 遵循宿主默认认证策略 |

## 业务规则

1. 必须提供非空用户 ID。
2. 详情 Endpoint 必须通过 `IUserRepository.GetAsync` 加载完整用户聚合。
3. 响应只映射网络契约所需的 ID、名称和邮箱，不直接序列化领域实体。
4. 不存在的用户必须产生一致、可识别的未找到结果，不返回伪造空对象。

## 实现范围

### .NET

- 维护详情响应 DTO、GET 路由和取消令牌，通过 `IUserRepository.GetAsync` 获取完整聚合后映射响应。
- 详情能力不在 `IUserQuery` 中增加单条投影方法，Endpoint 不直接操作 DbContext。

### React

- 通过 `UserApiService.getById` 获取详情，弹窗展示 ID、名称和邮箱。
- 请求期间呈现加载状态；失败时明确提示，不能把列表行数据伪装成已成功获取的最新详情。

## 验收标准

- [ ] AC-001 [pending]: 有效 ID 通过 `IUserRepository.GetAsync` 加载完整用户聚合，并返回对应的 ID、名称和邮箱。
- [ ] AC-002 [pending]: 不存在或非法 ID 返回一致错误，并且不暴露领域实体或内部异常。
- [ ] AC-003 [pending]: 详情 Endpoint 不直接依赖 DbContext，也不通过 `IUserQuery` 投影单条详情。
- [ ] AC-004 [pending]: 前端详情弹窗具备加载、成功和失败状态，调用契约类型安全。
- [ ] AC-005 [pending]: 后端与前端构建通过，并具有详情行为验证证据。

## 来源与风险

来源：`GetUserEndpoint.cs`、`IUserRepository.cs`、UserApiService 和用户详情 Modal。

需确认仓储 `GetAsync` 在用户不存在时产生的领域异常能够被宿主统一映射为约定的未找到响应；当前前端请求失败时仍可能静默保留列表行数据。

## Loop 执行日志

<!-- LOOP_LOG_START -->
<!-- LOOP_LOG_END -->
