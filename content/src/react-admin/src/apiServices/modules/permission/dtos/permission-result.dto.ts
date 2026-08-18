/** 权限列表单行结果。 */
export interface PermissionRowResultDto {
  /** 权限唯一标识，26 位 ULID。 */
  id: string;
  /** 所属系统 ID。 */
  systemId: string;
  /** 所属系统名称。 */
  systemName: string;
  /** 权限名称。 */
  name: string;
  /** 权限说明。 */
  description: string | null;
  /** 是否启用。 */
  isEnabled: boolean;
}

/** 权限分页查询结果。 */
export interface PermissionPageResultDto {
  /** 符合条件的记录总数。 */
  totalCount: number;
  /** 当前页权限数据。 */
  items: PermissionRowResultDto[];
}

/** 权限详情结果。 */
export type PermissionResultDto = PermissionRowResultDto;
