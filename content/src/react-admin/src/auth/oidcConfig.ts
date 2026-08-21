/**
 * @file OIDC / OAuth 2.0 客户端配置与 UserManager 单例
 * @description 从环境变量加载 SSO 配置，启用 PKCE 授权码模式。
 */

import { UserManager, type UserManagerSettings, WebStorageStateStore } from 'oidc-client-ts';

export const oidcSettings: UserManagerSettings = {
  authority: import.meta.env.VITE_SSO_AUTHORITY || 'http://localhost:12257',
  client_id: import.meta.env.VITE_SSO_CLIENT_ID || 'dedsinative-web',
  redirect_uri: import.meta.env.VITE_SSO_REDIRECT_URI || `${window.location.origin}/callback`,
  post_logout_redirect_uri:
    import.meta.env.VITE_SSO_POST_LOGOUT_REDIRECT_URI || `${window.location.origin}/login`,
  response_type: 'code',
  scope: import.meta.env.VITE_SSO_SCOPE || 'openid profile email roles dedsinative_api',
  userStore: new WebStorageStateStore({ store: window.localStorage }),
  loadUserInfo: true,
  filterProtocolClaims: false,
  automaticSilentRenew: false,
};

/**
 * 全局 OIDC UserManager 单例
 */
export const userManager = new UserManager(oidcSettings);
