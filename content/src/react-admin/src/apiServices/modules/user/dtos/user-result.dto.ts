/**
 * @file 用户模块 - 响应结果 Result DTO 声明
 * @description 对接后端 DedsiIdentity.Host FastEndpoints 接口 (UserEndpoints) 响应参数规范
 */

/**
 * 获取单个用户详情响应 DTO (对接 GET /api/user/{id})
 */
export interface UserResultDto {
  /** 用户唯一标识 ID (Guid，JSON 中以字符串表示) */
  id: string;
  /** 用户名称 */
  name: string;
  /** 电子邮箱地址 */
  email: string;
  /** 用户联系电话。 */
  phone: string | null;
  /** 用户身份证号码。 */
  idCardNumber: string | null;
  /** 用户资料最后更新时间（UTC）。 */
  lastUpdatedAt: string;
  /** 最后成功登录时间（UTC）。 */
  lastLoginTime: string | null;
  /** 最后成功登录 IP 地址。 */
  lastLoginIp: string | null;
  /** 软删除时间；未删除时为空。 */
  softDeletedAt: string | null;
  /** 用户登录信息，不包含密码哈希和盐值。 */
  loginInfo: UserLoginInfoResultDto | null;
  /** 用户关联的岗位列表。 */
  positions: UserPositionResultDto[];
}

/** 用户岗位关联结果。 */
export interface UserPositionResultDto {
  /** 岗位唯一标识，26 位 ULID。 */
  positionId: string;
  /** 岗位名称快照。 */
  positionName: string;
}

/** 用户登录信息结果。 */
export interface UserLoginInfoResultDto {
  /** 登录账号。 */
  account: string;
  /** 账户状态：1 正常、2 禁用、3 锁定、4 注销。 */
  status: number;
}

/**
 * 用户分页数据行 DTO
 */
export interface PagedUserRowDto {
  /** 用户唯一标识 ID (ULID) */
  id: string;
  /** 用户名称 */
  name: string;
  /** 电子邮箱地址 */
  email: string;
  /** 用户联系电话。 */
  phone: string | null;
  /** 用户资料最后更新时间（UTC）。 */
  lastUpdatedAt: string;
}

/**
 * 用户分页数据响应 Result DTO (对接 POST /api/user/pagedQuery)
 */
export interface UserPageResultDto {
  /** 符合条件的记录总条数 */
  totalCount: number;
  /** 当前页的数据列表 */
  items: PagedUserRowDto[];
}
