import request from '../../core/request';
import type { LoginAuditQueryInputDto } from './dtos/login-audit-input.dto';
import type {
  LoginAuditPageResultDto,
  LoginAuditResultDto,
} from './dtos/login-audit-result.dto';

/** 登录审计模块 API 服务。 */
export class LoginAuditApiService {
  /**
   * 分页查询登录审计记录。
   * @param input 页码及筛选条件。
   */
  static getPageList(input: LoginAuditQueryInputDto): Promise<LoginAuditPageResultDto> {
    return request.post<LoginAuditPageResultDto, LoginAuditQueryInputDto>(
      '/api/login-audit/pagedQuery',
      input,
    );
  }

  /**
   * 获取指定登录审计记录详情。
   * @param id 审计记录唯一标识。
   */
  static getById(id: string): Promise<LoginAuditResultDto> {
    return request.get<LoginAuditResultDto>(
      `/api/login-audit/${encodeURIComponent(id)}`,
    );
  }
}
