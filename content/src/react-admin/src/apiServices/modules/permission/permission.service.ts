import request from '../../core/request';
import type {
  CreatePermissionInputDto,
  PermissionQueryInputDto,
  SetPermissionStatusInputDto,
  UpdatePermissionInputDto,
} from './dtos/permission-input.dto';
import type {
    PermissionPageResultDto,
    PermissionResultDto,
    PermissionRowResultDto,
} from './dtos/permission-result.dto';

/** 权限管理模块 API 服务。 */
export class PermissionApiService {
  /** 获取指定系统的全部权限选项。 */
  static getAll(systemId: string): Promise<PermissionRowResultDto[]> {
    return request.get<PermissionRowResultDto[]>(
      `/api/permission/getAll/${encodeURIComponent(systemId)}`,
    );
  }

  /** 分页查询权限。 */
  static getPageList(input: PermissionQueryInputDto): Promise<PermissionPageResultDto> {
    return request.post<PermissionPageResultDto, PermissionQueryInputDto>(
      '/api/permission/pagedQuery',
      input,
    );
  }

  /** 获取权限详情。 */
  static getById(id: string): Promise<PermissionResultDto> {
    return request.get<PermissionResultDto>(`/api/permission/${encodeURIComponent(id)}`);
  }

  /** 创建权限并返回新权限 ID。 */
  static create(input: CreatePermissionInputDto): Promise<string> {
    return request.post<string, CreatePermissionInputDto>('/api/permission/create', input);
  }

  /** 更新指定权限。 */
  static update(id: string, input: UpdatePermissionInputDto): Promise<boolean> {
    return request.post<boolean, UpdatePermissionInputDto>(
      `/api/permission/update/${encodeURIComponent(id)}`,
      input,
    );
  }

  /** 修改指定权限的启用状态。 */
  static setStatus(id: string, input: SetPermissionStatusInputDto): Promise<boolean> {
    return request.post<boolean, SetPermissionStatusInputDto>(
      `/api/permission/status/${encodeURIComponent(id)}`,
      input,
    );
  }

  /** 删除指定权限。 */
  static delete(id: string): Promise<boolean> {
    return request.post<boolean>(`/api/permission/delete/${encodeURIComponent(id)}`);
  }
}
