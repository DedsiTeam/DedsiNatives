import type { LoginReason, LoginResult } from './login-audit-input.dto';

/** 登录审计列表行数据。 */
export interface LoginAuditRowResultDto {
  /** 审计记录唯一标识。 */
  id: string;
  /** 登录尝试发生的 UTC 时间，使用后端返回的 ISO 8601 字符串。 */
  loginTimeUtc: string;
  /** 本次登录成功或失败。 */
  result: LoginResult;
  /** 本次登录的具体结果原因。 */
  reason: LoginReason;
  /** 实际提交且已规范化的登录账号。 */
  account: string;
  /** 匹配到的用户名；无法匹配用户时为空。 */
  userName: string | null;
  /** 匹配到的用户标识；无法匹配用户时为空。 */
  userId: string | null;
  /** 经可信代理处理后的客户端 IP 地址。 */
  clientIp: string | null;
  /** 登录失败的审计说明；成功记录为空。 */
  failureDescription: string | null;
}

/** 登录审计详情结果。 */
export interface LoginAuditResultDto extends LoginAuditRowResultDto {
  /** 发起登录请求的客户端 User-Agent。 */
  userAgent: string | null;
}

/** 登录审计分页查询结果。 */
export interface LoginAuditPageResultDto {
  /** 符合筛选条件的记录总数。 */
  totalCount: number;
  /** 当前页审计记录。 */
  items: LoginAuditRowResultDto[];
}
