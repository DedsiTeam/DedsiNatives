using Dedsi.Ddd.Domain.Repositories;

namespace DedsiNative.Menus;

/// <summary>
/// 菜单聚合仓储，提供菜单写入及树形关系校验能力。
/// </summary>
public interface IMenuRepository : IDedsiCqrsRepository<Menu, string>
{
    /// <summary>
    /// 判断指定菜单是否仍包含直接子菜单。
    /// </summary>
    /// <param name="id">待删除菜单的标识。</param>
    /// <param name="cancellationToken">用于取消异步操作的令牌。</param>
    /// <returns>存在直接子菜单时返回 <see langword="true"/>。</returns>
    Task<bool> HasChildrenAsync(
        string id,
        CancellationToken cancellationToken);

    /// <summary>
    /// 判断系统内是否已经存在相同的菜单编码。
    /// </summary>
    /// <param name="systemId">菜单所属系统标识。</param>
    /// <param name="code">待校验的菜单编码。</param>
    /// <param name="cancellationToken">用于取消异步操作的令牌。</param>
    /// <param name="excludedMenuId">更新时需要排除的当前菜单标识。</param>
    /// <returns>编码已被占用时返回 <see langword="true"/>。</returns>
    Task<bool> ExistsBySystemAndCodeAsync(
        string systemId,
        string code,
        CancellationToken cancellationToken,
        string? excludedMenuId = null);

    /// <summary>
    /// 判断将候选父级设置给菜单后是否会形成循环引用。
    /// </summary>
    /// <param name="menuId">待修改菜单的标识。</param>
    /// <param name="candidateParentId">候选父菜单标识。</param>
    /// <param name="cancellationToken">用于取消异步操作的令牌。</param>
    /// <returns>会形成循环引用时返回 <see langword="true"/>。</returns>
    Task<bool> WouldCreateCycleAsync(
        string menuId,
        string candidateParentId,
        CancellationToken cancellationToken);
}
