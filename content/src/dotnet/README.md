# DedsiNative Clean Architecture Template

基于 **ABP Framework**、**FastEndpoints** 和 **PostgreSQL** 的 .NET Clean Architecture 解决方案模板。

## 快速开始

### 安装模板

```bash
dotnet new install DedsiNative.Template
```

### 创建项目

```bash
dotnet new dedsi-native -n YourProjectName -o ./YourProjectName
```

### 配置数据库

修改生成项目中的 `host/YourProjectName.Host/appsettings.json`：

```json
{
  "ConnectionStrings": {
    "YourProjectNameDB": "Host=localhost;Port=5432;Username=postgres;Password=your_password;Database=YourProjectNameDB"
  }
}
```

### 添加数据库迁移

```bash
cd YourProjectName

dotnet ef migrations add InitialCreate \
  --project src/YourProjectName.Infrastructure \
  --startup-project host/YourProjectName.Host \
  --context YourProjectNameDbContext \
  --output-dir EntityFrameworkCore/Migrations

dotnet ef database update \
  --project src/YourProjectName.Infrastructure \
  --startup-project host/YourProjectName.Host
```

## 技术栈

| 技术 | 版本 | 说明 |
|------|------|------|
| [ABP Framework](https://abp.io) | 10.5.0 | 模块化 DDD 基础框架 |
| [FastEndpoints](https://fast-endpoints.com) | 8.2.0 | 高性能 API 端点框架 |
| [PostgreSQL (Npgsql)](https://www.npgsql.org) | 10.0.3 | 数据库提供程序 |
| [Serilog](https://serilog.net) | 10.0.0 | 结构化日志 |
| [.NET Aspire](https://learn.microsoft.com/aspire) | 13.5.1 | 可观测性 & 服务默认值 |

## 项目结构

```
YourProjectName/
├── src/
│   ├── YourProjectName.Core/           # 领域层：实体、仓储接口、领域事件
│   ├── YourProjectName.Infrastructure/ # 基础设施层：EF Core、仓储实现
│   └── YourProjectName.Endpoints/      # 接口层：API 端点与 OpenAPI 注册
├── host/
│   └── YourProjectName.Host/           # 宿主层：启动配置与中间件
├── asipres/
│   ├── YourProjectName.AppHost/        # Aspire AppHost
│   └── YourProjectName.ServiceDefaults/# Aspire 服务默认值
├── Directory.Build.props               # 全局构建属性
├── Directory.Build.targets             # 框架版本统一管理
└── global.json                         # SDK 版本锁定
```

## 参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `-n` / `--name` | string | - | 项目名称（必填） |
| `--DbConnectionString` | string | `Host=localhost;Port=5432;...` | PostgreSQL 连接字符串 |

## License

MIT
