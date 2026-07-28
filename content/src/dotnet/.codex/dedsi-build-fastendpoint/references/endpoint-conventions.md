# Endpoint 约定

## 目录布局

```text
DedsiNative.Host/
└── Endpoints/
    └── ProductEndpoints/
        ├── CreateProductEndpoint.cs
        ├── GetProductEndpoint.cs
        ├── UpdateProductEndpoint.cs
        ├── DeleteProductEndpoint.cs
        └── PagedProductEndpoint.cs
```

一个 Endpoint 文件默认共置它自己的 DTO：

```csharp
/// <summary>
/// 创建产品的请求参数。
/// </summary>
/// <param name="Name">产品名称，不能为空。</param>
public sealed record CreateProductRequest(string Name);

/// <summary>
/// 创建产品的响应。
/// </summary>
/// <param name="Id">新产品的唯一标识。</param>
public sealed record CreateProductResponse(string Id);

/// <summary>
/// 创建产品端点，负责接收请求并调用领域仓储完成持久化。
/// </summary>
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
    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        var domainId = Ulid.NewUlid().ToString();
        var product = new Product(domainId, req.Name);

        await productRepository.InsertAsync(product, true, ct);
        await Send.OkAsync(new CreateProductResponse(domainId), ct);
    }
}
```

## 共置规则

- Request、Response 和 Endpoint 必须位于同一个功能目录。
- 默认将一个 Endpoint 的 Request、Response 和 Endpoint 写在同一个文件，保持阅读路径短。
- DTO 被多个 Endpoint 复用时，才允许提取到同一功能目录的 `Dtos/` 子目录。
- Validator 只服务一个请求时，优先与 Endpoint 同文件；较大时拆成同目录的 `{Request}Validator.cs`。
- 不把多个不同 Endpoint 类塞入同一个文件。

## DTO

- 请求优先使用 `sealed record`；需要继承分页基类或可变绑定属性时使用 class。
- 响应使用专用 DTO，不暴露领域实体或 EF Core IQueryable。
- 为 record 本身和每个位置参数写中文 XML 文档。
- 分页请求继承项目的 `DedsiPagedRequestDto`，分页响应继承 `DedsiPagedResultDto<T>`。

## 授权和响应

- 默认要求认证，只有登录、健康检查等明确公开接口使用 `AllowAnonymous()`。
- 返回与结果语义一致的状态码，不用 `200 + false` 表示所有失败。
- 对不存在、冲突、验证失败和未授权分别使用合适的 FastEndpoints 响应方法。
- 不在日志或响应中泄露密码、Token、连接字符串或敏感个人信息。

## 查询

- 在 Core 定义 `I{Feature}Query`，在 Infrastructure 实现查询和 EF Core 投影。
- Endpoint 只负责接收查询参数、调用查询服务并发送响应。
- 为只读查询使用 `AsNoTracking()`。
- 先完成过滤再统计总数；非导出模式才应用排序和分页。
- 对每一个筛选字段检查对应实体属性，禁止复制粘贴后保留错误属性。
