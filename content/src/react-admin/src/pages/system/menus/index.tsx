import { useCallback, useEffect, useState } from 'react';
import { DeleteOutlined, EditOutlined, EyeOutlined, PlusOutlined, ReloadOutlined, SearchOutlined } from '@ant-design/icons';
import { Button, Card, Col, Descriptions, Empty, Form, Input, InputNumber, Modal, Popconfirm, Row, Select, Space, Switch, Table, Tag, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { MenuApiService, PermissionApiService, SystemApiService, type MenuInputDto, type MenuResultDto, type MenuType, type PermissionRowResultDto, type SystemRowResultDto } from '../../../apiServices';
import styles from './index.module.css';

const pageSize = 10;

/** 将服务端菜单结果转换为表单提交契约。 */
function toMenuInput(menu: MenuResultDto): MenuInputDto {
  return {
    systemId: menu.systemId, code: menu.code, name: menu.name, parentId: menu.parentId ?? undefined,
    type: menu.type, routePath: menu.routePath ?? undefined, component: menu.component ?? undefined,
    redirect: menu.redirect ?? undefined, icon: menu.icon ?? undefined, permissionId: menu.permissionId ?? undefined,
    sort: menu.sort, level: menu.level, isVisible: menu.isVisible, isDisabled: menu.isDisabled,
    isExternal: menu.isExternal, externalUrl: menu.externalUrl ?? undefined, keepAlive: menu.keepAlive,
    isAffix: menu.isAffix, description: menu.description ?? undefined,
  };
}

/** 菜单管理页面，覆盖筛选、维护、详情与删除确认。 */
export default function MenuManagement() {
  /** 当前页菜单。 */
  const [tableItems, setTableItems] = useState<MenuResultDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [draftName, setDraftName] = useState('');
  const [name, setName] = useState('');
  const [pageIndex, setPageIndex] = useState(1);
  const [editing, setEditing] = useState<MenuResultDto | null | undefined>(undefined);
  const [detail, setDetail] = useState<MenuResultDto | undefined>();
  const [systems, setSystems] = useState<SystemRowResultDto[]>([]);
  const [permissions, setPermissions] = useState<PermissionRowResultDto[]>([]);
  const [items, setItems] = useState<MenuResultDto[]>([]);
  const [form] = Form.useForm<MenuInputDto>();
  const selectedSystemId = Form.useWatch('systemId', form);
  const selectedType = Form.useWatch('type', form);
  const isExternal = Form.useWatch('isExternal', form);

  /** 使用已提交条件刷新当前分页数据。 */
  const loadMenus = useCallback(async () => {
    setLoading(true);
    try {
      const result = await MenuApiService.getPageList({ pageIndex, pageSize, name: name || undefined });
      setTableItems(result.items);
      setTotalCount(result.totalCount);
    } catch {
      setTableItems([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  }, [name, pageIndex]);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => void loadMenus(), 0);
    return () => window.clearTimeout(timeoutId);
  }, [loadMenus]);
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

      void Promise.all([PermissionApiService.getAll(selectedSystemId), MenuApiService.getAll(selectedSystemId)])
        .then(([permissionItems, menuItems]) => { setPermissions(permissionItems); setItems(menuItems); })
        .catch(() => message.error('加载权限或菜单选项失败。'));
    }, 0);
    return () => window.clearTimeout(timeoutId);
  }, [selectedSystemId]);

  /** 打开新增表单，并写入与领域规则一致的默认值。 */
  const openCreate = () => {
    setEditing(null);
    form.setFieldsValue({ type: 2, sort: 0, level: 1, isVisible: true, isDisabled: false, isExternal: false, keepAlive: true, isAffix: false });
  };
  const submitSearch = () => { setPageIndex(1); setName(draftName.trim()); };
  const resetSearch = () => { setDraftName(''); setName(''); setPageIndex(1); };
  const saveMenu = async () => {
    const values = await form.validateFields();
    if (editing) await MenuApiService.update(editing.id, values); else await MenuApiService.create(values);
    message.success('菜单已保存。'); setEditing(undefined); form.resetFields(); void loadMenus();
  };
  const deleteMenu = async (menu: MenuResultDto) => {
    await MenuApiService.delete(menu.id);
    message.success('菜单已删除。');
    if (tableItems.length === 1 && pageIndex > 1) setPageIndex(pageIndex - 1); else void loadMenus();
  };
  const menuTypeLabel = (type: MenuType) => (type === 1 ? '目录' : type === 2 ? '页面' : '按钮');
  const columns: ColumnsType<MenuResultDto> = [
    { title: '菜单名称', dataIndex: 'name', key: 'name' }, { title: '编码', dataIndex: 'code', key: 'code' },
    { title: '系统', dataIndex: 'systemName', key: 'systemName' }, { title: '类型', dataIndex: 'type', key: 'type', render: menuTypeLabel },
    { title: '状态', key: 'status', render: (_, menu) => menu.isDisabled ? <Tag color="error">禁用</Tag> : <Tag color="success">正常</Tag> },
    { title: '操作', key: 'actions', fixed: 'right', render: (_, menu) => <Space size={4}>
      <Button type="text" size="small" icon={<EyeOutlined />} onClick={() => setDetail(menu)}>详情</Button>
      <Button type="text" size="small" icon={<EditOutlined />} onClick={() => { setEditing(menu); form.setFieldsValue(toMenuInput(menu)); }}>编辑</Button>
      <Popconfirm title="确认删除该菜单？" description="存在子菜单时系统会拒绝删除。" onConfirm={() => void deleteMenu(menu)}><Button danger type="text" size="small" icon={<DeleteOutlined />}>删除</Button></Popconfirm>
    </Space> },
  ];

  return <main className={styles.page}>
    <Card className={styles.headerCard}><div className={styles.toolbar}><Input className={styles.search} allowClear prefix={<SearchOutlined />} placeholder="按菜单名称搜索" value={draftName} onChange={(event) => setDraftName(event.target.value)} onPressEnter={submitSearch} /><Space wrap><Button type="primary" onClick={submitSearch}>查询</Button><Button onClick={resetSearch}>重置</Button><Button icon={<ReloadOutlined spin={loading} />} onClick={() => void loadMenus()}>刷新</Button><Button type="primary" className="create-primary-button" icon={<PlusOutlined />} onClick={openCreate}>新增菜单</Button></Space></div></Card>
    <Card className={styles.tableCard}><Table<MenuResultDto> rowKey="id" columns={columns} dataSource={tableItems} loading={loading} scroll={{ x: 860 }} locale={{ emptyText: <Empty description="暂无菜单数据" /> }} pagination={{ current: pageIndex, pageSize, total: totalCount, onChange: setPageIndex }} /></Card>
    <Modal open={editing !== undefined} title={editing ? '编辑菜单' : '新增菜单'} onOk={() => void saveMenu()} onCancel={() => { setEditing(undefined); form.resetFields(); }} width={840} confirmLoading={loading}>
      <Form form={form} layout="vertical"><div className={styles.sectionCard}><div className={styles.sectionTitle}>基本信息</div><Row gutter={16}><Col span={12}><Form.Item name="systemId" label="所属系统" rules={[{ required: true, message: '请选择所属系统' }]}><Select options={systems.map((item) => ({ value: item.id, label: item.name }))} /></Form.Item></Col><Col span={12}><Form.Item name="type" label="菜单类型" rules={[{ required: true }]}><Select options={[{ value: 1, label: '目录' }, { value: 2, label: '页面' }, { value: 3, label: '按钮' }]} /></Form.Item></Col><Col span={12}><Form.Item name="name" label="菜单名称" rules={[{ required: true }]}><Input /></Form.Item></Col><Col span={12}><Form.Item name="code" label="菜单编码" rules={[{ required: true }]}><Input /></Form.Item></Col><Col span={12}><Form.Item name="parentId" label="父级菜单" rules={selectedType === 3 ? [{ required: true, message: '按钮必须选择父级菜单' }] : []}><Select allowClear options={items.filter((item) => item.id !== editing?.id && item.systemId === selectedSystemId).map((item) => ({ value: item.id, label: item.name }))} /></Form.Item></Col><Col span={12}><Form.Item name="permissionId" label="关联权限"><Select allowClear options={permissions.filter((item) => item.systemId === selectedSystemId).map((item) => ({ value: item.id, label: item.name }))} /></Form.Item></Col></Row></div><div className={styles.sectionCard}><div className={styles.sectionTitle}>路由与显示</div><Row gutter={16}><Col span={12}><Form.Item name="routePath" label="路由路径" rules={selectedType === 2 ? [{ required: true, message: '页面菜单必须填写路由路径' }] : []}><Input /></Form.Item></Col><Col span={12}><Form.Item name="component" label="组件路径"><Input /></Form.Item></Col><Col span={12}><Form.Item name="sort" label="排序"><InputNumber min={0} className={styles.numberInput} /></Form.Item></Col><Col span={12}><Form.Item name="level" label="层级"><InputNumber min={1} className={styles.numberInput} /></Form.Item></Col><Col span={12}><Form.Item name="isExternal" valuePropName="checked" label="外链"><Switch /></Form.Item></Col>{isExternal && <Col span={12}><Form.Item name="externalUrl" label="外链地址" rules={[{ required: true, message: '外链菜单必须填写地址' }]}><Input /></Form.Item></Col>}<Col span={24}><Form.Item name="description" label="说明"><Input.TextArea rows={2} /></Form.Item></Col></Row></div><Space wrap><Form.Item name="isVisible" valuePropName="checked" label="可见"><Switch /></Form.Item><Form.Item name="isDisabled" valuePropName="checked" label="禁用"><Switch /></Form.Item><Form.Item name="keepAlive" valuePropName="checked" label="缓存"><Switch /></Form.Item><Form.Item name="isAffix" valuePropName="checked" label="固定标签"><Switch /></Form.Item></Space></Form>
    </Modal>
    <Modal open={detail !== undefined} title="菜单详情" footer={null} onCancel={() => setDetail(undefined)}>{detail && <Descriptions bordered size="small" column={2} items={[{ key: 'name', label: '菜单名称', children: detail.name }, { key: 'code', label: '菜单编码', children: detail.code }, { key: 'system', label: '所属系统', children: detail.systemName }, { key: 'type', label: '菜单类型', children: menuTypeLabel(detail.type) }, { key: 'route', label: '路由路径', children: detail.routePath ?? '-' }, { key: 'permission', label: '关联权限', children: detail.permissionName ?? '-' }, { key: 'status', label: '状态', children: detail.isDisabled ? '禁用' : '正常' }, { key: 'description', label: '说明', children: detail.description ?? '-' }]} />}</Modal>
  </main>;
}
