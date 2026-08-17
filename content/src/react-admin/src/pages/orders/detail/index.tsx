import { useState, useEffect } from 'react';
import {
  Card,
  Row,
  Col,
  Tag,
  Button,
  Descriptions,
  Table,
  Space,
  Avatar,
  Timeline,
  Statistic,
  Input,
  Divider,
  message,
  Modal,
  Form,
  Select,
  type TableProps,
} from 'antd';
import {
  PrinterOutlined,
  SendOutlined,
  CarOutlined,
  UserOutlined,
  CheckCircleOutlined,
  SafetyCertificateOutlined,
  ArrowLeftOutlined,
} from '@ant-design/icons';
import {
  OrderApiService,
  type OrderDetailResultDto,
  type OrderItemDto,
  type OrderLogDto,
} from '../../../apiServices';

const mockOrderDetail: OrderDetailResultDto = {
  orderId: 'ORD-20260727-8891',
  status: 'paid',
  statusLabel: '已付款，待发货',
  createdAt: '2026-07-27 10:15:30',
  paidAt: '2026-07-27 10:17:02',
  paymentMethod: '支付宝 (Alipay)',
  transactionId: '2026072722001452931488219034',
  totalAmount: 12890.0,
  discountAmount: 400.0,
  freightAmount: 0.0,
  actualAmount: 12490.0,
  buyerName: '周小杰 (Jay)',
  buyerAvatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=Jay',
  buyerAccount: 'jay.zhou@example.com',
  buyerEmail: 'jay.zhou@example.com',
  memberLevel: 'VIP5 黑金会员',
  receiverName: '周小杰',
  receiverPhone: '138****8888',
  receiverAddress: '上海市浦东新区张江高科技园区博云路 2 号 Dedsi 研发大厦 8 楼',
  logisticsCompany: '顺丰速运 (SF Express)',
  trackingNumber: 'SF1409283719283',
  items: [
    {
      key: '1',
      skuCode: 'SKU-MAC-M3-01',
      goodsName: 'Apple MacBook Pro 16 英寸 (M3 Max 64G 1TB)',
      goodsImage: '💻',
      spec: '深空黑 / 64G统一内存 / 1TB SSD',
      unitPrice: 10890.0,
      quantity: 1,
      discountAmount: 300.0,
      totalPrice: 10590.0,
    },
    {
      key: '2',
      skuCode: 'SKU-DISP-4K-02',
      goodsName: 'Dell UltraSharp 27 英寸 4K 98% DCI-P3 显示器',
      goodsImage: '🖥️',
      spec: '4K IPS / Type-C 90W 供电 / 黑色',
      unitPrice: 2000.0,
      quantity: 1,
      discountAmount: 100.0,
      totalPrice: 1900.0,
    },
  ],
  logs: [
    {
      key: '1',
      operator: '买家 (周小杰)',
      action: '提交订单',
      node: '创建订单',
      result: 'success',
      timestamp: '2026-07-27 10:15:30',
      remark: '来自网页端下单',
    },
    {
      key: '2',
      operator: '支付网关',
      action: '完成第三方支付担保',
      node: '订单支付',
      result: 'success',
      timestamp: '2026-07-27 10:17:02',
      remark: '支付宝流水号：2026072722001452931488219034',
    },
  ],
  customerRemark: '请打包时务必加上泡泡防护膜，发货后顺丰送货上门，谢谢！',
  adminRemark: '已安排优先仓储拣货，预计今天 17:00 前安排顺丰揽收。',
};

const primaryTagStyle = {
  color: 'var(--color-primary)',
  background: 'var(--color-primary-light)',
  borderColor: 'var(--color-border)',
};

export function OrderDetail() {
  const [order, setOrder] = useState<OrderDetailResultDto>(mockOrderDetail);
  const [loading, setLoading] = useState<boolean>(false);
  const [remarkText, setRemarkText] = useState<string>('');
  const [isShipModalOpen, setIsShipModalOpen] = useState<boolean>(false);
  const [shipForm] = Form.useForm();

  useEffect(() => {
    async function loadData() {
      setLoading(true);
      try {
        const res = await OrderApiService.getOrderDetail('ORD-20260727-8891');
        if (res && res.data) {
          setOrder(res.data);
        }
      } catch {
        // 保留 mock 演示数据
      } finally {
        setLoading(false);
      }
    }
    loadData();
  }, []);

  const handleShipSubmit = () => {
    shipForm.validateFields().then(async (values) => {
      try {
        await OrderApiService.shipOrder(order.orderId, values.company, values.trackingNumber);
        message.success('订单已成功标记发货！');
      } catch {
        message.success('已更新发货状态 (模拟)');
      } finally {
        setIsShipModalOpen(false);
      }
    });
  };

  const itemColumns: TableProps<OrderItemDto>['columns'] = [
    {
      title: '商品名称',
      dataIndex: 'goodsName',
      key: 'goodsName',
      render: (text: string, record: OrderItemDto) => (
        <span>
          <strong>{text}</strong>
          <br />
          <span style={{ fontSize: 12, color: 'var(--color-muted)' }}>
            {record.spec} (SKU: {record.skuCode})
          </span>
        </span>
      ),
    },
    {
      title: '单价',
      dataIndex: 'unitPrice',
      key: 'unitPrice',
      align: 'right',
      render: (val: number) => `¥ ${val.toFixed(2)}`,
    },
    {
      title: '数量',
      dataIndex: 'quantity',
      key: 'quantity',
      align: 'center',
      render: (qty: number) => <Tag style={primaryTagStyle}>x {qty}</Tag>,
    },
    {
      title: '小计',
      dataIndex: 'totalPrice',
      key: 'totalPrice',
      align: 'right',
      render: (val: number) => <strong>¥ {val.toFixed(2)}</strong>,
    },
  ];

  const logColumns: TableProps<OrderLogDto>['columns'] = [
    {
      title: '操作节点',
      dataIndex: 'node',
      key: 'node',
    },
    {
      title: '动作说明',
      dataIndex: 'action',
      key: 'action',
    },
    {
      title: '操作人',
      dataIndex: 'operator',
      key: 'operator',
      render: (text: string) => <Tag style={primaryTagStyle}>{text}</Tag>,
    },
    {
      title: '处理时间',
      dataIndex: 'timestamp',
      key: 'timestamp',
    },
  ];

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
      {/* 顶部 Header */}
      <Card loading={loading} style={{ borderRadius: 12 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Space size={16}>
            <Button icon={<ArrowLeftOutlined />} style={{ borderRadius: 8 }}>
              返回
            </Button>
            <div>
              <h2 style={{ margin: 0, fontSize: 20, fontWeight: 700, color: 'var(--color-title)' }}>
                订单编号：{order.orderId}
              </h2>
              <span style={{ fontSize: 13, color: 'var(--color-muted)' }}>
                下单时间：{order.createdAt}
              </span>
            </div>
            <Tag color="processing">
              <CheckCircleOutlined /> {order.statusLabel}
            </Tag>
          </Space>

          <Space>
            <Button icon={<PrinterOutlined />} style={{ borderRadius: 8 }}>
              打印面单
            </Button>
            <Button icon={<SendOutlined />} style={{ borderRadius: 8 }}>
              提醒买家
            </Button>
            <Button
              type="primary"
              icon={<CarOutlined />}
              onClick={() => setIsShipModalOpen(true)}
              style={{ borderRadius: 8 }}
            >
              立即发货
            </Button>
          </Space>
        </div>

        <Divider style={{ margin: '16px 0' }} />

        <Row gutter={[24, 16]}>
          <Col xs={12} sm={6}>
            <Statistic title="商品总额" value={order.totalAmount} precision={2} prefix="¥" />
          </Col>
          <Col xs={12} sm={6}>
            <Statistic title="优惠抵扣" value={order.discountAmount} precision={2} prefix="- ¥" valueStyle={{ color: 'var(--color-error)' }} />
          </Col>
          <Col xs={12} sm={6}>
            <Statistic title="配送运费" value="包邮 (¥0.00)" valueStyle={{ color: 'var(--color-success)', fontSize: 16 }} />
          </Col>
          <Col xs={12} sm={6}>
            <Statistic title="实付总额" value={order.actualAmount} precision={2} prefix="¥" valueStyle={{ color: 'var(--color-primary)', fontWeight: 700 }} />
          </Col>
        </Row>
      </Card>

      {/* 主 2:1 排版区块 */}
      <Row gutter={[24, 24]}>
        {/* 左侧主要多表格与多字段 */}
        <Col xs={24} lg={16} style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
          {/* 收货与配送多字段 Descriptions */}
          <Card title="收货与配送物流信息" style={{ borderRadius: 12 }}>
            <Descriptions column={2} bordered size="small">
              <Descriptions.Item label="收货人">{order.receiverName}</Descriptions.Item>
              <Descriptions.Item label="联系电话">{order.receiverPhone}</Descriptions.Item>
              <Descriptions.Item label="快递公司">{order.logisticsCompany}</Descriptions.Item>
              <Descriptions.Item label="快递单号">
                <Tag style={primaryTagStyle}>{order.trackingNumber}</Tag>
              </Descriptions.Item>
              <Descriptions.Item label="收货地址" span={2}>
                {order.receiverAddress}
              </Descriptions.Item>
            </Descriptions>

            <div style={{ fontWeight: 600, marginBottom: 12, marginTop: 12 }}>物流轨迹节点：</div>
            <Timeline
              items={[
                { color: 'var(--color-success)', children: '顺丰速运已揽收，准备分拨发货 (2026-07-27 11:30)' },
                { color: 'var(--color-info)', children: '买家支付成功，订单已进入仓储拣货流程 (2026-07-27 10:17)' },
                { color: 'var(--color-muted)', children: '买家提交订单成功 (2026-07-27 10:15)' },
              ]}
            />
          </Card>

          {/* 表格 1: 商品明细 */}
          <Card title={`商品明细列表 (${order.items.length} 件商品)`} style={{ borderRadius: 12 }}>
            <Table columns={itemColumns} dataSource={order.items} pagination={false} size="middle" />
          </Card>

          {/* 表格 2: 履历与审计日志 */}
          <Card title="订单操作节点履历日志" style={{ borderRadius: 12 }}>
            <Table columns={logColumns} dataSource={order.logs} pagination={false} size="small" />
          </Card>
        </Col>

        {/* 右侧边栏多字段展示 */}
        <Col xs={24} lg={8} style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
          <Card title="买家客户档案" style={{ borderRadius: 12 }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 16 }}>
              <Avatar size={48} src={order.buyerAvatar} icon={<UserOutlined />} />
              <div>
                <div style={{ fontWeight: 700, fontSize: 16 }}>{order.buyerName}</div>
                <Tag style={primaryTagStyle}>{order.memberLevel}</Tag>
              </div>
            </div>
            <Descriptions column={1} size="small" bordered>
              <Descriptions.Item label="客户账号">{order.buyerAccount}</Descriptions.Item>
              <Descriptions.Item label="电子邮箱">{order.buyerEmail}</Descriptions.Item>
              <Descriptions.Item label="信用评级">
                <Tag color="success"><SafetyCertificateOutlined /> A+ 极佳</Tag>
              </Descriptions.Item>
            </Descriptions>
          </Card>

          <Card title="支付与结算卡片" style={{ borderRadius: 12 }}>
            <Descriptions column={1} size="small" bordered>
              <Descriptions.Item label="支付方式">{order.paymentMethod}</Descriptions.Item>
              <Descriptions.Item label="支付时间">{order.paidAt}</Descriptions.Item>
              <Descriptions.Item label="交易流水">{order.transactionId}</Descriptions.Item>
            </Descriptions>
          </Card>

          <Card title="客服沟通与处理备注" style={{ borderRadius: 12 }}>
            <div style={{ marginBottom: 12 }}>
              <span style={{ color: 'var(--color-muted)', fontSize: 13 }}>买家留言：</span>
              <div style={{ padding: 8, background: 'var(--color-warning-light)', border: '1px solid var(--color-warning-border)', borderRadius: 6, fontSize: 13, marginTop: 4 }}>
                {order.customerRemark}
              </div>
            </div>
            <Input.TextArea
              rows={3}
              placeholder="内部备注..."
              value={remarkText || order.adminRemark}
              onChange={(e) => setRemarkText(e.target.value)}
              style={{ borderRadius: 8, marginBottom: 8 }}
            />
            <Button size="small" onClick={() => message.success('客服备注已保存')}>
              保存备注
            </Button>
          </Card>
        </Col>
      </Row>

      {/* 发货 Modal */}
      <Modal
        title="订单发货处理"
        open={isShipModalOpen}
        onOk={handleShipSubmit}
        onCancel={() => setIsShipModalOpen(false)}
        okText="确认发货"
        cancelText="取消"
        style={{ borderRadius: 12 }}
      >
        <Form form={shipForm} layout="vertical" initialValues={{ company: '顺丰速运 (SF Express)', trackingNumber: 'SF1409283719283' }} style={{ marginTop: 16 }}>
          <Form.Item name="company" label="快递公司" rules={[{ required: true }]}>
            <Select
              options={[
                { value: '顺丰速运 (SF Express)', label: '顺丰速运 (SF Express)' },
                { value: '京东物流 (JD Logistics)', label: '京东物流 (JD Logistics)' },
              ]}
            />
          </Form.Item>
          <Form.Item name="trackingNumber" label="快递单号" rules={[{ required: true }]}>
            <Input placeholder="请输入快递单号" style={{ borderRadius: 8 }} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}

export default OrderDetail;
