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

// 3. 导出订单业务模块及其 DTO
export * from './modules/order/order.service';
export * from './modules/order/dtos/order-input.dto';
export * from './modules/order/dtos/order-result.dto';

// 4. 导出认证业务模块及其 DTO
export * from './modules/auth/auth.service';
export * from './modules/auth/dtos/auth-input.dto';
export * from './modules/auth/dtos/auth-result.dto';
