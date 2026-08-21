import type { PageInputDto } from '../../../core/base-dto';

/**
 * OpenIddict 客户端分页查询输入 DTO。
 */
export interface OpenIddictApplicationQueryInputDto extends PageInputDto {
  clientId?: string;
  displayName?: string;
  clientType?: string;
}

/**
 * 创建 OpenIddict 客户端输入 DTO。
 */
export interface CreateOpenIddictApplicationInputDto {
  clientId: string;
  displayName: string;
  clientType: 'public' | 'confidential' | string;
  clientSecret?: string;
  consentType?: string;
  redirectUris?: string[];
  postLogoutRedirectUris?: string[];
  permissions?: string[];
  requirements?: string[];
}

/**
 * 更新 OpenIddict 客户端输入 DTO。
 */
export interface UpdateOpenIddictApplicationInputDto {
  displayName: string;
  clientType: 'public' | 'confidential' | string;
  consentType?: string;
  redirectUris?: string[];
  postLogoutRedirectUris?: string[];
  permissions?: string[];
  requirements?: string[];
}

/**
 * 重置客户端 Secret 输入 DTO。
 */
export interface ResetOpenIddictApplicationSecretInputDto {
  newSecret?: string;
}

/**
 * OpenIddict 作用域分页查询输入 DTO。
 */
export interface OpenIddictScopeQueryInputDto extends PageInputDto {
  name?: string;
  displayName?: string;
}

/**
 * 创建 OpenIddict 作用域输入 DTO。
 */
export interface CreateOpenIddictScopeInputDto {
  name: string;
  displayName?: string;
  description?: string;
  resources?: string[];
}

/**
 * 更新 OpenIddict 作用域输入 DTO。
 */
export interface UpdateOpenIddictScopeInputDto {
  displayName?: string;
  description?: string;
  resources?: string[];
}

/**
 * OpenIddict 授权记录分页查询输入 DTO。
 */
export interface OpenIddictAuthorizationQueryInputDto extends PageInputDto {
  subject?: string;
  applicationId?: string;
  status?: string;
}

/**
 * OpenIddict 令牌分页查询输入 DTO。
 */
export interface OpenIddictTokenQueryInputDto extends PageInputDto {
  subject?: string;
  applicationId?: string;
  type?: string;
  status?: string;
}
