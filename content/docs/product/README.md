# 产研一体需求流程

本目录供产品经理在 VS Code 中与 GitHub Copilot 讨论、构思和编写需求。产品经理明确确认后，产品经理 Agent 通过 Azure DevOps MCP 发布工作项；准备好的编码工作项再由 `DedsiNative.WorkItemListener` 无人值守领取和交付。

## 总体流程

```text
产品经理 + VS Code + Copilot
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
copilot-loop + copilot-ready
    │
    ▼
DedsiNative.WorkItemListener
    │ CAS 领取 + worktree + Copilot CLI + Backend/Frontend
    ▼
PR + Pipeline + 分支策略
    ▼
copilot-completed
```

## 初次使用

1. 安装 VS Code、GitHub Copilot 和 Node.js 20 或更高版本。
2. 以生成项目根目录打开 VS Code。
3. 在 `.vscode/mcp.json` 中启动 `ado` Server，首次使用时通过浏览器完成 Microsoft 账号交互登录。
4. 打开 Copilot Chat，切换到 Agent 模式，手动选择“产品经理”。
5. 先讨论需求；在本地文件和发布预览都满意前，不确认任何远程写入。

`.vscode/mcp.json` 只启用 Azure DevOps `core` 和 `work-items` 域，不保存 PAT 或 Token。MCP 用于本地产品交互，Listener 仍通过自身 REST 客户端运行，两者不共享调度职责。

## 编写和发布

1. 从 [产品需求模板](templates/product-requirement.md) 创建 `requirements/YYYY-MM-DD-<slug>.md`。
2. 完成业务问题、目标、规则、范围、验收、原型决策、待决事项和编码子工作项。
3. 让产品经理 Agent 检查重复需求、自包含性、可验收性和拆分粒度。
4. 要求“预览将上传的 Azure DevOps 工作项，不写入”。
5. 审核父需求、子工作项、字段、标签和父子关系。
6. 明确回复“确认上传”后，Agent 才能调用 MCP。
7. Agent 回读核对并将远程 ID、URL、revision 和时间写回本地文件。

需求发布不等于进入编码队列。首次发布后工作项仍保持产品阶段；只有 Ready 检查和条件性原型门禁全部通过，并再次获得人工确认后，才添加 `copilot-ready`。

父需求使用 `product-requirement; product-draft`，编码子工作项使用 `product-work-item; product-draft`。不设置 `Assigned To`；进入自动队列后由 Listener 使用标签和 revision CAS 领取。

## 事实来源

- `draft`：本地 Markdown 是需求编写事实来源。
- `published`：Azure DevOps 是执行事实来源；本地 Markdown 是发布快照和产品思考记录。
- `revision-draft`：本地存在待发布修订，远程仍保持上一个有效版本。
- `publish-failed`：发布部分失败，必须核对已创建 ID，避免重复创建。

Listener 不读取 `docs/product/` 作为编码输入。每个进入队列的 Azure DevOps 编码工作项仍必须包含完整 Description 和 Acceptance Criteria。

## 建议对话

```text
我有一个关于用户搜索的想法。先和我讨论业务目标和边界，不创建文件，不写入 Azure DevOps。

把已确认内容整理为本地需求文件，标记待决事项，不上传。

预览将上传到 Azure DevOps 的父需求和编码子工作项，不写入。

确认上传这一版，上传后回读核对，但不要添加 copilot-ready。

预览哪些编码子工作项已满足 Ready，不更新 Azure DevOps。
```

## 相关文档

- [Azure DevOps 无人值守编码工作项模板](../work-item-templates/azure-devops-coding-work-item.md)
- [前端静态原型晋级流程](../work-item-templates/frontend-static-prototype-workflow.md)
- [Work Item Listener 说明](../../WORK-ITEM-LISTENER.md)
