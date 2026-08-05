# 持久化约定

## Entity Configuration

- 每个聚合创建独立的 `IEntityTypeConfiguration<T>` 实现。
- 使用 `DedsiNativeCoreConsts.DbSchemaName` 指定 Schema。
- 显式配置主键、最大长度、必填性、索引、关系、删除行为和并发令牌。
- ULID 字符串主键配置最大长度 26。
- 配置继承自 Dedsi 聚合根的审计字段，保持 PostgreSQL UTC 时间要求。
- 对 `ConcurrencyStamp` 调用 `IsConcurrencyToken()`。
- 为配置类、`Configure()` 和非显然映射规则写中文注释。

## DbContext

- 在 `IDedsiNativeDbContext` 与 `DedsiNativeDbContext` 同时增加 `DbSet<T>`。
- 保持 `[ConnectionStringName(DedsiNativeCoreConsts.ConnectionStringName)]`。
- 继续通过 `ApplyConfigurationsFromAssembly` 自动加载实体配置。
- 不在 DbContext 的 `OnModelCreating` 中堆叠单个实体的字段配置。

## Repository

在 Core 定义：

```csharp
/// <summary>
/// 产品聚合仓储，提供产品的持久化操作。
/// </summary>
public interface IProductRepository : IDedsiCqrsRepository<Product, string>;
```

在 Infrastructure 实现：

```csharp
/// <summary>
/// 产品仓储的 EF Core 实现。
/// </summary>
/// <param name="dbContextProvider">用于获取产品数据库上下文的提供者。</param>
public sealed class ProductRepository(
    IDbContextProvider<DedsiNativeDbContext> dbContextProvider)
    : DedsiDddEfCoreRepository<DedsiNativeDbContext, Product, string>(dbContextProvider),
      IProductRepository;
```

## Query

- 在 Core 暴露不依赖 EF Core 的查询接口。
- 在 Infrastructure 注入 `IDedsiNativeDbContext` 实现查询。
- Query 只用于列表、分页和导出等投影查询；单条详情通过仓储 `GetAsync` 加载完整聚合。
- 把筛选、排序、分页、统计和投影放在 Query 实现中。
- 可选筛选条件使用 `WhereIf` 逐项链式组合，并核对条件与实体属性一一对应。
- 使用 `AsNoTracking()` 执行只读查询。
- 返回 DTO 或面向应用的查询结果，不把 IQueryable 暴露到 Host。

## 中文注释

- 所有新增公共类型和成员必须有中文 XML 文档。
- 注释描述领域含义和数据库约束，不只写“配置字段”。
- 对索引、唯一性、级联删除和精度选择说明业务原因。
- 修改字段时同步更新旧注释，禁止注释与数据库模型不一致。
