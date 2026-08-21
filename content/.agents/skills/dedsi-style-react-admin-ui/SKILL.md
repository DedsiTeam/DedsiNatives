---
name: dedsi-style-react-admin-ui
description: 按 DedsiNative 的 Ant Design、CRUD 组件和主题 Token 创建、修改或审查 React Admin UI。用于页面、表格、表单、弹窗、布局、响应式和纯视觉优化；不负责改变业务规则或 API 流程。
---

# 实现 Dedsi React Admin UI

保持界面结构统一、状态完整且不改变既有业务语义。

## 准备

1. 将包含 `package.json` 的目录作为 React 根，读取内容根 `AGENTS.md`、`.agents/rules/react-admin.md`、React 根 `src/index.css` 和相邻页面。
2. 完整读取 [UI / UX 规范](references/ui-guidelines.md)。创建或重构完整页面时再读取 [页面与样式示例](references/ui-example.md)；局部样式调整无需加载完整示例。
3. 列出本次允许修改的组件、文案和样式边界；UI-only 任务不得改变 API 路径、请求参数、权限、路由语义或数据刷新流程。

## 实现要求

- 标准分页管理页复用 `CrudToolbar`、`CrudTable`、`useCrudTable` 和 `CopyableIdTag`；特殊布局仅复用仍适用的组件。
- 页面样式使用同目录 CSS Module；全局 Token 只在 `src/index.css` 定义，消费端不得硬编码字面色值。
- 优先使用 Ant Design 现有组件表达表格、表单、弹窗、详情和反馈，不重复实现已有交互基础设施。
- 保持 loading、empty、disabled、提交中、失败和窄屏等适用状态；错误反馈遵循请求层与页面层既有职责，不为同一异常重复提示。
- 保持键盘可访问性、可见焦点、正确标签和文字对比度；注释只解释非显然的展示模型或交互原因。

## 验证

1. 检查业务及 API 流程没有因纯 UI 调整而变化。
2. 检查新增样式复用 Token，标准 CRUD 页面没有重新实现通用工具栏、表格或分页逻辑。
3. 从 React 根运行 `bun run build`；存在适用命令时运行聚焦 lint，并在条件允许时检查受影响的桌面与移动端交互。
4. 交付时说明视觉变化、状态覆盖、验证结果和无法完成的浏览器检查。
