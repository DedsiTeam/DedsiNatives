/**
 * @file 订单 API 服务 (OrderApiService)
 * @description 提供订单详情查询及相关操作接口
 */

import request from '../../core/request';
import type { ApiResult } from '../../core/base-dto';
import type { OrderDetailResultDto } from './dtos/order-result.dto';

export class OrderApiService {
  /**
   * 获取订单多字段与多表格详细信息 (GET)
   * @param orderId 订单编号
   */
  static async getOrderDetail(orderId: string): Promise<ApiResult<OrderDetailResultDto>> {
    return request.get(`/api/orders/${orderId}`);
  }

  /**
   * 发货处理 (POST)
   * @param orderId 订单编号
   * @param logisticsCompany 快递公司
   * @param trackingNumber 快递单号
   */
  static async shipOrder(orderId: string, logisticsCompany: string, trackingNumber: string): Promise<ApiResult<boolean>> {
    return request.post(`/api/orders/${orderId}/ship`, { logisticsCompany, trackingNumber });
  }
}
