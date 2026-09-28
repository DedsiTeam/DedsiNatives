# 产研一体需求流程

本目录供产品经理在 Codex 中讨论、构思和编写需求。明确确认后，`product_manager` Agent 通过 Azure DevOps MCP 发布工作项。

## 总体流程

```text
产品经理 + Codex
    │ 讨论、构思、查看现有代码和工作项
    ▼
docs/product/requirements/*.md
    │ 本地草稿 + 人工评审
    ▼
产品经理 Agent + Azure DevOps MCP
    │ 发布预览 + 明确“确认上传”
    ▼
Azure DevOps 父需求 + 编码子工作项
    │
    ├─ 需要新页面/关键交互 → 静态原型评审 → prototype-approved
    └─ Bug/纯后端/明确小改动 → NotRequired
    │ Ready 再次人工确认
    ▼
已确认、可独立验收的工作项
    │
    ▼
在 Codex 中实施、验证并交付
```

## 初次使用

1. 以生成项目根目录打开 Codex，检查 `.codex/config.toml` 中的 Azure DevOps 组织配置。
2. 选择 `product_manager` Agent，首次使用 Azure DevOps MCP 时完成交互登录。
3. 安装 Node.js 以运行配置中的 `npx @azure-devops/mcp`。
4. 先讨论需求；在本地文件和发布预览都满意前，不确认任何远程写入。

`.codex/config.toml` 只为产品角色启用 Azure DevOps `core` 和 `work-items` 域，不保存 PAT 或 Token。

## 编写和发布

1. 从 [产品需求模板](templates/product-requirement.md) 创建 `requirements/YYYY-MM-DD-<slug>.md`。
2. 完成业务问题、目标、规则、范围、验收、原型决策、待决事项和编码子工作项。
3. 让产品经理 Agent 检查重复需求、自包含性、可验收性和拆分粒度。
4. 要求“预览将上传的 Azure DevOps 工作项，不写入”。
5. 审核父需求、子工作项、字段、标签和父子关系。
6. 明确回复“确认上传”后，Agent 才能调用 MCP。
7. Agent 回读核对并将远程 ID、URL、revision 和时间写回本地文件。

需求发布不等于开始编码。首次发布后工作项仍保持产品阶段；完成 Ready 检查、条件性原型门禁并再次获得人工确认后，再交给 Codex 实施。

父需求使用 `product-requirement; product-draft`，编码子工作项使用 `product-work-item; product-draft`。不设置 `Assigned To`；具体实施由用户在 Codex 中发起。

## 事实来源

- `draft`：本地 Markdown 是需求编写事实来源。
- `published`：Azure DevOps 是执行事实来源；本地 Markdown 是发布快照和产品思考记录。
- `revision-draft`：本地存在待发布修订，远程仍保持上一个有效版本。
- `publish-failed`：发布部分失败，必须核对已创建 ID，避免重复创建。

每个交给 Codex 的 Azure DevOps 编码工作项必须包含完整 Description 和 Acceptance Criteria，不能仅指向本地文件。

## 建议对话

```text
我有一个关于用户搜索的想法。先和我讨论业务目标和边界，不创建文件，不写入 Azure DevOps。

把已确认内容整理为本地需求文件，标记待决事项，不上传。

预览将上传到 Azure DevOps 的父需求和编码子工作项，不写入。

确认上传这一版，上传后回读核对，暂不开始编码。

预览哪些编码子工作项已满足 Ready，不更新 Azure DevOps。
```

## 相关文档

- [产品需求编写模板](templates/product-requirement.md)
- [前端静态原型晋级流程](../work-item-templates/frontend-static-prototype-workflow.md)
