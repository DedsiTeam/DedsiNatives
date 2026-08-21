using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DedsiNative.LoginAudits;
using DedsiNative.Menus;
using DedsiNative.Permissions;
using DedsiNative.Positions;
using DedsiNative.Users;
using FastEndpoints;

namespace DedsiNative.Endpoints.MenuEndpoints;

/// <summary>
/// 当前登录用户动态菜单响应项（包含子菜单树形结构）。
/// </summary>
public sealed record CurrentUserMenuResponse(
    string Id,
    string SystemId,
    string SystemName,
    string Code,
    string Name,
    string? ParentId,
    MenuType Type,
    string? RoutePath,
    string? Component,
    string? Redirect,
    string? Icon,
    string? PermissionName,
    int Sort,
    int Level,
    bool KeepAlive,
    bool IsAffix,
    string? Description,
    IReadOnlyList<CurrentUserMenuResponse> Children);

/// <summary>
/// 获取当前登录用户可访问的动态菜单端点。
/// 根据用户所拥有的岗位权限进行鉴权过滤，并严格保证层级与排序（按 Sort 升序）。
/// </summary>
public sealed class GetCurrentUserMenusEndpoint(
    IUserRepository userRepository,
    IPositionRepository positionRepository,
    IPermissionQuery permissionQuery,
    IMenuQuery menuQuery)
    : EndpointWithoutRequest<IReadOnlyList<CurrentUserMenuResponse>>
{
    /// <summary>
    /// 配置获取当前用户动态菜单接口。
    /// </summary>
    public override void Configure()
    {
        Get("/api/menu/currentUser");
        Description(x => x.WithTags("菜单管理"));
        Summary(s =>
        {
            s.Summary = "获取当前用户动态菜单";
            s.Description = "根据当前登录用户的岗位有效权限，返回已授权且启用的多级菜单树（严格按 Sort 升序排序）。";
        });
    }

    /// <summary>
    /// 加载当前用户有效权限并返回过滤后的已排序菜单树。
    /// </summary>
    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        // 1. 验证用户状态有效性
        var user = await userRepository.GetAsync(userId, true, ct);
        if (user.SoftDeletedAt is not null || user.LoginInfo is null || user.LoginInfo.Status != AccountStatus.Normal)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        // 2. 收集当前用户所属有效岗位的权限
        var userPermissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var activePositions = new List<Position>();
        foreach (var userPosition in user.Positions)
        {
            var position = await positionRepository.GetAsync(userPosition.PositionId, true, ct);
            if (position.IsEnabled)
            {
                activePositions.Add(position);
            }
        }

        var allPositionPermissions = activePositions.SelectMany(p => p.Permissions).ToList();
        foreach (var systemId in allPositionPermissions.Select(p => p.SystemId).Distinct(StringComparer.Ordinal))
        {
            var pagedPermissions = await permissionQuery.GetPagedAsync(
                new PermissionPagedQuery(systemId, null, true, 0, 1, true),
                ct);
            var enabledDict = pagedPermissions.Items.ToDictionary(p => p.Id, p => p.Name, StringComparer.Ordinal);
            foreach (var posPerm in allPositionPermissions.Where(p => p.SystemId == systemId))
            {
                if (enabledDict.TryGetValue(posPerm.PermissionId, out var permName))
                {
                    userPermissions.Add(permName);
                }
            }
        }

        // 合并 JWT Claim 中的权限
        var claimPermissions = User.FindAll(LoginAuditPermissions.ClaimType)
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v));
        foreach (var claim in claimPermissions)
        {
            userPermissions.Add(claim);
        }

        // 3. 查询系统中所有启用且可见的菜单项（排除禁用和不可见项）
        var menuQueryResult = await menuQuery.GetPagedAsync(
            new MenuPagedQuery(null, null, null, null, null, true, false, false, 0, 1, true),
            ct);

        // 4. 递归构建并过滤菜单树，严格遵循菜单排序规范（Sort 升序）
        var menuTree = BuildMenuTree(menuQueryResult.Items, userPermissions, null);

        await Send.OkAsync(menuTree, ct);
    }

    private static List<CurrentUserMenuResponse> BuildMenuTree(
        IReadOnlyList<MenuQueryItem> allMenus,
        HashSet<string> userPermissions,
        string? parentId)
    {
        var result = new List<CurrentUserMenuResponse>();

        // 筛选当前层级的子菜单，严格保证按 Sort 升序，Sort 相同按 Id 升序
        var currentLevelMenus = allMenus
            .Where(m => string.IsNullOrEmpty(parentId) ? string.IsNullOrEmpty(m.ParentId) : m.ParentId == parentId)
            .OrderBy(m => m.Sort)
            .ThenBy(m => m.Id, StringComparer.Ordinal)
            .ToList();

        foreach (var menu in currentLevelMenus)
        {
            // 递归获取并过滤子菜单项
            var children = BuildMenuTree(allMenus, userPermissions, menu.Id);

            if (menu.Type == MenuType.Directory)
            {
                // 如果是目录类型：仅当其下存在用户可访问的子菜单时才呈现
                if (children.Count > 0)
                {
                    result.Add(CreateMenuResponse(menu, children));
                }
            }
            else
            {
                // 如果是页面或按钮菜单：未绑定权限编码时默认公开；绑定权限编码时验证用户是否具备该权限
                var hasAccess = string.IsNullOrWhiteSpace(menu.PermissionName)
                    || userPermissions.Contains(menu.PermissionName);

                if (hasAccess)
                {
                    result.Add(CreateMenuResponse(menu, children));
                }
            }
        }

        return result;
    }

    private static CurrentUserMenuResponse CreateMenuResponse(MenuQueryItem menu, IReadOnlyList<CurrentUserMenuResponse> children)
    {
        return new CurrentUserMenuResponse(
            menu.Id,
            menu.SystemId,
            menu.SystemName,
            menu.Code,
            menu.Name,
            menu.ParentId,
            menu.Type,
            menu.RoutePath,
            menu.Component,
            menu.Redirect,
            menu.Icon,
            menu.PermissionName,
            menu.Sort,
            menu.Level,
            menu.KeepAlive,
            menu.IsAffix,
            menu.Description,
            children);
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(userIdValue, out userId);
    }
}
