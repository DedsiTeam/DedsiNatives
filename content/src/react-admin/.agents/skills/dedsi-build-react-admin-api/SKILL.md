---
name: dedsi-build-react-admin-api
description: 按 DedsiNative React Admin 项目约定创建、修改和审查类型安全的前端 API 层，包括 Axios 请求客户端、Input/Result DTO、分页契约、业务 Service 和统一导出。用于对接 FastEndpoints、修正 API 类型、增加模块接口或排查响应包装不一致。
---

# 开发 Dedsi React Admin API

## 强制规则

- 将包含 `package.json` 的目录作为 React 根；先读取内容根与 React 根的 `AGENTS.md`、`src/apiServices/core/`、目标后端 Endpoint 和相邻前端 API 模块。
- 开始编码前必须完整读取 [API 示例代码](references/api-example.md)，按示例的目录、命名、类型导入、中文注释和返回方式输出。
- 禁止使用显式或隐式 `any`。请求体默认使用 `unknown` 泛型约束或准确 DTO；错误对象使用 `unknown` 并通过 `axios.isAxiosError()` 收窄。
- 以后端实际 JSON 为唯一响应依据。后端返回原始 DTO 时直接声明该 DTO；只有明确返回统一包装时才使用 `ApiResult<T>`。
- DTO 只描述网络契约，不包含组件状态、Ant Design 类型或展示格式化逻辑。
- 为每个 DTO、字段和公开 Service 方法编写中文 TSDoc，注明业务含义；有助于核对时可注明对应路由。
- 类型导入使用 `import type`，模块统一通过 `src/apiServices/index.ts` 对外导出。
- 不在 Service 中显示成功消息、维护页面状态或吞掉异常；全局鉴权和通用错误由请求客户端处理，业务页面处理业务结果。

## 目录约定

```text
src/apiServices/
├─ core/
│  ├─ base-dto.ts
│  └─ request.ts
├─ modules/
│  └─ product/
│     ├─ dtos/
│     │  ├─ product-input.dto.ts
│     │  └─ product-result.dto.ts
│     └─ product.service.ts
└─ index.ts
```

## 工作流程

1. 读取 [API 示例代码](references/api-example.md) 和后端 Endpoint，逐项记录方法、路径、路径参数、查询参数、请求体和实际响应体。
2. 复用 `PageInputDto` 等现有通用契约；仅在多个模块确实共享时才扩展 core DTO。
3. 分别创建 Input DTO 和 Result DTO。可选字段必须与后端可空性一致，禁止为方便页面而随意设为可选。
4. 创建静态 Service 方法并显式声明 `Promise<T>`；路径参数使用 `encodeURIComponent()`。
5. 更新 `src/apiServices/index.ts`，同时导出 Service 与 `type`。
6. 搜索调用方，确认响应没有被多解包一层，分页字段名称和页码起点一致。
7. 运行 `bun run build`，必要时运行 `bun run lint`。

## 请求客户端约束

- Axios 实例负责 `baseURL`、超时、Bearer Token、统一数据解包和通用错误。
- 请求拦截器只在令牌存在时设置 Authorization。
- 401 流程必须与项目登录策略一致；实现退出时同时清理令牌，不能只导航。
- 泛型请求方法不得以 `any` 作为默认类型或 Axios 响应占位类型。
- 修改拦截器时检查所有模块，因为统一解包会影响每个 Service 的返回类型。

## 完成检查

- API 路径、HTTP 方法及 DTO 字段与后端逐项一致。
- Input 与 Result DTO 分离，且没有 UI 专属字段。
- 全部公共类型和字段具有准确中文注释。
- Service 无页面提示、组件状态和重复异常处理。
- 统一出口已导出新增类型和 Service。
- 全项目没有因本次修改新增 `any` 或类型断言逃逸。
