import { useCallback, useEffect, useMemo, useState } from 'react';
import {
  ApartmentOutlined,
  CheckCircleOutlined,
  DeleteOutlined,
  EditOutlined,
  EyeOutlined,
  PlusOutlined,
  SearchOutlined,
  StopOutlined,
} from '@ant-design/icons';
import {
  Avatar,
  Button,
  Card,
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
  Table,
  Tag,
  Tooltip,
  TreeSelect,
  Typography,
  message,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  OrganizationApiService,
  SystemApiService,
  type CreateOrganizationRequestDto,
  type OrganizationDetailResultDto,
  type OrganizationTreeNodeResultDto,
  type SystemRowResultDto,
  type UpdateOrganizationRequestDto,
} from '../../../apiServices';
import styles from './index.module.css';

const { Text } = Typography;

interface OrganizationFormValues {
  systemId: string;
  code: string;
  name: string;
  name1?: string;
  name2?: string;
  name3?: string;
  name4?: string;
  parentId?: string;
  sort: number;
  description?: string;
}

import { checkPermission } from '../../../components/Auth';
import { PERMISSIONS } from '../../../auth/permissions';

export default function OrganizationManagement() {
  const canCreate = checkPermission(PERMISSIONS.organizations.create);
  const canUpdate = checkPermission(PERMISSIONS.organizations.update);
  const canDelete = checkPermission(PERMISSIONS.organizations.delete);
  const [treeData, setTreeData] = useState<OrganizationTreeNodeResultDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [systems, setSystems] = useState<SystemRowResultDto[]>([]);
  const [selectedSystemId, setSelectedSystemId] = useState<string>('');

  // 检索草稿与实际生效条件
  const [draftKeyword, setDraftKeyword] = useState('');
  const [keyword, setKeyword] = useState('');

  // 弹窗状态
  const [editingOrg, setEditingOrg] = useState<OrganizationDetailResultDto | null | undefined>(undefined);
  const [detailOrg, setDetailOrg] = useState<OrganizationDetailResultDto | undefined>();
  const [form] = Form.useForm<OrganizationFormValues>();

  // 加载系统列表
  useEffect(() => {
    void SystemApiService.getAll()
      .then((res) => {
        setSystems(res);
        if (res.length > 0) {
          setSelectedSystemId(res[0].id);
        }
      })
      .catch(() => message.error('加载系统列表失败。'));
  }, []);

  /** 递归清理空子数组，确保无下级节点的行不渲染折叠展开图标 */
  const cleanTreeNodes = (nodes: OrganizationTreeNodeResultDto[]): OrganizationTreeNodeResultDto[] => {
    return nodes.map((node) => {
      const hasChildren = Boolean(node.children && node.children.length > 0);
      return {
        ...node,
        children: hasChildren ? cleanTreeNodes(node.children!) : undefined,
      };
    });
  };

  // 加载组织树
  const loadTree = useCallback(async () => {
    if (!selectedSystemId) return;
    setLoading(true);
    try {
      const res = await OrganizationApiService.getOrganizationTree(selectedSystemId);
      setTreeData(cleanTreeNodes(res || []));
    } catch {
      setTreeData([]);
    } finally {
      setLoading(false);
    }
  }, [selectedSystemId]);

  useEffect(() => {
    void loadTree();
  }, [loadTree]);

  // 前端关键字过滤
  const filteredTree = useMemo(() => {
    if (!keyword.trim()) return cleanTreeNodes(treeData);
    const kw = keyword.trim().toLowerCase();

    function filterNodes(nodes: OrganizationTreeNodeResultDto[]): OrganizationTreeNodeResultDto[] {
      const result: OrganizationTreeNodeResultDto[] = [];
      for (const node of nodes) {
        const matchesSelf =
          node.name.toLowerCase().includes(kw) ||
          node.code.toLowerCase().includes(kw) ||
          (node.name1 && node.name1.toLowerCase().includes(kw)) ||
          (node.name2 && node.name2.toLowerCase().includes(kw)) ||
          (node.name3 && node.name3.toLowerCase().includes(kw)) ||
          (node.name4 && node.name4.toLowerCase().includes(kw));

        const matchedChildren = node.children && node.children.length > 0 ? filterNodes(node.children) : [];
        if (matchesSelf || matchedChildren.length > 0) {
          result.push({
            ...node,
            children: matchedChildren.length > 0 ? matchedChildren : undefined,
          });
        }
      }
      return result;
    }

    return filterNodes(treeData);
  }, [treeData, keyword]);

  // 构建 TreeSelect 数据源
  const treeSelectOptions = useMemo(() => {
    function mapNode(node: OrganizationTreeNodeResultDto): {
      title: string;
      value: string;
      disabled?: boolean;
      children?: { title: string; value: string }[];
    } {
      const isSelf = editingOrg && node.id === editingOrg.id;
      return {
        title: `${node.name} (${node.code})`,
        value: node.id,
        disabled: Boolean(isSelf),
        children: node.children && node.children.length > 0 ? node.children.map(mapNode) : undefined,
      };
    }
    return treeData.map(mapNode);
  }, [treeData, editingOrg]);

  const submitSearch = () => {
    setKeyword(draftKeyword.trim());
  };

  const resetSearch = () => {
    setDraftKeyword('');
    setKeyword('');
  };

  // 打开新增弹窗
  const openCreate = (parent?: OrganizationTreeNodeResultDto) => {
    setEditingOrg(null);
    form.resetFields();
    form.setFieldsValue({
      systemId: selectedSystemId,
      parentId: parent?.id,
      sort: 0,
    });
  };

  // 打开编辑弹窗
  const openEdit = (org: OrganizationTreeNodeResultDto) => {
    const detail: OrganizationDetailResultDto = {
      id: org.id,
      systemId: org.systemId,
      systemName: org.systemName,
      code: org.code,
      name: org.name,
      name1: org.name1,
      name2: org.name2,
      name3: org.name3,
      name4: org.name4,
      parentId: org.parentId,
      sort: org.sort,
      level: org.level,
      isEnabled: org.isEnabled,
      description: org.description,
    };
    setEditingOrg(detail);
    form.setFieldsValue({
      systemId: detail.systemId,
      code: detail.code,
      name: detail.name,
      name1: detail.name1,
      name2: detail.name2,
      name3: detail.name3,
      name4: detail.name4,
      parentId: detail.parentId,
      sort: detail.sort,
      description: detail.description,
    });
  };

  // 提交保存
  const handleSave = async () => {
    const values = await form.validateFields();
    if (editingOrg) {
      const updateDto: UpdateOrganizationRequestDto = {
        name: values.name,
        name1: values.name1 || undefined,
        name2: values.name2 || undefined,
        name3: values.name3 || undefined,
        name4: values.name4 || undefined,
        parentId: values.parentId || undefined,
        sort: values.sort,
        description: values.description || undefined,
      };
      await OrganizationApiService.updateOrganization(editingOrg.id, updateDto);
      message.success('组织机构已成功更新。');
    } else {
      const createDto: CreateOrganizationRequestDto = {
        systemId: values.systemId,
        code: values.code,
        name: values.name,
        name1: values.name1 || undefined,
        name2: values.name2 || undefined,
        name3: values.name3 || undefined,
        name4: values.name4 || undefined,
        parentId: values.parentId || undefined,
        sort: values.sort,
        description: values.description || undefined,
      };
      await OrganizationApiService.createOrganization(createDto);
      message.success('组织机构已成功创建。');
    }

    setEditingOrg(undefined);
    form.resetFields();
    void loadTree();
  };

  // 删除
  const handleDelete = async (id: string) => {
    await OrganizationApiService.deleteOrganization(id);
    message.success('组织机构已删除。');
    void loadTree();
  };

  // 启停切换
  const handleToggleStatus = async (org: OrganizationTreeNodeResultDto, checked: boolean) => {
    await OrganizationApiService.setOrganizationStatus(org.id, { isEnabled: checked });
    message.success(`已${checked ? '启用' : '停用'}组织机构「${org.name}」。`);
    void loadTree();
  };

  const columns: ColumnsType<OrganizationTreeNodeResultDto> = [
    {
      title: '组织机构名称',
      dataIndex: 'name',
      key: 'name',
      width: 240,
      render: (name: string, record) => (
        <Space>
          <ApartmentOutlined style={{ color: 'var(--color-primary)' }} />
          <Text strong>{name}</Text>
          {record.name1 && <Tag color="blue">{record.name1}</Tag>}
        </Space>
      ),
    },
    {
      title: '组织编码',
      dataIndex: 'code',
      key: 'code',
      width: 140,
      render: (code: string) => <Text code>{code}</Text>,
    },
    {
      title: '扩展别名',
      key: 'aliases',
      width: 200,
      render: (_, record) => {
        const aliases = [record.name2, record.name3, record.name4].filter(Boolean);
        if (aliases.length === 0) return '-';
        return (
          <div className={styles.aliasTags}>
            {aliases.map((alias, i) => (
              <Tag key={i} bordered={false}>
                {alias}
              </Tag>
            ))}
          </div>
        );
      },
    },
    {
      title: '排序',
      dataIndex: 'sort',
      key: 'sort',
      width: 80,
      align: 'center',
    },
    {
      title: '状态',
      dataIndex: 'isEnabled',
      key: 'isEnabled',
      width: 100,
      render: (isEnabled: boolean, record) => (
        <Switch
          size="small"
          checked={isEnabled}
          disabled={!canUpdate}
          checkedChildren="启用"
          unCheckedChildren="停用"
          onChange={(checked) => void handleToggleStatus(record, checked)}
        />
      ),
    },
    {
      title: '说明备注',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
      render: (text?: string) => text || '-',
    },
    {
      title: '操作',
      key: 'actions',
      fixed: 'right',
      width: 330,
      render: (_, record) => (
        <Space size={4}>
          <Tooltip title="查看组织详细信息">
            <Button
              type="text"
              size="small"
              icon={<EyeOutlined />}
              onClick={() => setDetailOrg(record)}
              style={{ color: 'var(--color-primary)', fontWeight: 500 }}
            >
              详情
            </Button>
          </Tooltip>
          <Tooltip title="新增当前组织的下级子部门">
            <Button
              type="text"
              size="small"
              icon={<PlusOutlined />}
              hidden={!canCreate}
              onClick={() => openCreate(record)}
              style={{ color: '#52c41a', fontWeight: 500 }}
            >
              新增下级
            </Button>
          </Tooltip>
          <Tooltip title="编辑组织机构资料">
            <Button
              type="text"
              size="small"
              icon={<EditOutlined />}
              hidden={!canUpdate}
              onClick={() => openEdit(record)}
              style={{ color: 'var(--color-primary)', fontWeight: 500 }}
            >
              编辑
            </Button>
          </Tooltip>
          <Popconfirm
            title="确认删除该组织机构？"
            description="删除后无法恢复。若存在下级子组织则无法删除。"
            okText="确定删除"
            cancelText="取消"
            okButtonProps={{ danger: true }}
            onConfirm={() => void handleDelete(record.id)}
          >
            <Button type="text" size="small" danger icon={<DeleteOutlined />} hidden={!canDelete} style={{ fontWeight: 500 }}>
              删除
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <main className={styles.pageContainer}>
      {/* 检索卡片 */}
      <Card className={styles.headerCard} bordered={false}>
        <div className={styles.searchHeader}>
          <div className={styles.searchForm}>
            <Select
              style={{ width: 200 }}
              placeholder="选择所属系统"
              value={selectedSystemId || undefined}
              onChange={(val) => setSelectedSystemId(val)}
              options={systems.map((s) => ({ label: s.name, value: s.id }))}
            />
            <Input
              style={{ width: 220 }}
              placeholder="按组织名称/编码搜索..."
              prefix={<SearchOutlined style={{ color: 'var(--color-placeholder)' }} />}
              value={draftKeyword}
              onChange={(e) => setDraftKeyword(e.target.value)}
              onPressEnter={submitSearch}
              allowClear
            />
            <Button type="primary" onClick={submitSearch}>
              查询
            </Button>
            <Button onClick={resetSearch}>重置</Button>
          </div>

          <Button
            type="primary"
            className="create-primary-button"
            icon={<PlusOutlined />}
            hidden={!canCreate}
            onClick={() => openCreate()}
          >
            新增组织机构
          </Button>
        </div>
      </Card>

      {/* 数据表格卡片 */}
      <Card className={styles.tableCard} bordered={false}>
        <Table<OrganizationTreeNodeResultDto>
          rowKey="id"
          loading={loading}
          columns={columns}
          dataSource={filteredTree}
          pagination={false}
          defaultExpandAllRows
          scroll={{ x: 1000 }}
        />
      </Card>

      {/* 新增/编辑弹窗 */}
      <Modal
        title={editingOrg ? '编辑组织机构' : '新增组织机构'}
        open={editingOrg !== undefined}
        onOk={() => void handleSave()}
        onCancel={() => {
          setEditingOrg(undefined);
          form.resetFields();
        }}
        width={720}
        destroyOnClose
      >
        <Form form={form} layout="vertical" className={styles.modalContent}>
          <div className={styles.sectionCard}>
            <div className={styles.sectionTitle}>
              <span>基本信息</span>
            </div>
            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  name="systemId"
                  label="所属系统"
                  rules={[{ required: true, message: '请选择所属系统' }]}
                >
                  <Select
                    placeholder="选择所属系统"
                    disabled={Boolean(editingOrg)}
                    options={systems.map((s) => ({ label: s.name, value: s.id }))}
                  />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item name="parentId" label="上级组织机构">
                  <TreeSelect
                    placeholder="选择上级组织（顶级可不选）"
                    allowClear
                    treeDefaultExpandAll
                    treeData={treeSelectOptions}
                  />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item
                  name="name"
                  label="组织主名称"
                  rules={[{ required: true, message: '请输入组织主名称' }]}
                >
                  <Input placeholder="例如: 研发中心" maxLength={128} />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item
                  name="code"
                  label="组织机构编码"
                  rules={[{ required: true, message: '请输入组织机构编码' }]}
                >
                  <Input placeholder="例如: CORP_RD" maxLength={64} disabled={Boolean(editingOrg)} />
                </Form.Item>
              </Col>
            </Row>
          </div>

          <div className={styles.sectionCard}>
            <div className={styles.sectionTitle}>
              <span>扩展名称 / 别名</span>
            </div>
            <Row gutter={16}>
              <Col span={12}>
                <Form.Item name="name1" label="组织名称 1（英文/别名）">
                  <Input placeholder="例如: R&D Center" maxLength={128} />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item name="name2" label="组织名称 2（简称/多语言）">
                  <Input placeholder="例如: 技术部" maxLength={128} />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item name="name3" label="组织名称 3">
                  <Input placeholder="可选扩展名称" maxLength={128} />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item name="name4" label="组织名称 4">
                  <Input placeholder="可选扩展名称" maxLength={128} />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item name="sort" label="同级展示排序">
                  <InputNumber style={{ width: '100%' }} min={0} max={9999} />
                </Form.Item>
              </Col>
              <Col span={24}>
                <Form.Item name="description" label="职能说明 / 备注">
                  <Input.TextArea rows={2} placeholder="组织职责或说明..." maxLength={512} />
                </Form.Item>
              </Col>
            </Row>
          </div>
        </Form>
      </Modal>

      {/* 详情弹窗 */}
      <Modal
        title="组织机构详情"
        open={Boolean(detailOrg)}
        onCancel={() => setDetailOrg(undefined)}
        footer={null}
        width={680}
      >
        {detailOrg && (
          <div>
            <div className={styles.detailBanner}>
              <Avatar size={52} icon={<ApartmentOutlined />} className={styles.detailAvatar}>
                {detailOrg.name.charAt(0)}
              </Avatar>
              <div className={styles.detailHeaderContent}>
                <div className={styles.detailTitle}>{detailOrg.name}</div>
                <Space>
                  <Tag color="blue">{detailOrg.code}</Tag>
                  {detailOrg.isEnabled ? (
                    <Tag color="success" icon={<CheckCircleOutlined />}>
                      正常
                    </Tag>
                  ) : (
                    <Tag color="error" icon={<StopOutlined />}>
                      停用
                    </Tag>
                  )}
                </Space>
              </div>
            </div>

            <Descriptions bordered size="small" column={2}>
              <Descriptions.Item label="所属系统">{detailOrg.systemName}</Descriptions.Item>
              <Descriptions.Item label="组织编码">
                <Text code>{detailOrg.code}</Text>
              </Descriptions.Item>
              <Descriptions.Item label="组织主名称">{detailOrg.name}</Descriptions.Item>
              <Descriptions.Item label="层级深度">{detailOrg.level} 级</Descriptions.Item>
              <Descriptions.Item label="组织名称 1">{detailOrg.name1 || '-'}</Descriptions.Item>
              <Descriptions.Item label="组织名称 2">{detailOrg.name2 || '-'}</Descriptions.Item>
              <Descriptions.Item label="组织名称 3">{detailOrg.name3 || '-'}</Descriptions.Item>
              <Descriptions.Item label="组织名称 4">{detailOrg.name4 || '-'}</Descriptions.Item>
              <Descriptions.Item label="排序序号">{detailOrg.sort}</Descriptions.Item>
              <Descriptions.Item label="组织 ID">
                <Text code copyable>
                  {detailOrg.id}
                </Text>
              </Descriptions.Item>
              <Descriptions.Item label="职能说明" span={2}>
                {detailOrg.description || '-'}
              </Descriptions.Item>
            </Descriptions>
          </div>
        )}
      </Modal>
    </main>
  );
}
