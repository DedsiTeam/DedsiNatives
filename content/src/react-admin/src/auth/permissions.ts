/**
 * 前端使用的细粒度权限编码，与后端 ManagementPermissions 保持一致。
 */
export const PERMISSIONS = {
  systems: { view: 'system:systems:view', create: 'system:systems:create', update: 'system:systems:update', delete: 'system:systems:delete' },
  permissions: { view: 'system:permissions:view', create: 'system:permissions:create', update: 'system:permissions:update', delete: 'system:permissions:delete' },
  menus: { view: 'system:menus:view', create: 'system:menus:create', update: 'system:menus:update', delete: 'system:menus:delete' },
  positions: { view: 'system:positions:view', create: 'system:positions:create', update: 'system:positions:update', delete: 'system:positions:delete', assign: 'system:positions:assign' },
  organizations: { view: 'system:organizations:view', create: 'system:organizations:create', update: 'system:organizations:update', delete: 'system:organizations:delete' },
  users: { view: 'system:users:view', create: 'system:users:create', update: 'system:users:update', delete: 'system:users:delete', resetPassword: 'system:users:reset-password', assignPosition: 'system:users:assign-position' },
  storage: { view: 'system:storage:view', upload: 'system:storage:upload', delete: 'system:storage:delete' },
  dictionaries: { view: 'system:dictionaries:view', create: 'system:dictionaries:create', update: 'system:dictionaries:update' },
  loginAudits: { view: 'system:login-audits:view' },
  openiddict: { view: 'system:openiddict:view', manage: 'system:openiddict:manage' },
} as const;

/** 平台内置权限名称，用于在权限管理页禁用破坏性维护入口。 */
export const BUILT_IN_PERMISSION_NAMES: ReadonlySet<string> = new Set(
  Object.values(PERMISSIONS).flatMap((modulePermissions) => Object.values(modulePermissions))
);
