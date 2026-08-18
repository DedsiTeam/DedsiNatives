/**
 * @file 认证模块 - 请求输入 DTO 声明
 * @description 对接后端 DedsiIdentity.Host 的 FastEndpoints 登录接口参数规范
 */

/**
 * 登录请求输入 DTO (对接 POST /api/auth/login)
 */
export interface LoginInputDto {
  /** 系统用户名 */
  username: string;
  /** 登录密码 */
  password: string;
}
