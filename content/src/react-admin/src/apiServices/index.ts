/**
 * @file apiServices 统一入口门面 (Facade Export)
 * @description 集中导出核心基建、全局通用 DTO 及各业务模块的 Service 与 DTO
 */

// 1. 导出通用基础设施与通用 DTO
export * from './core/base-dto';

// 2. 导出用户业务模块及其 DTO
export * from './modules/user/user.service';
export * from './modules/user/dtos/user-input.dto';
export * from './modules/user/dtos/user-result.dto';

// 3. 导出认证业务模块及其 DTO
export * from './modules/auth/auth.service';
export * from './modules/auth/dtos/auth-input.dto';
export * from './modules/auth/dtos/auth-result.dto';

// 5. 导出系统业务模块及其 DTO
export * from './modules/system/system.service';
export * from './modules/system/dtos/system-input.dto';
export * from './modules/system/dtos/system-result.dto';

// 6. 导出权限业务模块及其 DTO
export * from './modules/permission/permission.service';
export * from './modules/permission/dtos/permission-input.dto';
export * from './modules/permission/dtos/permission-result.dto';

// 7. 导出岗位业务模块及其 DTO
export * from './modules/position/position.service';
export * from './modules/position/dtos/position-input.dto';
export * from './modules/position/dtos/position-result.dto';
export * from './modules/menu/menu.service';
export * from './modules/menu/dtos/menu-input.dto';
export * from './modules/menu/dtos/menu-result.dto';
export * from './modules/dictionary/dictionary.service';
export * from './modules/dictionary/dtos/dictionary-input.dto';
export * from './modules/dictionary/dtos/dictionary-result.dto';
export type { UpdateUserPositionsInputDto } from './modules/user/dtos/user-input.dto';
export type { UserPositionResultDto } from './modules/user/dtos/user-result.dto';
export { ProfileApiService } from './modules/profile/profile.service';
export type { ChangePasswordInputDto } from './modules/profile/profile.service';
export { LoginAuditApiService } from './modules/login-audit/login-audit.service';
export {
  LoginReason,
  LoginResult,
} from './modules/login-audit/dtos/login-audit-input.dto';
export type { LoginAuditQueryInputDto } from './modules/login-audit/dtos/login-audit-input.dto';
export type {
  LoginAuditPageResultDto,
  LoginAuditResultDto,
  LoginAuditRowResultDto,
} from './modules/login-audit/dtos/login-audit-result.dto';

// 8. 导出组织机构业务模块及其 DTO
export * from './modules/organization/organization.service';
export * from './modules/organization/dtos/organization-request.dto';
export * from './modules/organization/dtos/organization-result.dto';

// 9. 导出文件与对象存储业务模块及其 DTO
export * from './modules/storage/storage-file.service';
export * from './modules/storage/dtos/storage-file-request.dto';
export * from './modules/storage/dtos/storage-file-result.dto';

// 10. 导出 SSO 单点登录 (OpenIddict) 业务模块及其 DTO
export * from './modules/openiddict/openiddict.service';
export * from './modules/openiddict/dtos/openiddict-input.dto';
export * from './modules/openiddict/dtos/openiddict-result.dto';
