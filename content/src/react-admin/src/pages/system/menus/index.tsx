/**
 * @file 菜单管理页面 (MenuManagement)
 * @description 覆盖筛选、维护、详情与删除确认，基于通用 CrudToolbar / CrudTable / useCrudTable / CopyableIdTag 组件。
 */

import { useEffect, useMemo, useState } from 'react';
import {
  Button,
  Col,
  Descriptions,
  Form,
  Input,
  InputNumber,
  Modal,
  Popconfirm,
  Row,
  Select,
  Space,
  Switch,
  Tag,
  message,
} from 'antd';
import {
  DeleteOutlined,
  EditOutlined,
  EyeOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import {
  MenuApiService,
  PermissionApiService,
  SystemApiService,
  type MenuInputDto,
  type MenuResultDto,
  type MenuType,
  type PermissionRowResultDto,
  type SystemRowResultDto,
} from '../../../apiServices';
import {
  CrudTable,
  CrudToolbar,
  useCrudTable,
} from '../../../components';
import styles from './index.module.css';

/** 将服务端菜单结果转换为表单提交契约。 */
function toMenuInput(menu: MenuResultDto): MenuInputDto {
  return {
    systemId: menu.systemId,
    code: menu.code,
    name: menu.name,
    parentId: menu.parentId ?? undefined,
    type: menu.type,
    routePath: menu.routePath ?? undefined,
    component: menu.component ?? undefined,
    redirect: menu.redirect ?? undefined,
    icon: menu.icon ?? undefined,
    permissionId: menu.permissionId ?? undefined,
    sort: menu.sort,
    level: menu.level,
    isVisible: menu.isVisible,
    isDisabled: menu.isDisabled,
    isExternal: menu.isExternal,
    externalUrl: menu.externalUrl ?? undefined,
    keepAlive: menu.keepAlive,
    isAffix: menu.isAffix,
    description: menu.description ?? undefined,
  };
}

/** 菜单管理页面，覆盖筛选、维护、详情与删除确认。 */
export default function MenuManagement() {
  const [draftName, setDraftName] = useState('');
  const [name, setName] = useState('');
  const [editing, setEditing] = useState<MenuResultDto | null | undefined>(undefined);
  const [detail, setDetail] = useState<MenuResultDto | undefined>();
  const [systems, setSystems] = useState<SystemRowResultDto[]>([]);
  const [permissions, setPermissions] = useState<PermissionRowResultDto[]>([]);
  const [items, setItems] = useState<MenuResultDto[]>([]);
  const [form] = Form.useForm<MenuInputDto>();
  const selectedSystemId = Form.useWatch('systemId', form);
  const selectedType = Form.useWatch('type', form);
  const isExternal = Form.useWatch('isExternal', form);

  // 1. 通用 CRUD Hook 接管分页与数据拉取
  const queryFilters = useMemo(() => ({ name: name || undefined }), [name]);

  const {
    items: tableItems,
    loading,
    pagination,
    loadData: loadMenus,
    handleDelete,
  } = useCrudTable<MenuResultDto, { name?: string }>({
    fetchApi: MenuApiService.getPageList,
    deleteApi: MenuApiService.delete,
    filters: queryFilters,
  });

  useEffect(() => {
    void SystemApiService.getAll().then(setSystems).catch(() => message.error('加载系统选项失败。'));
  }, []);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      if (!selectedSystemId) {
        setPermissions([]);
        setItems([]);
        return;
      }

      void Promise.all([
        PermissionApiService.getAll(selectedSystemId),
        MenuApiService.getAll(selectedSystemId),
      ])
        .then(([permissionItems, menuItems]) => {
          setPermissions(permissionItems);
          setItems(menuItems);
        })
        .catch(() => message.error('加载权限或菜单选项失败。'));
    }, 0);
    return () => window.clearTimeout(timeoutId);
  }, [selectedSystemId]);

  /** 打开新增表单，并写入与领域规则一致的默认值。 */
  const openCreate = () => {
    setEditing(null);
    form.setFieldsValue({
      type: 2,
      sort: 0,
      level: 1,
      isVisible: true,
      isDisabled: false,
      isExternal: false,
      keepAlive: true,
      isAffix: false,
    });
  };

  const submitSearch = () => {
    setName(draftName.trim());
  };

  const resetSearch = () => {
    setDraftName('');
    setName('');
  };

  const saveMenu = async () => {
    const values = await form.validateFields();
    if (editing) {
      await MenuApiService.update(editing.id, values);
    } else {
      await MenuApiService.create(values);
    }
    message.success('菜单已保存。');
    setEditing(undefined);
    form.resetFields();
    void loadMenus();
  };

  const menuTypeLabel = (type: MenuType) => (type === 1 ? '目录' : type === 2 ? '页面' : '按钮');

  const columns: ColumnsType<MenuResultDto> = [
    { title: '菜单名称', dataIndex: 'name', key: 'name' },
    { title: '编码', dataIndex: 'code', key: 'code' },
    { title: '系统', dataIndex: 'systemName', key: 'systemName' },
    { title: '类型', dataIndex: 'type', key: 'type', render: menuTypeLabel },
    {
      title: '状态',
      key: 'status',
      render: (_, menu) =>
        menu.isDisabled ? <Tag color="error">禁用</Tag> : <Tag color="success">正常</Tag>,
    },
    {
      title: '操作',
      key: 'actions',
      fixed: 'right',
      render: (_, menu) => (
        <Space size={4}>
          <Button type="text" size="small" icon={<EyeOutlined />} onClick={() => setDetail(menu)}>
            详情
          </Button>
          <Button
            type="text"
            size="small"
            icon={<EditOutlined />}
            onClick={() => {
              setEditing(menu);
              form.setFieldsValue(toMenuInput(menu));
            }}
          >
            编辑
          </Button>
          <Popconfirm
            title="确认删除该菜单？"
            description="存在子菜单时系统会拒绝删除。"
            onConfirm={() => void handleDelete(menu.id, '菜单已删除。')}
          >
            <Button danger type="text" size="small" icon={<DeleteOutlined />}>
              删除
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <main className={styles.page}>
      {/* 1. 顶部检索工具栏 */}
      <CrudToolbar
        searchPlaceholder="按菜单名称搜索..."
        searchValue={draftName}
        onSearchChange={setDraftName}
        onSearch={submitSearch}
        onReset={resetSearch}
        createButton={{
          text: '新增菜单',
          onClick: openCreate,
        }}
      />

      {/* 2. 数据表格卡片 */}
      <CrudTable<MenuResultDto>
        rowKey="id"
        columns={columns}
        dataSource={tableItems}
        loading={loading}
        pagination={pagination}
        emptyText="暂无菜单数据"
        scroll={{ x: 860 }}
      />

      {/* 3. 新增 / 编辑菜单弹窗 */}
      <Modal
        open={editing !== undefined}
        title={editing ? '编辑菜单' : '新增菜单'}
        onOk={() => void saveMenu()}
        onCancel={() => {
          setEditing(undefined);
          form.resetFields();
        }}
        width={840}
        confirmLoading={loading}
      >
        <Form form={form} layout="vertical">
          <div className={styles.sectionCard}>
            <div className={styles.sectionTitle}>基本信息</div>
            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  name="systemId"
                  label="所属系统"
                  rules={[{ required: true, message: '请选择所属系统' }]}
                >
                  <Select options={systems.map((item) => ({ value: item.id, label: item.name }))} />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item name="type" label="菜单类型" rules={[{ required: true }]}>
                  <Select
                    options={[
                      { value: 1, label: '目录' },
                      { value: 2, label: '页面' },
                      { value: 3, label: '按钮' },
                    ]}
                  />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item name="name" label="菜单名称" rules={[{ required: true }]}>
                  <Input />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item name="code" label="菜单编码" rules={[{ required: true }]}>
                  <Input />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item
                  name="parentId"
                  label="父级菜单"
                  rules={selectedType === 3 ? [{ required: true, message: '按钮必须选择父级菜单' }] : []}
                >
                  <Select
                    allowClear
                    options={items
                      .filter((item) => item.id !== editing?.id && item.systemId === selectedSystemId)
                      .map((item) => ({ value: item.id, label: item.name }))}
                  />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item name="permissionId" label="关联权限">
                  <Select
                    allowClear
                    options={permissions
                      .filter((item) => item.systemId === selectedSystemId)
                      .map((item) => ({ value: item.id, label: item.name }))}
                  />
                </Form.Item>
              </Col>
            </Row>
          </div>
          <div className={styles.sectionCard}>
            <div className={styles.sectionTitle}>路由与显示</div>
            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  name="routePath"
                  label="路由路径"
                  rules={selectedType === 2 ? [{ required: true, message: '页面菜单必须填写路由路径' }] : []}
                >
                  <Input />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item name="component" label="组件路径">
                  <Input />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item name="sort" label="排序">
                  <InputNumber min={0} className={styles.numberInput} />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item name="level" label="层级">
                  <InputNumber min={1} className={styles.numberInput} />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item name="isExternal" valuePropName="checked" label="外链">
                  <Switch />
                </Form.Item>
              </Col>
              {isExternal && (
                <Col span={12}>
                  <Form.Item
                    name="externalUrl"
                    label="外链地址"
                    rules={[{ required: true, message: '外链菜单必须填写地址' }]}
                  >
                    <Input />
                  </Form.Item>
                </Col>
              )}
              <Col span={24}>
                <Form.Item name="description" label="说明">
                  <Input.TextArea rows={2} />
                </Form.Item>
              </Col>
            </Row>
          </div>
          <Space wrap>
            <Form.Item name="isVisible" valuePropName="checked" label="可见">
              <Switch />
            </Form.Item>
            <Form.Item name="isDisabled" valuePropName="checked" label="禁用">
              <Switch />
            </Form.Item>
            <Form.Item name="keepAlive" valuePropName="checked" label="缓存">
              <Switch />
            </Form.Item>
            <Form.Item name="isAffix" valuePropName="checked" label="固定标签">
              <Switch />
            </Form.Item>
          </Space>
        </Form>
      </Modal>

      {/* 4. 菜单详情 Modal */}
      <Modal
        open={detail !== undefined}
        title="菜单详情"
        footer={null}
        onCancel={() => setDetail(undefined)}
      >
        {detail && (
          <Descriptions
            bordered
            size="small"
            column={2}
            items={[
              { key: 'name', label: '菜单名称', children: detail.name },
              { key: 'code', label: '菜单编码', children: detail.code },
              { key: 'system', label: '所属系统', children: detail.systemName },
              { key: 'type', label: '菜单类型', children: menuTypeLabel(detail.type) },
              { key: 'route', label: '路由路径', children: detail.routePath ?? '-' },
              { key: 'permission', label: '关联权限', children: detail.permissionName ?? '-' },
              { key: 'status', label: '状态', children: detail.isDisabled ? '禁用' : '正常' },
              { key: 'description', label: '说明', children: detail.description ?? '-' },
            ]}
          />
        )}
      </Modal>
    </main>
  );
}
