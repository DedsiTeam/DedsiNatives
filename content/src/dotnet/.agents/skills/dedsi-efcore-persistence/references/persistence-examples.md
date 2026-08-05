# 持久化示例代码

以下示例以 `Product` 聚合为基准，展示映射、DbContext、仓储和分页查询的标准写法。

## 目录

- [实体映射](#实体映射)
- [DbContext](#dbcontext)
- [仓储实现](#仓储实现)
- [查询实现](#查询实现)
- [输出检查](#输出检查)

## 实体映射

文件：`EntityFrameworkCore/Configurations/ProductConfiguration.cs`

```csharp
using DedsiNative.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DedsiNative.EntityFrameworkCore.Configurations;

/// <summary>
/// 产品聚合根的 EF Core 映射配置。
/// </summary>
public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <summary>
    /// 配置产品表、主键、字段约束、审计字段和并发令牌。
    /// </summary>
    /// <param name="builder">产品实体类型构建器。</param>
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", DedsiNativeCoreConsts.DbSchemaName);

        // 产品使用 26 位 ULID 字符串作为领域主键。
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id)
            .HasMaxLength(26)
            .IsRequired();

        builder.Property(product => product.Name)
            .HasMaxLength(128)
            .IsRequired();

        // PostgreSQL 使用 decimal(18,2) 保存金额，避免浮点精度损失。
        builder.Property(product => product.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        // 审计时间统一使用 UTC，具体时钟策略由基础设施模块配置。
        builder.Property(product => product.CreationTime)
            .IsRequired();

        builder.Property(product => product.CreatorId)
            .IsRequired();

        builder.Property(product => product.CreatorName)
            .HasMaxLength(64)
            .IsRequired(false);

        // 将并发戳配置为乐观并发令牌，防止静默覆盖并发更新。
        builder.Property(product => product.ConcurrencyStamp)
            .HasMaxLength(40)
            .IsRequired(false)
            .IsConcurrencyToken();
    }
}
```

## DbContext

在接口和实现中同时增加 DbSet：

```csharp
/// <summary>
/// 产品聚合根对应的数据集。
/// </summary>
DbSet<Product> Products { get; }
```

```csharp
/// <summary>
/// 产品聚合根对应的数据集。
/// </summary>
public DbSet<Product> Products { get; set; }
```

保留程序集扫描：

```csharp
/// <summary>
/// 从基础设施程序集加载全部实体映射配置。
/// </summary>
/// <param name="modelBuilder">数据库模型构建器。</param>
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    ArgumentNullException.ThrowIfNull(modelBuilder);

    modelBuilder.ApplyConfigurationsFromAssembly(
        typeof(DedsiNativeDbContext).Assembly);

    base.OnModelCreating(modelBuilder);
}
```

## 仓储实现

文件：`EntityFrameworkCore/Repositories/ProductRepository.cs`

```csharp
using Dedsi.EntityFrameworkCore.Repositories;
using DedsiNative.Products;
using Volo.Abp.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Repositories;

/// <summary>
/// 产品仓储的 EF Core 实现。
/// </summary>
/// <param name="dbContextProvider">产品数据库上下文提供者。</param>
public sealed class ProductRepository(
    IDbContextProvider<DedsiNativeDbContext> dbContextProvider)
    : DedsiDddEfCoreRepository<DedsiNativeDbContext, Product, string>(
        dbContextProvider),
      IProductRepository;
```

## 查询实现

文件：`EntityFrameworkCore/Queries/ProductQuery.cs`

Query 只承载列表、分页和导出投影；单条详情通过仓储 `GetAsync` 加载完整聚合。
可选筛选条件使用 `WhereIf` 链式组合。

```csharp
using DedsiNative.Products;
using Microsoft.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Queries;

/// <summary>
/// 产品查询服务的 EF Core 实现。
/// </summary>
/// <param name="dbContext">DedsiNative 数据库上下文。</param>
public sealed class ProductQuery(IDedsiNativeDbContext dbContext)
    : IProductQuery
{
    /// <summary>
    /// 按名称筛选产品，并根据导出模式决定是否分页。
    /// </summary>
    /// <param name="query">产品分页查询条件。</param>
    /// <param name="cancellationToken">用于取消异步查询的令牌。</param>
    /// <returns>产品分页查询结果。</returns>
    public async Task<ProductPagedQueryResult> GetPagedAsync(
        ProductPagedQuery query,
        CancellationToken cancellationToken)
    {
        var products = dbContext.Products
            .AsNoTracking()
            .WhereIf(
                !string.IsNullOrWhiteSpace(query.Name),
                product => product.Name.Contains(query.Name!));

        var totalCount = await products.LongCountAsync(cancellationToken);

        if (!query.IsExport)
        {
            products = products
                .OrderByDescending(product => product.CreationTime)
                .PageBy(query.SkipCount, query.MaxResultCount);
        }

        var items = await products
            .Select(product => new ProductQueryItem(
                product.Id,
                product.Name,
                product.Price))
            .ToListAsync(cancellationToken);

        return new ProductPagedQueryResult(totalCount, items);
    }
}
```

## 输出检查

- 把所有业务名称、字段、长度和精度替换为目标聚合的真实要求。
- 不保留示例中的 `Product` 名称。
- DbContext 接口和实现必须同时更新。
- 查询条件必须逐项对应正确实体属性。
- 详情 Endpoint 注入 `IProductRepository`，列表、分页和导出 Endpoint 注入 `IProductQuery`，均不注入 DbContext。
- 模型修改完成后按 `migrations.md` 生成迁移。
