/**
 * @file SSO 认证集成服务 (AuthService)
 * @description 封装 OIDC 授权码 + PKCE 登录流程、回调处理与登出服务。
 */

import { userManager } from './oidcConfig';
import type { LoginUserResultDto, LoginUserPositionResultDto } from '../apiServices';

let callbackPromise: Promise<LoginUserResultDto> | null = null;

export class SsoAuthService {
  /**
   * 发起 SSO 授权码重定向登录 (PKCE)
   */
  static async loginViaSso(): Promise<void> {
    callbackPromise = null;
    await userManager.signinRedirect();
  }

  /**
   * 处理 OIDC 登录回调：
   * 1. 兑换 SSO AuthServer 签发的标准 Access Token 与 ID Token；
   * 2. 直接将 AuthServer 的 Access Token 存入本地存储，供直接访问 DedsiNative.Host 资源服务；
   * 3. 解析 Profile / Userinfo 中的账号、岗位与细粒度权限，与本地用户会话无缝对齐。
   */
  static async handleCallback(): Promise<LoginUserResultDto> {
    if (callbackPromise) {
      return callbackPromise;
    }

    callbackPromise = (async () => {
      try {
        const oidcUser = await userManager.signinCallback();
        if (!oidcUser || !oidcUser.access_token) {
          throw new Error('SSO 认证中心未返回有效的 Access Token。');
        }

        // 1. 直接保存 AuthServer 签发的 Access Token（DedsiNative.Host 将直接校验该 Token）
        localStorage.setItem('access_token', oidcUser.access_token);

        // 2. 解析用户 Claims 资料
        const profile = oidcUser.profile;
        const account =
          (profile.preferred_username as string) ||
          (profile.name as string) ||
          profile.sub;

        const permissions: string[] = Array.isArray(profile.permissions)
          ? (profile.permissions as string[])
          : typeof profile.permissions === 'string'
          ? [profile.permissions]
          : [];

        const positions: LoginUserPositionResultDto[] = Array.isArray(profile.positions)
          ? (profile.positions as LoginUserPositionResultDto[])
          : typeof profile.roles === 'string'
          ? [{ positionId: '0', positionName: profile.roles }]
          : Array.isArray(profile.roles)
          ? (profile.roles as string[]).map((r, idx) => ({ positionId: String(idx), positionName: r }))
          : [];

        const currentUser: LoginUserResultDto = {
          id: profile.sub,
          name: (profile.name as string) || account,
          email: (profile.email as string) || '',
          account,
          permissions,
          positions,
        };

        localStorage.setItem('current_user', JSON.stringify(currentUser));
        return currentUser;
      } catch (err: any) {
        callbackPromise = null;
        throw err;
      }
    })();

    return callbackPromise;
  }

  /**
   * 获取当前 OIDC 登录态用户
   */
  static async getOidcUser() {
    return await userManager.getUser();
  }

  /**
   * 退出 SSO 登录
   */
  static async logout(): Promise<void> {
    callbackPromise = null;
    localStorage.removeItem('access_token');
    localStorage.removeItem('current_user');
    try {
      await userManager.signoutRedirect();
    } catch {
      window.location.href = '/login';
    }
  }
}
