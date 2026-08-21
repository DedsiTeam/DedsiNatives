/**
 * @file SSO 客户端应用管理页面 (SsoApplications)
 * @description 直连 OpenIddictApiService 与对应 DTO 类型。
 * 基于通用的 CrudToolbar / CrudTable / useCrudTable / CopyableIdTag 组件实现标准化 CRUD 布局。
 */

import { useState, useMemo } from 'react';
import {
  Button,
  Form,
  Input,
  Select,
  Modal,
  Popconfirm,
  Space,
  Tag,
  Tooltip,
  message,
  Typography,
  Checkbox,
  type TableProps,
} from 'antd';
import {
  AppstoreOutlined,
  KeyOutlined,
  EditOutlined,
  DeleteOutlined,
  CopyOutlined,
} from '@ant-design/icons';
import {
  OpenIddictApiService,
  type OpenIddictApplicationRowResultDto,
  type CreateOpenIddictApplicationInputDto,
  type UpdateOpenIddictApplicationInputDto,
} from '../../../apiServices';
import {
  CopyableIdTag,
  CrudTable,
  CrudToolbar,
  useCrudTable,
} from '../../../components';
import styles from '../sso.module.css';

const { Text } = Typography;

const STANDARD_PERMISSIONS = [
  { label: '授权码端点 (Endpoints.Authorization)', value: 'ept:authorization' },
  { label: '令牌端点 (Endpoints.Token)', value: 'ept:token' },
  { label: '注销端点 (Endpoints.EndSession)', value: 'ept:logout' },
  { label: '授权码模式 (GrantTypes.AuthorizationCode)', value: 'gt:authorization_code' },
  { label: '客户端凭据模式 (GrantTypes.ClientCredentials)', value: 'gt:client_credentials' },
  { label: '刷新令牌 (GrantTypes.RefreshToken)', value: 'gt:refresh_token' },
  { label: '响应类型 Code (ResponseTypes.Code)', value: 'rst:code' },
  { label: 'OpenId 作用域 (scp:openid)', value: 'scp:openid' },
  { label: 'Profile 资料作用域 (scp:profile)', value: 'scp:profile' },
  { label: 'Email 邮箱作用域 (scp:email)', value: 'scp:email' },
  { label: 'Roles 角色作用域 (scp:roles)', value: 'scp:roles' },
  { label: 'DedsiNative API (scp:dedsinative_api)', value: 'scp:dedsinative_api' },
];

export default function SsoApplications() {
  // 1. 查询筛选状态
  const [draftClientId, setDraftClientId] = useState('');
  const [clientId, setClientId] = useState('');

  // 2. 新增 / 编辑弹窗状态
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<OpenIddictApplicationRowResultDto | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [form] = Form.useForm<CreateOpenIddictApplicationInputDto>();

  // 3. 密钥重置弹窗状态
  const [secretModalOpen, setSecretModalOpen] = useState(false);
  const [newSecretVal, setNewSecretVal] = useState('');

  // 4. 通用 CRUD Hook 接管分页与数据加载
  const filters = useMemo(() => ({
    clientId: clientId || undefined,
  }), [clientId]);

  const {
    items,
    loading,
    pagination,
    loadData,
    handleDelete,
  } = useCrudTable<OpenIddictApplicationRowResultDto, { clientId?: string }>({
    fetchApi: OpenIddictApiService.getApplicationPageList,
    deleteApi: OpenIddictApiService.deleteApplication,
    filters,
  });

  const handleSearch = () => {
    setClientId(draftClientId.trim());
  };

  const handleReset = () => {
    setDraftClientId('');
    setClientId('');
  };

  const handleOpenCreate = () => {
    setEditing(null);
    form.resetFields();
    form.setFieldsValue({
      clientType: 'public',
      consentType: 'explicit',
      permissions: [
        'ept:authorization',
        'ept:token',
        'ept:logout',
        'gt:authorization_code',
        'gt:refresh_token',
        'rst:code',
        'scp:openid',
        'scp:profile',
        'scp:email',
        'scp:roles',
        'scp:dedsinative_api',
      ],
    });
    setModalOpen(true);
  };

  const handleOpenEdit = (record: OpenIddictApplicationRowResultDto) => {
    setEditing(record);
    form.setFieldsValue({
      clientId: record.clientId,
      displayName: record.displayName,
      clientType: record.clientType,
      consentType: record.consentType,
      redirectUris: record.redirectUris,
      postLogoutRedirectUris: record.postLogoutRedirectUris,
      permissions: record.permissions,
    });
    setModalOpen(true);
  };

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      setSubmitting(true);

      const redirectUris = Array.isArray(values.redirectUris)
        ? values.redirectUris
        : typeof values.redirectUris === 'string'
        ? (values.redirectUris as string).split('\n').map((s) => s.trim()).filter(Boolean)
        : [];

      const postLogoutRedirectUris = Array.isArray(values.postLogoutRedirectUris)
        ? values.postLogoutRedirectUris
        : typeof values.postLogoutRedirectUris === 'string'
        ? (values.postLogoutRedirectUris as string).split('\n').map((s) => s.trim()).filter(Boolean)
        : [];

      if (editing) {
        await OpenIddictApiService.updateApplication(editing.id, {
          displayName: values.displayName,
          clientType: values.clientType,
          consentType: values.consentType,
          redirectUris,
          postLogoutRedirectUris,
          permissions: values.permissions,
        } as UpdateOpenIddictApplicationInputDto);
        message.success('客户端信息已更新');
      } else {
        await OpenIddictApiService.createApplication({
          ...values,
          redirectUris,
          postLogoutRedirectUris,
        });
        message.success('客户端已注册');
      }

      setModalOpen(false);
      form.resetFields();
      await loadData();
    } catch {
      // 表单校验失败由 AntD 自行处理
    } finally {
      setSubmitting(false);
    }
  };

  const handleResetSecret = async (record: OpenIddictApplicationRowResultDto) => {
    try {
      const res = await OpenIddictApiService.resetApplicationSecret(record.id);
      setNewSecretVal(res.newSecret);
      setSecretModalOpen(true);
      message.success('客户端密钥重置成功');
    } catch {
      message.error('重置密钥失败');
    }
  };

  // 5. 标准 Antd Table 列定义
  const columns: TableProps<OpenIddictApplicationRowResultDto>['columns'] = [
    {
      title: '客户端标识与名称',
      key: 'clientInfo',
      width: 280,
      render: (_, record) => (
        <Space direction="vertical" size={2}>
          <Text strong style={{ color: 'var(--color-title)', fontSize: 14 }}>
            {record.displayName}
          </Text>
          <CopyableIdTag id={record.clientId ?? ''} label="ClientId" />
        </Space>
      ),
    },
    {
      title: '客户端类型',
      dataIndex: 'clientType',
      key: 'clientType',
      width: 140,
      render: (val) =>
        val === 'confidential' ? (
          <Tag color="purple">Confidential (机密)</Tag>
        ) : (
          <Tag color="cyan">Public (公开)</Tag>
        ),
    },
    {
      title: '授权确认模式',
      dataIndex: 'consentType',
      key: 'consentType',
      width: 130,
      render: (val) => (
        <Tag color={val === 'explicit' ? 'blue' : 'default'}>{val ?? 'explicit'}</Tag>
      ),
    },
    {
      title: '重定向地址 (RedirectUris)',
      dataIndex: 'redirectUris',
      key: 'redirectUris',
      render: (uris: string[]) => (
        uris && uris.length > 0 ? (
          <Space direction="vertical" size={2}>
            {uris.slice(0, 2).map((u) => (
              <Tag key={u} color="geekblue">{u}</Tag>
            ))}
            {uris.length > 2 && (
              <Tooltip title={uris.slice(2).join('\n')}>
                <Tag color="default">+{uris.length - 2} 更多</Tag>
              </Tooltip>
            )}
          </Space>
        ) : <Text type="secondary">-</Text>
      ),
    },
    {
      title: '操作',
      key: 'action',
      width: 210,
      fixed: 'right',
      render: (_, record) => (
        <Space size={8}>
          <Button
            type="text"
            size="small"
            icon={<EditOutlined />}
            onClick={() => handleOpenEdit(record)}
            style={{ fontWeight: 500 }}
          >
            编辑
          </Button>
          {record.clientType === 'confidential' && (
            <Popconfirm
              title="确定要重置此客户端的 Secret 吗？"
              description="重置后旧 Secret 将立即失效。"
              onConfirm={() => handleResetSecret(record)}
              okText="确认重置"
              cancelText="取消"
            >
              <Button type="text" size="small" icon={<KeyOutlined />} style={{ color: 'var(--color-purple)' }}>
                重置密钥
              </Button>
            </Popconfirm>
          )}
          <Popconfirm
            title="确定要删除该客户端应用吗？"
            description="删除后关联的所有授权与令牌将一并清除。"
            onConfirm={() => void handleDelete(record.id, '客户端已删除')}
            okText="确定删除"
            cancelText="取消"
            okButtonProps={{ danger: true }}
          >
            <Button type="text" size="small" danger icon={<DeleteOutlined />} style={{ fontWeight: 500 }}>
              删除
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div className={styles.pageContainer}>
      {/* 1. 顶部检索工具栏 */}
      <CrudToolbar
        searchPlaceholder="按 ClientId 搜索..."
        searchValue={draftClientId}
        onSearchChange={setDraftClientId}
        onSearch={handleSearch}
        onReset={handleReset}
        createButton={{
          text: '注册客户端',
          onClick: handleOpenCreate,
        }}
      />

      {/* 2. 数据表格 */}
      <CrudTable<OpenIddictApplicationRowResultDto>
        rowKey="id"
        columns={columns}
        dataSource={items}
        loading={loading}
        pagination={pagination}
        emptyText="暂无客户端应用数据"
        scroll={{ x: 900 }}
      />

      {/* 3. 新建 / 编辑客户端弹窗 Modal */}
      <Modal
        title={
          <Space size={8}>
            <AppstoreOutlined style={{ color: 'var(--color-primary)' }} />
            <span style={{ fontWeight: 700, fontSize: 16 }}>
              {editing ? `编辑客户端: ${editing.displayName}` : '注册新客户端应用'}
            </span>
          </Space>
        }
        open={modalOpen}
        onOk={() => void handleSubmit()}
        onCancel={() => !submitting && setModalOpen(false)}
        confirmLoading={submitting}
        cancelButtonProps={{ disabled: submitting }}
        keyboard={!submitting}
        maskClosable={!submitting}
        okText="保存"
        cancelText="取消"
        className={styles.userModal}
        width={680}
        destroyOnClose
      >
        <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item
            name="clientId"
            label="客户端唯一标识 (ClientId)"
            rules={[{ required: true, message: '请输入 ClientId' }]}
          >
            <Input
              disabled={!!editing}
              placeholder="例如: dedsinative-web / my-custom-app"
              className={styles.formControl}
            />
          </Form.Item>

          <Form.Item
            name="displayName"
            label="客户端显示名称"
            rules={[{ required: true, message: '请输入显示名称' }]}
          >
            <Input placeholder="例如: 业务管理控制台 Web 应用" className={styles.formControl} />
          </Form.Item>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
            <Form.Item
              name="clientType"
              label="客户端类型"
              rules={[{ required: true, message: '请选择客户端类型' }]}
            >
              <Select className={styles.formControl}>
                <Select.Option value="public">Public (公开应用，如 SPA/移动端，配合 PKCE)</Select.Option>
                <Select.Option value="confidential">Confidential (机密应用，如服务端系统，需 Secret)</Select.Option>
              </Select>
            </Form.Item>

            <Form.Item name="consentType" label="授权确认模式 (Consent)">
              <Select className={styles.formControl}>
                <Select.Option value="explicit">Explicit (用户必须显式确认授权)</Select.Option>
                <Select.Option value="implicit">Implicit (第一方信任应用，免显式确认)</Select.Option>
              </Select>
            </Form.Item>
          </div>

          <Form.Item
            noStyle
            shouldUpdate={(prev, curr) => prev.clientType !== curr.clientType}
          >
            {({ getFieldValue }) =>
              getFieldValue('clientType') === 'confidential' && !editing && (
                <Form.Item name="clientSecret" label="初始密钥 (ClientSecret，留空将自动生成)">
                  <Input.Password placeholder="请输入高强度密钥" className={styles.formControl} />
                </Form.Item>
              )
            }
          </Form.Item>

          <Form.Item
            name="redirectUris"
            label="登录回调地址 (RedirectUris，每行一个)"
            tooltip="OIDC 认证成功后允许重定向的回调 URL 列表"
          >
            <Input.TextArea
              rows={3}
              placeholder={'http://localhost:11026/signin-oidc\nhttp://localhost:12256/swagger/oauth2-redirect.html'}
              style={{ borderRadius: 'var(--radius-btn)' }}
            />
          </Form.Item>

          <Form.Item
            name="postLogoutRedirectUris"
            label="登出回调地址 (PostLogoutRedirectUris，每行一个)"
          >
            <Input.TextArea
              rows={2}
              placeholder={'http://localhost:11026/signout-callback-oidc\nhttp://localhost:11026/'}
              style={{ borderRadius: 'var(--radius-btn)' }}
            />
          </Form.Item>

          <Form.Item name="permissions" label="支持的端点、流程与作用域权限">
            <Checkbox.Group
              options={STANDARD_PERMISSIONS}
              style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '8px 16px' }}
            />
          </Form.Item>
        </Form>
      </Modal>

      {/* 4. 密钥生成展示 Modal */}
      <Modal
        title={
          <Space size={8}>
            <KeyOutlined style={{ color: 'var(--color-primary)' }} />
            <span style={{ fontWeight: 700, fontSize: 16 }}>客户端密钥已重置</span>
          </Space>
        }
        open={secretModalOpen}
        onOk={() => setSecretModalOpen(false)}
        onCancel={() => setSecretModalOpen(false)}
        footer={[
          <Button key="ok" type="primary" onClick={() => setSecretModalOpen(false)}>
            我知道了
          </Button>,
        ]}
        className={styles.userModal}
      >
        <p style={{ color: 'var(--color-warning-strong)', marginTop: 12 }}>
          请务必立即复制并妥善保存新的 ClientSecret，该密钥仅展示一次：
        </p>
        <div className={styles.secretBox}>
          <Text code copyable>{newSecretVal}</Text>
          <Button
            type="primary"
            icon={<CopyOutlined />}
            size="small"
            onClick={() => {
              navigator.clipboard.writeText(newSecretVal);
              message.success('密钥已复制到剪贴板');
            }}
          >
            复制密钥
          </Button>
        </div>
      </Modal>
    </div>
  );
}
