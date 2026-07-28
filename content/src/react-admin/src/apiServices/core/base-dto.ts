/**
 * @file 全局通用 DTO 类型声明
 * @description 包含通用的接口响应包 ApiResult、通用分页查询 InputDto、通用分页响应 PageResultDto
 */

/**
 * 接口统一响应包装结构
 */
export interface ApiResult<T = unknown> {
  /** 状态码 (例如: 200 为成功) */
  code: number;
  /** 响应消息说明 */
  message: string;
  /** 业务响应数据主体 */
  data: T;
  /** 时间戳 */
  timestamp?: string;
}

/**
 * 基础分页查询输入 DTO
 */
export interface PageInputDto {
  /** 当前页码 (从 1 开始) */
  pageIndex: number;
  /** 每页展示记录条数 */
  pageSize: number;
}

/**
 * 基础通用分页响应 DTO
 */
export interface PageResultDto<T> {
  /** 当前页的数据列表 */
  items: T[];
  /** 符合条件的记录总条数 */
  totalCount: number;
  /** 当前页码 */
  pageIndex: number;
  /** 每页条数 */
  pageSize: number;
}
