# 需求编写文件

本目录保存产品经理与 VS Code Copilot 讨论后形成的需求草稿和 Azure DevOps 发布快照。

命名格式：

```text
YYYY-MM-DD-<lowercase-kebab-case>.md
```

例如：

```text
2026-08-28-user-search.md
```

使用 [产品需求模板](../templates/product-requirement.md) 创建文件。发布成功后不需要移动或重命名；通过 frontmatter 中的 `status`、Azure DevOps ID、revision 和时间区分草稿与发布快照。

这些文件不是 Listener 输入。工作项进入 `copilot-ready` 前，必须确保 Azure DevOps 子工作项正文自包含全部编码上下文。
