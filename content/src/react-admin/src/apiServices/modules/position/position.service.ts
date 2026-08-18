import request from '../../core/request';
import type {
  CreatePositionInputDto,
  PositionQueryInputDto,
  SetPositionStatusInputDto,
  UpdatePositionAssignmentsInputDto,
  UpdatePositionInputDto,
} from './dtos/position-input.dto';
import type { PositionPageResultDto, PositionResultDto } from './dtos/position-result.dto';

/** 岗位管理模块 API 服务。 */
export class PositionApiService {
  /** 分页查询岗位。 */
  static getPageList(input: PositionQueryInputDto): Promise<PositionPageResultDto> {
    return request.post<PositionPageResultDto, PositionQueryInputDto>('/api/position/pagedQuery', input);
  }

  /** 获取岗位详情及其权限、组织机构关联。 */
  static getById(id: string): Promise<PositionResultDto> {
    return request.get<PositionResultDto>(`/api/position/${encodeURIComponent(id)}`);
  }

  /** 创建岗位。 */
  static create(input: CreatePositionInputDto): Promise<string> {
    return request.post<string, CreatePositionInputDto>('/api/position/create', input);
  }

  /** 更新岗位资料。 */
  static update(id: string, input: UpdatePositionInputDto): Promise<boolean> {
    return request.post<boolean, UpdatePositionInputDto>(`/api/position/update/${encodeURIComponent(id)}`, input);
  }

  /** 修改岗位启用状态。 */
  static setStatus(id: string, input: SetPositionStatusInputDto): Promise<boolean> {
    return request.post<boolean, SetPositionStatusInputDto>(`/api/position/status/${encodeURIComponent(id)}`, input);
  }

  /** 替换岗位权限和组织机构关联。 */
  static updateAssignments(id: string, input: UpdatePositionAssignmentsInputDto): Promise<boolean> {
    return request.post<boolean, UpdatePositionAssignmentsInputDto>(
      `/api/position/assignments/${encodeURIComponent(id)}`,
      input,
    );
  }

  /** 删除岗位。 */
  static delete(id: string): Promise<boolean> {
    return request.post<boolean>(`/api/position/delete/${encodeURIComponent(id)}`);
  }
}
