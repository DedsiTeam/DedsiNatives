# AI Agent 项目规范与开发指南

本规则适用于内容根下的 `src/react-admin/`。涉及 UI、布局或样式时，同时读取内容根的 `.agents/prompts/ui.md`；颜色、阴影、间距与圆角的运行时事实来源是 `src/react-admin/src/index.css`。

## UI 开发核心规则

- **详细规范入口**：完整读取 [UI 设计规范](../prompts/ui.md)。该文件是文字规范的唯一详细来源，本规则只保留不可绕过的边界。
- **设计风格**：欧美扁平化科技产品风格，保持清晰的视觉层级、充足留白与简洁质感。
- **色彩 Token 唯一来源**：所有十六进制、`rgb()`、`rgba()`、`hsl()` 色值只能在 `src/react-admin/src/index.css` 的 `:root` Token 中声明；页面 CSS Module、组件 JSX/TSX 与 Ant Design 主题配置必须引用 `var(--color-*)`、`var(--shadow-*)` 等全局变量，不得再次硬编码或复制色值。
- **间距与形状**：采用 4px 基础网格，优先使用 `src/index.css` 已有 `--space-*`、`--radius-*` Token，不复制具体值。
- **新建按钮规范**：所有表示“新增”“新建”或“创建”的页面主操作按钮必须使用全局 `create-primary-button` 类，禁止在页面 CSS Module 或 JSX 内重复实现。
- **品牌渐变禁用**：品牌主操作、选中态、标题区和装饰背景不得使用主色到辅助色的渐变；统一使用纯色或对应的浅色 Token。
- **改动边界**：仅调整前端界面样式、布局与响应式交互状态，切勿擅自修改业务逻辑、数据接口或功能流程。
