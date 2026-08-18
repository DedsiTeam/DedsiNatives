/**
 * @file 岗位管理页面 (PositionManagement)
 * @description 直连 PositionApiService, PermissionApiService, SystemApiService，对应 PositionResultDto, CreatePositionInputDto 等类型。
 * 遵循共享 Skill dedsi-style-react-admin-ui 的 UI/UX 规范。
 */

import { useCallback, useEffect, useState } from 'react';
import {
  Button,
  Card,
  Checkbox,
  Descriptions,
  Empty,
  Form,
  Input,
  List,
  Modal,
  Popconfirm,
  Select,
  Space,
  Switch,
  Table,
  Tag,
  Tooltip,
  Avatar,
  Typography,
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
  SolutionOutlined,
  CheckCircleOutlined,
  StopOutlined,
  KeyOutlined,
  BankOutlined,
} from '@ant-design/icons';
import {
  PositionApiService,
  PermissionApiService,
  SystemApiService,
  type CreatePositionInputDto,
  type PermissionRowResultDto,
  type PositionOrganizationInputDto,
  type PositionResultDto,
  type PositionRowResultDto,
  type SystemRowResultDto,
  type UpdatePositionInputDto,
} from '../../../apiServices';
import styles from './index.module.css';

const { Text } = Typography;

/** 岗位新增/编辑表单值，包含尚未提交到岗位资料接口的权限选择。 */
interface PositionFormValues extends CreatePositionInputDto {
  /** 当前选择的权限 ID。 */
  permissionIds: string[];
}

/** 根据岗位名称生成固定的头像背景色 */
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

/** 岗位管理页面，负责岗位资料、状态和关联数量展示。 */
export default function PositionManagement() {
  const [items, setItems] = useState<PositionRowResultDto[]>([]);
  const [systems, setSystems] = useState<SystemRowResultDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [pageIndex, setPageIndex] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [draftName, setDraftName] = useState('');
  const [name, setName] = useState('');
  const [systemId, setSystemId] = useState<string | undefined>();
  const [status, setStatus] = useState<boolean | undefined>();
  const [editing, setEditing] = useState<PositionRowResultDto | null>(null);
  const [detail, setDetail] = useState<PositionResultDto | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [detailOpen, setDetailOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [formLoading, setFormLoading] = useState(false);
  const [permissionLoading, setPermissionLoading] = useState(false);
  const [permissionLoadError, setPermissionLoadError] = useState(false);
  const [permissionOptions, setPermissionOptions] = useState<PermissionRowResultDto[]>([]);
  const [permissionSearch, setPermissionSearch] = useState('');
  const [editingOrganizations, setEditingOrganizations] = useState<PositionOrganizationInputDto[]>([]);
  const [form] = Form.useForm<PositionFormValues>();
  const formSystemId = Form.useWatch('systemId', form);
  const selectedPermissionIds = Form.useWatch('permissionIds', form) ?? [];
  const visiblePermissionOptions = permissionOptions.filter((permission) =>
    permission.name.toLocaleLowerCase().includes(permissionSearch.trim().toLocaleLowerCase())
  );

  /** 加载系统选项 */
  const loadSystems = useCallback(async () => {
    try {
      const result = await SystemApiService.getAll();
      setSystems(result);
    } catch {
      setSystems([]);
    }
  }, []);

  /** 加载指定系统下可分配的启用权限。 */
  const loadPermissions = useCallback(async (selectedSystemId?: string) => {
    if (!selectedSystemId) {
      setPermissionOptions([]);
      setPermissionLoadError(false);
      return;
    }

    setPermissionLoading(true);
    setPermissionLoadError(false);
    try {
      const result = await PermissionApiService.getAll(selectedSystemId);
      setPermissionOptions(result.filter((permission) => permission.isEnabled));
    } catch {
      setPermissionOptions([]);
      setPermissionLoadError(true);
    } finally {
      setPermissionLoading(false);
    }
  }, []);

  /** 按当前筛选条件加载岗位。 */
  const loadPositions = useCallback(async () => {
    setLoading(true);
    try {
      const result = await PositionApiService.getPageList({
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
      void loadPositions();
    }, 0);
    return () => window.clearTimeout(timeoutId);
  }, [loadPositions]);

  /** 提交筛选条件并回到第一页。 */
  const handleSearch = () => {
    setPageIndex(1);
    setName(draftName.trim());
  };

  /** 重置筛选条件 */
  const handleResetSearch = () => {
    setDraftName('');
    setName('');
    setSystemId(undefined);
    setStatus(undefined);
    setPageIndex(1);
  };

  /** 打开岗位新增或编辑表单。 */
  const openForm = async (item?: PositionRowResultDto) => {
    setEditing(item ?? null);
    setPermissionOptions([]);
    setPermissionSearch('');
    setPermissionLoadError(false);
    setEditingOrganizations([]);
    setModalOpen(true);

    if (!item) {
      form.setFieldsValue({
        name: '',
        systemId: undefined,
        description: '',
        isEnabled: true,
        permissionIds: [],
      });
      return;
    }

    setFormLoading(true);
    try {
      const position = await PositionApiService.getById(item.id);
      setEditingOrganizations(
        position.organizations.map((organization) => ({
          organizationId: organization.organizationId,
          organizationName: organization.organizationName,
        }))
      );
      await loadPermissions(item.systemId);
      form.setFieldsValue({
        name: item.name,
        systemId: item.systemId,
        description: item.description ?? '',
        isEnabled: item.isEnabled,
        permissionIds: position.permissions.map((permission) => permission.permissionId),
      });
    } catch {
      message.error('岗位详情加载失败，请重试');
      setModalOpen(false);
    } finally {
      setFormLoading(false);
    }
  };

  /** 切换岗位所属系统时清空旧系统权限 */
  const handleFormSystemChange = (value: string) => {
    form.setFieldValue('systemId', value);
    form.setFieldValue('permissionIds', []);
    setPermissionSearch('');
    void loadPermissions(value);
  };

  /** 切换单个权限的选中状态。 */
  const handlePermissionToggle = (permissionId: string, checked: boolean) => {
    const nextIds = new Set(selectedPermissionIds);
    if (checked) nextIds.add(permissionId);
    else nextIds.delete(permissionId);
    form.setFieldValue('permissionIds', [...nextIds]);
  };

  /** 选择当前搜索结果中的全部权限。 */
  const selectVisiblePermissions = () => {
    const nextIds = new Set(selectedPermissionIds);
    visiblePermissionOptions.forEach((permission) => nextIds.add(permission.id));
    form.setFieldValue('permissionIds', [...nextIds]);
  };

  /** 清除当前搜索结果中的权限。 */
  const clearVisiblePermissions = () => {
    const visibleIds = new Set(visiblePermissionOptions.map((permission) => permission.id));
    form.setFieldValue(
      'permissionIds',
      selectedPermissionIds.filter((permissionId) => !visibleIds.has(permissionId))
    );
  };

  /** 提交岗位新增或编辑。 */
  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      setSubmitting(true);
      const permissionIds = form.getFieldValue('permissionIds') ?? [];
      let positionId = editing?.id;
      if (editing) {
        const input: UpdatePositionInputDto = {
          name: values.name,
          systemId: values.systemId,
          description: values.description,
        };
        await PositionApiService.update(editing.id, input);
        if (values.isEnabled !== editing.isEnabled) {
          await PositionApiService.setStatus(editing.id, { isEnabled: values.isEnabled });
        }
      } else {
        positionId = await PositionApiService.create({
          name: values.name,
          systemId: values.systemId,
          description: values.description,
          isEnabled: values.isEnabled,
          permissionIds,
          organizations: editingOrganizations,
        });
      }
      if (editing && positionId) {
        await PositionApiService.updateAssignments(positionId, {
          permissionIds,
          organizations: editingOrganizations,
        });
      }
      message.success(editing ? '岗位信息和权限已更新' : '岗位及权限已创建');
      setModalOpen(false);
      form.resetFields();
      await loadPositions();
    } catch {
      // 表单错误由 Form 展示
    } finally {
      setSubmitting(false);
    }
  };

  /** 切换岗位启用状态。 */
  const handleStatusChange = async (item: PositionRowResultDto, isEnabled: boolean) => {
    try {
      await PositionApiService.setStatus(item.id, { isEnabled });
      message.success(isEnabled ? '岗位已启用' : '岗位已停用');
      await loadPositions();
    } catch {
      // 统一拦截器提示
    }
  };

  /** 删除岗位并处理当前页最后一条记录。 */
  const handleDelete = async (id: string) => {
    try {
      await PositionApiService.delete(id);
      message.success('岗位已删除');
      if (items.length === 1 && pageIndex > 1) setPageIndex((current) => current - 1);
      else await loadPositions();
    } catch {
      // 统一拦截器提示
    }
  };

  /** 加载岗位详情 */
  const openDetail = async (item: PositionRowResultDto) => {
    setDetailOpen(true);
    setDetail(null);
    setDetailLoading(true);
    try {
      setDetail(await PositionApiService.getById(item.id));
    } catch {
      setDetail(null);
    } finally {
      setDetailLoading(false);
    }
  };

  const columns: TableProps<PositionRowResultDto>['columns'] = [
    {
      title: '岗位名称与归属',
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
      title: '岗位 ID',
      dataIndex: 'id',
      key: 'id',
      width: 240,
      render: (id: string) => (
        <Tooltip title="点击复制 ID">
          <span className={styles.idTag} onClick={() => void copyToClipboard(id, '岗位 ID')}>
            {id}
            <CopyOutlined style={{ fontSize: 11, opacity: 0.6 }} />
          </span>
        </Tooltip>
      ),
    },
    {
      title: '权限数量',
      dataIndex: 'permissionCount',
      key: 'permissionCount',
      width: 120,
      render: (count: number) => (
        <Tag color="blue" style={{ borderRadius: 10 }}>
          <KeyOutlined style={{ marginRight: 4 }} />
          {count} 个权限
        </Tag>
      ),
    },
    {
      title: '关联机构数',
      dataIndex: 'organizationCount',
      key: 'organizationCount',
      width: 130,
      render: (count: number) => (
        <Tag color="cyan" style={{ borderRadius: 10 }}>
          <BankOutlined style={{ marginRight: 4 }} />
          {count} 个机构
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
          <Tooltip title="查看岗位详情与权限范围">
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
          <Tooltip title="编辑岗位资料及分配权限">
            <Button
              type="text"
              icon={<EditOutlined />}
              size="small"
              onClick={() => void openForm(record)}
              style={{ color: 'var(--color-primary)', fontWeight: 500 }}
            >
              编辑
            </Button>
          </Tooltip>
          <Popconfirm
            title="确认删除该岗位？"
            description="岗位权限和组织机构关联也会被删除。"
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
              options={systems.map((item) => ({ label: item.name, value: item.id }))}
            />
            <Input
              allowClear
              className={styles.searchInput}
              prefix={<SearchOutlined style={{ color: 'var(--color-placeholder)' }} />}
              placeholder="按岗位名称搜索..."
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
              onClick={() => void loadPositions()}
              style={{ borderRadius: 'var(--radius-btn)' }}
            >
              刷新
            </Button>
            <Button
              type="primary"
              className="create-primary-button"
              icon={<PlusOutlined />}
              onClick={() => void openForm()}
            >
              新增岗位
            </Button>
          </Space>
        </div>
      </Card>

      {/* 2. 数据表格卡片 */}
      <Card className={styles.tableCard} styles={{ body: { padding: '16px 24px' } }}>
        <Table<PositionRowResultDto>
          rowKey="id"
          columns={columns}
          dataSource={items}
          loading={loading}
          locale={{ emptyText: <Empty description="暂无岗位数据" /> }}
          scroll={{ x: 1080 }}
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

      {/* 3. 新增 / 编辑岗位弹窗 Modal */}
      <Modal
        title={
          <Space size={8}>
            <SolutionOutlined style={{ color: 'var(--color-primary)' }} />
            <span style={{ fontWeight: 700, fontSize: 16 }}>
              {editing ? `编辑岗位: ${editing.name}` : '新增岗位'}
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
        okText="保存提交"
        cancelText="取消"
        className={styles.userModal}
        width={680}
      >
        {formLoading ? (
          <div className={styles.formLoading}>正在加载岗位权限配置...</div>
        ) : null}
        <Form form={form} layout="vertical" style={{ marginTop: 12 }}>
          {/* 基本信息卡片 */}
          <div className={styles.sectionCard}>
            <div className={styles.sectionTitle}>
              <div className={styles.sectionTitleLeft}>
                <SolutionOutlined style={{ color: 'var(--color-primary)' }} />
                <span>岗位基本资料</span>
              </div>
            </div>
            <Form.Item
              name="systemId"
              label="所属系统"
              rules={[{ required: true, message: '请选择所属系统' }]}
            >
              <Select
                placeholder="请选择系统"
                options={systems.map((item) => ({ label: item.name, value: item.id }))}
                onChange={handleFormSystemChange}
                disabled={formLoading || submitting}
                className={styles.formControl}
              />
            </Form.Item>
            <Form.Item
              name="name"
              label="岗位名称"
              rules={[{ required: true, message: '请输入岗位名称' }]}
            >
              <Input
                placeholder="例如：系统管理员"
                disabled={formLoading || submitting}
                className={styles.formControl}
              />
            </Form.Item>
            <Form.Item name="description" label="岗位说明">
              <Input.TextArea
                rows={3}
                placeholder="请输入岗位职责与说明"
                disabled={formLoading || submitting}
                style={{ borderRadius: 'var(--radius-btn)' }}
              />
            </Form.Item>
            <Form.Item
              name="isEnabled"
              label="启用状态"
              valuePropName="checked"
              style={{ marginBottom: 0 }}
            >
              <Switch
                checkedChildren="启用"
                unCheckedChildren="停用"
                disabled={formLoading || submitting}
              />
            </Form.Item>
          </div>

          {/* 岗位权限选择卡片 */}
          <div className={styles.sectionCard} style={{ marginBottom: 0 }}>
            <div className={styles.sectionTitle}>
              <div className={styles.sectionTitleLeft}>
                <KeyOutlined style={{ color: 'var(--color-primary)' }} />
                <span>关联系统权限</span>
              </div>
              <Tag color="blue" style={{ borderRadius: 10 }}>
                已选 {selectedPermissionIds.length} 项
              </Tag>
            </div>
            <Form.Item name="permissionIds" hidden>
              <Input />
            </Form.Item>
            <div
              className={styles.permissionPanel}
              aria-disabled={formLoading || submitting || !formSystemId}
            >
              <div className={styles.permissionToolbar}>
                <Input.Search
                  allowClear
                  value={permissionSearch}
                  placeholder={formSystemId ? '搜索系统权限名称...' : '请先选择所属系统'}
                  onChange={(event) => setPermissionSearch(event.target.value)}
                  disabled={formLoading || submitting || !formSystemId}
                  className={styles.formControl}
                />
                <Space size={8} wrap style={{ justifyContent: 'flex-end' }}>
                  <Button
                    type="link"
                    size="small"
                    onClick={selectVisiblePermissions}
                    disabled={
                      !formSystemId ||
                      !visiblePermissionOptions.length ||
                      formLoading ||
                      submitting
                    }
                  >
                    全选当前过滤项
                  </Button>
                  <Button
                    type="link"
                    size="small"
                    onClick={clearVisiblePermissions}
                    disabled={
                      !selectedPermissionIds.length ||
                      !visiblePermissionOptions.length ||
                      formLoading ||
                      submitting
                    }
                  >
                    清除当前过滤项
                  </Button>
                </Space>
              </div>
              <List
                className={styles.permissionList}
                bordered
                size="small"
                loading={permissionLoading}
                dataSource={visiblePermissionOptions}
                locale={{
                  emptyText: permissionLoadError
                    ? '权限加载失败，请重试'
                    : formSystemId
                    ? '当前系统暂无匹配权限'
                    : '请选择系统以加载可选权限',
                }}
                renderItem={(permission) => (
                  <List.Item>
                    <Checkbox
                      checked={selectedPermissionIds.includes(permission.id)}
                      onChange={(event) =>
                        handlePermissionToggle(permission.id, event.target.checked)
                      }
                      disabled={formLoading || submitting || !formSystemId}
                    >
                      <span className={styles.permissionName}>{permission.name}</span>
                      {permission.description ? (
                        <Text type="secondary" className={styles.permissionDescription}>
                          {permission.description}
                        </Text>
                      ) : null}
                    </Checkbox>
                  </List.Item>
                )}
              />
            </div>
            {permissionLoadError ? (
              <Button
                type="link"
                size="small"
                className={styles.retryButton}
                onClick={() => void loadPermissions(formSystemId)}
              >
                重新加载权限
              </Button>
            ) : null}
          </div>
        </Form>
      </Modal>

      {/* 4. 岗位详情 Modal */}
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
                const row: PositionRowResultDto = {
                  id: detail.id,
                  systemId: detail.systemId,
                  systemName: detail.systemName,
                  name: detail.name,
                  description: detail.description,
                  isEnabled: detail.isEnabled,
                  permissionCount: detail.permissions.length,
                  organizationCount: detail.organizations.length,
                };
                void openForm(row);
              }
            }}
            style={{ borderRadius: 'var(--radius-btn)', backgroundColor: 'var(--color-primary)' }}
          >
            编辑此岗位
          </Button>,
          <Button
            key="close"
            onClick={() => setDetailOpen(false)}
            style={{ borderRadius: 'var(--radius-btn)' }}
          >
            关闭
          </Button>,
        ]}
        width={560}
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
                width: 130,
                fontWeight: 600,
                color: 'var(--color-text-secondary)',
                backgroundColor: 'var(--color-surface-subtle)',
              }}
              contentStyle={{ color: 'var(--color-title)' }}
            >
              <Descriptions.Item label="岗位 ID">
                <span
                  style={{ fontFamily: 'monospace', color: 'var(--color-body)', cursor: 'pointer' }}
                  onClick={() => void copyToClipboard(detail.id, '岗位 ID')}
                >
                  {detail.id} <CopyOutlined style={{ color: 'var(--color-placeholder)', marginLeft: 4 }} />
                </span>
              </Descriptions.Item>
              <Descriptions.Item label="岗位名称">{detail.name}</Descriptions.Item>
              <Descriptions.Item label="所属系统">{detail.systemName}</Descriptions.Item>
              <Descriptions.Item label="状态">
                {detail.isEnabled ? '启用' : '停用'}
              </Descriptions.Item>
              <Descriptions.Item label="岗位权限">
                {detail.permissions.length ? (
                  <Space wrap size={[0, 6]}>
                    {detail.permissions.map((p) => (
                      <Tag key={p.permissionId} color="blue" icon={<KeyOutlined />}>
                        {p.permissionName}
                      </Tag>
                    ))}
                  </Space>
                ) : (
                  <span style={{ color: 'var(--color-placeholder)' }}>暂无权限</span>
                )}
              </Descriptions.Item>
              <Descriptions.Item label="关联组织机构">
                {detail.organizations.length ? (
                  <Space wrap size={[0, 6]}>
                    {detail.organizations.map((o) => (
                      <Tag key={o.organizationId} color="cyan" icon={<BankOutlined />}>
                        {o.organizationName}
                      </Tag>
                    ))}
                  </Space>
                ) : (
                  <span style={{ color: 'var(--color-placeholder)' }}>暂无组织机构</span>
                )}
              </Descriptions.Item>
              <Descriptions.Item label="岗位说明">
                {detail.description || '暂无说明'}
              </Descriptions.Item>
            </Descriptions>
          </div>
        ) : (
          <Empty description="无法加载岗位详情" />
        )}
      </Modal>
    </div>
  );
}
