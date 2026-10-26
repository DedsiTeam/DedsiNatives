import { ApiRoutePrefix } from './api';

export { ApiRoutePrefix } from './api';

/** 默认业务 API 地址；服务地址为空时使用同源请求，前缀只在此拼接一次。 */
export const DefaultApiServiceUrl: string =
  `${(import.meta.env.VITE_API_SERVICE_URL ?? '').replace(/\/+$/, '')}${ApiRoutePrefix}`;
