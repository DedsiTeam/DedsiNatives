/**
 * @file 认证模块 - 响应结果 DTO 声明
 * @description 对接后端 DedsiIdentity.Host 的 FastEndpoints 登录接口响应规范
 */

/**
 * 登录成功后返回的当前用户安全基本资料。
 */
export interface LoginUserResultDto {
  /** 用户唯一标识。 */
  id: string;
  /** 用户姓名。 */
  name: string;
  /** 用户邮箱。 */
  email: string;
  /** 用户登录账号。 */
  account: string;
  /** 当前用户拥有的权限编码，用于前端入口可见性控制。 */
  permissions: string[];
}

/**
 * 登录响应 Result DTO（对接 POST /api/auth/login）。
 */
export interface LoginResultDto {
  /** 签发的 JWT Bearer Token 字符串 */
  token: string;
  /** Token UTC 过期时间字符串 */
  expiresAt: string;
  /** 当前登录用户的安全基本资料。 */
  user: LoginUserResultDto;
}
