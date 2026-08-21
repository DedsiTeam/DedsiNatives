/**
 * @file 权限管理页面 (PermissionManagement)
 * @description 直连 PermissionApiService 与 SystemApiService，对应 PermissionResultDto, CreatePermissionInputDto 等类型。
 * 基于通用 CrudToolbar / CrudTable / useCrudTable / CopyableIdTag 组件实现高复用度布局与统一标准。
 */

import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Button,
  Descriptions,
  Empty,
  Form,
  Input,
  Modal,
  Popconfirm,
  Select,
  Space,
  Switch,
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
  KeyOutlined,
  CheckCircleOutlined,
  StopOutlined,
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
import {
  CopyableIdTag,
  CrudTable,
  CrudToolbar,
  useCrudTable,
} from '../../../components';
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

/** 权限管理页面，负责权限查询、维护和启用状态管理。 */
export default function PermissionManagement() {
  // 1. 系统选项与搜索筛选状态
  const [systems, setSystems] = useState<SystemRowResultDto[]>([]);
  const [draftName, setDraftName] = useState('');
  const [name, setName] = useState('');
  const [systemId, setSystemId] = useState<string | undefined>();
  const [status, setStatus] = useState<boolean | undefined>();

  // 2. 弹窗与表单状态
  const [editing, setEditing] = useState<PermissionRowResultDto | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [form] = Form.useForm<CreatePermissionInputDto>();

  // 3. 详情弹窗状态
  const [detailOpen, setDetailOpen] = useState(false);
  const [detail, setDetail] = useState<PermissionResultDto | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);

  /** 加载系统下拉选项 */
  const loadSystems = useCallback(async () => {
    try {
      const result = await SystemApiService.getAll();
      setSystems(result);
    } catch {
      setSystems([]);
    }
  }, []);

  useEffect(() => {
    void loadSystems();
  }, [loadSystems]);

  // 4. 通用 CRUD Hook 接管分页与数据加载
  const queryFilters = useMemo(
    () => ({
      systemId,
      name: name || undefined,
      isEnabled: status,
    }),
    [systemId, name, status]
  );

  const {
    items,
    loading,
    pagination,
    loadData,
    handleDelete,
  } = useCrudTable<
    PermissionRowResultDto,
    { systemId?: string; name?: string; isEnabled?: boolean }
  >({
    fetchApi: PermissionApiService.getPageList,
    deleteApi: PermissionApiService.delete,
    filters: queryFilters,
  });

  /** 提交筛选条件 */
  const handleSearch = () => {
    setName(draftName.trim());
  };

  /** 重置所有筛选条件 */
  const handleResetSearch = () => {
    setDraftName('');
    setName('');
    setSystemId(undefined);
    setStatus(undefined);
  };

  /** 打开新增或编辑权限表单。 */
  const openForm = (item?: PermissionRowResultDto) => {
    setEditing(item ?? null);
    form.setFieldsValue(
      item
        ? {
            systemId: item.systemId,
            name: item.name,
            description: item.description ?? '',
            isEnabled: item.isEnabled,
          }
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
      await loadData();
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
      await loadData();
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

  // 5. 表格列定义
  const columns: TableProps<PermissionRowResultDto>['columns'] = [
    {
      title: '权限标识与名称',
      key: 'name',
      width: 240,
      render: (_, record) => (
        <div className={styles.cellWrapper}>
          <div className={styles.cellInfo}>
            <span className={styles.cellTitle}>{record.name}</span>
            <span className={styles.cellSub}>系统: {record.systemName}</span>
          </div>
        </div>
      ),
    },
    {
      title: '权限 ID',
      dataIndex: 'id',
      key: 'id',
      width: 260,
      render: (id: string) => <CopyableIdTag id={id} label="权限 ID" />,
    },
    {
      title: '所属系统',
      dataIndex: 'systemName',
      key: 'systemName',
      width: 160,
      render: (sysName: string) => (
        <Tag color="purple" style={{ borderRadius: 10 }}>
          {sysName}
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
            onConfirm={() => void handleDelete(record.id, '权限已删除')}
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
      <CrudToolbar
        searchPlaceholder="按权限名称搜索..."
        searchValue={draftName}
        onSearchChange={setDraftName}
        onSearch={handleSearch}
        onReset={handleResetSearch}
        createButton={{
          text: '新增权限',
          onClick: () => openForm(),
        }}
        extraFilters={
          <>
            <Select
              allowClear
              className={styles.systemSelect}
              placeholder="选择所属系统"
              value={systemId}
              onChange={(value: string | undefined) => {
                setSystemId(value);
              }}
              options={systems.map((system) => ({ label: system.name, value: system.id }))}
            />
            <Select
              allowClear
              className={styles.statusSelect}
              placeholder="全部状态"
              value={status}
              onChange={(value: boolean | undefined) => {
                setStatus(value);
              }}
              options={[
                { label: '启用', value: true },
                { label: '停用', value: false },
              ]}
            />
          </>
        }
      />

      {/* 2. 数据表格卡片 */}
      <CrudTable<PermissionRowResultDto>
        rowKey="id"
        columns={columns}
        dataSource={items}
        loading={loading}
        pagination={pagination}
        emptyText="暂无权限数据"
        scroll={{ x: 980 }}
      />

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
        width={560}
      >
        <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item
            name="systemId"
            label="所属系统"
            rules={[{ required: true, message: '请选择所属系统' }]}
          >
            <Select
              className={styles.formControl}
              placeholder="选择所属业务系统"
              options={systems.map((system) => ({ label: system.name, value: system.id }))}
            />
          </Form.Item>
          <Form.Item
            name="name"
            label="权限名称"
            rules={[{ required: true, message: '请输入权限名称' }]}
          >
            <Input className={styles.formControl} placeholder="例如：用户管理-读取" />
          </Form.Item>
          <Form.Item name="description" label="权限说明">
            <Input.TextArea
              rows={3}
              placeholder="请输入权限的具体功能说明"
              style={{ borderRadius: 'var(--radius-btn)' }}
            />
          </Form.Item>
          <Form.Item
            name="isEnabled"
            label="是否立即启用"
            valuePropName="checked"
            style={{ marginBottom: 0 }}
          >
            <Switch />
          </Form.Item>
        </Form>
      </Modal>

      {/* 4. 权限详情 Modal */}
      <Modal
        title={
          <Space size={8}>
            <KeyOutlined style={{ color: 'var(--color-primary)' }} />
            <span style={{ fontWeight: 700, fontSize: 16 }}>权限详情</span>
          </Space>
        }
        open={detailOpen}
        onCancel={() => setDetailOpen(false)}
        footer={null}
        width={500}
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
                <span className={styles.detailHeaderName}>{detail.name}</span>
                <Space size={6}>
                  <Tag color="purple" style={{ borderRadius: 10 }}>
                    所属系统: {detail.systemName}
                  </Tag>
                  {detail.isEnabled ? (
                    <Tag color="success" icon={<CheckCircleOutlined />} style={{ borderRadius: 10 }}>
                      启用
                    </Tag>
                  ) : (
                    <Tag color="error" icon={<StopOutlined />} style={{ borderRadius: 10 }}>
                      停用
                    </Tag>
                  )}
                </Space>
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
                <CopyableIdTag id={detail.id} label="权限 ID" />
              </Descriptions.Item>
              <Descriptions.Item label="权限名称">{detail.name}</Descriptions.Item>
              <Descriptions.Item label="所属系统">{detail.systemName}</Descriptions.Item>
              <Descriptions.Item label="状态">
                {detail.isEnabled ? <Tag color="success">正常启用</Tag> : <Tag color="error">已停用</Tag>}
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
