/**
 * @file 用户模块 - 请求输入 Input DTO 声明
 * @description 对接后端 DedsiIdentity.Host FastEndpoints 接口 (UserEndpoints) 请求参数规范
 */

import type { PageInputDto } from '../../../core/base-dto';

/** 用户登录信息输入；密码仅在创建或重置密码时传送。 */
export interface UserLoginInfoInputDto {
  /** 登录账号。 */
  account: string;
  /** 登录密码；编辑时留空则保留现有密码。 */
  password?: string;
  /** 账户状态：1 正常、2 禁用、3 锁定、4 注销。 */
  status: number;
}

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
  /** 用户联系电话。 */
  phone?: string;
  /** 用户身份证号码。 */
  idCardNumber?: string;
  /** 初始关联的岗位 ID 列表。 */
  positionIds?: string[];
  /** 初始登录信息。 */
  loginInfo?: UserLoginInfoInputDto;
}

/**
 * 修改用户信息请求输入 DTO (对接 POST /api/user/update/{id})
 */
export interface UpdateUserInputDto {
  /** 用户名称，不能为空 */
  name: string;
  /** 用户邮箱地址，不能为空 */
  email: string;
  /** 用户联系电话。 */
  phone?: string;
  /** 用户身份证号码。 */
  idCardNumber?: string;
  /** 替换后的岗位 ID 列表。 */
  positionIds?: string[];
  /** 修改后的登录信息。 */
  loginInfo?: UserLoginInfoInputDto;
}

/** 替换用户岗位关联的请求参数。 */
export interface UpdateUserPositionsInputDto {
  /** 用户要关联的岗位 ID 列表。 */
  positionIds: string[];
}
