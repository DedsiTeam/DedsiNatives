---
work-item-id: WI-USERS-001
work-item-title: 创建用户
work-item-status: completed
work-item-stage: done
work-item-priority: medium
work-item-domain: 用户
work-item-scope: full-stack
work-item-attempt: 1
work-item-updated-at: "2026-07-28T18:14:14+08:00"
---

# WI-USERS-001 · 创建用户

由 CreateUserEndpoint 及其领域、持久化和前端调用反向整理。

## 目标与用户故事

作为用户管理员，我希望输入名称和邮箱创建用户，以便系统生成稳定的用户身份并触发创建后的领域副作用。

## API 契约

| 项目 | 契约 |
|---|---|
| 方法与路由 | `POST /api/user/create` |
| 请求 | `{ name: string, email: string }` |
| 成功响应 | 新用户的 26 位 ULID 字符串 |
| 认证 | 遵循宿主默认认证策略，不声明匿名访问 |

## 业务规则

1. 名称和邮箱不得为 null、空字符串或纯空白。
2. 新用户 ID 由服务端生成 ULID，客户端不得指定。
3. 用户通过 `IUserRepository` 持久化，不从 Endpoint 直接操作 DbContext。
4. 用户聚合创建时自动且仅注册一次 `UserCreatedEvent`，由事件处理器执行通知副作用。

## 实现范围

### .NET

- 维护 User 聚合构造规则、创建事件、仓储写入和 FastEndpoint 契约。
- 移除与业务无关的临时变量，不记录当前用户或其他敏感信息。

### React

- 提供名称、邮箱表单和邮箱格式校验，通过 `UserApiService.createUser` 提交。
- 提交期间禁止重复操作；成功后关闭表单并刷新列表。

## 验收标准

- [x] AC-001 [passed]: 合法名称和邮箱创建成功，响应为新用户 ULID。
- [x] AC-002 [passed]: 空白名称或邮箱被领域校验拒绝，且不会持久化用户。
- [x] AC-003 [passed]: 创建操作注册一次用户创建领域事件，副作用不写入 Endpoint。
- [x] AC-004 [passed]: 前端创建表单使用类型安全 DTO，具备校验、提交中状态和成功刷新。
- [x] AC-005 [passed]: 后端与前端生产构建通过，并具有创建行为验证证据。

## 来源与风险

来源：`CreateUserEndpoint.cs`、`User.cs`、`UserCreatedEvent.cs`、`UserCreatedEventHandler.cs`、前端 UserApiService 与用户管理页面。

邮箱格式由前端表单校验改善输入体验，但当前不属于后端领域不变量；邮箱唯一性尚未形成明确业务规则，本工作项不增加唯一索引或迁移。

## Loop 执行日志

<!-- LOOP_LOG_START -->
- `2026-07-28T18:08:33+08:00` · attempt 1 · `in-progress/domain`
  - 用户已明确要求开始此 draft 工作项，视为人工批准进入执行；本次只处理 `WI-USERS-001`。
  - 后端 Skills：`dedsi-add-dotnet-feature`、`dedsi-build-fastendpoint`；本工作项不改变持久化结构或仓储实现，`dedsi-efcore-persistence` 不适用。
  - 前端 Skills：`dedsi-add-react-admin-feature`、`dedsi-build-react-admin-api`、`dedsi-style-react-admin-ui`。
  - 领域边界：名称与邮箱执行非空及既有长度约束；前端校验邮箱格式；邮箱唯一性不在已批准验收范围内，不生成唯一索引或迁移。
  - Domain：已同步 `docs/domains/用户.md`，以构造函数自动登记一次 `UserCreatedEvent` 及事件快照为当前领域事实。
  - Backend：创建 Endpoint 使用 `IUserRepository` 持久化并返回服务端 ULID；补充空白输入和 26 位标识测试，后端构建成功，Core 测试 8/8 通过。
  - Frontend：创建 DTO、Service 与 Ant Design Form 使用准确类型；邮箱格式校验、提交中防重复、成功后等待刷新均已落实；新增 CSS Module，并清除请求客户端与通用 DTO 中的显式 `any`。生产构建与 ESLint 均通过。
- `2026-07-28T18:14:14+08:00` · attempt 1 · `completed/done`
  - 修改路径：`docs/domains/用户.md`、User 创建 Endpoint 与 Core 测试、React 请求客户端/User API/用户创建表单及 CSS Module。
  - AC-001：`CreateUserEndpoint` 生成 ULID、构造 `User`、调用 `IUserRepository.InsertAsync` 并返回标识；测试验证标识长度为 26。
  - AC-002：`User` 构造函数复用领域守卫；参数化测试覆盖空字符串和纯空白名称/邮箱。
  - AC-003：测试验证聚合仅含一个 `UserCreatedEvent`；`UserCreatedEventHandler` 独立执行邮件通知。
  - AC-004：`CreateUserInputDto`、泛型 Form 和泛型 HTTP Body 保持类型安全；邮箱规则、`confirmLoading`、提交期关闭保护和成功后刷新已验证通过编译与 lint。
  - AC-005：`dotnet build src/dotnet/DedsiNative.slnx --no-restore` 退出 0；Core 测试 8/8；`bun run build` 与 `bun run lint` 退出 0；显式 `any` 搜索无结果。
  - 非阻塞告警：后端仍报告既有 `Microsoft.OpenApi 2.0.0` 漏洞警告；Vite 报告主包超过 500 kB，均不影响本工作项验收。
<!-- LOOP_LOG_END -->
