/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** 后端 API 基础服务地址 */
  readonly VITE_API_SERVICE_URL: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
