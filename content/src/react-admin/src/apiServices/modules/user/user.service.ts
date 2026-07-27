/**
 * @file 用户 API 服务 (UserApiService)
 * @description 封装后端 FastEndpoints 用户模块 HTTP 接口:
 * - POST /api/user/pagedQuery (分页获取用户列表)
 * - GET  /api/user/{id}        (获取指定用户详情)
 * - POST /api/user/create     (创建新用户)
 * - POST /api/user/update/{id} (更新用户信息)
 * - POST /api/user/delete/{id} (删除用户账号)
 */

import request from '../../core/request';
import type { UserQueryInputDto, CreateUserInputDto, UpdateUserInputDto } from './dtos/user-input.dto';
import type { UserResultDto, UserPageResultDto } from './dtos/user-result.dto';

export class UserApiService {
  /**
   * 分页获取用户列表 (POST /api/user/pagedQuery)
   * @param params 检索与分页输入 DTO
   */
  static async getPageList(params: UserQueryInputDto): Promise<UserPageResultDto> {
    return request.post<UserPageResultDto>('/api/user/pagedQuery', params);
  }

  /**
   * 获取单个用户详细信息 (GET /api/user/{id})
   * @param id 用户唯一 ID
   */
  static async getById(id: string): Promise<UserResultDto> {
    return request.get<UserResultDto>(`/api/user/${id}`);
  }

  /**
   * 创建新用户 (POST /api/user/create)
   * @param data 用户新建输入 DTO
   * @returns 新创建用户的唯一标识 ID (ULID)
   */
  static async createUser(data: CreateUserInputDto): Promise<string> {
    return request.post<string>('/api/user/create', data);
  }

  /**
   * 更新用户信息 (POST /api/user/update/{id})
   * @param id 用户唯一 ID
   * @param data 用户更新输入 DTO
   */
  static async updateUser(id: string, data: UpdateUserInputDto): Promise<boolean> {
    return request.post<boolean>(`/api/user/update/${id}`, data);
  }

  /**
   * 删除用户账号 (POST /api/user/delete/{id})
   * @param id 用户唯一 ID
   */
  static async deleteUser(id: string): Promise<boolean> {
    return request.post<boolean>(`/api/user/delete/${id}`);
  }
}
