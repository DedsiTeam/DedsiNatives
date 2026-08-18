using DedsiNative.Menus;
using DedsiNative.Permissions;
using FastEndpoints;

namespace DedsiNative.Endpoints.MenuEndpoints;

/// <summary>
/// 菜单命令端点共用的跨聚合关系校验。
/// </summary>
internal static class MenuEndpointValidation
{
    /// <summary>
    /// 校验父菜单、权限和循环引用是否满足菜单领域规则。
    /// </summary>
    /// <param name="menus">菜单聚合仓储。</param>
    /// <param name="permissions">权限聚合仓储。</param>
    /// <param name="systemId">目标系统标识。</param>
    /// <param name="parentId">候选父菜单标识。</param>
    /// <param name="permissionId">候选权限标识。</param>
    /// <param name="menuId">更新时的当前菜单标识；创建时为空。</param>
    /// <param name="cancellationToken">用于取消异步操作的令牌。</param>
    /// <returns>读取到的权限及全部业务错误。</returns>
    internal static async Task<MenuRelationValidationResult> ValidateRelationsAsync(
        IMenuRepository menus,
        IPermissionRepository permissions,
        string systemId,
        string? parentId,
        string? permissionId,
        string? menuId,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        if (!string.IsNullOrWhiteSpace(parentId))
        {
            if (parentId == menuId)
            {
                errors.Add("父菜单不能是菜单自身。");
            }
            else
            {
                var parent = await menus.GetAsync(parentId, true, cancellationToken);
                if (parent.SystemId != systemId)
                {
                    errors.Add("父菜单必须属于同一系统。");
                }

                if (menuId is not null && await menus.WouldCreateCycleAsync(menuId, parentId, cancellationToken))
                {
                    errors.Add("父菜单设置会形成循环引用。");
                }
            }
        }

        var permission = await GetPermissionAsync(permissions, permissionId, cancellationToken);
        if (permission is not null && permission.SystemId != systemId)
        {
            errors.Add("权限必须属于同一系统。");
        }

        return new MenuRelationValidationResult(permission, errors);
    }

    /// <summary>
    /// 在需要创建或更新聚合时读取可选权限。
    /// </summary>
    /// <param name="permissions">权限聚合仓储。</param>
    /// <param name="permissionId">可选权限标识。</param>
    /// <param name="cancellationToken">用于取消异步操作的令牌。</param>
    /// <returns>未绑定权限时返回 <see langword="null"/>。</returns>
    internal static async Task<Permission?> GetPermissionAsync(
        IPermissionRepository permissions,
        string? permissionId,
        CancellationToken cancellationToken)
    {
        return string.IsNullOrWhiteSpace(permissionId)
            ? null
            : await permissions.GetAsync(permissionId, true, cancellationToken);
    }
}

/// <summary>
/// 菜单跨聚合关系校验的结果。
/// </summary>
/// <param name="Permission">已读取的可选权限。</param>
/// <param name="Errors">全部可展示的业务错误。</param>
internal sealed record MenuRelationValidationResult(
    Permission? Permission,
    IReadOnlyList<string> Errors);
