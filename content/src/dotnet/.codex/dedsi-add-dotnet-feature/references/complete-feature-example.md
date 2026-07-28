# 完整功能示例

本示例展示新增 `Product` 功能时的标准纵向结构。输出新功能时复用结构和写法，不照抄业务字段。

## 目录

- [文件结构](#文件结构)
- [聚合根](#聚合根)
- [仓储与查询契约](#仓储与查询契约)
- [领域事件](#领域事件)
- [输出要求](#输出要求)

## 文件结构

```text
src/
├── DedsiNative.Core/
│   └── Products/
│       ├── Product.cs
│       ├── IProductRepository.cs
│       ├── IProductQuery.cs
│       ├── Events/
│       │   └── ProductCreatedEvent.cs
│       └── EventHandlers/
│           └── ProductCreatedEventHandler.cs
├── DedsiNative.Infrastructure/
│   └── EntityFrameworkCore/
│       ├── Configurations/ProductConfiguration.cs
│       ├── Repositories/ProductRepository.cs
│       └── Queries/ProductQuery.cs
└── DedsiNative.Host/
    └── Endpoints/
        └── ProductEndpoints/
            ├── CreateProductEndpoint.cs
            ├── GetProductEndpoint.cs
            ├── UpdateProductEndpoint.cs
            ├── DeleteProductEndpoint.cs
            └── PagedProductEndpoint.cs
```

同时更新 `IDedsiNativeDbContext`、`DedsiNativeDbContext` 并生成 EF Core 迁移。

## 聚合根

```csharp
using Dedsi.Ddd.Domain.Entities;
using DedsiNative.Products.Events;
using Volo.Abp;

namespace DedsiNative.Products;

/// <summary>
/// 产品聚合根，负责维护产品名称和价格等业务状态。
/// </summary>
public class Product : DedsiAggregateRoot<string>
{
    /// <summary>
    /// 供 ORM 框架反射创建实体的受保护构造函数。
    /// </summary>
    protected Product()
    {
    }

    /// <summary>
    /// 创建产品聚合根。
    /// </summary>
    /// <param name="id">产品唯一标识，使用 ULID 字符串。</param>
    /// <param name="name">产品名称，不能为空。</param>
    /// <param name="price">产品价格，不能小于零。</param>
    public Product(string id, string name, decimal price) : base(id)
    {
        ChangeName(name);
        ChangePrice(price);
    }

    /// <summary>
    /// 产品名称。
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 产品价格。
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// 修改产品名称。
    /// </summary>
    /// <param name="name">新的产品名称。</param>
    /// <returns>当前产品聚合根，支持链式调用。</returns>
    public Product ChangeName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(Name));
        return this;
    }

    /// <summary>
    /// 修改产品价格。
    /// </summary>
    /// <param name="price">新的产品价格，不能小于零。</param>
    /// <returns>当前产品聚合根，支持链式调用。</returns>
    public Product ChangePrice(decimal price)
    {
        if (price < 0)
        {
            throw new BusinessException("Product:PriceCannotBeNegative");
        }

        Price = price;
        return this;
    }

    /// <summary>
    /// 注册产品创建完成的本地领域事件。
    /// </summary>
    public void AddCreatedEvent()
    {
        AddLocalEvent(new ProductCreatedEvent(this));
    }
}
```

## 仓储与查询契约

```csharp
using Dedsi.Ddd.Domain.Queries;
using Dedsi.Ddd.Domain.Repositories;

namespace DedsiNative.Products;

/// <summary>
/// 产品仓储，提供产品聚合根的持久化操作。
/// </summary>
public interface IProductRepository : IDedsiCqrsRepository<Product, string>;

/// <summary>
/// 产品分页查询条件。
/// </summary>
/// <param name="Name">产品名称筛选条件。</param>
/// <param name="SkipCount">跳过的记录数量。</param>
/// <param name="MaxResultCount">最多返回的记录数量。</param>
/// <param name="IsExport">是否为导出查询；导出时不分页。</param>
public sealed record ProductPagedQuery(
    string? Name,
    int SkipCount,
    int MaxResultCount,
    bool IsExport);

/// <summary>
/// 产品查询结果中的单行数据。
/// </summary>
/// <param name="Id">产品唯一标识。</param>
/// <param name="Name">产品名称。</param>
/// <param name="Price">产品价格。</param>
public sealed record ProductQueryItem(string Id, string Name, decimal Price);

/// <summary>
/// 产品分页查询结果。
/// </summary>
/// <param name="TotalCount">符合条件的记录总数。</param>
/// <param name="Items">当前查询返回的产品列表。</param>
public sealed record ProductPagedQueryResult(
    long TotalCount,
    IReadOnlyList<ProductQueryItem> Items);

/// <summary>
/// 产品查询接口，隔离 Host 与 EF Core。
/// </summary>
public interface IProductQuery : IDedsiQuery
{
    /// <summary>
    /// 按条件分页查询产品。
    /// </summary>
    /// <param name="query">产品分页查询条件。</param>
    /// <param name="cancellationToken">用于取消异步查询的令牌。</param>
    /// <returns>产品分页查询结果。</returns>
    Task<ProductPagedQueryResult> GetPagedAsync(
        ProductPagedQuery query,
        CancellationToken cancellationToken = default);
}
```

## 领域事件

```csharp
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace DedsiNative.Products.Events;

/// <summary>
/// 产品创建完成后发布的本地领域事件。
/// </summary>
/// <param name="Product">新创建的产品聚合根。</param>
public sealed record ProductCreatedEvent(Product Product);

/// <summary>
/// 产品创建事件处理器，负责执行创建后的应用内副作用。
/// </summary>
public sealed class ProductCreatedEventHandler
    : ILocalEventHandler<ProductCreatedEvent>, ITransientDependency
{
    /// <summary>
    /// 处理产品创建事件。
    /// </summary>
    /// <param name="eventData">产品创建事件数据。</param>
    public Task HandleEventAsync(ProductCreatedEvent eventData)
    {
        // 在此调用 Core 定义的外部能力接口，不直接依赖具体基础设施。
        return Task.CompletedTask;
    }
}
```

## 输出要求

- Infrastructure 代码必须遵循 `$dedsi-efcore-persistence` 的示例。
- Host Endpoint 必须遵循 `$dedsi-build-fastendpoint` 的示例。
- 实际功能不需要领域事件时，不创建空事件和空处理器。
- 不生成没有业务用途的接口、DTO 或占位服务。
