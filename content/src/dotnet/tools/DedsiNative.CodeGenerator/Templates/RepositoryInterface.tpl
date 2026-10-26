using Dedsi.Ddd.Domain.Repositories;
namespace {{Namespace}};

/// <summary>
/// {{Name}} 聚合仓储，负责完整聚合的持久化。
/// </summary>
public interface I{{Name}}Repository : IDedsiCqrsRepository<{{Name}}, {{KeyType}}>
{
    /// <summary>
    /// 整批替换聚合可写字段及明细，并一次保存。
    /// </summary>
    Task ReplaceAsync(
        {{KeyType}} id,
        Action<{{Name}}> update,
        CancellationToken ct);
    /// <summary>
    /// 删除聚合及其所属明细，并一次保存。
    /// </summary>
    Task DeleteWithDetailsAsync({{KeyType}} id, CancellationToken ct);
}
