# 产研一体需求流程

本目录供产品经理在 Codex 中讨论、构思和编写需求。`product_manager` Agent 将需求、验收标准、评审结论和实施记录保存在本地 Markdown 文件中。

## 编写和评审

1. 以生成项目根目录打开 Codex，选择 `product_manager` Agent，先讨论业务目标和边界。
2. 从 [产品需求模板](templates/product-requirement.md) 创建 `requirements/YYYY-MM-DD-<slug>.md`。
3. 完成业务问题、目标、规则、范围、验收、原型决策、待决事项和可独立验收的编码任务。
4. 检查重复需求、可验收性和拆分粒度，将待决事项与已确认规则分开。
5. 产品评审通过后，将需求状态从 `draft` 更新为 `confirmed`，记录评审结论和日期。
6. 需要新页面或关键交互时，完成静态原型评审；纯后端或明确小改动可记录为 `NotRequired`。
7. 规则、接口契约、依赖和原型均满足 Ready 检查后，由用户在 Codex 中明确发起实施。
8. 实施完成后记录验证结果，人工验收通过后将需求状态更新为 `completed`。

需求确认不等于开始编码。本地需求文件是需求、验收标准、评审结论和实施记录的事实来源。

## 建议对话

```text
我有一个关于用户搜索的想法。先和我讨论业务目标和边界，暂不创建文件。

把已确认内容整理为本地需求文件，标记待决事项。

检查这份需求的编码任务是否可以独立验收，列出还不满足 Ready 的事项。

按已确认的需求实施指定任务，完成构建和相关测试，并记录验证结果。
```

## 相关文档

- [产品需求编写模板](templates/product-requirement.md)
- [前端静态原型晋级流程](../work-item-templates/frontend-static-prototype-workflow.md)
