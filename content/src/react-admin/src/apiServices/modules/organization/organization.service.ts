import request from '../../core/request';
import type {
  CreateOrganizationRequestDto,
  OrganizationPagedRequestDto,
  SetOrganizationStatusRequestDto,
  UpdateOrganizationRequestDto,
} from './dtos/organization-request.dto';
import type {
  OrganizationDetailResultDto,
  OrganizationPagedResultDto,
  OrganizationTreeNodeResultDto,
} from './dtos/organization-result.dto';

/**
 * 组织机构管理 API 服务
 */
export class OrganizationApiService {
  /**
   * 获取指定系统下的多级组织机构树
   */
  static getOrganizationTree(systemId: string) {
    return request.get<OrganizationTreeNodeResultDto[]>(`/api/organization/tree/${systemId}`);
  }

  /**
   * 获取所有组织机构构成的完整组织机构树
   */
  static getAllOrganizationTree() {
    return request.get<OrganizationTreeNodeResultDto[]>('/api/organization/all-tree');
  }

  /**
   * 获取用于用户增改下拉选择专用的组织机构树选项
   */
  static getUserOrganizationOptions() {
    return request.get<import('./dtos/organization-result.dto').UserOrganizationOptionNodeDto[]>(
      '/api/organization/user-options'
    );
  }

  /**
   * 分页检索组织机构列表
   */
  static getOrganizationPaged(params: OrganizationPagedRequestDto) {
    return request.post<OrganizationPagedResultDto>('/api/organization/pagedQuery', params);
  }

  /**
   * 获取组织机构详情
   */
  static getOrganizationDetail(id: string) {
    return request.get<OrganizationDetailResultDto>(`/api/organization/${id}`);
  }

  /**
   * 创建组织机构
   */
  static createOrganization(data: CreateOrganizationRequestDto) {
    return request.post<{ id: string }>('/api/organization/create', data);
  }

  /**
   * 更新组织机构
   */
  static updateOrganization(id: string, data: UpdateOrganizationRequestDto) {
    return request.post<{ success: boolean }>(`/api/organization/update/${id}`, data);
  }

  /**
   * 删除组织机构
   */
  static deleteOrganization(id: string) {
    return request.post<{ success: boolean }>(`/api/organization/delete/${id}`);
  }

  /**
   * 设置组织机构启用状态
   */
  static setOrganizationStatus(id: string, data: SetOrganizationStatusRequestDto) {
    return request.post<{ success: boolean }>(`/api/organization/setStatus/${id}`, data);
  }
}
