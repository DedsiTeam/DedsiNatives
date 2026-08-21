import request from '../../core/request';
import type {
  OpenIddictApplicationQueryInputDto,
  CreateOpenIddictApplicationInputDto,
  UpdateOpenIddictApplicationInputDto,
  ResetOpenIddictApplicationSecretInputDto,
  OpenIddictScopeQueryInputDto,
  CreateOpenIddictScopeInputDto,
  UpdateOpenIddictScopeInputDto,
  OpenIddictAuthorizationQueryInputDto,
  OpenIddictTokenQueryInputDto,
} from './dtos/openiddict-input.dto';
import type {
  OpenIddictApplicationPageResultDto,
  OpenIddictApplicationResultDto,
  OpenIddictScopePageResultDto,
  OpenIddictScopeResultDto,
  OpenIddictAuthorizationPageResultDto,
  OpenIddictTokenPageResultDto,
} from './dtos/openiddict-result.dto';

export class OpenIddictApiService {
  // === 1. 客户端管理 (Applications) ===

  /** 分页查询客户端列表 */
  static async getApplicationPageList(params: OpenIddictApplicationQueryInputDto): Promise<OpenIddictApplicationPageResultDto> {
    return request.post<OpenIddictApplicationPageResultDto, OpenIddictApplicationQueryInputDto>(
      '/api/openiddict/applications/pagedQuery',
      params,
    );
  }

  /** 获取客户端详情 */
  static async getApplicationById(id: string): Promise<OpenIddictApplicationResultDto> {
    return request.get<OpenIddictApplicationResultDto>(
      `/api/openiddict/applications/${encodeURIComponent(id)}`,
    );
  }

  /** 创建客户端 */
  static async createApplication(data: CreateOpenIddictApplicationInputDto): Promise<string> {
    return request.post<string, CreateOpenIddictApplicationInputDto>(
      '/api/openiddict/applications',
      data,
    );
  }

  /** 更新客户端 */
  static async updateApplication(id: string, data: UpdateOpenIddictApplicationInputDto): Promise<void> {
    return request.put<void, UpdateOpenIddictApplicationInputDto>(
      `/api/openiddict/applications/${encodeURIComponent(id)}`,
      data,
    );
  }

  /** 重置客户端 Secret */
  static async resetApplicationSecret(id: string, data?: ResetOpenIddictApplicationSecretInputDto): Promise<{ newSecret: string }> {
    return request.post<{ newSecret: string }, ResetOpenIddictApplicationSecretInputDto>(
      `/api/openiddict/applications/${encodeURIComponent(id)}/reset-secret`,
      data ?? {},
    );
  }

  /** 删除客户端 */
  static async deleteApplication(id: string): Promise<void> {
    return request.delete<void>(
      `/api/openiddict/applications/${encodeURIComponent(id)}`,
    );
  }

  // === 2. 作用域管理 (Scopes) ===

  /** 分页查询作用域列表 */
  static async getScopePageList(params: OpenIddictScopeQueryInputDto): Promise<OpenIddictScopePageResultDto> {
    return request.post<OpenIddictScopePageResultDto, OpenIddictScopeQueryInputDto>(
      '/api/openiddict/scopes/pagedQuery',
      params,
    );
  }

  /** 获取作用域详情 */
  static async getScopeById(id: string): Promise<OpenIddictScopeResultDto> {
    return request.get<OpenIddictScopeResultDto>(
      `/api/openiddict/scopes/${encodeURIComponent(id)}`,
    );
  }

  /** 创建作用域 */
  static async createScope(data: CreateOpenIddictScopeInputDto): Promise<string> {
    return request.post<string, CreateOpenIddictScopeInputDto>(
      '/api/openiddict/scopes',
      data,
    );
  }

  /** 更新作用域 */
  static async updateScope(id: string, data: UpdateOpenIddictScopeInputDto): Promise<void> {
    return request.put<void, UpdateOpenIddictScopeInputDto>(
      `/api/openiddict/scopes/${encodeURIComponent(id)}`,
      data,
    );
  }

  /** 删除作用域 */
  static async deleteScope(id: string): Promise<void> {
    return request.delete<void>(
      `/api/openiddict/scopes/${encodeURIComponent(id)}`,
    );
  }

  // === 3. 授权记录 (Authorizations) ===

  /** 分页查询授权记录列表 */
  static async getAuthorizationPageList(params: OpenIddictAuthorizationQueryInputDto): Promise<OpenIddictAuthorizationPageResultDto> {
    return request.post<OpenIddictAuthorizationPageResultDto, OpenIddictAuthorizationQueryInputDto>(
      '/api/openiddict/authorizations/pagedQuery',
      params,
    );
  }

  /** 吊销指定授权 */
  static async revokeAuthorization(id: string): Promise<void> {
    return request.post<void>(
      `/api/openiddict/authorizations/${encodeURIComponent(id)}/revoke`,
    );
  }

  // === 4. 令牌管理 (Tokens) ===

  /** 分页查询令牌列表 */
  static async getTokenPageList(params: OpenIddictTokenQueryInputDto): Promise<OpenIddictTokenPageResultDto> {
    return request.post<OpenIddictTokenPageResultDto, OpenIddictTokenQueryInputDto>(
      '/api/openiddict/tokens/pagedQuery',
      params,
    );
  }

  /** 吊销指定令牌 */
  static async revokeToken(id: string): Promise<void> {
    return request.post<void>(
      `/api/openiddict/tokens/${encodeURIComponent(id)}/revoke`,
    );
  }
}
