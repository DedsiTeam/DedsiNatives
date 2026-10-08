# 需求编写文件

本目录保存产品经理与 Codex 讨论后形成的本地需求、验收标准、评审结论和实施记录。

命名格式：

```text
YYYY-MM-DD-<lowercase-kebab-case>.md
```

例如：

```text
2026-08-28-user-search.md
```

使用 [产品需求模板](../templates/product-requirement.md) 创建文件。通过 frontmatter 中的 `status` 和 `updated` 区分草稿、已确认需求和已完成需求，状态分别为 `draft`、`confirmed` 和 `completed`。

交给 Codex 实施时，明确需求文件路径和具体编码任务，确保任务包含完整业务上下文、接口契约和验收标准。
