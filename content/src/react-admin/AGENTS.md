# AI Agent 项目规范与开发指南

## UI 设计与样式规范 (UI Design Specifications)

在本项目中进行任何前端界面重构、UI 组件开发、CSS 样式修改或页面布局调整时，**AI Agent 必须优先读取并严格遵守以下 UI 设计规范文件**：

1. **可视化 UI 设计规范指南 (HTML 演示页面)**：
   - 相对路径：[.agents/prompts/ui.html](file:///.agents/prompts/ui.html)
   - 说明：包含项目统一的颜色 Token（主色 `#315efb` / 辅助色 `#8b31fb`）、8px 梯级间距网格、12px 卡片圆角、8px 控件圆角、按钮与表单各交互状态 (Hover / Focus / Active / Disabled) 以及微动效示例。

2. **Markdown 版 UI 设计 Prompt 规范**：
   - 相对路径：[.agents/prompts/UI.MD](file:///.agents/prompts/UI.MD)
   - 说明：包含了核心 UI 优化的指导原则、色盘定义与设计要求条目。

---

### UI 开发核心遵守规则：
- **设计风格**：欧美扁平化科技产品风格，保持清晰的视觉层级、充足留白与简洁质感。
- **配色系统**：
  - **Primary**：`#315efb`（Primary Hover: `#244bd6`）
  - **Secondary**：`#8b31fb`（Secondary Hover: `#7023d8`）
  - **文本**：主标题 `#111827` / 正文 `#374151` / 次要 `#6b7280`
  - **背景与边框**：页面背景 `#f7f9fc` / 卡片 `#ffffff` / 边框 `#e5e7eb`
- **间距与尺寸**：严格采用 8px 梯级倍数间距（8px, 16px, 24px, 32px, 48px, 64px）；最大内容宽度控制在 `1200px` ~ `1440px`。
- **圆角规范**：卡片与 Modal 统一使用 `12px` 圆角；按钮与输入框统一使用 `8px` 圆角。
- **改动边界**：仅调整前端界面样式、布局与响应式交互状态，切勿擅自修改业务逻辑、数据接口或功能流程。
