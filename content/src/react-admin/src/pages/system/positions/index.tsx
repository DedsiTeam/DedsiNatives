/**
 * @file 岗位管理页面 (PositionManagement)
 * @description 直连 PositionApiService, PermissionApiService, SystemApiService，对应 PositionResultDto, CreatePositionInputDto 等类型。
 * 基于通用的 CrudToolbar / CrudTable / useCrudTable / CopyableIdTag 组件实现高复用度布局与统一标准。
 */

import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Button,
  Checkbox,
  Col,
  Descriptions,
  Empty,
  Form,
  Input,
  List,
  Modal,
  Popconfirm,
  Row,
  Select,
  Space,
  Switch,
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
import {
  CopyableIdTag,
  CrudTable,
  CrudToolbar,
  useCrudTable,
} from '../../../components';
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

/** 岗位管理页面，负责岗位资料、状态和关联数量展示。 */
import { checkPermission } from '../../../components/Auth';
import { PERMISSIONS } from '../../../auth/permissions';

export default function PositionManagement() {
  const canCreate = checkPermission(PERMISSIONS.positions.create);
  const canUpdate = checkPermission(PERMISSIONS.positions.update);
  const canDelete = checkPermission(PERMISSIONS.positions.delete);
  const canAssign = checkPermission(PERMISSIONS.positions.assign);
  // 1. 系统选项与搜索筛选状态
  const [systems, setSystems] = useState<SystemRowResultDto[]>([]);
  const [draftName, setDraftName] = useState('');
  const [name, setName] = useState('');
  const [systemId, setSystemId] = useState<string | undefined>();
  const [status, setStatus] = useState<boolean | undefined>();

  // 2. 弹窗与表单状态
  const [editing, setEditing] = useState<PositionRowResultDto | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
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

  // 3. 详情弹窗状态
  const [detailOpen, setDetailOpen] = useState(false);
  const [detail, setDetail] = useState<PositionResultDto | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);

  /** 加载系统选项 */
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
    PositionRowResultDto,
    { systemId?: string; name?: string; isEnabled?: boolean }
  >({
    fetchApi: PositionApiService.getPageList,
    deleteApi: PositionApiService.delete,
    filters: queryFilters,
  });

  /** 提交筛选条件 */
  const handleSearch = () => {
    setName(draftName.trim());
  };

  /** 重置筛选条件 */
  const handleResetSearch = () => {
    setDraftName('');
    setName('');
    setSystemId(undefined);
    setStatus(undefined);
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

  /** 系统切换时重置权限选择并加载新系统的可选权限。 */
  const handleFormSystemChange = async (nextSystemId: string) => {
    form.setFieldValue('permissionIds', []);
    setPermissionSearch('');
    await loadPermissions(nextSystemId);
  };

  /** 切换权限选中状态 */
  const handlePermissionToggle = (permissionId: string, checked: boolean) => {
    const nextIds = checked
      ? Array.from(new Set([...selectedPermissionIds, permissionId]))
      : selectedPermissionIds.filter((id) => id !== permissionId);
    form.setFieldValue('permissionIds', nextIds);
  };

  /** 全选当前可见权限 */
  const selectVisiblePermissions = () => {
    const visibleIds = visiblePermissionOptions.map((permission) => permission.id);
    const nextIds = Array.from(new Set([...selectedPermissionIds, ...visibleIds]));
    form.setFieldValue('permissionIds', nextIds);
  };

  /** 清除当前可见权限的选择 */
  const clearVisiblePermissions = () => {
    const visibleIds = new Set(visiblePermissionOptions.map((permission) => permission.id));
    const nextIds = selectedPermissionIds.filter((id) => !visibleIds.has(id));
    form.setFieldValue('permissionIds', nextIds);
  };

  /** 提交新增或修改岗位。 */
  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      setSubmitting(true);
      if (editing) {
        const updateInput: UpdatePositionInputDto = {
          name: values.name,
          systemId: values.systemId,
          description: values.description,
        };
        await PositionApiService.update(editing.id, updateInput);
        await PositionApiService.updateAssignments(editing.id, {
          permissionIds: values.permissionIds ?? [],
          organizations: editingOrganizations,
        });
        if (values.isEnabled !== editing.isEnabled) {
          await PositionApiService.setStatus(editing.id, { isEnabled: values.isEnabled });
        }
        message.success('岗位信息已更新');
      } else {
        const createInput: CreatePositionInputDto = {
          name: values.name,
          systemId: values.systemId,
          description: values.description,
          isEnabled: values.isEnabled,
          permissionIds: values.permissionIds ?? [],
          organizations: editingOrganizations,
        };
        await PositionApiService.create(createInput);
        message.success('岗位已创建');
      }
      setModalOpen(false);
      form.resetFields();
      await loadData();
    } catch {
      // 表单错误由 Form 自行展示
    } finally {
      setSubmitting(false);
    }
  };

  /** 直接切换岗位启用状态。 */
  const handleStatusChange = async (item: PositionRowResultDto, isEnabled: boolean) => {
    try {
      await PositionApiService.setStatus(item.id, { isEnabled });
      message.success(isEnabled ? '岗位已启用' : '岗位已停用');
      await loadData();
    } catch {
      // 统一拦截器处理
    }
  };

  /** 打开岗位详情弹窗。 */
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

  // 5. 表格列定义
  const columns: TableProps<PositionRowResultDto>['columns'] = [
    {
      title: '岗位名称与归属',
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
      title: '岗位 ID',
      dataIndex: 'id',
      key: 'id',
      width: 260,
      render: (id: string) => <CopyableIdTag id={id} label="岗位 ID" />,
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
            disabled={!canUpdate}
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
              hidden={!canUpdate || !canAssign}
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
            onConfirm={() => void handleDelete(record.id, '岗位已删除')}
          >
            <Button type="text" danger icon={<DeleteOutlined />} size="small" hidden={!canDelete} style={{ fontWeight: 500 }}>
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
        searchPlaceholder="按岗位名称搜索..."
        searchValue={draftName}
        onSearchChange={setDraftName}
        onSearch={handleSearch}
        onReset={handleResetSearch}
        createButton={{
          text: '新增岗位',
          hidden: !canCreate,
          onClick: () => void openForm(),
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
              options={systems.map((item) => ({ label: item.name, value: item.id }))}
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
      <CrudTable<PositionRowResultDto>
        rowKey="id"
        columns={columns}
        dataSource={items}
        loading={loading}
        pagination={pagination}
        emptyText="暂无岗位数据"
        scroll={{ x: 1080 }}
      />

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
        width={860}
      >
        {formLoading ? (
          <div className={styles.formLoading}>正在加载岗位权限配置...</div>
        ) : null}
        <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item name="permissionIds" hidden>
            <Input />
          </Form.Item>

          <Row gutter={[20, 20]}>
            {/* 左侧：岗位基本资料 */}
            <Col xs={24} md={10}>
              <div className={styles.modalColCard}>
                <div className={styles.colHeader}>
                  <Space size={6}>
                    <SolutionOutlined style={{ color: 'var(--color-primary)' }} />
                    <span className={styles.colTitle}>岗位基本资料</span>
                  </Space>
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
                    rows={4}
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
            </Col>

            {/* 右侧：关联系统权限 */}
            <Col xs={24} md={14}>
              <div className={styles.modalColCard}>
                <div className={styles.colHeader}>
                  <Space size={6}>
                    <KeyOutlined style={{ color: 'var(--color-primary)' }} />
                    <span className={styles.colTitle}>关联系统权限</span>
                  </Space>
                  <Tag color="blue" style={{ borderRadius: 10, margin: 0 }}>
                    已选 {selectedPermissionIds.length} 项
                  </Tag>
                </div>

                <div
                  className={styles.permissionPanel}
                  aria-disabled={formLoading || submitting || !formSystemId}
                >
                  <div className={styles.permissionToolbar}>
                    <Input.Search
                      allowClear
                      value={permissionSearch}
                      placeholder={formSystemId ? '搜索权限名称或说明...' : '请先在左侧选择所属系统'}
                      onChange={(event) => setPermissionSearch(event.target.value)}
                      disabled={formLoading || submitting || !formSystemId}
                      className={styles.formControl}
                    />
                    <div className={styles.permissionActions}>
                      <Text type="secondary" style={{ fontSize: 12 }}>
                        {formSystemId ? `共 ${visiblePermissionOptions.length} 项可选` : '未选择系统'}
                      </Text>
                      <Space size={8}>
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
                          全选当前
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
                          清除已选
                        </Button>
                      </Space>
                    </div>
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
                        : '请先在左侧选择系统以加载可选权限',
                    }}
                    renderItem={(permission) => {
                      const isChecked = selectedPermissionIds.includes(permission.id);
                      return (
                        <List.Item
                          className={`${styles.permissionItem} ${isChecked ? styles.permissionItemSelected : ''}`}
                          onClick={() => {
                            if (!formLoading && !submitting && formSystemId) {
                              handlePermissionToggle(permission.id, !isChecked);
                            }
                          }}
                        >
                          <Checkbox
                            checked={isChecked}
                            onChange={(event) =>
                              handlePermissionToggle(permission.id, event.target.checked)
                            }
                            disabled={formLoading || submitting || !formSystemId}
                            onClick={(e) => e.stopPropagation()}
                          >
                            <div className={styles.permissionItemContent}>
                              <span className={styles.permissionName}>{permission.name}</span>
                              {permission.description ? (
                                <Text type="secondary" className={styles.permissionDescription}>
                                   {permission.description}
                                </Text>
                              ) : null}
                            </div>
                          </Checkbox>
                        </List.Item>
                      );
                    }}
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
            </Col>
          </Row>
        </Form>
      </Modal>

      {/* 4. 岗位详情 Modal */}
      <Modal
        title={
          <Space size={8}>
            <SolutionOutlined style={{ color: 'var(--color-primary)' }} />
            <span style={{ fontWeight: 700, fontSize: 16 }}>岗位详情</span>
          </Space>
        }
        open={detailOpen}
        onCancel={() => setDetailOpen(false)}
        footer={null}
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
                <CopyableIdTag id={detail.id} label="岗位 ID" />
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
