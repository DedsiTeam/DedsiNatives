import React from 'react';
import { Card, Row, Col, Statistic, Table, Tag, Button } from 'antd';
import {
  UserOutlined,
  ShoppingOutlined,
  PayCircleOutlined,
  RiseOutlined,
  PlusOutlined,
  ArrowUpOutlined,
} from '@ant-design/icons';

const mockData = [
  { key: '1', name: '全自动订单处理', status: '成功', amount: '¥ 12,400', date: '2026-07-27 13:45' },
  { key: '2', name: '云服务节点伸缩', status: '处理中', amount: '¥ 8,200', date: '2026-07-27 12:30' },
  { key: '3', name: '数据库每日备份', status: '成功', amount: '¥ 0', date: '2026-07-27 04:00' },
  { key: '4', name: '多租户安全审计', status: '预警', amount: '¥ 3,500', date: '2026-07-26 19:10' },
];

const columns = [
  { title: '任务/事件', dataIndex: 'name', key: 'name' },
  {
    title: '状态',
    dataIndex: 'status',
    key: 'status',
    render: (status: string) => {
      let color = 'blue';
      if (status === '成功') color = 'green';
      if (status === '预警') color = 'gold';
      return <Tag color={color}>{status}</Tag>;
    },
  },
  { title: '交易额', dataIndex: 'amount', key: 'amount' },
  { title: '更新时间', dataIndex: 'date', key: 'date' },
];

export const Dashboard: React.FC = () => {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
      {/* 顶部 Welcome Header 卡片 */}
      <Card
        style={{
          borderRadius: 12,
          background: 'linear-gradient(135deg, #ffffff 0%, #f0f5ff 100%)',
          border: '1px solid #e5e7eb',
        }}
      >
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div>
            <h2 style={{ color: '#111827', margin: 0, fontSize: 22, fontWeight: 700 }}>
              欢迎回来，超级管理员 👋
            </h2>
            <p style={{ color: '#6b7280', marginTop: 4, margin: 0 }}>
              Dedsi React Admin 控制台已准备就绪，今日系统运行状态良好。
            </p>
          </div>
          <Button
            type="primary"
            icon={<PlusOutlined />}
            size="large"
            style={{ borderRadius: 8, backgroundColor: '#315efb' }}
          >
            新建业务项目
          </Button>
        </div>
      </Card>

      {/* 指标卡片 Row */}
      <Row gutter={[24, 24]}>
        <Col xs={24} sm={12} lg={6}>
          <Card style={{ borderRadius: 12 }} bordered={true}>
            <Statistic
              title={<span style={{ color: '#6b7280' }}>总用户数</span>}
              value={24890}
              prefix={<UserOutlined style={{ color: '#315efb', marginRight: 8 }} />}
              suffix={<span style={{ fontSize: 12, color: '#16a34a' }}><ArrowUpOutlined /> +12%</span>}
            />
          </Card>
        </Col>

        <Col xs={24} sm={12} lg={6}>
          <Card style={{ borderRadius: 12 }}>
            <Statistic
              title={<span style={{ color: '#6b7280' }}>今日总收入</span>}
              value={98450}
              precision={2}
              prefix={<PayCircleOutlined style={{ color: '#8b31fb', marginRight: 8 }} />}
              suffix={<span style={{ fontSize: 12, color: '#16a34a' }}><ArrowUpOutlined /> +8.5%</span>}
            />
          </Card>
        </Col>

        <Col xs={24} sm={12} lg={6}>
          <Card style={{ borderRadius: 12 }}>
            <Statistic
              title={<span style={{ color: '#6b7280' }}>有效订单</span>}
              value={1420}
              prefix={<ShoppingOutlined style={{ color: '#315efb', marginRight: 8 }} />}
              suffix={<span style={{ fontSize: 12, color: '#16a34a' }}><ArrowUpOutlined /> +5.2%</span>}
            />
          </Card>
        </Col>

        <Col xs={24} sm={12} lg={6}>
          <Card style={{ borderRadius: 12 }}>
            <Statistic
              title={<span style={{ color: '#6b7280' }}>转化率</span>}
              value={32.8}
              precision={1}
              suffix="%"
              prefix={<RiseOutlined style={{ color: '#8b31fb', marginRight: 8 }} />}
            />
          </Card>
        </Col>
      </Row>

      {/* 业务表格数据展示 */}
      <Card
        title={<span style={{ color: '#111827', fontWeight: 600 }}>最近系统事件与交易</span>}
        style={{ borderRadius: 12 }}
      >
        <Table dataSource={mockData} columns={columns} pagination={false} size="middle" />
      </Card>
    </div>
  );
};

export default Dashboard;
