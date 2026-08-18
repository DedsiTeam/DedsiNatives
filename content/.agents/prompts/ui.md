# DedsiNative React Admin 前端 UI / UX 布局与设计规范

本文档为 DedsiNative 前端管理系统（React + TypeScript + Ant Design）的标准 UI / UX 布局规范。后续所有后台管理页面在新增或重构时，均须严格遵循此规范。

## 文档导航

- [整体页面结构规范](#1-整体页面结构规范-page-layout-structure)
- [检索与刷新交互规范](#2-检索与刷新交互规范-search--refresh-ux)
- [表格列设计与数据呈现规范](#3-表格列设计与数据呈现规范-table--cell-presentation)
- [表单弹窗规范](#4-表单弹窗规范-form-modal-layout)
- [详情弹窗规范](#5-详情弹窗规范-detail-view-modal)
- [CSS Token 与设计主题](#6-css-token-与设计主题-design-tokens)
- [质量与构建要求](#7-质量与构建要求)

## 1. 整体页面结构规范 (Page Layout Structure)

每个功能页面应当采用标准的两卡片垂直布局（`pageContainer`）：

```
+-----------------------------------------------------------------------+
|  Card 1: 检索与操作卡片 (headerCard)                                   |
|  [搜索框: 按...搜索] [查询] [重置]                     [刷新] [新增用户]  |
+-----------------------------------------------------------------------+
|  Card 2: 数据表格卡片 (tableCard)                                      |
|  +-----------------------------------------------------------------+  |
|  | 用户信息 | ID (ULID) | 联系电话 | 最近更新 | 操作                 |  |
|  +-----------------------------------------------------------------+  |
|  | [Avatar] | C01...    | 138...   | 2026... | 详情 编辑 重置密码 删除|  |
|  +-----------------------------------------------------------------+  |
|  显示第 1 - 10 条，共 XX 条记录                              < 1 2 3 > |
+-----------------------------------------------------------------------+
```

### 样式约束：
- 页面外层使用 `display: flex; flex-direction: column; gap: var(--space-20);`。
- 卡片统一直角圆角 `var(--radius-card)` (12px)，带有浅边框 `1px solid var(--color-border)` 与微阴影 `var(--shadow-sm)`。

---

## 2. 检索与刷新交互规范 (Search & Refresh UX)

### 2.1 搜索草稿机制 (Draft Input Pattern)
* **禁止**在搜索框 `onChange` 事件中直接触发远程网络请求，避免高频输入导致 API 流量轰炸与渲染卡顿。
* 应当维护 `draftSearchText` 独立草稿状态，仅在以下情况生效并写入 `searchText`：
  1. 用户按下 **Enter 回车键** (`onPressEnter`)。
  2. 用户点击 **查询** 按钮。
* 触发查询时，自动将 `pageIndex` 重置为 `1`。

### 2.2 重置逻辑 (Reset Pattern)
* 点击 **重置** 按钮时，同步清空草稿框 (`draftSearchText = ''`) 与实际查询条件 (`searchText = ''`)，并将 `pageIndex` 重置为 `1`。

### 2.3 刷新逻辑 (Refresh Action)
* 按钮使用 `<Button icon={<ReloadOutlined spin={loading} />}>刷新</Button>`。
* 保持当前页码和已有查询条件重新拉取 API，且旋转图标直观反馈 Loading 状态。

### 2.4 新增类主操作按钮 (Create Primary Action)

* 所有语义为“新增”“新建”或“创建”的页面主操作按钮必须使用 `<Button type="primary" className="create-primary-button">`。
* `create-primary-button` 使用纯主色背景；无边框、圆角、阴影及 Hover、Focus、Disabled 状态统一定义在 `src/react-admin/src/index.css`，页面不得覆盖或复制这些样式。
* 按钮应搭配与业务语义匹配的新增图标，例如 `PlusOutlined` 或 `UserAddOutlined`，不得只用图标替代文字。
* 禁止在页面 CSS Module 或 JSX `style={{ ... }}` 中再次定义新建按钮的背景色和阴影。

---

## 3. 表格列设计与数据呈现规范 (Table & Cell Presentation)

### 3.1 身份 / 主体列 (Identity Column)
* **Avatar 动态背景色**：根据主体名称通过算法哈希生成固定的高饱和度纯色背景，中间显示名称首字母/大写字符；不得使用品牌渐变。
* **双行信息展示**：
  * **第一行（主名称）**：字号 14px，字重 600（`var(--color-title)`）。
  * **第二行（辅助信息/邮箱）**：字号 12px，次要字体颜色（`var(--color-muted)`），支持文本溢出省略。

### 3.2 ID 编码列 (ULID / Guid Column)
* 统一采用等宽字体呈现（`ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace`），搭配浅灰背景小标签。
* 悬浮带有 `Tooltip` 提示 `"点击复制 ID"`，点击后调用 `copyToClipboard` 将文本写入剪贴板并提示成功的 Message。

### 3.3 状态标签与空值显示 (Status & Nullable Tags)
* **状态 Tag 映射**：必须使用 AntD 语义化色彩结合图标呈现：
  * `1` 正常/启用：`<Tag color="success" icon={<CheckCircleOutlined />}>正常</Tag>`
  * `2` 禁用/停用：`<Tag color="error" icon={<StopOutlined />}>禁用</Tag>`
  * `3` 锁定：`<Tag color="warning" icon={<LockOutlined />}>锁定</Tag>`
* **空值占位**：当电话、身份证或说明字段为空时，统一输出 `<Tag bordered={false} style={{ color: 'var(--color-placeholder)' }}>未填写</Tag>` 或 `-`。

### 3.4 操作列 (Actions Column)
* 固定在表格最右侧（`fixed: 'right'`）。
* 使用 `<Space size={4}>` 保持紧凑间隔。
* 按钮使用纯文本按钮 (`type="text" size="small"`) 并搭配专属色彩：
  * **详情**：品牌主色 (`var(--color-primary)`)，搭配 `<EyeOutlined />` 与 Tooltip。
  * **编辑**：品牌主色 (`var(--color-primary)`)，搭配 `<EditOutlined />` 与 Tooltip。
  * **重置密码/高危设置**：警告色 (`var(--color-warning)`)，搭配 `<LockOutlined />`，且须包裹 `Popconfirm`。
  * **删除**：Danger 红色 (`danger`)，搭配 `<DeleteOutlined />`，须包裹二次确认 `Popconfirm`。

---

## 4. 表单弹窗规范 (Form Modal Layout)

### 4.1 弹窗与分栏结构
* 推荐宽度：双栏表单为 `840px` ~ `860px`。
* 内部使用 `Row` / `Col` 进行布局划分为若干逻辑卡片区（`sectionCard`），背景色使用 `var(--color-surface-subtle)`。
* 每个 `sectionCard` 顶部包含 `sectionTitle`，由主题 Icon、标题文字及右侧辅助计数 Tag 组成；背景使用 `var(--color-surface-subtle)`，不得直接写色值。

```tsx
<div className={styles.sectionCard}>
  <div className={styles.sectionTitle}>
    <div className={styles.sectionTitleLeft}>
      <UserOutlined style={{ color: 'var(--color-primary)' }} />
      <span>基本资料</span>
    </div>
  </div>
  {/* Form Items... */}
</div>
```

### 4.2 密码与安全字段
* 密码输入框搭配“随机生成”按钮，一键生成 20 位包含大小写字母、数字与符号的高强度密码，并同步触发表单校验。

### 4.3 可搜索关联列表 (Selectable Position / Role List)
* 对岗位、角色或权限等多选分配组件，顶部放置带 `SearchOutlined` 的检索 Input 框，支持按关键字实时过滤。
* 右上角提供 `已选 X 个` 的动态 Tag 统计。
* 列表选项支持整行高亮与整行点击切换选定（使用 Checkbox）。

---

## 5. 详情弹窗规范 (Detail View Modal)

### 5.1 头部 Summary Banner
弹窗顶部使用 `var(--color-primary-light)` 纯色浅背景的主体概要 Header Card，不使用品牌渐变：
- 左侧：大尺寸 Avatar (54px)。
- 右侧：主体名称 (18px, Bold) + 状态 Tag + 邮箱/账号说明。

### 5.2 结构化 Descriptions 表格
* 使用 `Descriptions` 控件，配置 `bordered` 与 `size="small"`。
* 统一 Label 样式：固宽 `width: 130px`、字重 `600`、背景色 `var(--color-surface-subtle)`。
* 编码、IP 地址使用 `<Text code>` 呈现。
* 底部 Footer 提供行动点按钮（如 `编辑此用户`），方便管理员直接从详情切换至编辑模式。

---

## 6. CSS Token 与设计主题 (Design Tokens)

所有页面 CSS Module 必须统一使用 `src/react-admin/src/index.css` 定义的变量：

`src/react-admin/src/index.css` 的 `:root` 是项目色彩的唯一事实来源。十六进制、`rgb()`、`rgba()`、`hsl()` 等字面色值只允许出现在该 Token 定义区；CSS Module、JSX/TSX 内联样式及 Ant Design 主题配置均不得硬编码色值。需要新颜色时，先按语义在 `:root` 增加可复用变量，再在消费端引用变量。

| 变量名 | 推荐用途 |
| :--- | :--- |
| `var(--color-primary)` | 品牌主色、主要按钮、选中高亮 |
| `var(--color-primary-light)`| 悬浮轻背景、选中卡片背景 |
| `var(--color-title)` | 主标题、重要字段文本 |
| `var(--color-muted)` | 次要文本、辅助说明、图标 |
| `var(--color-border)` | 卡片边框、分隔线 |
| `var(--radius-card)` | 卡片与 Modal 圆角 |
| `var(--radius-btn)` | 按钮与 Input 输入框圆角 |
| `var(--shadow-sm)` | 卡片默认轻阴影 |
| `var(--shadow-lg)` | Modal 模态框阴影 |

具体值只在 `src/react-admin/src/index.css` 的 `:root` 中维护。间距采用 4px 基础网格并优先使用现有 `--space-*` Token。

---

## 7. 质量与构建要求

* 任何 UI/UX 调整后，必须在 `src/react-admin` 目录下运行 `bun run build` 确保 TypeScript 类型检查及 Vite 构建无任何报错。
