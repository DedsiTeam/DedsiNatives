import request from '../../core/request';

/** 当前登录用户资料。 */
export interface ProfileResultDto { id: string; name: string; email: string; phone?: string; account: string; accountStatus: number; lastLoginTime?: string; }
/** 修改当前用户密码的参数。 */
export interface ChangePasswordInputDto { currentPassword: string; newPassword: string; confirmPassword: string; }
/** 个人中心 API 服务。 */
export class ProfileApiService {
  /** 获取当前用户资料。 */
  static get(): Promise<ProfileResultDto> { return request.get<ProfileResultDto>('/api/profile'); }
  /** 修改当前用户密码。 */
  static changePassword(input: ChangePasswordInputDto): Promise<boolean> { return request.post<boolean, ChangePasswordInputDto>('/api/profile/changePassword', input); }
}
