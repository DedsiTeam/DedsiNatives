/**
 * @file 组织机构模块 - 响应结果 DTO 声明
 */

/**
 * 组织机构树节点结果模型。
 */
export interface OrganizationTreeNodeResultDto {
  /** 组织唯一标识，26 位 ULID */
  id: string;
  /** 所属系统标识 */
  systemId: string;
  /** 所属系统名称 */
  systemName: string;
  /** 组织机构编码 */
  code: string;
  /** 组织机构主名称 */
  name: string;
  /** 组织机构名称 1（可选） */
  name1?: string;
  /** 组织机构名称 2（可选） */
  name2?: string;
  /** 组织机构名称 3（可选） */
  name3?: string;
  /** 组织机构名称 4（可选） */
  name4?: string;
  /** 父级组织标识 */
  parentId?: string;
  /** 同级排序序号 */
  sort: number;
  /** 组织层级深度 */
  level: number;
  /** 是否启用 */
  isEnabled: boolean;
  /** 组织说明 */
  description?: string;
  /** 下级子组织列表 */
  children?: OrganizationTreeNodeResultDto[];
}

/**
 * 组织机构详情模型。
 */
export interface OrganizationDetailResultDto {
  /** 组织唯一标识 */
  id: string;
  /** 所属系统标识 */
  systemId: string;
  /** 所属系统名称 */
  systemName: string;
  /** 组织机构编码 */
  code: string;
  /** 组织机构主名称 */
  name: string;
  /** 组织机构名称 1 */
  name1?: string;
  /** 组织机构名称 2 */
  name2?: string;
  /** 组织机构名称 3 */
  name3?: string;
  /** 组织机构名称 4 */
  name4?: string;
  /** 父级组织标识 */
  parentId?: string;
  /** 同级排序序号 */
  sort: number;
  /** 组织层级深度 */
  level: number;
  /** 是否启用 */
  isEnabled: boolean;
  /** 组织说明 */
  description?: string;
}

/**
 * 组织机构分页查询结果。
 */
export interface OrganizationPagedResultDto {
  /** 总记录数 */
  totalCount: number;
  /** 当前页记录 */
  items: OrganizationDetailResultDto[];
}
