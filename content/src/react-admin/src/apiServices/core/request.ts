/**
 * @file 面向对象的 Axios HTTP 请求封装类 HttpClient
 * @description 封装通用请求客户端，构造函数可传入 baseURL，提供 request, get, post 泛型方法
 */

import axios, {
  type AxiosInstance,
  type AxiosRequestConfig,
  type AxiosResponse,
} from 'axios';
import { message } from 'antd';
import { DefaultApiServiceUrl } from '../../configs';

/**
 * 后端通用错误响应模型。
 */
interface ErrorResponseBody {
  /** HTTP 状态码或业务错误码。 */
  code?: number;
  /** 错误消息提示。 */
  message?: string;
  /** 服务器时间。 */
  serviceTime?: string;
  /** 兼容状态描述字段。 */
  status?: string;
  /** 兼容原因描述字段。 */
  reason?: string;
  /** 额外提示说明。 */
  note?: string;
}

/**
 * 解析非 200 响应中的错误文本（将 code + message 告知用户）。
 */
function parseErrorMessage(data: unknown, httpStatus?: number, defaultMessage?: string): string {
  if (typeof data === 'object' && data !== null) {
    const errorBody = data as ErrorResponseBody;
    const code = errorBody.code ?? httpStatus;

    if (errorBody.message && typeof errorBody.message === 'string' && errorBody.message.trim()) {
      const msg = errorBody.message.trim();
      return code !== undefined ? `[${code}] ${msg}` : msg;
    }

    const parts: string[] = [];
    if (errorBody.status && typeof errorBody.status === 'string' && errorBody.status.trim()) {
      parts.push(errorBody.status.trim());
    }
    if (errorBody.reason && typeof errorBody.reason === 'string' && errorBody.reason.trim()) {
      parts.push(errorBody.reason.trim());
    }

    if (parts.length > 0) {
      const msg = parts.join(' ');
      return code !== undefined ? `[${code}] ${msg}` : msg;
    }
  } else if (typeof data === 'string' && data.trim()) {
    const msg = data.trim();
    return httpStatus !== undefined ? `[${httpStatus}] ${msg}` : msg;
  }

  const fallback = defaultMessage || '网络请求异常';
  return httpStatus !== undefined ? `[${httpStatus}] ${fallback}` : fallback;
}

/**
 * 通用 HTTP 请求客户端封装类
 */
export class HttpClient {
  /** 内部 Axios 实例 */
  private instance: AxiosInstance;

  /**
   * 构造函数
   * @param baseURL 基础服务请求 URL 路径
   * @param timeout 超时时间 (毫秒)，默认 10000ms
   */
  constructor(baseURL: string = DefaultApiServiceUrl, timeout: number = 10000) {
    this.instance = axios.create({
      baseURL,
      timeout,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    this.setupInterceptors();
  }

  /**
   * 初始化请求与响应拦截器
   */
  private setupInterceptors(): void {
    // 1. 请求拦截器 (挂载 Bearer Token)
    this.instance.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('access_token');
        if (token && config.headers) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error: unknown) => Promise.reject(error)
    );

    // 2. 响应拦截器统一处理异常；成功响应由泛型请求方法按准确类型解包。
    this.instance.interceptors.response.use(
      (response) => response,
      (error: unknown) => {
        const axiosError = axios.isAxiosError<ErrorResponseBody>(error)
          ? error
          : undefined;
        const httpStatus = axiosError?.response?.status;
        const responseData = axiosError?.response?.data;
        const errorMessage = parseErrorMessage(responseData, httpStatus, axiosError?.message);

        // 如果页面上有 loading 消息，销毁避免覆盖
        message.destroy('login');

        switch (httpStatus) {
          case 401:
            localStorage.removeItem('access_token');
            localStorage.removeItem('current_user');
            message.error(errorMessage || '登录状态已过期，请重新登录');
            if (window.location.pathname !== '/login') {
              window.location.href = '/login';
            }
            break;
          default:
            message.error(errorMessage);
            break;
        }

        return Promise.reject(error);
      }
    );
  }

  /**
   * 通用 AxiosRequestConfig 请求方法
   * @param config 请求配置参数
   */
  public request<TResponse>(
    config: AxiosRequestConfig,
  ): Promise<TResponse> {
    return this.instance
      .request<TResponse>(config)
      .then((response) => response.data);
  }

  /**
   * GET 请求方法
   * @param url 请求相对路径
   * @param config 额外配置参数
   */
  public get<TResponse>(
    url: string,
    config?: AxiosRequestConfig,
  ): Promise<TResponse> {
    return this.instance
      .get<TResponse>(url, config)
      .then((response) => response.data);
  }

  /**
   * POST 请求方法
   * @param url 请求相对路径
   * @param data Body 请求体数据
   * @param config 额外配置参数
   */
  public post<TResponse, TBody = unknown>(
    url: string,
    data?: TBody,
    config?: AxiosRequestConfig<TBody>,
  ): Promise<TResponse> {
    return this.instance
      .post<TResponse, AxiosResponse<TResponse, TBody>, TBody>(url, data, config)
      .then((response) => response.data);
  }
}

/**
 * 默认使用 DefaultApiServiceUrl 实例化的全局请求单例
 */
export const request = new HttpClient(DefaultApiServiceUrl);
export default request;
