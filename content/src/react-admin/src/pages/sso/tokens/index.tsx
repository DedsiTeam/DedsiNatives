/**
 * @file SSO 活跃令牌审计页面 (SsoTokens)
 * @description 直连 OpenIddictApiService 与对应 DTO 类型。
 * 基于通用的 CrudToolbar / CrudTable / useCrudTable / CopyableIdTag 组件实现标准化只读与吊销列表布局。
 */

import { useState, useMemo } from 'react';
import {
  Button,
  Popconfirm,
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
  type OpenIddictTokenRowResultDto,
} from '../../../apiServices';
import {
  CopyableIdTag,
  CrudTable,
  CrudToolbar,
  useCrudTable,
} from '../../../components';
import styles from '../sso.module.css';

const { Text } = Typography;

import { checkPermission } from '../../../components/Auth';
import { PERMISSIONS } from '../../../auth/permissions';

export default function SsoTokens() {
  const canManage = checkPermission(PERMISSIONS.openiddict.manage);
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
  } = useCrudTable<OpenIddictTokenRowResultDto, { subject?: string }>({
    fetchApi: OpenIddictApiService.getTokenPageList,
    deleteApi: OpenIddictApiService.revokeToken,
    filters,
  });

  const handleRevokeToken = async (id: string) => {
    try {
      await OpenIddictApiService.revokeToken(id);
      message.success('已强制吊销该令牌');
      await loadData();
    } catch {
      message.error('吊销令牌失败');
    }
  };

  // 3. 标准 Antd Table 列定义
  const columns: TableProps<OpenIddictTokenRowResultDto>['columns'] = [
    {
      title: '令牌标识 (Token ID)',
      dataIndex: 'id',
      key: 'id',
      width: 240,
      render: (val) => <CopyableIdTag id={val} label="Token ID" />,
    },
    {
      title: '所属客户端',
      dataIndex: 'clientId',
      key: 'clientId',
      width: 180,
      render: (val) => <Tag color="geekblue">{val ?? '-'}</Tag>,
    },
    {
      title: '用户标识 (Subject)',
      dataIndex: 'subject',
      key: 'subject',
      width: 200,
      render: (val) => (val ? <Text code>{val}</Text> : <Text type="secondary">-</Text>),
    },
    {
      title: '令牌类型',
      dataIndex: 'type',
      key: 'type',
      width: 300,
      render: (val) => <Tag color="purple">{val}</Tag>,
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
      width: 110,
      render: (val) => (
        <Tag color={val === 'valid' ? 'green' : 'red'}>
          {val === 'valid' ? '有效' : '已废止'}
        </Tag>
      ),
    },
    {
      title: '签发时间',
      dataIndex: 'creationDate',
      key: 'creationDate',
      width: 170,
      render: (val) => (val ? new Date(val).toLocaleString() : '-'),
    },
    {
      title: '过期时间',
      dataIndex: 'expirationDate',
      key: 'expirationDate',
      width: 170,
      render: (val) => (val ? new Date(val).toLocaleString() : '-'),
    },
    {
      title: '操作',
      key: 'action',
      width: 110,
      fixed: 'right',
      render: (_, record) => (
        record.status === 'valid' ? (
          <Popconfirm
            title="确定要强制吊销此令牌吗？"
            description="吊销后使用该令牌的会话将立即失效并被强制退出。"
            onConfirm={() => void handleRevokeToken(record.id)}
            okText="强制吊销"
            cancelText="取消"
            okButtonProps={{ danger: true }}
          >
            <Button type="text" size="small" danger icon={<StopOutlined />} hidden={!canManage} style={{ fontWeight: 500 }}>
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
      <CrudTable<OpenIddictTokenRowResultDto>
        rowKey="id"
        columns={columns}
        dataSource={items}
        loading={loading}
        pagination={pagination}
        emptyText="暂无活跃令牌记录"
        scroll={{ x: 900 }}
      />
    </div>
  );
}
