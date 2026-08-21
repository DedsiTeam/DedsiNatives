import request from '../../core/request';

/** 修改当前用户密码的参数。 */
export interface ChangePasswordInputDto { currentPassword: string; newPassword: string; confirmPassword: string; }
/** 个人中心 API 服务。 */
export class ProfileApiService {
  /** 修改当前用户密码。 */
  static changePassword(input: ChangePasswordInputDto): Promise<boolean> { return request.post<boolean, ChangePasswordInputDto>('/api/profile/changePassword', input); }
}
