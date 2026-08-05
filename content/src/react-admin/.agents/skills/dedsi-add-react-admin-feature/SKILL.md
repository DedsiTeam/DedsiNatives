---
name: dedsi-add-react-admin-feature
description: 按 DedsiNative 的 React 19、TypeScript、Vite、Ant Design、Axios 与 React Router 约定新增完整后台业务功能。用于新增 CRUD 管理模块、列表/详情页面，或需要同时创建 DTO、API Service、页面、样式、路由和菜单的纵向前端功能。
---

# 新增 Dedsi React Admin 业务功能

## 强制规则

- 将包含 `package.json` 的目录作为 React 根；先读取内容根与 React 根的 `AGENTS.md`、目标目录下更近的 `AGENTS.md`、相邻业务模块和后端 API 契约。
- 开始编码前必须完整读取 [完整功能示例](references/complete-feature-example.md)，按示例的目录、类型边界、中文注释和代码组织输出，只替换实际业务概念与字段。
- 新增 DTO 和 API Service 时同时遵循 `$dedsi-build-react-admin-api`。
- 新增或调整页面视觉时同时遵循 `$dedsi-style-react-admin-ui`，并读取项目要求的 UI 规范文件。
- 所有新增业务代码使用 TypeScript，禁止 `any`；无法确定的外部数据先用 `unknown`，经类型守卫或 Axios 类型约束后再使用。
- 为 DTO 字段、Service 公共方法、页面组件、复杂状态和非显然业务分支编写准确的中文注释。
- 注释说明业务含义、约束或原因，禁止逐行翻译代码或添加无意义分隔注释。
- API 方法和 DTO 按业务模块放入 `src/apiServices/modules/{module}/`；页面放入 `src/pages/{area}/{feature}/`，页面入口使用 `index.tsx`。
- 页面样式默认使用同目录 CSS Module；优先复用 `src/index.css` 变量和 Ant Design Token，禁止无必要的硬编码颜色和大段内联样式。
- 远程列表必须覆盖加载、空数据、失败、分页和筛选状态；表单必须覆盖校验、提交中状态和成功后的数据刷新。
- 不复制现有临时凭证、模拟数据、未清理令牌或不一致响应包装等问题。

## 工作流程

1. 读取 [完整功能示例](references/complete-feature-example.md)，定位相邻页面、API 模块、路由和布局。
2. 从真实后端 Endpoint 或 OpenAPI 确认路由、HTTP 方法、请求体、返回体、分页字段和认证要求；禁止根据页面臆造契约。
3. 在 `src/apiServices/modules/{module}/dtos/` 创建输入与结果 DTO，在模块根目录创建 `{module}.service.ts`，并更新 `src/apiServices/index.ts`。
4. 在 `src/pages/{area}/{feature}/` 创建 `index.tsx` 与 `index.module.css`。根据功能实现列表、搜索、分页、详情和必要的新增/编辑/删除操作。
5. 在 `src/router/index.tsx` 注册路由；仅当现有菜单需要展示该功能时更新布局菜单，保持菜单路径与路由一致。
6. 检查请求竞态、重复提交、页码重置、删除末页最后一条数据等边界。
7. 从 React 根运行 `bun run build`；需要时再运行 `bun run lint`。
8. 交付时说明新增文件、接口契约来源、验证结果及尚未覆盖的测试。

## 变更边界

- 优先扩展已有模块，不创建第二套请求客户端、主题系统或路由体系。
- UI-only 需求不得改变业务规则、请求参数、路由语义或接口调用顺序。
- 不顺手重构无关页面；相邻代码存在问题时，仅做完成当前功能所需的最小修正。
- 没有真实接口时，不把模拟数据伪装成已接通 API；明确隔离并标注临时数据。

## 完成检查

- DTO、Service、页面、样式、路由和菜单之间的名称及路径一致。
- 没有 `any`、未使用导入、硬编码凭证或无意义注释。
- 新增公共类型、DTO 字段、API 方法和关键业务逻辑具备中文注释。
- 搜索时重置到第一页，分页变化能准确触发请求。
- 请求响应结构与后端一致，没有重复解包或错误套用 `ApiResult<T>`。
- `bun run build` 成功，或明确说明未运行的原因和阻塞项。
