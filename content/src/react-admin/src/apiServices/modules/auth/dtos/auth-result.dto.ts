/**
 * @file 认证模块 - 响应结果 DTO 声明
 * @description 对接后端 DedsiNative.Host 的 FastEndpoints 登录接口响应规范
 */

/**
 * 登录响应 Result DTO (对接 POST /api/auth/login)
 */
export interface LoginResultDto {
  /** 签发的 JWT Bearer Token 字符串 */
  token: string;
  /** Token UTC 过期时间字符串 */
  expiresAt: string;
}
