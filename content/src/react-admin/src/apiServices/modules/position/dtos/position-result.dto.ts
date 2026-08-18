/** 岗位列表单行结果。 */
export interface PositionRowResultDto {
  /** 岗位唯一标识，26 位 ULID。 */
  id: string;
  /** 岗位名称。 */
  name: string;
  /** 所属系统 ID。 */
  systemId: string;
  /** 所属系统名称。 */
  systemName: string;
  /** 岗位说明。 */
  description: string | null;
  /** 是否启用。 */
  isEnabled: boolean;
  /** 岗位权限数量。 */
  permissionCount: number;
  /** 岗位组织机构数量。 */
  organizationCount: number;
}

/** 岗位详情中的权限关联。 */
export interface PositionPermissionResultDto {
  /** 权限 ID。 */
  permissionId: string;
  /** 权限名称。 */
  permissionName: string;
  /** 系统 ID。 */
  systemId: string;
  /** 系统名称。 */
  systemName: string;
}

/** 岗位详情中的组织机构关联。 */
export interface PositionOrganizationResultDto {
  /** 组织机构 ID。 */
  organizationId: string;
  /** 组织机构名称。 */
  organizationName: string;
}

/** 岗位详情结果。 */
export interface PositionResultDto extends PositionRowResultDto {
  /** 岗位权限关联。 */
  permissions: PositionPermissionResultDto[];
  /** 岗位组织机构关联。 */
  organizations: PositionOrganizationResultDto[];
}

/** 岗位分页查询结果。 */
export interface PositionPageResultDto {
  /** 符合条件的记录总数。 */
  totalCount: number;
  /** 当前页岗位数据。 */
  items: PositionRowResultDto[];
}
