/**
 * @file SSO 用户授权记录页面 (SsoAuthorizations)
 * @description 直连 OpenIddictApiService 与对应 DTO 类型。
 * 基于通用的 CrudToolbar / CrudTable / useCrudTable / CopyableIdTag 组件实现标准化只读与吊销列表布局。
 */

import { useState, useMemo } from 'react';
import {
  Button,
  Popconfirm,
  Space,
  Tag,
  message,
  Typography,
  type TableProps,
} from 'antd';
import {
  ReloadOutlined,
  StopOutlined,
} from '@ant-design/icons';
import {
  OpenIddictApiService,
  type OpenIddictAuthorizationRowResultDto,
} from '../../../apiServices';
import {
  CopyableIdTag,
  CrudTable,
  CrudToolbar,
  useCrudTable,
} from '../../../components';
import styles from '../sso.module.css';

const { Text } = Typography;

export default function SsoAuthorizations() {
  // 1. 查询筛选状态
  const [draftSubject, setDraftSubject] = useState('');
  const [subject, setSubject] = useState('');

  // 2. 通用 CRUD Hook 接管分页与数据加载
  const filters = useMemo(() => ({ subject: subject || undefined }), [subject]);

  const {
    items,
    loading,
    pagination,
    loadData,
  } = useCrudTable<OpenIddictAuthorizationRowResultDto, { subject?: string }>({
    fetchApi: OpenIddictApiService.getAuthorizationPageList,
    deleteApi: OpenIddictApiService.revokeAuthorization,
    filters,
  });

  const handleRevokeAuth = async (id: string) => {
    try {
      await OpenIddictApiService.revokeAuthorization(id);
      message.success('已吊销该用户授权');
      await loadData();
    } catch {
      message.error('吊销授权失败');
    }
  };

  // 3. 标准 Antd Table 列定义
  const columns: TableProps<OpenIddictAuthorizationRowResultDto>['columns'] = [
    {
      title: '用户标识 (Subject)',
      dataIndex: 'subject',
      key: 'subject',
      width: 240,
      render: (val) => <CopyableIdTag id={val ?? ''} label="用户 ID" />,
    },
    {
      title: '授权客户端应用',
      dataIndex: 'clientId',
      key: 'clientId',
      render: (val, record) => (
        <Space direction="vertical" size={2}>
          <Text strong style={{ color: 'var(--color-title)' }}>
            {record.applicationDisplayName ?? val}
          </Text>
          <Text type="secondary" style={{ fontSize: 12 }}>{val}</Text>
        </Space>
      ),
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
      width: 120,
      render: (val) => (
        <Tag color={val === 'valid' ? 'green' : 'red'}>
          {val === 'valid' ? '有效 (Valid)' : '已吊销 (Revoked)'}
        </Tag>
      ),
    },
    {
      title: '授权作用域 (Scopes)',
      dataIndex: 'scopes',
      key: 'scopes',
      render: (scopes: string[]) => (
        scopes && scopes.length > 0 ? (
          <Space wrap size={4}>
            {scopes.map((s) => <Tag key={s} color="blue">{s}</Tag>)}
          </Space>
        ) : <Text type="secondary">-</Text>
      ),
    },
    {
      title: '授权创建时间',
      dataIndex: 'creationDate',
      key: 'creationDate',
      width: 180,
      render: (val) => val ? new Date(val).toLocaleString() : '-',
    },
    {
      title: '操作',
      key: 'action',
      width: 110,
      fixed: 'right',
      render: (_, record) => (
        record.status === 'valid' ? (
          <Popconfirm
            title="确定要吊销此用户对该应用的授权吗？"
            description="吊销后用户需重新确认授权。"
            onConfirm={() => void handleRevokeAuth(record.id)}
            okText="确定吊销"
            cancelText="取消"
            okButtonProps={{ danger: true }}
          >
            <Button type="text" size="small" danger icon={<StopOutlined />} style={{ fontWeight: 500 }}>
              吊销
            </Button>
          </Popconfirm>
        ) : <Text type="secondary">已失效</Text>
      ),
    },
  ];

  return (
    <div className={styles.pageContainer}>
      {/* 1. 顶部检索工具栏 */}
      <CrudToolbar
        searchPlaceholder="按用户 Subject 搜索..."
        searchValue={draftSubject}
        onSearchChange={setDraftSubject}
        onSearch={() => setSubject(draftSubject.trim())}
        onReset={() => {
          setDraftSubject('');
          setSubject('');
        }}
        extraActions={
          <Button icon={<ReloadOutlined />} onClick={() => void loadData()} style={{ borderRadius: 'var(--radius-btn)' }}>
            刷新
          </Button>
        }
      />

      {/* 2. 数据表格 */}
      <CrudTable<OpenIddictAuthorizationRowResultDto>
        rowKey="id"
        columns={columns}
        dataSource={items}
        loading={loading}
        pagination={pagination}
        emptyText="暂无用户授权记录"
        scroll={{ x: 800 }}
      />
    </div>
  );
}
