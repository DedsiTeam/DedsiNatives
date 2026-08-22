using DedsiNative.LoginAudits;
using DedsiNative.OpenIddict;
using DedsiNative.Permissions;

namespace DedsiNative.EntityFrameworkCore.Configurations;

/// <summary>
/// 身份管理系统的内置权限种子目录，集中维护稳定标识与权限编码的对应关系。
/// </summary>
internal static class BuiltInPermissionSeedCatalog
{
    /// <summary>
    /// 身份管理系统的稳定标识。
    /// </summary>
    internal const string IdentitySystemId = "01ARZ3NDEKTSV4RRFFQ69G5FAV";

    /// <summary>
    /// 默认系统管理员岗位的稳定标识。
    /// </summary>
    internal const string AdministratorPositionId = "01ARZ3NDEKTSV4RRFFQ69G5FB0";

    /// <summary>
    /// 返回全部内置权限定义。标识一旦发布不可重排，避免迁移将既有授权误关联到其他权限。
    /// </summary>
    internal static BuiltInPermissionDefinition[] All =>
    [
        new("01ARZ3NDEKTSV4RRFFQ69G5FB2", LoginAuditPermissions.View, "查看登录审计列表和详情。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FB3", OpenIddictPermissions.View, "查看 SSO 客户端、作用域、授权与令牌。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FB4", OpenIddictPermissions.Manage, "管理 SSO 客户端、作用域、授权与令牌。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FC0", ManagementPermissions.Systems.View, "查看系统。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FC1", ManagementPermissions.Systems.Create, "创建系统。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FC2", ManagementPermissions.Systems.Update, "修改系统。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FC3", ManagementPermissions.Systems.Delete, "删除系统。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FC4", ManagementPermissions.Permissions.View, "查看权限定义。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FC5", ManagementPermissions.Permissions.Create, "创建权限定义。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FC6", ManagementPermissions.Permissions.Update, "修改权限定义。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FC7", ManagementPermissions.Permissions.Delete, "删除权限定义。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FC8", ManagementPermissions.Menus.View, "查看菜单。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FC9", ManagementPermissions.Menus.Create, "创建菜单。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCA", ManagementPermissions.Menus.Update, "修改菜单。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCB", ManagementPermissions.Menus.Delete, "删除菜单。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCC", ManagementPermissions.Positions.View, "查看岗位。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCD", ManagementPermissions.Positions.Create, "创建岗位。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCE", ManagementPermissions.Positions.Update, "修改岗位。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCF", ManagementPermissions.Positions.Delete, "删除岗位。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCG", ManagementPermissions.Positions.Assign, "配置岗位权限与组织范围。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCH", ManagementPermissions.Organizations.View, "查看组织机构。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCJ", ManagementPermissions.Organizations.Create, "创建组织机构。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCK", ManagementPermissions.Organizations.Update, "修改组织机构。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCM", ManagementPermissions.Organizations.Delete, "删除组织机构。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCN", ManagementPermissions.Users.View, "查看用户。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCP", ManagementPermissions.Users.Create, "创建用户。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCQ", ManagementPermissions.Users.Update, "修改用户。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCR", ManagementPermissions.Users.Delete, "删除用户。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCS", ManagementPermissions.Users.ResetPassword, "重置用户密码。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCT", ManagementPermissions.Users.AssignPosition, "分配用户岗位。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCV", ManagementPermissions.Storage.View, "查看、预览和下载文件。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCW", ManagementPermissions.Storage.Upload, "上传文件。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCX", ManagementPermissions.Storage.Delete, "删除文件。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCY", ManagementPermissions.Dictionaries.View, "查看字典及字典项。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FCZ", ManagementPermissions.Dictionaries.Create, "创建字典及字典项。"),
        new("01ARZ3NDEKTSV4RRFFQ69G5FD0", ManagementPermissions.Dictionaries.Update, "修改字典及字典项。")
    ];
}

/// <summary>
/// 内置权限种子定义。
/// </summary>
/// <param name="Id">权限稳定标识。</param>
/// <param name="Name">权限编码。</param>
/// <param name="Description">权限用途说明。</param>
internal sealed record BuiltInPermissionDefinition(string Id, string Name, string Description);
