import type { MenuType } from './menu-input.dto';

/** 菜单详情及列表行结果。 */
export interface MenuResultDto {
  /** 菜单 ULID。 */ id: string;
  /** 系统 ULID。 */ systemId: string;
  /** 系统名称快照。 */ systemName: string;
  /** 菜单编码。 */ code: string;
  /** 菜单名称。 */ name: string;
  /** 父菜单 ULID。 */ parentId: string | null;
  /** 菜单类型。 */ type: MenuType;
  /** 路由路径。 */ routePath: string | null;
  /** 组件路径。 */ component: string | null;
  /** 重定向路径。 */ redirect: string | null;
  /** 图标名称。 */ icon: string | null;
  /** 权限 ULID。 */ permissionId: string | null;
  /** 权限名称快照。 */ permissionName: string | null;
  /** 排序。 */ sort: number;
  /** 层级。 */ level: number;
  /** 是否可见。 */ isVisible: boolean;
  /** 是否禁用。 */ isDisabled: boolean;
  /** 是否外链。 */ isExternal: boolean;
  /** 外链地址。 */ externalUrl: string | null;
  /** 是否缓存。 */ keepAlive: boolean;
  /** 是否固定。 */ isAffix: boolean;
  /** 说明。 */ description: string | null;
}

/** 菜单分页结果。 */
export interface MenuPageResultDto {
  /** 匹配记录总数。 */ totalCount: number;
  /** 当前页菜单。 */ items: MenuResultDto[];
}

/** 当前登录用户的动态菜单项。 */
export interface CurrentUserMenuResultDto {
  /** 菜单 ULID。 */ id: string;
  /** 系统 ULID。 */ systemId: string;
  /** 系统名称快照。 */ systemName: string;
  /** 菜单编码。 */ code: string;
  /** 菜单名称。 */ name: string;
  /** 父菜单 ULID。 */ parentId: string | null;
  /** 菜单类型。 */ type: MenuType;
  /** 路由路径。 */ routePath: string | null;
  /** 组件路径。 */ component: string | null;
  /** 重定向路径。 */ redirect: string | null;
  /** 图标名称。 */ icon: string | null;
  /** 权限名称快照。 */ permissionName: string | null;
  /** 排序。 */ sort: number;
  /** 层级。 */ level: number;
  /** 是否缓存。 */ keepAlive: boolean;
  /** 是否固定。 */ isAffix: boolean;
  /** 说明。 */ description: string | null;
  /** 子菜单列表。 */ children: CurrentUserMenuResultDto[];
}
