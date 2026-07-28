# Endpoint 示例代码

以下示例是 Endpoint 输出的标准形式。每个 Endpoint 独立文件，其 Request、Response 和 Endpoint 共置在该文件中。

## 目录

- [创建端点](#创建端点)
- [详情端点](#详情端点)
- [分页查询端点](#分页查询端点)
- [匿名端点](#匿名端点)

## 创建端点

文件：`Endpoints/ProductEndpoints/CreateProductEndpoint.cs`

```csharp
using DedsiNative.Products;
using FastEndpoints;

namespace DedsiNative.Endpoints.ProductEndpoints;

/// <summary>
/// 创建产品的请求参数。
/// </summary>
/// <param name="Name">产品名称，不能为空。</param>
/// <param name="Price">产品价格，不能小于零。</param>
public sealed record CreateProductRequest(string Name, decimal Price);

/// <summary>
/// 创建产品的响应。
/// </summary>
/// <param name="Id">新产品的唯一标识。</param>
public sealed record CreateProductResponse(string Id);

/// <summary>
/// 创建产品端点，负责创建产品聚合根并持久化。
/// </summary>
/// <param name="productRepository">产品仓储。</param>
public sealed class CreateProductEndpoint(IProductRepository productRepository)
    : Endpoint<CreateProductRequest, CreateProductResponse>
{
    /// <summary>
    /// 配置创建产品接口的路由和 HTTP 方法。
    /// </summary>
    public override void Configure()
    {
        Post("/api/product/create");
    }

    /// <summary>
    /// 创建产品并返回生成的领域标识。
    /// </summary>
    /// <param name="req">创建产品的请求参数。</param>
    /// <param name="ct">用于取消异步操作的令牌。</param>
    public override async Task HandleAsync(
        CreateProductRequest req,
        CancellationToken ct)
    {
        var domainId = Ulid.NewUlid().ToString();
        var product = new Product(domainId, req.Name, req.Price);

        product.AddCreatedEvent();
        await productRepository.InsertAsync(product, true, ct);

        await Send.OkAsync(new CreateProductResponse(domainId), ct);
    }
}
```

## 详情端点

文件：`Endpoints/ProductEndpoints/GetProductEndpoint.cs`

```csharp
using DedsiNative.Products;
using FastEndpoints;

namespace DedsiNative.Endpoints.ProductEndpoints;

/// <summary>
/// 产品详情响应。
/// </summary>
/// <param name="Id">产品唯一标识。</param>
/// <param name="Name">产品名称。</param>
/// <param name="Price">产品价格。</param>
public sealed record GetProductResponse(string Id, string Name, decimal Price);

/// <summary>
/// 获取产品详情端点。
/// </summary>
/// <param name="productRepository">产品仓储。</param>
public sealed class GetProductEndpoint(IProductRepository productRepository)
    : EndpointWithoutRequest<GetProductResponse>
{
    /// <summary>
    /// 配置产品详情接口的路由和 HTTP 方法。
    /// </summary>
    public override void Configure()
    {
        Get("/api/product/{id}");
    }

    /// <summary>
    /// 根据路由中的产品标识返回产品详情。
    /// </summary>
    /// <param name="ct">用于取消异步操作的令牌。</param>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id");
        var product = await productRepository.GetAsync(id, true, ct);
        var response = new GetProductResponse(
            product.Id,
            product.Name,
            product.Price);

        await Send.OkAsync(response, ct);
    }
}
```

## 分页查询端点

文件：`Endpoints/ProductEndpoints/PagedProductEndpoint.cs`

```csharp
using Dedsi.Ddd.Application.Contracts.Dtos;
using DedsiNative.Products;
using FastEndpoints;

namespace DedsiNative.Endpoints.ProductEndpoints;

/// <summary>
/// 产品分页查询请求。
/// </summary>
public sealed class PagedProductRequest : DedsiPagedRequestDto
{
    /// <summary>
    /// 产品名称筛选条件，为空时不按名称过滤。
    /// </summary>
    public string? Name { get; set; }
}

/// <summary>
/// 产品分页结果中的单行数据。
/// </summary>
/// <param name="Id">产品唯一标识。</param>
/// <param name="Name">产品名称。</param>
/// <param name="Price">产品价格。</param>
public sealed record PagedProductRowResponse(
    string Id,
    string Name,
    decimal Price);

/// <summary>
/// 产品分页查询响应。
/// </summary>
public sealed class PagedProductResponse
    : DedsiPagedResultDto<PagedProductRowResponse>;

/// <summary>
/// 产品分页查询端点，通过查询契约隔离 Host 与 EF Core。
/// </summary>
/// <param name="productQuery">产品查询服务。</param>
public sealed class PagedProductEndpoint(IProductQuery productQuery)
    : Endpoint<PagedProductRequest, PagedProductResponse>
{
    /// <summary>
    /// 配置产品分页查询接口的路由和 HTTP 方法。
    /// </summary>
    public override void Configure()
    {
        Post("/api/product/pagedQuery");
    }

    /// <summary>
    /// 按筛选条件查询产品并返回分页结果。
    /// </summary>
    /// <param name="req">产品分页查询请求。</param>
    /// <param name="ct">用于取消异步查询的令牌。</param>
    public override async Task HandleAsync(
        PagedProductRequest req,
        CancellationToken ct)
    {
        var query = new ProductPagedQuery(
            req.Name,
            req.GetSkipCount(),
            req.PageSize,
            req.IsExport);
        var result = await productQuery.GetPagedAsync(query, ct);
        var response = new PagedProductResponse
        {
            TotalCount = result.TotalCount,
            Items = result.Items
                .Select(item => new PagedProductRowResponse(
                    item.Id,
                    item.Name,
                    item.Price))
                .ToList()
        };

        await Send.OkAsync(response, ct);
    }
}
```

## 匿名端点

只有明确允许公开访问时才添加：

```csharp
/// <summary>
/// 配置公开接口；该接口不要求用户认证。
/// </summary>
public override void Configure()
{
    Post("/api/auth/login");

    // 登录接口必须允许匿名访问，否则用户无法获取初始访问令牌。
    AllowAnonymous();
}
```

禁止从现有临时登录代码复制硬编码账号、密码或 JWT Secret。
