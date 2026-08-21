/**
 * @file SSO 权限作用域管理页面 (SsoScopes)
 * @description 直连 OpenIddictApiService 与对应 DTO 类型。
 * 基于通用的 CrudToolbar / CrudTable / useCrudTable 组件实现标准化 CRUD 布局。
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
  message,
  Typography,
  type TableProps,
} from 'antd';
import {
  SafetyCertificateOutlined,
  EditOutlined,
  DeleteOutlined,
} from '@ant-design/icons';
import {
  OpenIddictApiService,
  type OpenIddictScopeRowResultDto,
  type CreateOpenIddictScopeInputDto,
  type UpdateOpenIddictScopeInputDto,
} from '../../../apiServices';
import {
  CrudTable,
  CrudToolbar,
  useCrudTable,
} from '../../../components';
import styles from '../sso.module.css';

const { Text } = Typography;

export default function SsoScopes() {
  // 1. 查询筛选状态
  const [draftName, setDraftName] = useState('');
  const [name, setName] = useState('');

  // 2. 新增 / 编辑弹窗状态
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<OpenIddictScopeRowResultDto | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [form] = Form.useForm<CreateOpenIddictScopeInputDto>();

  // 3. 通用 CRUD Hook 接管分页与数据加载
  const filters = useMemo(() => ({ name: name || undefined }), [name]);

  const {
    items,
    loading,
    pagination,
    loadData,
    handleDelete,
  } = useCrudTable<OpenIddictScopeRowResultDto, { name?: string }>({
    fetchApi: OpenIddictApiService.getScopePageList,
    deleteApi: OpenIddictApiService.deleteScope,
    filters,
  });

  const handleOpenCreate = () => {
    setEditing(null);
    form.resetFields();
    setModalOpen(true);
  };

  const handleOpenEdit = (record: OpenIddictScopeRowResultDto) => {
    setEditing(record);
    form.setFieldsValue({
      name: record.name,
      displayName: record.displayName,
      description: record.description,
      resources: record.resources,
    });
    setModalOpen(true);
  };

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      setSubmitting(true);

      if (editing) {
        await OpenIddictApiService.updateScope(editing.id, {
          displayName: values.displayName,
          description: values.description,
          resources: values.resources,
        } as UpdateOpenIddictScopeInputDto);
        message.success('作用域信息已更新');
      } else {
        await OpenIddictApiService.createScope(values);
        message.success('作用域已创建');
      }

      setModalOpen(false);
      form.resetFields();
      await loadData();
    } catch {
      // 校验错误由 AntD 自行展示
    } finally {
      setSubmitting(false);
    }
  };

  // 4. 标准 Antd Table 列定义
  const columns: TableProps<OpenIddictScopeRowResultDto>['columns'] = [
    {
      title: '作用域名称 (Name)',
      dataIndex: 'name',
      key: 'name',
      width: 200,
      render: (val) => <Tag color="blue" style={{ fontSize: 13, padding: '2px 8px' }}>{val}</Tag>,
    },
    {
      title: '显示名称',
      dataIndex: 'displayName',
      key: 'displayName',
      width: 200,
      render: (val) => <Text strong style={{ color: 'var(--color-title)' }}>{val ?? '-'}</Text>,
    },
    {
      title: '描述说明',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
      render: (val) => val ? <Text type="secondary">{val}</Text> : '-',
    },
    {
      title: '关联目标资源 (Resources)',
      dataIndex: 'resources',
      key: 'resources',
      render: (res: string[]) => (
        res && res.length > 0 ? (
          <Space wrap size={4}>
            {res.map((r) => <Tag key={r}>{r}</Tag>)}
          </Space>
        ) : <Text type="secondary">-</Text>
      ),
    },
    {
      title: '操作',
      key: 'action',
      width: 160,
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
          <Popconfirm
            title="确定要删除此作用域吗？"
            description="删除后依赖该作用域的客户端将无法请求授权。"
            onConfirm={() => void handleDelete(record.id, '作用域已删除')}
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
        searchPlaceholder="按作用域名称搜索..."
        searchValue={draftName}
        onSearchChange={setDraftName}
        onSearch={() => setName(draftName.trim())}
        onReset={() => {
          setDraftName('');
          setName('');
        }}
        createButton={{
          text: '新增作用域',
          onClick: handleOpenCreate,
        }}
      />

      {/* 2. 数据表格 */}
      <CrudTable<OpenIddictScopeRowResultDto>
        rowKey="id"
        columns={columns}
        dataSource={items}
        loading={loading}
        pagination={pagination}
        emptyText="暂无作用域数据"
        scroll={{ x: 750 }}
      />

      {/* 3. 作用域 新建 / 编辑 Modal */}
      <Modal
        title={
          <Space size={8}>
            <SafetyCertificateOutlined style={{ color: 'var(--color-primary)' }} />
            <span style={{ fontWeight: 700, fontSize: 16 }}>
              {editing ? `编辑作用域: ${editing.name}` : '新增 SSO 作用域'}
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
        destroyOnClose
      >
        <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
          <Form.Item
            name="name"
            label="作用域名称 (Name)"
            rules={[{ required: true, message: '请输入作用域名称' }]}
          >
            <Input
              disabled={!!editing}
              placeholder="例如: dedsinative_api / user_profile"
              className={styles.formControl}
            />
          </Form.Item>

          <Form.Item name="displayName" label="显示名称">
            <Input placeholder="例如: 业务系统接口访问权限" className={styles.formControl} />
          </Form.Item>

          <Form.Item name="description" label="描述说明">
            <Input.TextArea rows={3} placeholder="用于描述该作用域代表的权限范围" style={{ borderRadius: 'var(--radius-btn)' }} />
          </Form.Item>

          <Form.Item name="resources" label="关联目标资源标识 (Resources)">
            <Select mode="tags" placeholder="例如: dedsinative_api" className={styles.formControl} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
}
