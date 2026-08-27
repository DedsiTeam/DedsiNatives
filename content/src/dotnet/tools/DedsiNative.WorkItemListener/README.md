# DedsiNative Work Item Listener

这是一个只需本地出站网络的 WIQL 轮询器。它每分钟查询一次 Azure DevOps，筛选分配给指定账号且带有 `copilot-loop` 与可领取状态标签的工作项，并串行启动 GitHub Copilot CLI 执行仓库现有的 `work-item-loop`。

## 本地配置

程序不再读取环境变量，所有设置都来自项目目录中的 `appsettings.local.json`。

该文件已加入仓库根目录的 `.gitignore`，不会被提交，也不会进入 `dotnet publish` 产物。仓库只提交不含令牌的 `appsettings.example.json`。

首次使用时：

```bash
cd content/src/dotnet/tools/DedsiNative.WorkItemListener
cp appsettings.example.json appsettings.local.json
```

然后编辑 `appsettings.local.json`，将 Azure DevOps PAT 填入：

```json
"ADO_TOKEN": "你的 Azure DevOps PAT"
```

默认使用 PAT 的 `Basic` 认证。令牌至少需要读取 Work Item 的权限。如果填写的是 Microsoft Entra access token，请把 `ADO_AUTH_SCHEME` 改为 `Bearer`。

当前默认配置为：

- Organization：`DedsiTeam`
- Project：`DedsiNatives`
- Assigned To：`1768065921@qq.com`
- 查询间隔：60 秒
- 同一 revision 重试冷却：300 秒
- Copilot Loop：开启
- 仓库目录：`/Users/cohen/GitHub/DedsiNatives/content`

## 运行

先安装并登录 GitHub Copilot CLI，同时为 Copilot CLI 配置名称为 `ado` 的 Azure DevOps MCP。确认 `copilot --version` 可用后运行：

```bash
dotnet run --project content/src/dotnet/tools/DedsiNative.WorkItemListener
```

自动执行前会检查 Git 工作区；存在未提交改动时会拒绝运行。同一进程同一时间只执行一个工作项。

## 候选规则

候选项必须：

- `Assigned To` 等于配置中的 `ADO_ASSIGNED_TO`；
- 包含 `copilot-loop`；
- 包含 `copilot-ready`、`copilot-in-progress` 或 `copilot-failed` 之一。

实际领取、revision 重读、分支、验证、PR、Pipeline 和状态回写仍以 `.github/skills/work-item-loop/` 中的协议为准。
