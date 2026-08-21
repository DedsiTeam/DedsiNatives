/**
 * @file 组织机构模块 - 请求参数 DTO 声明
 */

/**
 * 分页检索组织机构请求参数。
 */
export interface OrganizationPagedRequestDto {
  /** 所属系统标识筛选 */
  systemId?: string;
  /** 组织名称或编码模糊检索关键字 */
  keyword?: string;
  /** 父级组织标识筛选 */
  parentId?: string;
  /** 启用状态筛选 */
  isEnabled?: boolean;
  /** 当前页码，默认 1 */
  pageIndex?: number;
  /** 每页条数，默认 10 */
  pageSize?: number;
}

/**
 * 创建组织机构请求参数。
 */
export interface CreateOrganizationRequestDto {
  /** 所属系统标识 */
  systemId: string;
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
  /** 父级组织标识（顶级为 null） */
  parentId?: string;
  /** 同级排序权重序号 */
  sort?: number;
  /** 组织职责说明 */
  description?: string;
}

/**
 * 更新组织机构请求参数。
 */
export interface UpdateOrganizationRequestDto {
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
  /** 父级组织标识（顶级为 null） */
  parentId?: string;
  /** 同级排序权重序号 */
  sort?: number;
  /** 组织职责说明 */
  description?: string;
}

/**
 * 设置组织机构启用状态请求参数。
 */
export interface SetOrganizationStatusRequestDto {
  /** 是否启用 */
  isEnabled: boolean;
}
