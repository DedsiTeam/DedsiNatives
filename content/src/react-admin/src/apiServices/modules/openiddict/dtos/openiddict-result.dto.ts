import type { PageResultDto } from '../../../core/base-dto';

/**
 * 客户端列表单行数据 DTO。
 */
export interface OpenIddictApplicationRowResultDto {
  id: string;
  clientId?: string;
  displayName?: string;
  clientType?: string;
  consentType?: string;
  redirectUris: string[];
  postLogoutRedirectUris: string[];
  permissions: string[];
}

/**
 * 客户端详情 DTO。
 */
export interface OpenIddictApplicationResultDto {
  id: string;
  clientId?: string;
  displayName?: string;
  clientType?: string;
  consentType?: string;
  redirectUris: string[];
  postLogoutRedirectUris: string[];
  permissions: string[];
  requirements: string[];
}

export type OpenIddictApplicationPageResultDto = PageResultDto<OpenIddictApplicationRowResultDto>;

/**
 * 作用域列表单行数据 DTO。
 */
export interface OpenIddictScopeRowResultDto {
  id: string;
  name?: string;
  displayName?: string;
  description?: string;
  resources: string[];
}

/**
 * 作用域详情 DTO。
 */
export interface OpenIddictScopeResultDto {
  id: string;
  name?: string;
  displayName?: string;
  description?: string;
  resources: string[];
}

export type OpenIddictScopePageResultDto = PageResultDto<OpenIddictScopeRowResultDto>;

/**
 * 授权记录单行数据 DTO。
 */
export interface OpenIddictAuthorizationRowResultDto {
  id: string;
  applicationId?: string;
  clientId?: string;
  applicationDisplayName?: string;
  subject?: string;
  status?: string;
  type?: string;
  scopes: string[];
  creationDate?: string;
}

export type OpenIddictAuthorizationPageResultDto = PageResultDto<OpenIddictAuthorizationRowResultDto>;

/**
 * 令牌单行数据 DTO。
 */
export interface OpenIddictTokenRowResultDto {
  id: string;
  applicationId?: string;
  clientId?: string;
  subject?: string;
  status?: string;
  type?: string;
  creationDate?: string;
  expirationDate?: string;
  redemptionDate?: string;
}

export type OpenIddictTokenPageResultDto = PageResultDto<OpenIddictTokenRowResultDto>;
