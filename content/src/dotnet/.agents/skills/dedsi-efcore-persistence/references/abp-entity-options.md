# AbpEntityOptions 配置说明

## 目的

`AbpEntityOptions` 用于声明聚合根的默认明细查询规则。详情 Endpoint 使用仓储的：

```csharp
var aggregate = await repository.GetAsync(id, true, ct);
```

第二个参数 `true` 表示按 `DefaultWithDetailsFunc` 加载完整聚合。这样详情查询的关联范围集中在 Infrastructure 模块中，Host 不需要依赖 `DbContext` 或实现重复的 `GetWithRelationsAsync` 方法。

## 配置位置

在 `DedsiIdentityInfrastructureModule.ConfigureServices` 中配置，并确保引入：

```csharp
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DependencyInjection;
```

示例：

```csharp
Configure<AbpEntityOptions>(options =>
{
    options.Entity<User>(entityOptions =>
    {
        entityOptions.DefaultWithDetailsFunc = query => query
            .Include(user => user.LoginInfo)
            .Include(user => user.Positions);
    });

    options.Entity<Position>(entityOptions =>
    {
        entityOptions.DefaultWithDetailsFunc = query => query
            .Include(position => position.Permissions)
            .Include(position => position.Organizations);
    });

    options.Entity<Permission>(entityOptions =>
    {
        entityOptions.DefaultWithDetailsFunc = query => query;
    });

    options.Entity<DedsiIdentity.Systems.System>(entityOptions =>
    {
        entityOptions.DefaultWithDetailsFunc = query => query;
    });
});
```

## 配置规则

- 每个聚合根都要显式注册 `DefaultWithDetailsFunc`。
- 一对多明细使用聚合根公开的集合属性，例如 `Permissions`、`Organizations`、`Positions`。
- 一对一或引用明细使用导航属性，例如 `LoginInfo`。
- 没有明细导航的聚合也配置为 `query => query`，保持详情查询规则完整且一致。
- 仅加载详情响应需要的聚合内明细，不要为了列表、分页或导出查询扩大 Include 范围。
- 新增聚合明细时，同步检查实体导航属性、EF Core 映射、`AbpEntityOptions` 和详情响应 DTO。
- 详情查询保持聚合完整性；不要在 Host Endpoint 中直接 Include，也不要通过多个仓储查询拼装聚合。

## 与仓储和 Endpoint 的配合

详情 Endpoint 应直接调用：

```csharp
var position = await positionRepository.GetAsync(id, true, ct);
```

列表、分页和导出使用 Query 服务，并使用 `AsNoTracking()` 与数据库端投影；不要复用 `DefaultWithDetailsFunc` 作为列表查询方案。

## 常见错误

- 只修改 Endpoint 使用 `Include`，未在 `AbpEntityOptions` 中配置默认明细。
- 详情 Endpoint 使用自定义 `GetWithRelationsAsync`，导致明细规则分散且重复。
- 配置了导航属性但没有在实体上维护对应的公开集合属性。
- 将所有关联都加入默认明细，导致详情查询加载不必要的数据。
- 修改聚合关系后忘记重新运行后端构建和持久化静态检查。
