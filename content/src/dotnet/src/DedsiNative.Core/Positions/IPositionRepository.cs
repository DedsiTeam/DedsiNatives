using Dedsi.Ddd.Domain.Repositories;

namespace DedsiNative.Positions;

/// <summary>岗位聚合仓储，提供岗位及其子实体的持久化操作。</summary>
public interface IPositionRepository : IDedsiCqrsRepository<Position, string>
{
    /// <summary>
    /// 查询包含指定权限关联的岗位聚合，并加载岗位权限集合。
    /// </summary>
    /// <param name="permissionId">权限唯一标识。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>关联岗位聚合列表。</returns>
    Task<IReadOnlyList<Position>> GetByPermissionIdAsync(
        string permissionId,
        CancellationToken cancellationToken);
}
