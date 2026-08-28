---
applyTo: "docs/product/**"
---

# 产品需求文件规则

- `docs/product/requirements/` 是产品经理与 VS Code Copilot 的本地需求编写区，只保存讨论成稿和发布快照，不作为 Listener 的输入。
- 每份文件使用 `docs/product/templates/product-requirement.md` 结构，并保留可机械识别的 YAML frontmatter。
- 待决事项、假设和已确认规则必须分开；未确认的领域语义不得写入编码子工作项的 Ready 内容。
- 一份需求文件先表达完整产品意图，再列出竖向编码子工作项；不按数据库、后端、前端等技术层单独拆分。
- 每个编码子工作项必须自包含业务目标、规则、范围、排除范围、契约、依赖和可验证验收标准，不能只引用父需求或本地文件。
- 需求发布后，Azure DevOps 是执行事实来源；本地文件保留远程 ID、revision 和发布时间作为快照。
- 文件不得包含 PAT、Token、生产连接字符串、客户秘密、真实个人敏感数据或其他凭据。
