/**
 * @file 权限管理页面 (PermissionManagement)
 * @description 直连 PermissionApiService 与 SystemApiService，对应 PermissionResultDto, CreatePermissionInputDto 等类型。
 * 遵循共享 Skill dedsi-style-react-admin-ui 的 UI/UX 规范。
 */

import { useCallback, useEffect, useState } from 'react';
import {
  Button,
  Card,
  Descriptions,
  Empty,
  Form,
  Input,
  Modal,
  Popconfirm,
  Select,
  Space,
  Switch,
  Table,
  Tag,
  Tooltip,
  Avatar,
  message,
  type TableProps,
} from 'antd';
import {
  DeleteOutlined,
  EditOutlined,
  EyeOutlined,
  PlusOutlined,
  ReloadOutlined,
  SearchOutlined,
  CopyOutlined,
  KeyOutlined,
  CheckCircleOutlined,
  StopOutlined,
  SafetyCertificateOutlined,
} from '@ant-design/icons';
import {
  PermissionApiService,
  SystemApiService,
  type CreatePermissionInputDto,
  type PermissionResultDto,
  type PermissionRowResultDto,
  type SystemRowResultDto,
  type UpdatePermissionInputDto,
} from '../../../apiServices';
import styles from './index.module.css';

/** 根据权限名称生成固定的头像背景色 */
const getAvatarColor = (name: string): string => {
  const colors = [
    'var(--color-primary)',
    'var(--color-success)',
    'var(--color-warning-strong)',
    'var(--color-info)',
    'var(--color-purple)',
    'var(--color-pink)',
  ];
  let hash = 0;
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash);
  }
  const index = Math.abs(hash) % colors.length;
  return colors[index];
};

/** 复制文本通用辅助函数 */
const copyToClipboard = async (text: string, label = '内容') => {
  try {
    await navigator.clipboard.writeText(text);
    message.success(`已复制 ${label} 到剪贴板`);
  } catch {
    message.error('复制失败，请手动选择复制');
  }
};

/** 权限管理页面，负责权限查询、维护和启用状态管理。 */
export default function PermissionManagement() {
  const [items, setItems] = useState<PermissionRowResultDto[]>([]);
  const [systems, setSystems] = useState<SystemRowResultDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [pageIndex, setPageIndex] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [draftName, setDraftName] = useState('');
  const [name, setName] = useState('');
  const [systemId, setSystemId] = useState<string | undefined>();
  const [status, setStatus] = useState<boolean | undefined>();
  const [editing, setEditing] = useState<PermissionRowResultDto | null>(null);
  const [detail, setDetail] = useState<PermissionResultDto | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [detailOpen, setDetailOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [form] = Form.useForm<CreatePermissionInputDto>();

  /** 加载系统下拉选项 */
  const loadSystems = useCallback(async () => {
    try {
      const result = await SystemApiService.getAll();
      setSystems(result);
    } catch {
      setSystems([]);
    }
  }, []);

  /** 按已提交筛选条件加载权限列表。 */
  const loadPermissions = useCallback(async () => {
    setLoading(true);
    try {
      const result = await PermissionApiService.getPageList({
        pageIndex,
        pageSize,
        systemId,
        name: name || undefined,
        isEnabled: status,
      });
      setItems(result.items || []);
      setTotalCount(result.totalCount || 0);
    } catch {
      setItems([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  }, [name, pageIndex, pageSize, status, systemId]);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      void loadSystems();
    }, 0);
    return () => window.clearTimeout(timeoutId);
  }, [loadSystems]);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      void loadPermissions();
    }, 0);
    return () => window.clearTimeout(timeoutId);
  }, [loadPermissions]);

  /** 提交筛选条件并回到第一页。 */
  const handleSearch = () => {
    setPageIndex(1);
    setName(draftName.trim());
  };

  /** 重置所有筛选条件 */
  const handleResetSearch = () => {
    setDraftName('');
    setName('');
    setSystemId(undefined);
    setStatus(undefined);
    setPageIndex(1);
  };

  /** 打开新增或编辑权限表单。 */
  const openForm = (item?: PermissionRowResultDto) => {
    setEditing(item ?? null);
    form.setFieldsValue(
      item
        ? { systemId: item.systemId, name: item.name, description: item.description ?? '', isEnabled: item.isEnabled }
        : { systemId: undefined, name: '', description: '', isEnabled: true }
    );
    setModalOpen(true);
  };

  /** 提交权限创建或更新请求。 */
  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      setSubmitting(true);
      if (editing) {
        const input: UpdatePermissionInputDto = {
          systemId: values.systemId,
          name: values.name,
          description: values.description,
        };
        await PermissionApiService.update(editing.id, input);
        if (values.isEnabled !== editing.isEnabled) {
          await PermissionApiService.setStatus(editing.id, { isEnabled: values.isEnabled });
        }
        message.success('权限信息已更新');
      } else {
        const input: CreatePermissionInputDto = values;
        await PermissionApiService.create(input);
        message.success('权限已创建');
      }
      setModalOpen(false);
      form.resetFields();
      await loadPermissions();
    } catch {
      // 表单错误由 Form 展示
    } finally {
      setSubmitting(false);
    }
  };

  /** 直接切换权限启用状态。 */
  const handleStatusChange = async (item: PermissionRowResultDto, isEnabled: boolean) => {
    try {
      await PermissionApiService.setStatus(item.id, { isEnabled });
      message.success(isEnabled ? '权限已启用' : '权限已停用');
      await loadPermissions();
    } catch {
      // 统一拦截器提示
    }
  };

  /** 删除权限并处理当前页最后一条记录。 */
  const handleDelete = async (id: string) => {
    try {
      await PermissionApiService.delete(id);
      message.success('权限已删除');
      if (items.length === 1 && pageIndex > 1) {
        setPageIndex((current) => current - 1);
      } else {
        await loadPermissions();
      }
    } catch {
      // 统一拦截器提示
    }
  };

  /** 加载权限详情 */
  const openDetail = async (item: PermissionRowResultDto) => {
    setDetailOpen(true);
    setDetail(null);
    setDetailLoading(true);
    try {
      setDetail(await PermissionApiService.getById(item.id));
    } catch {
      setDetail(null);
    } finally {
      setDetailLoading(false);
    }
  };

  const columns: TableProps<PermissionRowResultDto>['columns'] = [
    {
      title: '权限标识与名称',
      key: 'name',
      width: 240,
      render: (_, record) => {
        return (
          <div className={styles.cellWrapper}>
            <div className={styles.cellInfo}>
              <span className={styles.cellTitle}>{record.name}</span>
              <span className={styles.cellSub}>系统: {record.systemName}</span>
            </div>
          </div>
        );
      },
    },
    {
      title: '权限 ID',
      dataIndex: 'id',
      key: 'id',
      width: 240,
      render: (id: string) => (
        <Tooltip title="点击复制 ID">
          <span className={styles.idTag} onClick={() => void copyToClipboard(id, '权限 ID')}>
            {id}
            <CopyOutlined style={{ fontSize: 11, opacity: 0.6 }} />
          </span>
        </Tooltip>
      ),
    },
    {
      title: '所属系统',
      dataIndex: 'systemName',
      key: 'systemName',
      width: 160,
      render: (name: string) => (
        <Tag color="purple" style={{ borderRadius: 10 }}>
          {name}
        </Tag>
      ),
    },
    {
      title: '权限说明',
      dataIndex: 'description',
      key: 'description',
      render: (value: string | null) =>
        value ? (
          <span style={{ color: 'var(--color-body)' }}>{value}</span>
        ) : (
          <Tag bordered={false} style={{ color: 'var(--color-placeholder)' }}>
            未填写
          </Tag>
        ),
    },
    {
      title: '启用状态',
      dataIndex: 'isEnabled',
      key: 'isEnabled',
      width: 160,
      render: (isEnabled: boolean, record) => (
        <Space size={8}>
          <Switch
            checked={isEnabled}
            onChange={(checked) => void handleStatusChange(record, checked)}
            size="small"
          />
          {isEnabled ? (
            <Tag color="success" icon={<CheckCircleOutlined />}>
              启用
            </Tag>
          ) : (
            <Tag color="error" icon={<StopOutlined />}>
              停用
            </Tag>
          )}
        </Space>
      ),
    },
    {
      title: '操作',
      key: 'actions',
      width: 220,
      fixed: 'right',
      render: (_, record) => (
        <Space size={4}>
          <Tooltip title="查看权限详情">
            <Button
              type="text"
              icon={<EyeOutlined />}
              size="small"
              onClick={() => void openDetail(record)}
              style={{ color: 'var(--color-primary)', fontWeight: 500 }}
            >
              详情
            </Button>
          </Tooltip>
          <Tooltip title="编辑权限资料">
            <Button
              type="text"
              icon={<EditOutlined />}
              size="small"
              onClick={() => openForm(record)}
              style={{ color: 'var(--color-primary)', fontWeight: 500 }}
            >
              编辑
            </Button>
          </Tooltip>
          <Popconfirm
            title="确认删除该权限？"
            description="如果权限已被授权，删除可能影响现有授权关系。"
            okText="确定删除"
            cancelText="取消"
            okButtonProps={{ danger: true }}
            onConfirm={() => void handleDelete(record.id)}
          >
            <Button type="text" danger icon={<DeleteOutlined />} size="small" style={{ fontWeight: 500 }}>
              删除
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div className={styles.pageContainer}>
      {/* 1. 顶部检索筛选卡片 */}
      <Card className={styles.headerCard} styles={{ body: { padding: '16px 24px' } }}>
        <div className={styles.toolbarWrapper}>
          <div className={styles.searchGroup}>
            <Select
              allowClear
              className={styles.systemSelect}
              placeholder="选择所属系统"
              value={systemId}
              onChange={(value: string | undefined) => {
                setSystemId(value);
                setPageIndex(1);
              }}
              options={systems.map((system) => ({ label: system.name, value: system.id }))}
            />
            <Input
              allowClear
              className={styles.searchInput}
              prefix={<SearchOutlined style={{ color: 'var(--color-placeholder)' }} />}
              placeholder="按权限名称搜索..."
              value={draftName}
              onChange={(event) => setDraftName(event.target.value)}
              onPressEnter={handleSearch}
            />
            <Select
              allowClear
              className={styles.statusSelect}
              placeholder="全部状态"
              value={status}
              onChange={(value: boolean | undefined) => {
                setStatus(value);
                setPageIndex(1);
              }}
              options={[
                { label: '启用', value: true },
                { label: '停用', value: false },
              ]}
            />
            <Button
              type="primary"
              icon={<SearchOutlined />}
              onClick={handleSearch}
              style={{ borderRadius: 'var(--radius-btn)', backgroundColor: 'var(--color-primary)' }}
            >
              查询
            </Button>
            <Button
              icon={<ReloadOutlined />}
              onClick={handleResetSearch}
              style={{ borderRadius: 'var(--radius-btn)' }}
            >
              重置
            </Button>
          </div>

          <Space size={12}>
            <Button
              icon={<ReloadOutlined spin={loading} />}
              onClick={() => void loadPermissions()}
              style={{ borderRadius: 'var(--radius-btn)' }}
            >
              刷新
            </Button>
            <Button
              type="primary"
              className="create-primary-button"
              icon={<PlusOutlined />}
              onClick={() => openForm()}
            >
              新增权限
            </Button>
          </Space>
        </div>
      </Card>

      {/* 2. 数据表格卡片 */}
      <Card className={styles.tableCard} styles={{ body: { padding: '16px 24px' } }}>
        <Table<PermissionRowResultDto>
          rowKey="id"
          columns={columns}
          dataSource={items}
          loading={loading}
          locale={{ emptyText: <Empty description="暂无权限数据" /> }}
          scroll={{ x: 980 }}
          pagination={{
            current: pageIndex,
            pageSize,
            total: totalCount,
            showTotal: (total, range) => `显示第 ${range[0]} - ${range[1]} 条，共 ${total} 条记录`,
            onChange: (nextPage, nextPageSize) => {
              setPageIndex(nextPageSize === pageSize ? nextPage : 1);
              setPageSize(nextPageSize);
            },
          }}
        />
      </Card>

      {/* 3. 新增 / 编辑权限弹窗 Modal */}
      <Modal
        title={
          <Space size={8}>
            <KeyOutlined style={{ color: 'var(--color-primary)' }} />
            <span style={{ fontWeight: 700, fontSize: 16 }}>
              {editing ? `编辑权限: ${editing.name}` : '新增权限'}
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
        width={540}
      >
        <Form form={form} layout="vertical" style={{ marginTop: 12 }}>
          <div className={styles.sectionCard}>
            <div className={styles.sectionTitle}>
              <SafetyCertificateOutlined style={{ color: 'var(--color-primary)' }} />
              <span>权限配置信息</span>
            </div>
            <Form.Item
              name="systemId"
              label="所属系统"
              rules={[{ required: true, message: '请选择所属系统' }]}
            >
              <Select
                placeholder="请选择归属系统"
                options={systems.map((system) => ({ label: system.name, value: system.id }))}
                className={styles.formControl}
              />
            </Form.Item>
            <Form.Item
              name="name"
              label="权限名称/标识"
              rules={[{ required: true, message: '请输入权限名称' }]}
            >
              <Input className={styles.formControl} placeholder="例如：user.read 或 用户查看" />
            </Form.Item>
            <Form.Item name="description" label="权限说明">
              <Input.TextArea
                rows={3}
                placeholder="请输入权限说明与作用范围"
                style={{ borderRadius: 'var(--radius-btn)' }}
              />
            </Form.Item>
            <Form.Item name="isEnabled" label="启用状态" valuePropName="checked" style={{ marginBottom: 0 }}>
              <Switch checkedChildren="启用" unCheckedChildren="停用" />
            </Form.Item>
          </div>
        </Form>
      </Modal>

      {/* 4. 权限详情 Modal */}
      <Modal
        title={null}
        open={detailOpen}
        onCancel={() => setDetailOpen(false)}
        footer={[
          <Button
            key="edit"
            type="primary"
            icon={<EditOutlined />}
            onClick={() => {
              setDetailOpen(false);
              if (detail) {
                const row: PermissionRowResultDto = {
                  id: detail.id,
                  systemId: detail.systemId,
                  systemName: detail.systemName,
                  name: detail.name,
                  description: detail.description,
                  isEnabled: detail.isEnabled,
                };
                openForm(row);
              }
            }}
            style={{ borderRadius: 'var(--radius-btn)', backgroundColor: 'var(--color-primary)' }}
          >
            编辑此权限
          </Button>,
          <Button
            key="close"
            onClick={() => setDetailOpen(false)}
            style={{ borderRadius: 'var(--radius-btn)' }}
          >
            关闭
          </Button>,
        ]}
        width={520}
        className={styles.userModal}
      >
        {detailLoading ? (
          <div style={{ padding: '40px 0', textAlign: 'center', color: 'var(--color-muted)' }}>
            数据加载中...
          </div>
        ) : detail ? (
          <div style={{ paddingTop: 8 }}>
            <div className={styles.detailHeader}>
              <Avatar
                size={52}
                style={{
                  background: getAvatarColor(detail.name),
                  fontSize: 22,
                  fontWeight: 700,
                }}
              >
                {detail.name ? detail.name.charAt(0).toUpperCase() : 'P'}
              </Avatar>
              <div className={styles.detailHeaderInfo}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                  <span className={styles.detailHeaderName}>{detail.name}</span>
                  {detail.isEnabled ? (
                    <Tag color="success" icon={<CheckCircleOutlined />}>
                      启用
                    </Tag>
                  ) : (
                    <Tag color="error" icon={<StopOutlined />}>
                      停用
                    </Tag>
                  )}
                </div>
                <Tag color="purple" style={{ borderRadius: 10, width: 'fit-content' }}>
                  系统: {detail.systemName}
                </Tag>
              </div>
            </div>

            <Descriptions
              column={1}
              bordered
              size="small"
              labelStyle={{
                width: 120,
                fontWeight: 600,
                color: 'var(--color-text-secondary)',
                backgroundColor: 'var(--color-surface-subtle)',
              }}
              contentStyle={{ color: 'var(--color-title)' }}
            >
              <Descriptions.Item label="权限 ID">
                <span
                  style={{ fontFamily: 'monospace', color: 'var(--color-body)', cursor: 'pointer' }}
                  onClick={() => void copyToClipboard(detail.id, '权限 ID')}
                >
                  {detail.id} <CopyOutlined style={{ color: 'var(--color-placeholder)', marginLeft: 4 }} />
                </span>
              </Descriptions.Item>
              <Descriptions.Item label="权限名称">{detail.name}</Descriptions.Item>
              <Descriptions.Item label="所属系统">{detail.systemName}</Descriptions.Item>
              <Descriptions.Item label="状态">
                {detail.isEnabled ? '启用' : '停用'}
              </Descriptions.Item>
              <Descriptions.Item label="权限说明">
                {detail.description || '暂无说明'}
              </Descriptions.Item>
            </Descriptions>
          </div>
        ) : (
          <Empty description="无法加载权限详情" />
        )}
      </Modal>
    </div>
  );
}
