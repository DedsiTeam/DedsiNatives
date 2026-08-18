import type { PageInputDto } from '../../../core/base-dto';

/** 权限分页查询参数。 */
export interface PermissionQueryInputDto extends PageInputDto {
  /** 按系统 ID 筛选。 */
  systemId?: string;
  /** 按权限名称模糊筛选。 */
  name?: string;
  /** 按启用状态筛选；为空时查询全部。 */
  isEnabled?: boolean;
  /** 是否为导出查询。 */
  isExport?: boolean;
}

/** 创建权限请求参数。 */
export interface CreatePermissionInputDto {
  /** 所属系统 ID。 */
  systemId: string;
  /** 权限名称，不能为空。 */
  name: string;
  /** 权限说明，可为空。 */
  description?: string;
  /** 是否启用。 */
  isEnabled: boolean;
}

/** 更新权限请求参数。 */
export interface UpdatePermissionInputDto {
  /** 所属系统 ID。 */
  systemId: string;
  /** 权限名称，不能为空。 */
  name: string;
  /** 权限说明，可为空。 */
  description?: string;
}

/** 修改权限状态请求参数。 */
export interface SetPermissionStatusInputDto {
  /** 目标启用状态。 */
  isEnabled: boolean;
}
