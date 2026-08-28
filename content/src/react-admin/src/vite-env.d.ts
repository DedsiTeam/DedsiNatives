/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** 后端 API 基础服务地址 */
  readonly VITE_API_SERVICE_URL: string;

  /** 是否开启静态原型评审入口 */
  readonly VITE_PROTOTYPE_MODE?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
