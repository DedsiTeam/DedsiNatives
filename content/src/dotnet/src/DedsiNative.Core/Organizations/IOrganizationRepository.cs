using Dedsi.Ddd.Domain.Repositories;

namespace DedsiNative.Organizations;

/// <summary>
/// 组织机构聚合仓储契约。
/// </summary>
public interface IOrganizationRepository : IDedsiCqrsRepository<Organization, string>
{
    /// <summary>
    /// 检查指定组织下是否包含未删除的子级组织节点。
    /// </summary>
    /// <param name="id">
    /// 待检查的组织机构唯一标识。
    /// </param>
    /// <param name="cancellationToken">
    /// 异步操作取消令牌。
    /// </param>
    /// <returns>
    /// 若存在子级组织返回 true，否则返回 false。
    /// </returns>
    Task<bool> HasChildrenAsync(string id, CancellationToken cancellationToken);

    /// <summary>
    /// 检查同一系统下是否已存在相同编码的组织机构。
    /// </summary>
    /// <param name="systemId">
    /// 所属系统唯一标识。
    /// </param>
    /// <param name="code">
    /// 待校验的组织机构编码。
    /// </param>
    /// <param name="cancellationToken">
    /// 异步操作取消令牌。
    /// </param>
    /// <param name="excludedId">
    /// 更新场景下需要排除的当前组织标识。
    /// </param>
    /// <returns>
    /// 若存在重复编码返回 true，否则返回 false。
    /// </returns>
    Task<bool> ExistsBySystemAndCodeAsync(
        string systemId,
        string code,
        CancellationToken cancellationToken,
        string? excludedId = null);

    /// <summary>
    /// 检查调整父级组织是否会导致树形结构出现循环引用（即候选父组织不能是当前组织或其任一后代子组织）。
    /// </summary>
    /// <param name="id">
    /// 当前组织唯一标识。
    /// </param>
    /// <param name="candidateParentId">
    /// 拟设置的目标父级组织唯一标识。
    /// </param>
    /// <param name="cancellationToken">
    /// 异步操作取消令牌。
    /// </param>
    /// <returns>
    /// 若会导致循环依赖返回 true，合法返回 false。
    /// </returns>
    Task<bool> WouldCreateCycleAsync(
        string id,
        string candidateParentId,
        CancellationToken cancellationToken);
}
