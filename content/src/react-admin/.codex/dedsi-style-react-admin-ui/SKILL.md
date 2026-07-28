---
name: dedsi-style-react-admin-ui
description: 按 DedsiNatives React Admin 的 Ant Design 与蓝紫色视觉规范创建、修改和审查后台页面 UI。用于实现列表、详情、表单、弹窗、仪表盘、布局、响应式样式或纯视觉优化，并严格保持既有业务逻辑和 API 流程。
---

# 实现 Dedsi React Admin UI

## 强制规则

- 先读取 `content/AGENTS.md`、`content/src/react-admin/AGENTS.md`、目标目录更近的 `AGENTS.md`、`.agents/prompts/ui.html` 和相邻页面。
- 若项目规范引用的文件缺失，记录缺失项并继续使用仍存在的规范，禁止自行编造其内容。
- 开始编码前必须完整读取 [页面与样式示例](references/ui-example.md)，按示例的组件结构、CSS Module、设计变量、中文注释和状态表达输出。
- UI-only 任务不得修改 API 路径、请求参数、业务规则、权限、路由语义或数据刷新流程。
- 使用 Ant Design 现有组件和 `ConfigProvider` 主题，避免重复实现表格、弹窗、表单、分页和提示。
- 页面样式使用 CSS Module；全局基础样式才放入 `src/index.css`。避免大段 `style={{...}}` 和无必要的 `!important`。
- 优先使用现有 CSS 变量：主色 `#315efb`、辅助色 `#8b31fb`、8px 间距体系、卡片/弹窗 12px 圆角、输入框/按钮 8px 圆角。
- 为页面组件、关键展示模型和非显然交互补充中文注释；不要给显而易见的 JSX 添加逐行注释。
- 页面必须覆盖 loading、empty、error、disabled、hover、focus 和移动端等与功能有关的状态。
- 保持键盘可访问性、可见焦点、正确标签和足够的文字对比度。

## 工作流程

1. 读取 [页面与样式示例](references/ui-example.md)、视觉规范、全局变量和相邻页面。
2. 区分业务逻辑与展示层，列出本次允许修改的组件、文案和样式边界。
3. 使用 Card、Table、Form、Modal、Descriptions、Result、Skeleton 等现有组件表达信息层级。
4. 创建或修改同目录 `index.module.css`；复用变量并保持 8px 间距倍数。
5. 对窄屏处理工具栏换行、表格横向滚动、弹窗宽度和内容边距。
6. 检查加载、空态、错误态和提交禁用状态，避免布局抖动和重复点击。
7. 运行 `bun run build`；条件允许时在浏览器检查桌面与移动端。

## 视觉约定

- 页面内容宽度遵循当前布局，复杂内容最大宽度通常在 1200–1440px。
- 主要操作使用主色，危险操作使用 Ant Design danger 语义，不用装饰色冒充状态色。
- 卡片保持浅色背景、克制阴影和清晰边框；不要使用过量渐变、发光或拟物效果。
- 标题、说明、辅助文本形成稳定层级；图标不能替代必要文字。
- 表格筛选区、操作区和分页区应在不同数据量下保持稳定。

## 完成检查

- 已读取现有视觉规范和相邻实现。
- 业务及 API 流程未因纯 UI 调整而改变。
- 新样式集中在 CSS Module，并复用主题变量。
- 页面在 loading、empty、error 和窄屏状态下可用。
- 新增关键代码具有中文注释，不存在无意义注释。
- `bun run build` 成功，或明确说明无法执行的原因。
