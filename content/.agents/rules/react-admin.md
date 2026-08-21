# AI Agent 项目规范与开发指南

本规则适用于内容根下的 `src/react-admin/`。涉及 UI、布局或样式时使用 `dedsi-style-react-admin-ui`；颜色、阴影、间距与圆角的运行时事实来源是 `src/react-admin/src/index.css`。

## UI 开发核心规则

- **详细规范入口**：涉及页面、布局、交互或样式时，完整读取 [UI 设计规范](../skills/dedsi-style-react-admin-ui/references/ui-guidelines.md)；仅修改 DTO、API Service 等非 UI 代码时无需加载。该文件是文字规范的唯一详细来源，本规则只保留不可绕过的边界。
- **设计风格**：欧美扁平化科技产品风格，保持清晰的视觉层级、充足留白与简洁质感。
- **色彩 Token 唯一来源**：所有十六进制、`rgb()`、`rgba()`、`hsl()` 色值只能在 `src/react-admin/src/index.css` 的 `:root` Token 中声明；页面 CSS Module、组件 JSX/TSX 与 Ant Design 主题配置必须引用 `var(--color-*)`、`var(--shadow-*)` 等全局变量，不得再次硬编码或复制色值。
- **间距与形状**：采用 4px 基础网格，优先使用 `src/index.css` 已有 `--space-*`、`--radius-*` Token，不复制具体值。
- **新建按钮规范**：所有表示“新增”“新建”或“创建”的页面主操作按钮必须使用全局 `create-primary-button` 类，禁止在页面 CSS Module 或 JSX 内重复实现。
- **品牌渐变禁用**：品牌主操作、选中态、标题区和装饰背景不得使用主色到辅助色的渐变；统一使用纯色或对应的浅色 Token。
- **改动边界**：当任务仅要求调整 UI 时，不得擅自修改业务逻辑、数据接口或功能流程。
- **错误反馈边界**：认证、网络和通用服务端错误由请求客户端统一提示；页面只提示请求层未覆盖且用户可采取行动的业务结果，禁止为同一异常重复显示消息。

## CRUD 列表页面架构规则

- 分页管理页面默认使用 `src/components/crud/` 提供的 `CrudToolbar`、`CrudTable`、`useCrudTable` 和 `CopyableIdTag`；从 `src/components` 统一出口导入。
- 通用组件负责工具栏和表格容器、加载与空状态、分页、筛选变化后回到第一页、只接纳最新列表请求的响应、删除刷新及删除末页最后一条后的页码回退。页面不得重复实现这些能力。
- 页面层只负责草稿与已提交筛选条件、列定义、业务表单、DTO 映射、详情请求，以及创建、更新、导出、启停等业务操作。
- 搜索输入与已提交条件必须分离，除非需求明确要求即时搜索；传给 `useCrudTable` 的 `filters` 必须来自 `useMemo` 或引用稳定的 state，不得在调用处传入每次渲染都会重新创建的对象。
- 筛选条件变化后的页码重置由 `useCrudTable` 处理，页面不得为此调用 `refresh(true)` 或维护另一套分页状态。确需保留页码时显式传入 `resetPageOnFilterChange: false`。
- 分页接口应接收业务筛选条件及 `pageIndex`、`pageSize`，并返回 `{ items, totalCount }`；不符合此契约时应在 API Service 层适配，避免页面解包或转换通用分页结构。
- 创建或更新成功后调用 `loadData()` 刷新列表；标准单项删除使用 `handleDelete()`；ID 展示与复制使用 `CopyableIdTag`。
- 额外筛选和操作通过 `CrudToolbar` 的 `extraFilters`、`extraActions` 扩展，不复制整套工具栏；列和业务操作仍使用 Ant Design Table 的标准配置。
- 树形表格、无限滚动、客户端分页、复杂主从列表等不适用标准分页模型的页面可以例外，但必须在页面注释中简述原因，并尽量复用仍适用的 CRUD 组件。
- 从零创建完整业务页面且相邻实现不足时，再读取 `../skills/dedsi-add-react-admin-feature/references/complete-feature-example.md`；普通页面修改无需加载。
