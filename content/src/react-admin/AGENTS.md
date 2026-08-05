# AI Agent 项目规范与开发指南

## UI 设计与样式规范 (UI Design Specifications)

在本项目中进行任何前端界面重构、UI 组件开发、CSS 样式修改或页面布局调整时，**AI Agent 必须优先读取并严格遵守以下 UI 设计规范文件**：

1. **UI 设计规范指南（Markdown）**：
   - 相对路径：[.agents/prompts/ui.md](.agents/prompts/ui.md)
   - 说明：包含项目统一的颜色 Token（主色 `#4361ee` / 辅助色 `#4895ef`）、8px 梯级间距网格、12px 卡片圆角、8px 控件圆角、按钮与表单各交互状态 (Hover / Focus / Active / Disabled) 以及微动效示例。

---

### UI 开发核心遵守规则：
- **设计风格**：欧美扁平化科技产品风格，保持清晰的视觉层级、充足留白与简洁质感。
- **配色系统**：
  - **Primary**：`#4361ee`（Primary Hover: `#3651d4`）
  - **Secondary**：`#4895ef`（Secondary Hover: `#3a7fc9`）
  - **文本**：主标题 `#111827` / 正文 `#374151` / 次要 `#6b7280`
  - **背景与边框**：页面背景 `#f7f9fc` / 卡片 `#ffffff` / 边框 `#e5e7eb`
- **色彩 Token 唯一来源**：所有十六进制、`rgb()`、`rgba()`、`hsl()` 色值只能在 `src/index.css` 的 `:root` Token 中声明；页面 CSS Module、组件 JSX/TSX 与 Ant Design 主题配置必须引用 `var(--color-*)`、`var(--shadow-*)` 等全局变量，不得再次硬编码或复制色值。
- **间距与尺寸**：严格采用 8px 梯级倍数间距（8px, 16px, 24px, 32px, 48px, 64px）；最大内容宽度控制在 `1200px` ~ `1440px`。
- **圆角规范**：卡片与 Modal 统一使用 `12px` 圆角；按钮与输入框统一使用 `8px` 圆角。
- **新建按钮规范**：所有表示“新增”“新建”或“创建”的页面主操作按钮，必须使用全局 `create-primary-button` 类；纯主色背景、无边框、8px 圆角、品牌色阴影以及 Hover、Focus、Disabled 状态统一定义在 `src/index.css`，禁止在页面 CSS Module 或 JSX 内重复实现。
- **品牌渐变禁用**：品牌主操作、选中态、标题区和装饰背景不得使用主色到辅助色的渐变；统一使用纯色或对应的浅色 Token。
- **改动边界**：仅调整前端界面样式、布局与响应式交互状态，切勿擅自修改业务逻辑、数据接口或功能流程。
