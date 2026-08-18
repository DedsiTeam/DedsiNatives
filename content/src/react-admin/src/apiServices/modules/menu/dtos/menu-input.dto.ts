import type { PageInputDto } from '../../../core/base-dto';

/** 菜单类型：目录、页面或页面内操作按钮。 */
export type MenuType = 1 | 2 | 3;

/** 菜单分页条件。 */
export interface MenuQueryInputDto extends PageInputDto {
  /** 所属系统标识。 */
  systemId?: string;
  /** 菜单名称关键字。 */
  name?: string;
  /** 菜单编码关键字。 */
  code?: string;
  /** 菜单类型。 */
  type?: MenuType;
  /** 父菜单标识。 */
  parentId?: string;
  /** 是否可见。 */
  isVisible?: boolean;
  /** 是否禁用。 */
  isDisabled?: boolean;
  /** 是否为外链。 */
  isExternal?: boolean;
  /** 是否导出全部匹配记录。 */
  isExport?: boolean;
}

/** 创建或更新菜单的请求参数。 */
export interface MenuInputDto {
  /** 所属系统标识。 */
  systemId: string;
  /** 同一系统内唯一的菜单编码。 */
  code: string;
  /** 菜单展示名称。 */
  name: string;
  /** 可选父菜单标识。 */
  parentId?: string;
  /** 菜单节点类型。 */
  type: MenuType;
  /** 页面菜单的路由路径。 */
  routePath?: string;
  /** 前端组件路径。 */
  component?: string;
  /** 可选重定向路径。 */
  redirect?: string;
  /** 图标名称。 */
  icon?: string;
  /** 关联权限标识。 */
  permissionId?: string;
  /** 同级排序值。 */
  sort: number;
  /** 菜单层级，从 1 开始。 */
  level: number;
  /** 是否在导航中可见。 */
  isVisible: boolean;
  /** 是否禁用。 */
  isDisabled: boolean;
  /** 是否为外链菜单。 */
  isExternal: boolean;
  /** 外链地址。 */
  externalUrl?: string;
  /** 是否保持页面缓存。 */
  keepAlive: boolean;
  /** 是否固定在标签栏。 */
  isAffix: boolean;
  /** 管理说明。 */
  description?: string;
}
