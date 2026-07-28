---
work-item-id: WI-USERS-003
work-item-title: 分页查询用户
work-item-status: draft
work-item-stage: backlog
work-item-priority: high
work-item-domain: 用户
work-item-scope: full-stack
work-item-attempt: 0
work-item-updated-at: "2026-07-28T18:00:00+08:00"
---

# WI-USERS-003 · 分页查询用户

由 PagedUserEndpoint、分页 DTO 和用户列表页面反向整理。

## 目标与用户故事

作为用户管理员，我希望按名称或邮箱筛选并分页浏览用户；在导出模式下获取全部匹配记录，以便完成管理和数据导出。

## API 契约

| 项目 | 契约 |
|---|---|
| 方法与路由 | `POST /api/user/pagedQuery` |
| 请求 | 公共分页字段 + `name?`、`email?`、`isExport?` |
| 成功响应 | `{ totalCount, items: [{ id, name, email }] }` |
| 排序 | 按创建时间倒序 |

## 业务规则

1. 名称与邮箱都是可选条件，Infrastructure Query 必须使用链式 `WhereIf` 组合筛选。
2. 名称筛选对 `Name` 执行模糊匹配，邮箱筛选对 `Email` 执行模糊匹配。
3. `totalCount` 必须统计应用全部筛选条件后的记录数。
4. 普通模式按页码和每页数量分页；导出模式不分页，但仍应用筛选与稳定排序。
5. 查询必须使用 `AsNoTracking` 并在数据库端投影 DTO。
6. Endpoint 只编排请求与响应，查询实现放在 Infrastructure 并通过 Core 的 `IUserQuery` 暴露。

## 实现范围

### .NET

- 完善 `IUserQuery`、`UserQuery.GetPagedAsync`、分页输入/输出 DTO 和 `PagedUserEndpoint`。
- 在 `GetPagedAsync` 中使用 `WhereIf` 分别组合名称与邮箱条件，校验页码、PageSize 和导出边界，并传递取消令牌。

### React

- 列表支持分页、名称筛选、邮箱筛选、查询与重置；筛选变化时回到第一页。
- 覆盖 loading、empty、error 和分页末页删除后的刷新行为。

## 验收标准

- [ ] AC-001 [pending]: `GetPagedAsync` 使用链式 `WhereIf`，名称条件只匹配 Name，邮箱条件只匹配 Email，组合条件共同生效。
- [ ] AC-002 [pending]: 普通模式返回正确页数据和筛选后的 totalCount，并按 CreationTime 倒序。
- [ ] AC-003 [pending]: 导出模式返回全部匹配记录，不应用分页但保持筛选和排序。
- [ ] AC-004 [pending]: Endpoint 不直接依赖 DbContext，查询通过 IUserQuery 在 Infrastructure 完成。
- [ ] AC-005 [pending]: 前端名称/邮箱筛选、重置和分页行为与 API 契约一致，失败状态可见。
- [ ] AC-006 [pending]: 后端与前端构建通过，并具有筛选、分页和导出验证证据。

## 来源与已知风险

来源：`PagedUserEndpoint.cs`、`IUserQuery.cs`、`UserQuery.cs`、分页前端 DTO、UserApiService 和用户列表页面。

后端分页查询应持续确保每个 `WhereIf` 条件与实体属性一一对应；前端当前只暴露名称筛选，且请求失败可能被静默吞掉。

## Loop 执行日志

<!-- LOOP_LOG_START -->
<!-- LOOP_LOG_END -->
