import { useMemo, useState } from 'react';
import {
  Alert,
  Avatar,
  Badge,
  Card,
  Col,
  Descriptions,
  Divider,
  Empty,
  Input,
  Row,
  Space,
  Tag,
  Typography,
} from 'antd';
import {
  ApartmentOutlined,
  CheckCircleOutlined,
  CopyOutlined,
  IdcardOutlined,
  MailOutlined,
  SafetyCertificateOutlined,
  SearchOutlined,
  UserOutlined,
} from '@ant-design/icons';
import type {
  LoginUserPositionResultDto,
  LoginUserResultDto,
} from '../../apiServices';
import styles from './index.module.css';

const { Paragraph, Text, Title } = Typography;

/** 从 localstorage 读取 current_user 信息 */
function getStoredCurrentUser(): LoginUserResultDto | null {
  try {
    const raw = localStorage.getItem('current_user');
    if (!raw) return null;
    const parsed = JSON.parse(raw) as Partial<LoginUserResultDto>;
    if (!parsed || typeof parsed !== 'object') return null;

    return {
      id: typeof parsed.id === 'string' ? parsed.id : '',
      name: typeof parsed.name === 'string' ? parsed.name : '',
      email: typeof parsed.email === 'string' ? parsed.email : '',
      account: typeof parsed.account === 'string' ? parsed.account : '',
      permissions: Array.isArray(parsed.permissions)
        ? parsed.permissions.filter((p): p is string => typeof p === 'string')
        : [],
      positions: Array.isArray(parsed.positions)
        ? parsed.positions.map((pos: Partial<LoginUserPositionResultDto>) => ({
            positionId: typeof pos.positionId === 'string' ? pos.positionId : '',
            positionName: typeof pos.positionName === 'string' ? pos.positionName : '',
          }))
        : [],
    };
  } catch {
    return null;
  }
}

/**
 * 个人中心页面组件
 * 从 localStorage current_user 读取当前登录用户信息并使用两个独立卡片展示所属岗位与有效权限
 */
export default function ProfilePage() {
  const [currentUser] = useState<LoginUserResultDto | null>(() => getStoredCurrentUser());
  const [permissionKeyword, setPermissionKeyword] = useState('');

  // 过滤权限列表
  const filteredPermissions = useMemo(() => {
    if (!currentUser?.permissions) return [];
    if (!permissionKeyword.trim()) return currentUser.permissions;
    const kw = permissionKeyword.trim().toLowerCase();
    return currentUser.permissions.filter((p) => p.toLowerCase().includes(kw));
  }, [currentUser?.permissions, permissionKeyword]);

  if (!currentUser) {
    return (
      <main className={styles.page}>
        <Alert
          type="warning"
          showIcon
          message="未检测到当前登录用户信息"
          description="本地缓存中未找到有效用户资料，请确认您已成功登录系统。"
        />
      </main>
    );
  }

  const avatarChar = (currentUser.name || currentUser.account || '用').charAt(0).toUpperCase();

  return (
    <main className={styles.page}>
      {/* 用户基本信息卡片 */}
      <Card className={styles.profileCard}>
        <div className={styles.headerRow}>
          <div className={styles.avatarWrapper}>
            <Avatar size={72} icon={<UserOutlined />} className={styles.avatar}>
              {avatarChar}
            </Avatar>
            <div className={styles.userInfo}>
              <div className={styles.nameRow}>
                <Title level={3} className={styles.userName}>
                  {currentUser.name || '未命名用户'}
                </Title>
                <Tag color="success">当前登录账号</Tag>
              </div>
              <Space orientation="horizontal" size="middle" className={styles.metaRow}>
                <Text type="secondary">
                  <IdcardOutlined /> {currentUser.account}
                </Text>
                {currentUser.email && (
                  <Text type="secondary">
                    <MailOutlined /> {currentUser.email}
                  </Text>
                )}
              </Space>
            </div>
          </div>
        </div>

        <Divider className={styles.divider} />

        <Descriptions
          bordered
          column={{ xs: 1, sm: 2, md: 3 }}
          size="small"
          className={styles.descriptions}
        >
          <Descriptions.Item label="姓名">{currentUser.name || '-'}</Descriptions.Item>
          <Descriptions.Item label="登录账号">{currentUser.account || '-'}</Descriptions.Item>
          <Descriptions.Item label="电子邮箱">{currentUser.email || '-'}</Descriptions.Item>
          <Descriptions.Item label="用户标识">
            <Text code copyable={{ text: currentUser.id }}>
              {currentUser.id || '-'}
            </Text>
          </Descriptions.Item>
          <Descriptions.Item label="所属岗位数">
            <Badge count={currentUser.positions?.length ?? 0} showZero color="#1e293b" />
          </Descriptions.Item>
          <Descriptions.Item label="权限条目数">
            <Badge count={currentUser.permissions?.length ?? 0} showZero color="#0284c7" />
          </Descriptions.Item>
        </Descriptions>
      </Card>

      {/* 两个卡片展示：所属岗位与有效权限各独占一行 */}
      <Row gutter={[24, 24]}>
        {/* 卡片 1：所属岗位信息 */}
        <Col span={24}>
          <Card
            className={styles.card}
            title={
              <Space>
                <ApartmentOutlined className={styles.cardIcon} />
                <span>所属岗位</span>
                <Tag color="blue">{currentUser.positions?.length ?? 0}</Tag>
              </Space>
            }
          >
            {currentUser.positions && currentUser.positions.length > 0 ? (
              <div className={styles.positionGrid}>
                {currentUser.positions.map((pos, index) => (
                  <div key={pos.positionId || index} className={styles.positionCard}>
                    <div className={styles.positionBadge}>
                      <ApartmentOutlined />
                    </div>
                    <div className={styles.positionContent}>
                      <Text strong className={styles.positionTitle}>
                        {pos.positionName || '未命名岗位'}
                      </Text>
                      <Text type="secondary" className={styles.positionDesc}>
                        已授权岗位
                      </Text>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <Empty
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description="当前账户暂无分配岗位"
                className={styles.empty}
              />
            )}
          </Card>
        </Col>

        {/* 卡片 2：权限列表信息 */}
        <Col span={24}>
          <Card
            className={styles.card}
            title={
              <Space>
                <SafetyCertificateOutlined className={styles.cardIcon} />
                <span>有效权限</span>
                <Tag color="cyan">{currentUser.permissions?.length ?? 0}</Tag>
              </Space>
            }
            extra={
              currentUser.permissions && currentUser.permissions.length > 0 ? (
                <Input
                  allowClear
                  placeholder="搜索权限编码..."
                  prefix={<SearchOutlined />}
                  value={permissionKeyword}
                  onChange={(e) => setPermissionKeyword(e.target.value)}
                  size="small"
                  className={styles.searchInput}
                />
              ) : null
            }
          >
            {filteredPermissions.length > 0 ? (
              <div className={styles.permissionTagsContainer}>
                {filteredPermissions.map((permission) => (
                  <Tag
                    key={permission}
                    color="geekblue"
                    icon={<CheckCircleOutlined />}
                    className={styles.permissionTag}
                  >
                    <span className={styles.permissionTagText}>{permission}</span>
                    <Paragraph
                      copyable={{
                        text: permission,
                        icon: [<CopyOutlined key="copy" className={styles.copyIcon} />],
                      }}
                      className={styles.copyWrapper}
                    />
                  </Tag>
                ))}
              </div>
            ) : (
              <Empty
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description={
                  permissionKeyword ? '没有匹配的权限项' : '当前账户暂无有效权限'
                }
                className={styles.empty}
              />
            )}
          </Card>
        </Col>
      </Row>
    </main>
  );
}
