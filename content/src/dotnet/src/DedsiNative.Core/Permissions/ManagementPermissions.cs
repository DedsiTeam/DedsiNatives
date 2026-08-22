namespace DedsiNative.Permissions;

/// <summary>
/// 中台管理功能使用的细粒度权限编码。
/// </summary>
public static class ManagementPermissions
{
    /// <summary>
    /// JWT 中保存权限编码的声明类型。
    /// </summary>
    public const string ClaimType = "permission";

    /// <summary>
    /// 系统管理权限。
    /// </summary>
    public static class Systems
    {
        /// <summary>
        /// 系统查看权限编码。
        /// </summary>
        public const string View = "system:systems:view";
        /// <summary>
        /// 系统创建权限编码。
        /// </summary>
        public const string Create = "system:systems:create";
        /// <summary>
        /// 系统修改权限编码。
        /// </summary>
        public const string Update = "system:systems:update";
        /// <summary>
        /// 系统删除权限编码。
        /// </summary>
        public const string Delete = "system:systems:delete";
    }

    /// <summary>
    /// 权限定义管理权限。
    /// </summary>
    public static class Permissions
    {
        /// <summary>
        /// 权限定义查看权限编码。
        /// </summary>
        public const string View = "system:permissions:view";
        /// <summary>
        /// 权限定义创建权限编码。
        /// </summary>
        public const string Create = "system:permissions:create";
        /// <summary>
        /// 权限定义修改权限编码。
        /// </summary>
        public const string Update = "system:permissions:update";
        /// <summary>
        /// 权限定义删除权限编码。
        /// </summary>
        public const string Delete = "system:permissions:delete";
    }

    /// <summary>
    /// 菜单管理权限。
    /// </summary>
    public static class Menus
    {
        /// <summary>
        /// 菜单查看权限编码。
        /// </summary>
        public const string View = "system:menus:view";
        /// <summary>
        /// 菜单创建权限编码。
        /// </summary>
        public const string Create = "system:menus:create";
        /// <summary>
        /// 菜单修改权限编码。
        /// </summary>
        public const string Update = "system:menus:update";
        /// <summary>
        /// 菜单删除权限编码。
        /// </summary>
        public const string Delete = "system:menus:delete";
    }

    /// <summary>
    /// 岗位管理权限。
    /// </summary>
    public static class Positions
    {
        /// <summary>
        /// 岗位查看权限编码。
        /// </summary>
        public const string View = "system:positions:view";
        /// <summary>
        /// 岗位创建权限编码。
        /// </summary>
        public const string Create = "system:positions:create";
        /// <summary>
        /// 岗位修改权限编码。
        /// </summary>
        public const string Update = "system:positions:update";
        /// <summary>
        /// 岗位删除权限编码。
        /// </summary>
        public const string Delete = "system:positions:delete";
        /// <summary>
        /// 岗位分配权限编码。
        /// </summary>
        public const string Assign = "system:positions:assign";
    }

    /// <summary>
    /// 组织机构管理权限。
    /// </summary>
    public static class Organizations
    {
        /// <summary>
        /// 组织机构查看权限编码。
        /// </summary>
        public const string View = "system:organizations:view";
        /// <summary>
        /// 组织机构创建权限编码。
        /// </summary>
        public const string Create = "system:organizations:create";
        /// <summary>
        /// 组织机构修改权限编码。
        /// </summary>
        public const string Update = "system:organizations:update";
        /// <summary>
        /// 组织机构删除权限编码。
        /// </summary>
        public const string Delete = "system:organizations:delete";
    }

    /// <summary>
    /// 用户管理权限。
    /// </summary>
    public static class Users
    {
        /// <summary>
        /// 用户查看权限编码。
        /// </summary>
        public const string View = "system:users:view";
        /// <summary>
        /// 用户创建权限编码。
        /// </summary>
        public const string Create = "system:users:create";
        /// <summary>
        /// 用户修改权限编码。
        /// </summary>
        public const string Update = "system:users:update";
        /// <summary>
        /// 用户删除权限编码。
        /// </summary>
        public const string Delete = "system:users:delete";
        /// <summary>
        /// 用户重置密码权限编码。
        /// </summary>
        public const string ResetPassword = "system:users:reset-password";
        /// <summary>
        /// 用户分配岗位权限编码。
        /// </summary>
        public const string AssignPosition = "system:users:assign-position";
    }

    /// <summary>
    /// 文件存储管理权限。
    /// </summary>
    public static class Storage
    {
        /// <summary>
        /// 文件查看权限编码。
        /// </summary>
        public const string View = "system:storage:view";
        /// <summary>
        /// 文件上传权限编码。
        /// </summary>
        public const string Upload = "system:storage:upload";
        /// <summary>
        /// 文件删除权限编码。
        /// </summary>
        public const string Delete = "system:storage:delete";
    }

    /// <summary>
    /// 字典管理权限。
    /// </summary>
    public static class Dictionaries
    {
        /// <summary>
        /// 字典查看权限编码。
        /// </summary>
        public const string View = "system:dictionaries:view";
        /// <summary>
        /// 字典创建权限编码。
        /// </summary>
        public const string Create = "system:dictionaries:create";
        /// <summary>
        /// 字典修改权限编码。
        /// </summary>
        public const string Update = "system:dictionaries:update";
    }

    /// <summary>
    /// 返回需要由宿主注册为授权策略的全部管理权限。
    /// </summary>
    public static string[] All =>
    [
        Systems.View, Systems.Create, Systems.Update, Systems.Delete,
        Permissions.View, Permissions.Create, Permissions.Update, Permissions.Delete,
        Menus.View, Menus.Create, Menus.Update, Menus.Delete,
        Positions.View, Positions.Create, Positions.Update, Positions.Delete, Positions.Assign,
        Organizations.View, Organizations.Create, Organizations.Update, Organizations.Delete,
        Users.View, Users.Create, Users.Update, Users.Delete, Users.ResetPassword, Users.AssignPosition,
        Storage.View, Storage.Upload, Storage.Delete,
        Dictionaries.View, Dictionaries.Create, Dictionaries.Update
    ];
}
