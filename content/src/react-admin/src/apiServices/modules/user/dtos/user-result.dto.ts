/**
 * @file 用户模块 - 响应结果 Result DTO 声明
 * @description 对接后端 DedsiNative.Endpoints FastEndpoints 接口 (UserEndpoints) 响应参数规范
 */

/**
 * 获取单个用户详情响应 DTO (对接 GET /api/user/{id})
 */
export interface UserResultDto {
  /** 用户唯一标识 ID (ULID) */
  id: string;
  /** 用户名称 */
  name: string;
  /** 电子邮箱地址 */
  email: string;
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
