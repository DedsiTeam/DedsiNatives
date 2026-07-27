/**
 * @file 面向对象的 Axios HTTP 请求封装类 HttpClient
 * @description 封装通用请求客户端，构造函数可传入 baseURL，提供 request, get, post 泛型方法
 */

import axios, { type AxiosInstance, type AxiosRequestConfig, type AxiosResponse } from 'axios';
import { message } from 'antd';
import { DefaultApiServiceUrl } from '../../configs';

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
      (error) => Promise.reject(error)
    );

    // 2. 响应拦截器 (解包数据与统一异常处理)
    this.instance.interceptors.response.use(
      (response: AxiosResponse) => {
        return response.data;
      },
      (error) => {
        const status = error.response?.status;
        const errorMessage = error.response?.data?.message || error.message || '网络请求异常';

        switch (status) {
          case 401:
            message.error('登录状态已过期，请重新登录');
            break;
          case 403:
            message.error('您没有权限访问该资源');
            break;
          case 500:
            message.error('服务器内部错误，请联系系统管理员');
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
  public request<T = any>(config: AxiosRequestConfig): Promise<T> {
    return this.instance.request<any, T>(config);
  }

  /**
   * GET 请求方法
   * @param url 请求相对路径
   * @param config 额外配置参数
   */
  public get<T = any>(url: string, config?: AxiosRequestConfig): Promise<T> {
    return this.instance.get<any, T>(url, config);
  }

  /**
   * POST 请求方法
   * @param url 请求相对路径
   * @param data Body 请求体数据
   * @param config 额外配置参数
   */
  public post<T = any>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    return this.instance.post<any, T>(url, data, config);
  }
}

/**
 * 默认使用 DefaultApiServiceUrl 实例化的全局请求单例
 */
export const request = new HttpClient(DefaultApiServiceUrl);
export default request;
