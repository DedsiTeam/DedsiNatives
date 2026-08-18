import request from '../../core/request';
import type {
  CreateSystemInputDto,
  SystemQueryInputDto,
  UpdateSystemInputDto,
} from './dtos/system-input.dto';
import type { SystemPageResultDto, SystemResultDto, SystemRowResultDto } from './dtos/system-result.dto';

/** 系统管理模块 API 服务。 */
export class SystemApiService {
  /** 获取全部系统选项。 */
  static getAll(): Promise<SystemRowResultDto[]> {
    return request.get<SystemRowResultDto[]>('/api/system/getAll');
  }

  /** 分页查询系统。 */
  static getPageList(input: SystemQueryInputDto): Promise<SystemPageResultDto> {
    return request.post<SystemPageResultDto, SystemQueryInputDto>(
      '/api/system/pagedQuery',
      input,
    );
  }

  /** 获取系统详情。 */
  static getById(id: string): Promise<SystemResultDto> {
    return request.get<SystemResultDto>(`/api/system/${encodeURIComponent(id)}`);
  }

  /** 创建系统并返回新系统 ID。 */
  static create(input: CreateSystemInputDto): Promise<string> {
    return request.post<string, CreateSystemInputDto>('/api/system/create', input);
  }

  /** 更新指定系统。 */
  static update(id: string, input: UpdateSystemInputDto): Promise<boolean> {
    return request.post<boolean, UpdateSystemInputDto>(
      `/api/system/update/${encodeURIComponent(id)}`,
      input,
    );
  }

  /** 删除指定系统。 */
  static delete(id: string): Promise<boolean> {
    return request.post<boolean>(`/api/system/delete/${encodeURIComponent(id)}`);
  }
}
