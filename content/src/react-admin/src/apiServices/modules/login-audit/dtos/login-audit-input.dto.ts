import type { PageInputDto } from '../../../core/base-dto';

/** 登录结果数值常量，与后端 LoginResult 数值保持一致。 */
export const LoginResult = {
  /** 登录认证成功。 */
  Success: 1,
  /** 登录认证失败。 */
  Failure: 2,
} as const;

/** 登录结果类型。 */
export type LoginResult = (typeof LoginResult)[keyof typeof LoginResult];

/** 登录原因数值常量，与后端 LoginReason 数值保持一致。 */
export const LoginReason = {
  /** 认证成功。 */
  SuccessfulAuthentication: 1,
  /** 提交的账号不存在。 */
  AccountNotFound: 2,
  /** 提交的密码错误。 */
  InvalidPassword: 3,
  /** 用户已被软删除。 */
  UserSoftDeleted: 4,
  /** 账号已被禁用。 */
  AccountDisabled: 5,
  /** 账号已被锁定。 */
  AccountLocked: 6,
  /** 账号已被注销。 */
  AccountCancelled: 7,
  /** 登录过程发生系统异常。 */
  SystemError: 8,
} as const;

/** 登录原因类型。 */
export type LoginReason = (typeof LoginReason)[keyof typeof LoginReason];

/** 登录审计分页查询参数，对应 POST /api/login-audit/pagedQuery。 */
export interface LoginAuditQueryInputDto extends PageInputDto {
  /** 起始登录时间（UTC ISO 8601），为空时不限制下界。 */
  startTimeUtc?: string;
  /** 结束登录时间（UTC ISO 8601），为空时不限制上界。 */
  endTimeUtc?: string;
  /** 登录结果筛选。 */
  result?: LoginResult;
  /** 登录原因筛选。 */
  reason?: LoginReason;
  /** 实际提交的登录账号筛选。 */
  account?: string;
  /** 匹配到的用户名筛选。 */
  userName?: string;
  /** 客户端 IP 地址筛选。 */
  clientIp?: string;
}
