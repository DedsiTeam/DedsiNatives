import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  Button,
  Card,
  Empty,
  Form,
  Input,
  InputNumber,
  message,
  Modal,
  Select,
  Space,
  Switch,
  Table,
  Tag,
  Typography,
  type TableProps,
} from 'antd';
import {
  BookOutlined,
  EditOutlined,
  EyeOutlined,
  PlusOutlined,
  ReloadOutlined,
  SearchOutlined,
} from '@ant-design/icons';
import {
  DictionaryApiService,
  SystemApiService,
  type DictionaryItemResultDto,
  type DictionaryResultDto,
  type DictionaryRowResultDto,
  type SaveDictionaryInputDto,
  type SaveDictionaryItemInputDto,
  type SystemRowResultDto,
} from '../../../apiServices';
import styles from './index.module.css';

/** 将字典项转换为更新接口要求的完整输入。 */
function toItemInput(item: DictionaryItemResultDto): SaveDictionaryItemInputDto {
  return {
    code: item.code,
    name: item.name,
    description: item.description,
    sort: item.sort,
    isEnabled: item.isEnabled,
    isDefault: item.isDefault,
    parentId: item.parentId,
  };
}

/** 字典分组与字典项的一体化管理页面。 */
export default function DictionaryManagement() {
  const [groups, setGroups] = useState<DictionaryRowResultDto[]>([]);
  const [systems, setSystems] = useState<SystemRowResultDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [pageIndex, setPageIndex] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [draftName, setDraftName] = useState('');
  const [draftSystemId, setDraftSystemId] = useState<string>();
  const [name, setName] = useState('');
  const [systemId, setSystemId] = useState<string>();

  const [groupForm] = Form.useForm<SaveDictionaryInputDto>();
  const [groupModalOpen, setGroupModalOpen] = useState(false);
  const [editingGroup, setEditingGroup] = useState<DictionaryRowResultDto | null>(null);
  const [groupSubmitting, setGroupSubmitting] = useState(false);

  const [detailOpen, setDetailOpen] = useState(false);
  const [detailLoading, setDetailLoading] = useState(false);
  const [detail, setDetail] = useState<DictionaryResultDto | null>(null);
  const [itemForm] = Form.useForm<SaveDictionaryItemInputDto>();
  const [itemModalOpen, setItemModalOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<DictionaryItemResultDto | null>(null);
  const [itemSubmitting, setItemSubmitting] = useState(false);
  const [togglingItemId, setTogglingItemId] = useState<string>();

  const loadGroups = useCallback(async () => {
    setLoading(true);
    try {
      const result = await DictionaryApiService.getPageList({
        pageIndex,
        pageSize,
        systemId,
        name: name || undefined,
      });
      setGroups(result.items ?? []);
      setTotalCount(result.totalCount ?? 0);
    } catch {
      setGroups([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  }, [name, pageIndex, pageSize, systemId]);

  useEffect(() => {
    void SystemApiService.getAll().then(setSystems).catch(() => setSystems([]));
  }, []);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => void loadGroups(), 0);
    return () => window.clearTimeout(timeoutId);
  }, [loadGroups]);

  const loadDetail = useCallback(async (id: string) => {
    setDetailLoading(true);
    try {
      setDetail(await DictionaryApiService.getById(id));
    } catch {
      setDetail(null);
    } finally {
      setDetailLoading(false);
    }
  }, []);

  const handleSearch = () => {
    setPageIndex(1);
    setName(draftName.trim());
    setSystemId(draftSystemId);
  };

  const handleReset = () => {
    setDraftName('');
    setDraftSystemId(undefined);
    setName('');
    setSystemId(undefined);
    setPageIndex(1);
  };

  const openGroupForm = (group?: DictionaryRowResultDto) => {
    setEditingGroup(group ?? null);
    groupForm.setFieldsValue({
      systemId: group?.systemId,
      name: group?.name ?? '',
    });
    setGroupModalOpen(true);
  };

  const submitGroup = async () => {
    try {
      const values = await groupForm.validateFields();
      setGroupSubmitting(true);
      if (editingGroup) {
        await DictionaryApiService.update(editingGroup.id, values);
        message.success('字典分组已更新');
      } else {
        await DictionaryApiService.create(values);
        message.success('字典分组已创建');
      }
      setGroupModalOpen(false);
      groupForm.resetFields();
      await loadGroups();
    } catch {
      // 表单校验和请求错误分别由 Ant Design 与请求拦截器展示。
    } finally {
      setGroupSubmitting(false);
    }
  };

  const openDetail = (group: DictionaryRowResultDto) => {
    setDetailOpen(true);
    setDetail(null);
    void loadDetail(group.id);
  };

  const openItemForm = (item?: DictionaryItemResultDto) => {
    setEditingItem(item ?? null);
    itemForm.setFieldsValue(
      item
        ? toItemInput(item)
        : {
            code: '',
            name: '',
            description: null,
            sort: 0,
            isEnabled: true,
            isDefault: false,
            parentId: null,
          },
    );
    setItemModalOpen(true);
  };

  const submitItem = async () => {
    if (!detail) return;
    try {
      const values = await itemForm.validateFields();
      setItemSubmitting(true);
      const input: SaveDictionaryItemInputDto = {
        ...values,
        description: values.description?.trim() || null,
        parentId: values.parentId || null,
      };
      if (editingItem) {
        await DictionaryApiService.updateItem(detail.id, editingItem.id, input);
        message.success('字典项已更新');
      } else {
        await DictionaryApiService.createItem(detail.id, input);
        message.success('字典项已创建');
      }
      setItemModalOpen(false);
      itemForm.resetFields();
      await loadDetail(detail.id);
      await loadGroups();
    } catch {
      // 表单校验和请求错误分别由 Ant Design 与请求拦截器展示。
    } finally {
      setItemSubmitting(false);
    }
  };

  const toggleEnabled = async (item: DictionaryItemResultDto, checked: boolean) => {
    if (!detail) return;
    setTogglingItemId(item.id);
    try {
      await DictionaryApiService.updateItem(detail.id, item.id, {
        ...toItemInput(item),
        isEnabled: checked,
        isDefault: checked ? item.isDefault : false,
      });
      message.success(checked ? '字典项已启用' : '字典项已停用');
      await loadDetail(detail.id);
    } catch {
      // 请求拦截器统一展示错误。
    } finally {
      setTogglingItemId(undefined);
    }
  };

  const parentOptions = useMemo(
    () =>
      (detail?.items ?? [])
        .filter((item) => item.id !== editingItem?.id)
        .map((item) => ({ label: `${item.name} (${item.code})`, value: item.id })),
    [detail?.items, editingItem?.id],
  );

  const groupColumns: TableProps<DictionaryRowResultDto>['columns'] = [
    {
      title: '字典分组',
      dataIndex: 'name',
      key: 'name',
      width: 300,
      render: (value: string) => <Typography.Text strong>{value}</Typography.Text>,
    },
    { title: '所属系统', dataIndex: 'systemName', key: 'systemName', width: 220 },
    {
      title: '字典项',
      dataIndex: 'itemCount',
      key: 'itemCount',
      width: 110,
      render: (count: number) => <Tag color="blue">{count} 项</Tag>,
    },
    {
      title: '操作',
      key: 'actions',
      width: 220,
      fixed: 'right',
      render: (_, group) => (
        <Space>
          <Button type="link" icon={<EyeOutlined />} onClick={() => openDetail(group)}>
            管理字典项
          </Button>
          <Button type="link" icon={<EditOutlined />} onClick={() => openGroupForm(group)}>
            编辑
          </Button>
        </Space>
      ),
    },
  ];

  const itemColumns: TableProps<DictionaryItemResultDto>['columns'] = [
    {
      title: '名称 / 编码',
      key: 'identity',
      render: (_, item) => (
        <div className={styles.itemIdentity}>
          <Typography.Text strong>{item.name}</Typography.Text>
          <Typography.Text code>{item.code}</Typography.Text>
        </div>
      ),
    },
    {
      title: '父级',
      dataIndex: 'parentId',
      width: 150,
      render: (parentId: string | null) =>
        detail?.items.find((item) => item.id === parentId)?.name ?? '-',
    },
    { title: '排序', dataIndex: 'sort', width: 80 },
    {
      title: '默认',
      dataIndex: 'isDefault',
      width: 80,
      render: (isDefault: boolean) => (isDefault ? <Tag color="gold">默认</Tag> : '-'),
    },
    {
      title: '启用',
      dataIndex: 'isEnabled',
      width: 90,
      render: (isEnabled: boolean, item) => (
        <Switch
          size="small"
          checked={isEnabled}
          loading={togglingItemId === item.id}
          onChange={(checked) => void toggleEnabled(item, checked)}
        />
      ),
    },
    {
      title: '操作',
      key: 'actions',
      width: 90,
      render: (_, item) => (
        <Button type="link" icon={<EditOutlined />} onClick={() => openItemForm(item)}>
          编辑
        </Button>
      ),
    },
  ];

  return (
    <div className={styles.pageContainer}>
      <Card className={styles.toolbarCard}>
        <div className={styles.toolbar}>
          <div className={styles.filters}>
            <Select
              allowClear
              showSearch
              optionFilterProp="label"
              className={styles.systemSelect}
              placeholder="选择所属系统"
              value={draftSystemId}
              options={systems.map((system) => ({ label: system.name, value: system.id }))}
              onChange={setDraftSystemId}
            />
            <Input
              allowClear
              className={styles.nameInput}
              prefix={<SearchOutlined />}
              placeholder="搜索字典分组名称"
              value={draftName}
              onChange={(event) => setDraftName(event.target.value)}
              onPressEnter={handleSearch}
            />
            <Button type="primary" icon={<SearchOutlined />} onClick={handleSearch}>查询</Button>
            <Button icon={<ReloadOutlined />} onClick={handleReset}>重置</Button>
          </div>
          <Space>
            <Button icon={<ReloadOutlined spin={loading} />} onClick={() => void loadGroups()}>刷新</Button>
            <Button
              type="primary"
              className="create-primary-button"
              icon={<PlusOutlined />}
              onClick={() => openGroupForm()}
            >
              新增字典分组
            </Button>
          </Space>
        </div>
      </Card>

      <Card className={styles.tableCard}>
        <Table<DictionaryRowResultDto>
          rowKey="id"
          columns={groupColumns}
          dataSource={groups}
          loading={loading}
          locale={{ emptyText: <Empty description="暂无字典分组" /> }}
          scroll={{ x: 760 }}
          pagination={{
            current: pageIndex,
            pageSize,
            total: totalCount,
            showSizeChanger: true,
            showTotal: (total) => `共 ${total} 个字典分组`,
            onChange: (nextPage, nextSize) => {
              setPageIndex(nextSize === pageSize ? nextPage : 1);
              setPageSize(nextSize);
            },
          }}
        />
      </Card>

      <Modal
        title={editingGroup ? '编辑字典分组' : '新增字典分组'}
        open={groupModalOpen}
        confirmLoading={groupSubmitting}
        okText="保存"
        cancelText="取消"
        maskClosable={!groupSubmitting}
        onOk={() => void submitGroup()}
        onCancel={() => !groupSubmitting && setGroupModalOpen(false)}
      >
        <Form form={groupForm} layout="vertical" className={styles.modalForm}>
          <Form.Item name="systemId" label="所属系统" rules={[{ required: true, message: '请选择所属系统' }]}>
            <Select
              showSearch
              optionFilterProp="label"
              placeholder="请选择所属系统"
              options={systems.map((system) => ({ label: system.name, value: system.id }))}
            />
          </Form.Item>
          <Form.Item
            name="name"
            label="字典分组名称"
            rules={[
              { required: true, whitespace: true, message: '请输入字典分组名称' },
              { max: 128, message: '名称不能超过 128 个字符' },
            ]}
          >
            <Input placeholder="例如：用户状态" />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={
          <Space>
            <BookOutlined />
            <span>{detail ? `${detail.name} · 字典项管理` : '字典项管理'}</span>
          </Space>
        }
        open={detailOpen}
        width={1000}
        footer={<Button onClick={() => setDetailOpen(false)}>关闭</Button>}
        onCancel={() => setDetailOpen(false)}
      >
        <div className={styles.detailToolbar}>
          <div>
            <Typography.Text type="secondary">所属系统：</Typography.Text>
            <Typography.Text strong>{detail?.systemName ?? '-'}</Typography.Text>
          </div>
          <Button
            type="primary"
            className="create-primary-button"
            icon={<PlusOutlined />}
            disabled={!detail}
            onClick={() => openItemForm()}
          >
            新增字典项
          </Button>
        </div>
        <Table<DictionaryItemResultDto>
          rowKey="id"
          size="small"
          columns={itemColumns}
          dataSource={detail?.items ?? []}
          loading={detailLoading}
          pagination={false}
          scroll={{ x: 760, y: 420 }}
          locale={{ emptyText: <Empty description="该分组暂无字典项" /> }}
        />
      </Modal>

      <Modal
        title={editingItem ? '编辑字典项' : '新增字典项'}
        open={itemModalOpen}
        width={640}
        confirmLoading={itemSubmitting}
        okText="保存"
        cancelText="取消"
        maskClosable={!itemSubmitting}
        onOk={() => void submitItem()}
        onCancel={() => !itemSubmitting && setItemModalOpen(false)}
      >
        <Form form={itemForm} layout="vertical" className={styles.modalForm}>
          <div className={styles.formGrid}>
            <Form.Item
              name="code"
              label="业务编码"
              rules={[
                { required: true, whitespace: true, message: '请输入业务编码' },
                { max: 128, message: '编码不能超过 128 个字符' },
              ]}
            >
              <Input placeholder="例如：enabled" />
            </Form.Item>
            <Form.Item
              name="name"
              label="显示名称"
              rules={[
                { required: true, whitespace: true, message: '请输入显示名称' },
                { max: 128, message: '名称不能超过 128 个字符' },
              ]}
            >
              <Input placeholder="例如：启用" />
            </Form.Item>
            <Form.Item name="parentId" label="父字典项">
              <Select allowClear placeholder="无父级" options={parentOptions} />
            </Form.Item>
            <Form.Item name="sort" label="展示排序" rules={[{ required: true, message: '请输入展示排序' }]}>
              <InputNumber precision={0} className={styles.fullWidth} />
            </Form.Item>
          </div>
          <Form.Item name="description" label="说明" rules={[{ max: 512, message: '说明不能超过 512 个字符' }]}>
            <Input.TextArea rows={3} placeholder="可选，说明该字典项的业务含义" />
          </Form.Item>
          <Space size="large">
            <Form.Item name="isEnabled" label="启用" valuePropName="checked">
              <Switch />
            </Form.Item>
            <Form.Item name="isDefault" label="默认项" valuePropName="checked">
              <Switch />
            </Form.Item>
          </Space>
        </Form>
      </Modal>
    </div>
  );
}
