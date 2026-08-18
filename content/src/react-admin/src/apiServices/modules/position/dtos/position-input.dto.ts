import type { PageInputDto } from '../../../core/base-dto';

/** 岗位分页查询参数。 */
export interface PositionQueryInputDto extends PageInputDto {
  /** 按系统 ID 筛选。 */
  systemId?: string;
  /** 按岗位名称模糊筛选。 */
  name?: string;
  /** 按启用状态筛选。 */
  isEnabled?: boolean;
  /** 是否为导出查询。 */
  isExport?: boolean;
}

/** 创建岗位请求参数。 */
export interface CreatePositionInputDto {
  /** 岗位名称。 */
  name: string;
  /** 所属系统 ID。 */
  systemId: string;
  /** 岗位说明。 */
  description?: string;
  /** 是否启用。 */
  isEnabled: boolean;
  /** 初始关联的权限 ID 列表。 */
  permissionIds?: string[];
  /** 初始关联的组织机构列表。 */
  organizations?: PositionOrganizationInputDto[];
}

/** 更新岗位请求参数。 */
export interface UpdatePositionInputDto {
  /** 岗位名称。 */
  name: string;
  /** 所属系统 ID。 */
  systemId: string;
  /** 岗位说明。 */
  description?: string;
}

/** 修改岗位状态请求参数。 */
export interface SetPositionStatusInputDto {
  /** 目标启用状态。 */
  isEnabled: boolean;
}

/** 岗位组织机构关联输入。 */
export interface PositionOrganizationInputDto {
  /** 组织机构 ID。 */
  organizationId: string;
  /** 组织机构名称。 */
  organizationName: string;
}

/** 替换岗位权限和组织机构关联的请求参数。 */
export interface UpdatePositionAssignmentsInputDto {
  /** 权限 ID 列表。 */
  permissionIds: string[];
  /** 组织机构关联列表。 */
  organizations: PositionOrganizationInputDto[];
}
