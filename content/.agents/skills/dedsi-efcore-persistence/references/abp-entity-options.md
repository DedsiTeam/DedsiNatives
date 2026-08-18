# AbpEntityOptions 配置说明

## 目的

`AbpEntityOptions` 用于声明聚合根的默认明细查询规则。任何需要查询完整领域模型或聚合明细的调用方都使用仓储的：

```csharp
var aggregate = await repository.GetAsync(id, true, cancellationToken);
```

第二个参数 `true` 表示按 `DefaultWithDetailsFunc` 加载完整聚合。这样聚合明细的关联范围集中在 Infrastructure 模块中，Endpoint、应用服务和事件处理器不需要依赖 `DbContext`，Query 也不需要实现重复的完整聚合加载方法。

## 配置位置

在 `DedsiNativeInfrastructureModule.ConfigureServices` 中配置，并确保引入：

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

    options.Entity<DedsiNative.Systems.System>(entityOptions =>
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
- 仅加载完整聚合响应需要的聚合内明细，不要为了列表、分页、统计、导出或 DTO 投影查询扩大 Include 范围。
- 新增聚合明细时，同步检查实体导航属性、EF Core 映射、`AbpEntityOptions` 和详情响应 DTO。
- 完整聚合查询保持聚合完整性；不要在 Endpoint、应用服务或事件处理器中直接 Include，也不要通过多个仓储查询拼装聚合。

## 与仓储和 Endpoint 的配合

查询完整领域模型或聚合明细时应直接调用：

```csharp
var position = await positionRepository.GetAsync(id, true, cancellationToken);
```

列表、分页、统计、导出和 DTO 投影使用 Query 服务，并默认使用 `AsNoTracking()` 与数据库端筛选、排序、分页、统计和投影；不要复用 `DefaultWithDetailsFunc` 作为普通查询方案。Query 不得返回实体、聚合根或 `IQueryable`。

## 常见错误

- 只修改 Endpoint 使用 `Include`，未在 `AbpEntityOptions` 中配置默认明细。
- Query 或 Endpoint 使用自定义 `GetWithRelationsAsync`，导致完整聚合加载规则分散且重复。
- 配置了导航属性但没有在实体上维护对应的公开集合属性。
- 将所有关联都加入默认明细，导致详情查询加载不必要的数据。
- 修改聚合关系后忘记重新运行后端构建和持久化静态检查。
