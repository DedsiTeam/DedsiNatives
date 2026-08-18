/**
 * @file 认证 API 服务 (AuthApiService)
 * @description 封装与用户登录、Token 签发相关的 HTTP 接口服务:
 * - POST /api/auth/login (用户登录并获取 JWT Token)
 */

import request from '../../core/request';
import type { LoginInputDto } from './dtos/auth-input.dto';
import type { LoginResultDto } from './dtos/auth-result.dto';

export class AuthApiService {
  /**
   * 用户登录接口 (POST /api/auth/login)
   * @param data 登录参数 (username, password)
   * @returns 包含 JWT Token 与过期时间的响应数据对象
   */
  static async login(data: LoginInputDto): Promise<LoginResultDto> {
    return request.post<LoginResultDto, LoginInputDto>('/api/auth/login', data);
  }
}
