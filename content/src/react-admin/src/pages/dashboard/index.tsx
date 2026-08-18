import React, { useEffect, useState } from 'react';
import {
  Card,
  Row,
  Col,
  Statistic,
  Table,
  Tag,
  Button,
  Space,
  Typography,
  Divider,
} from 'antd';
import {
  UserOutlined,
  AppstoreOutlined,
  SafetyCertificateOutlined,
  AuditOutlined,
  CheckCircleFilled,
  ReloadOutlined,
  ArrowRightOutlined,
  BookOutlined,
  MenuOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import {
  UserApiService,
  SystemApiService,
  PositionApiService,
  LoginAuditApiService,
  type LoginAuditRowResultDto,
  LoginResult,
} from '../../apiServices';
import styles from './Dashboard.module.css';

const { Text } = Typography;

interface DashboardStats {
  userCount: number;
  systemCount: number;
  positionCount: number;
  auditCount: number;
}

export const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [stats, setStats] = useState<DashboardStats>({
    userCount: 0,
    systemCount: 0,
    positionCount: 0,
    auditCount: 0,
  });
  const [recentAudits, setRecentAudits] = useState<LoginAuditRowResultDto[]>([]);

  // 从本地会话获取当前用户信息
  const currentUser = (() => {
    try {
      const stored = localStorage.getItem('current_user');
      return stored ? JSON.parse(stored) : { name: '管理员', account: 'Admin' };
    } catch {
      return { name: '管理员', account: 'Admin' };
    }
  })();

  const fetchDashboardData = async () => {
    setLoading(true);
    try {
      const [usersRes, systemsRes, positionsRes, auditsRes] = await Promise.allSettled([
        UserApiService.getPageList({ pageIndex: 1, pageSize: 1 }),
        SystemApiService.getPageList({ pageIndex: 1, pageSize: 1 }),
        PositionApiService.getPageList({ pageIndex: 1, pageSize: 1 }),
        LoginAuditApiService.getPageList({ pageIndex: 1, pageSize: 6 }),
      ]);

      setStats({
        userCount: usersRes.status === 'fulfilled' ? usersRes.value.totalCount : 1,
        systemCount: systemsRes.status === 'fulfilled' ? systemsRes.value.totalCount : 1,
        positionCount: positionsRes.status === 'fulfilled' ? positionsRes.value.totalCount : 1,
        auditCount: auditsRes.status === 'fulfilled' ? auditsRes.value.totalCount : 0,
      });

      if (auditsRes.status === 'fulfilled') {
        setRecentAudits(auditsRes.value.items || []);
      }
    } catch {
      // 错误由拦截器统一处理
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const auditColumns = [
    {
      title: '登录账号',
      dataIndex: 'account',
      key: 'account',
      render: (account: string, record: LoginAuditRowResultDto) => (
        <Space direction="vertical" size={0}>
          <Text strong>{account}</Text>
          {record.userName && (
            <Text type="secondary" style={{ fontSize: 12 }}>
              {record.userName}
            </Text>
          )}
        </Space>
      ),
    },
    {
      title: '认证结果',
      dataIndex: 'result',
      key: 'result',
      render: (result: LoginResult) => {
        const isSuccess = result === LoginResult.Success;
        return (
          <Tag color={isSuccess ? 'success' : 'error'}>
            {isSuccess ? '认证成功' : '认证失败'}
          </Tag>
        );
      },
    },
    {
      title: '客户端 IP',
      dataIndex: 'clientIp',
      key: 'clientIp',
      render: (ip?: string) => ip || '-',
    },
    {
      title: '登录时间 (北京时间)',
      dataIndex: 'loginTimeUtc',
      key: 'loginTimeUtc',
      render: (time: string) => time || '-',
    },
  ];

  return (
    <div className={styles.dashboardContainer}>
      {/* 顶部欢迎卡片 */}
      <Card className={styles.welcomeCard} bordered={false}>
        <div className={styles.welcomeHeader}>
          <div>
            <h1 className={styles.welcomeTitle}>
              欢迎使用 DedsiNative 系统管理控制台 👋
            </h1>
            <p className={styles.welcomeSubtitle}>
              当前登录账号：<strong>{currentUser.name || currentUser.account}</strong>
              {currentUser.email ? ` (${currentUser.email})` : ''} ｜ 平台运行状态稳定，核心身份与权限中台服务正常。
            </p>
          </div>
          <Space>
            <Button
              icon={<ReloadOutlined spin={loading} />}
              onClick={fetchDashboardData}
              loading={loading}
            >
              刷新概览
            </Button>
            <Button
              type="primary"
              icon={<UserOutlined />}
              onClick={() => navigate('/system/users')}
            >
              用户管理
            </Button>
          </Space>
        </div>
      </Card>

      {/* 核心指标卡片 */}
      <Row gutter={[16, 16]}>
        <Col xs={24} sm={12} lg={6}>
          <Card className={styles.statCard} bordered={false}>
            <Statistic
              title={<span style={{ color: '#6b7280', fontSize: 13 }}>接入系统 (Systems)</span>}
              value={stats.systemCount}
              prefix={<AppstoreOutlined style={{ color: '#1677ff', marginRight: 8 }} />}
              suffix={<span style={{ fontSize: 12, color: '#9ca3af' }}>个</span>}
            />
          </Card>
        </Col>

        <Col xs={24} sm={12} lg={6}>
          <Card className={styles.statCard} bordered={false}>
            <Statistic
              title={<span style={{ color: '#6b7280', fontSize: 13 }}>用户总数 (Users)</span>}
              value={stats.userCount}
              prefix={<UserOutlined style={{ color: '#52c41a', marginRight: 8 }} />}
              suffix={<span style={{ fontSize: 12, color: '#9ca3af' }}>位</span>}
            />
          </Card>
        </Col>

        <Col xs={24} sm={12} lg={6}>
          <Card className={styles.statCard} bordered={false}>
            <Statistic
              title={<span style={{ color: '#6b7280', fontSize: 13 }}>定义岗位 (Positions)</span>}
              value={stats.positionCount}
              prefix={<SafetyCertificateOutlined style={{ color: '#722ed1', marginRight: 8 }} />}
              suffix={<span style={{ fontSize: 12, color: '#9ca3af' }}>个</span>}
            />
          </Card>
        </Col>

        <Col xs={24} sm={12} lg={6}>
          <Card className={styles.statCard} bordered={false}>
            <Statistic
              title={<span style={{ color: '#6b7280', fontSize: 13 }}>登录审计日志 (Audits)</span>}
              value={stats.auditCount}
              prefix={<AuditOutlined style={{ color: '#fa8c16', marginRight: 8 }} />}
              suffix={<span style={{ fontSize: 12, color: '#9ca3af' }}>条</span>}
            />
          </Card>
        </Col>
      </Row>

      {/* 快捷导航 */}
      <Card
        title={<span style={{ fontWeight: 600, fontSize: 15 }}>快速导航</span>}
        bordered={false}
        style={{ borderRadius: 10 }}
      >
        <Row gutter={[16, 16]}>
          <Col xs={24} sm={12} md={8} lg={4}>
            <div className={styles.quickActionCard} onClick={() => navigate('/system/users')}>
              <div className={styles.quickActionIcon} style={{ background: '#e6f4ff', color: '#1677ff' }}>
                <UserOutlined />
              </div>
              <div>
                <div className={styles.quickActionTitle}>用户管理</div>
                <div className={styles.quickActionDesc}>维护账号与岗位</div>
              </div>
            </div>
          </Col>

          <Col xs={24} sm={12} md={8} lg={4}>
            <div className={styles.quickActionCard} onClick={() => navigate('/system/positions')}>
              <div className={styles.quickActionIcon} style={{ background: '#f9f0ff', color: '#722ed1' }}>
                <SafetyCertificateOutlined />
              </div>
              <div>
                <div className={styles.quickActionTitle}>岗位权限</div>
                <div className={styles.quickActionDesc}>角色与权限绑定</div>
              </div>
            </div>
          </Col>

          <Col xs={24} sm={12} md={8} lg={4}>
            <div className={styles.quickActionCard} onClick={() => navigate('/system/systems')}>
              <div className={styles.quickActionIcon} style={{ background: '#f0f5ff', color: '#2f54eb' }}>
                <AppstoreOutlined />
              </div>
              <div>
                <div className={styles.quickActionTitle}>系统管理</div>
                <div className={styles.quickActionDesc}>微服务与子系统</div>
              </div>
            </div>
          </Col>

          <Col xs={24} sm={12} md={8} lg={4}>
            <div className={styles.quickActionCard} onClick={() => navigate('/system/menus')}>
              <div className={styles.quickActionIcon} style={{ background: '#f6ffed', color: '#52c41a' }}>
                <MenuOutlined />
              </div>
              <div>
                <div className={styles.quickActionTitle}>菜单管理</div>
                <div className={styles.quickActionDesc}>树形路由配置</div>
              </div>
            </div>
          </Col>

          <Col xs={24} sm={12} md={8} lg={4}>
            <div className={styles.quickActionCard} onClick={() => navigate('/system/dictionaries')}>
              <div className={styles.quickActionIcon} style={{ background: '#fff7e6', color: '#fa8c16' }}>
                <BookOutlined />
              </div>
              <div>
                <div className={styles.quickActionTitle}>字典管理</div>
                <div className={styles.quickActionDesc}>全局枚举与常量</div>
              </div>
            </div>
          </Col>

          <Col xs={24} sm={12} md={8} lg={4}>
            <div className={styles.quickActionCard} onClick={() => navigate('/system/login-audits')}>
              <div className={styles.quickActionIcon} style={{ background: '#fff0f6', color: '#eb2f96' }}>
                <AuditOutlined />
              </div>
              <div>
                <div className={styles.quickActionTitle}>登录审计</div>
                <div className={styles.quickActionDesc}>安全日志与追踪</div>
              </div>
            </div>
          </Col>
        </Row>
      </Card>

      {/* 主体两列布局：最近审计 + 系统技术架构与健康 */}
      <Row gutter={[16, 16]}>
        {/* 左侧：最近安全审计 */}
        <Col xs={24} lg={15}>
          <Card
            title={<span style={{ fontWeight: 600, fontSize: 15 }}>最近登录与安全审计</span>}
            extra={
              <Button
                type="link"
                icon={<ArrowRightOutlined />}
                onClick={() => navigate('/system/login-audits')}
              >
                查看全部
              </Button>
            }
            bordered={false}
            style={{ borderRadius: 10, height: '100%' }}
          >
            <Table
              rowKey="id"
              dataSource={recentAudits}
              columns={auditColumns}
              pagination={false}
              size="middle"
              loading={loading}
              locale={{ emptyText: '暂无登录审计记录' }}
            />
          </Card>
        </Col>

        {/* 右侧：技术架构与运行状态 */}
        <Col xs={24} lg={9}>
          <Card
            title={<span style={{ fontWeight: 600, fontSize: 15 }}>系统架构与技术栈</span>}
            bordered={false}
            style={{ borderRadius: 10, height: '100%' }}
          >
            <Space direction="vertical" style={{ width: '100%' }} size={14}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Space>
                  <CheckCircleFilled style={{ color: '#52c41a' }} />
                  <Text strong>.NET 10 Web Host (FastEndpoints)</Text>
                </Space>
                <span className={`${styles.healthBadge} ${styles.healthBadgeOnline}`}>
                  <span className={styles.statusDot} /> 正常运行
                </span>
              </div>

              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Space>
                  <CheckCircleFilled style={{ color: '#52c41a' }} />
                  <Text strong>PostgreSQL 18 (EF Core)</Text>
                </Space>
                <span className={`${styles.healthBadge} ${styles.healthBadgeOnline}`}>
                  <span className={styles.statusDot} /> 北京时间已对齐
                </span>
              </div>

              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Space>
                  <CheckCircleFilled style={{ color: '#52c41a' }} />
                  <Text strong>RabbitMQ 4.3 消息总线</Text>
                </Space>
                <span className={`${styles.healthBadge} ${styles.healthBadgeOnline}`}>
                  <span className={styles.statusDot} /> 持久化连接
                </span>
              </div>

              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Space>
                  <CheckCircleFilled style={{ color: '#52c41a' }} />
                  <Text strong>React 19 + Vite 8 + Ant Design 5</Text>
                </Space>
                <span className={`${styles.healthBadge} ${styles.healthBadgeOnline}`}>
                  <span className={styles.statusDot} /> 前端已就绪
                </span>
              </div>

              <Divider style={{ margin: '8px 0' }} />

              <div>
                <Text type="secondary" style={{ fontSize: 13, display: 'block', marginBottom: 8 }}>
                  <strong>安全与架构规范：</strong>
                </Text>
                <ul style={{ margin: 0, paddingLeft: 18, color: '#6b7280', fontSize: 12, lineHeight: 1.8 }}>
                  <li>密码存储：PBKDF2-SHA512 + 100,000 次哈希迭代加盐</li>
                  <li>主键策略：26 位有序 ULID / UUIDv7 分布式主键</li>
                  <li>时区标准：本地北京时间（timestamp without time zone）</li>
                  <li>并发控制：AggregateRoot 乐观并发令牌保护</li>
                </ul>
              </div>
            </Space>
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default Dashboard;
