# DedsiNative 项目与 Codex 约束

`content/` 是 NuGet 模板展开后的项目根。Codex Agent 配置位于 `.codex/agents/`，专项 Skill 位于 `.agents/skills/`。处理代码或产品文档前，读取仓库根规则及对应目录规则：

- `src/dotnet/AGENTS.md`：.NET 架构、业务时间、API、注释和验证。
- `src/react-admin/AGENTS.md`：前端类型、UI、API、北京时间和验证。
- `docs/product/AGENTS.md`：产品需求文件与发布边界。

只按任务读取最小必要的 Skill 及其 references。Skill 是专项实现参考，不是任务领取或调度入口。

## 全项目约束

- 只修改当前任务范围内的文件，保留无关及用户已有改动；未经用户明确要求，不执行 Git 提交、推送、重置、创建 PR、回写工作项或破坏性数据库操作。
- 不在源码、模板、文档、日志或回复中写入真实系统的密码、令牌、连接字符串、私钥等秘密；允许明确只用于本地开发的测试凭据。
- 以现有代码、真实 Endpoint/OpenAPI、已发布工作项和可追溯产品快照确认事实，不臆造业务规则、接口字段、响应包装或权限。
- 涉及领域语义、公开契约、数据结构、权限或安全边界的歧义，停止受影响部分并确认；不依赖该决定的工作继续进行。低风险且易回退的实现细节按现有代码处理。
- 不手工编辑 EF Core Migration、Designer 或 ModelSnapshot；只有模型变化时使用项目工具生成并检查迁移，未经明确要求不执行 `database update`。
- 业务日期时间统一为北京时间墙钟。运行环境使用 `Asia/Shanghai`，业务代码使用 `DateTime.Now`，数据库使用 `timestamp without time zone`，API 和页面使用 `yyyy-MM-dd HH:mm:ss.FFFFFFF`。不接受旧 ISO、UTC 或 offset 业务时间输入；JWT、OAuth 等协议时间按协议处理。

## 架构与角色

- Core 定义领域模型、Repository/Query 契约；Infrastructure 实现持久化；Endpoints 编排应用与 HTTP；Host 负责启动、认证、日志等；React Admin 的 DTO 与 API Service 位于 `src/apiServices/`。
- 完整聚合及创建、修改、删除使用 Repository；列表、分页、统计、导出和 DTO 投影使用 Query。Endpoint、应用服务和事件处理器不得直接操作 DbContext。
- `backend` 只处理 `src/dotnet/**`；`frontend` 只处理 `src/react-admin/**`；`prototype` 只处理可晋级静态前端原型；`product_manager` 只写 `docs/product/**`。各角色遵守 `.codex/agents/` 的路径和远程权限边界。
- 用户明确要求实施指定任务后持续完成实现和验证；单纯分析或评审不视为实施授权。全栈任务先固定最小接口契约再协调前后端，主 Agent 负责整合与统一验证。
- Azure DevOps 操作只限模板参数 `{{ADO_PROJECT}}` 所指定的 Project；占位符未替换、Project 不存在或无权访问时停止，不枚举或回退到其他 Project。任何产品远程写入先展示完整预览并取得本次人工确认，发布后回读核对。

## 验证与交付

- 后端变更至少运行 `dotnet build src/dotnet/DedsiNative.slnx`，有相关测试时运行聚焦测试。
- 前端变更从 `src/react-admin` 运行 `bun run build`；涉及关键页面流程时运行适用的 Playwright E2E，存在适用命令时运行聚焦 lint。
- 全栈契约变更同时验证前后端及关键接口行为；文档或配置变更运行静态检查与 `git diff --check`。
- 交付时说明改动、验证结果和剩余问题。正式功能的自动化验证不代替用户人工验收；收到当前任务反馈后继续修正和复测。
