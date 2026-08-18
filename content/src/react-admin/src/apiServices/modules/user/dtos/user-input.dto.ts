/**
 * @file 用户模块 - 请求输入 Input DTO 声明
 * @description 对接后端 DedsiNative.Endpoints FastEndpoints 接口 (UserEndpoints) 请求参数规范
 */

import type { PageInputDto } from '../../../core/base-dto';

/**
 * 用户分页条件检索输入 DTO (对接 POST /api/user/pagedQuery)
 */
export interface UserQueryInputDto extends PageInputDto {
  /** 按用户名称模糊筛选，为空时不过滤 */
  name?: string;
  /** 按邮箱地址模糊筛选，为空时不过滤 */
  email?: string;
  /** 是否为导出模式 */
  isExport?: boolean;
}

/**
 * 创建新用户请求输入 DTO (对接 POST /api/user/create)
 */
export interface CreateUserInputDto {
  /** 用户名称，不能为空 */
  name: string;
  /** 用户邮箱地址，不能为空 */
  email: string;
}

/**
 * 修改用户信息请求输入 DTO (对接 POST /api/user/update/{id})
 */
export interface UpdateUserInputDto {
  /** 用户名称，不能为空 */
  name: string;
  /** 用户邮箱地址，不能为空 */
  email: string;
}
