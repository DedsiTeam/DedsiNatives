/**
 * @file 订单模块 - 响应结果 Result DTO 声明
 */

/**
 * 订单商品项实体 DTO
 */
export interface OrderItemDto {
  key: string;
  skuCode: string;
  goodsName: string;
  goodsImage: string;
  spec: string;
  unitPrice: number;
  quantity: number;
  discountAmount: number;
  totalPrice: number;
}

/**
 * 订单操作履历日志实体 DTO
 */
export interface OrderLogDto {
  key: string;
  operator: string;
  action: string;
  node: string;
  result: 'success' | 'warning' | 'info';
  timestamp: string;
  remark?: string;
}

/**
 * 订单多字段完整响应 Result DTO
 */
export interface OrderDetailResultDto {
  /** 订单号 */
  orderId: string;
  /** 状态: paid | shipped | completed | cancelled */
  status: 'paid' | 'shipped' | 'completed' | 'cancelled';
  /** 状态名称 */
  statusLabel: string;
  /** 创单时间 */
  createdAt: string;
  /** 支付时间 */
  paidAt: string;
  /** 支付方式 */
  paymentMethod: string;
  /** 支付交易号 */
  transactionId: string;
  
  /** 财务金额信息 */
  totalAmount: number;
  discountAmount: number;
  freightAmount: number;
  actualAmount: number;

  /** 买家客户信息 */
  buyerName: string;
  buyerAvatar: string;
  buyerAccount: string;
  buyerEmail: string;
  memberLevel: string;

  /** 物流收货信息 */
  receiverName: string;
  receiverPhone: string;
  receiverAddress: string;
  logisticsCompany: string;
  trackingNumber: string;

  /** 购买商品明细表 */
  items: OrderItemDto[];

  /** 操作日志表 */
  logs: OrderLogDto[];

  /** 客户备注与内部客服备注 */
  customerRemark?: string;
  adminRemark?: string;
}
