import request from '../../core/request';
import type { MenuInputDto, MenuQueryInputDto } from './dtos/menu-input.dto';
import type { CurrentUserMenuResultDto, MenuPageResultDto, MenuResultDto } from './dtos/menu-result.dto';

/** 菜单管理模块 API 服务。 */
export class MenuApiService {
  /** 获取当前登录用户可访问的动态菜单树（根据用户权限过滤并排序）。 */
  static getCurrentUserMenus(): Promise<CurrentUserMenuResultDto[]> {
    return request.get<CurrentUserMenuResultDto[]>('/api/menu/currentUser');
  }

  /** 获取指定系统的全部扁平菜单选项。 */
  static getAll(systemId: string): Promise<MenuResultDto[]> {
    return request.get<MenuResultDto[]>(`/api/menu/getAll/${encodeURIComponent(systemId)}`);
  }

  /** 分页查询菜单。 */
  static getPageList(input: MenuQueryInputDto): Promise<MenuPageResultDto> {
    return request.post<MenuPageResultDto, MenuQueryInputDto>('/api/menu/pagedQuery', input);
  }

  /** 获取菜单详情。 */
  static getById(id: string): Promise<MenuResultDto> {
    return request.get<MenuResultDto>(`/api/menu/${encodeURIComponent(id)}`);
  }

  /** 创建菜单并返回菜单标识。 */
  static create(input: MenuInputDto): Promise<string> {
    return request.post<string, MenuInputDto>('/api/menu/create', input);
  }

  /** 更新指定菜单。 */
  static update(id: string, input: MenuInputDto): Promise<boolean> {
    return request.post<boolean, MenuInputDto>(`/api/menu/update/${encodeURIComponent(id)}`, input);
  }

  /** 删除指定菜单。 */
  static delete(id: string): Promise<boolean> {
    return request.post<boolean>(`/api/menu/delete/${encodeURIComponent(id)}`);
  }
}
