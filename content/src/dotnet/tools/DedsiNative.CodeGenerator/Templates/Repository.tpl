using Dedsi.EntityFrameworkCore.Repositories;
using Volo.Abp.EntityFrameworkCore;
using {{Namespace}};
namespace DedsiNative.EntityFrameworkCore.Repositories;

/// <summary>
/// {{Name}} 聚合仓储的 EF Core 实现。
/// </summary>
public sealed class {{Name}}Repository(IDbContextProvider<DedsiNativeDbContext> dbContextProvider)
    : DedsiDddEfCoreRepository<DedsiNativeDbContext, {{Name}}, {{KeyType}}>(dbContextProvider),
      I{{Name}}Repository
{
    /// <inheritdoc />
    public async Task ReplaceAsync(
        {{KeyType}} id,
        Action<{{Name}}> update,
        CancellationToken ct)
    {
        var entity = await GetAsync(id, true, ct);
        var dbContext = await GetDbContextAsync();
        // 标记旧明细删除后替换集合；一次 SaveChanges 保证写入原子性。
        RemoveDetails(dbContext, entity);
        update(entity);
        await dbContext.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task DeleteWithDetailsAsync({{KeyType}} id, CancellationToken ct)
    {
        var entity = await GetAsync(id, true, ct);
        var dbContext = await GetDbContextAsync();
        RemoveDetails(dbContext, entity);
        dbContext.Remove(entity);
        await dbContext.SaveChangesAsync(ct);
    }

    /// <summary>
    /// 删除本聚合拥有的明细，保留外部引用的 Restrict 约束。
    /// </summary>
    private static void RemoveDetails(DedsiNativeDbContext dbContext, {{Name}} entity)
    {
{{RemoveDetails}}
    }
}
