# 项目架构约定

## 技术基线

- 使用 .NET 10、C# 最新语言版本和可空引用类型。
- 使用 ABP 模块组织依赖注入和应用初始化。
- 使用 FastEndpoints 暴露 HTTP API。
- 使用 EF Core 10 与 PostgreSQL 持久化数据。
- 使用 Dedsi Clean Architecture 与 Dedsi DDD 基类。

## 目录和职责

```text
src/
├── DedsiNative.Core/
│   └── {Feature}/
│       ├── {Aggregate}.cs
│       ├── I{Aggregate}Repository.cs
│       ├── I{Aggregate}Query.cs
│       └── Events/
├── DedsiNative.Infrastructure/
│   └── EntityFrameworkCore/
│       ├── Configurations/
│       ├── Repositories/
│       └── Queries/
└── DedsiNative.Host/
    ├── Applications/
    │   └── {Feature}/
    │       └── EventHandlers/
    └── Endpoints/
        └── {Feature}Endpoints/
```

### Core

- 只放领域实体、聚合根、值对象、领域事件、领域服务和仓储/查询契约。
- 使用聚合方法维护业务不变量，属性使用私有设置器。
- 外部能力在 Core 定义接口，在 Infrastructure 实现。
- 不引用 EF Core、FastEndpoints 或 Host。
- 只定义领域事件，不实现 EventHandler。

### Infrastructure

- 实现仓储、查询、DbContext、实体映射和外部服务。
- 把所有 `IEntityTypeConfiguration<T>` 放入 `EntityFrameworkCore/Configurations/`。
- 在 DbContext 接口和实现中同步增加 DbSet。

### Host

- 使用 FastEndpoints，不创建 MVC Controller。
- 按 `Endpoints/{Feature}Endpoints/` 组织 API。
- 创建、详情、更新和删除端点依赖仓储；详情通过 `GetAsync` 加载完整聚合。
- 列表、分页和导出端点依赖查询契约，由 Infrastructure 完成筛选、统计和投影。
- 不直接注入或操作 DbContext。
- 所有事件处理器位于 `Applications/{Feature}/EventHandlers/`，通过 Core 仓储或服务契约协调跨聚合副作用。

## 中文注释

- 为公共类、接口、构造参数、公共方法、公共属性及 DTO 参数编写中文 XML 文档。
- 注释说明业务含义、约束、原因和副作用。
- 为 UTC、并发控制、授权例外、事务提交等非显然约束补充中文说明。
- 不保留无意义占位注释、测试文字或与实现不一致的注释。
