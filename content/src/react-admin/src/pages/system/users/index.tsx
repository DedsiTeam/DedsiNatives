/**
 * @file 用户管理页面 (UserManagement)
 * @description 直连 UserApiService 与对应 DTO 类型 (UserResultDto, CreateUserInputDto, UpdateUserInputDto)。
 * 严格按照后端 OpenAPI / FastEndpoints 契约处理数据展现与交互。
 */

import React, { useState, useEffect, useCallback } from 'react';
import {
  Card,
  Table,
  Button,
  Input,
  Space,
  Avatar,
  Modal,
  Form,
  Popconfirm,
  message,
  Descriptions,
  type TableProps,
} from 'antd';
import {
  UserAddOutlined,
  SearchOutlined,
  ReloadOutlined,
  EditOutlined,
  DeleteOutlined,
  UserOutlined,
  EyeOutlined,
} from '@ant-design/icons';

// 统一直接从 apiServices 导入 Service 与服务对应的 DTO 类型
import {
  UserApiService,
  type UserResultDto,
  type PagedUserRowDto,
  type CreateUserInputDto,
  type UpdateUserInputDto,
} from '../../../apiServices';
import styles from './index.module.css';

/**
 * 用户管理主页面组件
 */
export const UserManagement: React.FC = () => {
  // ---------------------------------------------------------------------------
  // State 状态声明
  // ---------------------------------------------------------------------------

  /** 远程获取的用户列表数据 */
  const [users, setUsers] = useState<PagedUserRowDto[]>([]);

  /** 列表数据总记录数 */
  const [total, setTotal] = useState<number>(0);

  /** 表格数据加载 Loading 状态 */
  const [loading, setLoading] = useState<boolean>(false);

  /** 分页页码 (从 1 开始) */
  const [pageIndex, setPageIndex] = useState<number>(1);

  /** 每页展示记录数 */
  const [pageSize, setPageSize] = useState<number>(10);

  /** 搜索关键字 (用户名称) */
  const [searchText, setSearchText] = useState<string>('');

  /** 控制新增/编辑 Modal 显示隐藏状态 */
  const [isModalOpen, setIsModalOpen] = useState<boolean>(false);

  /** 当前正在编辑的用户 DTO 对象 (若为 null 则表示当前为新增模式) */
  const [editingUser, setEditingUser] = useState<PagedUserRowDto | null>(null);

  /** 表单提交中的按钮 Loading 状态 */
  const [submitting, setSubmitting] = useState<boolean>(false);

  /** 控制用户详情 Modal 显示隐藏状态 */
  const [isDetailModalOpen, setIsDetailModalOpen] = useState<boolean>(false);

  /** 当前查看详情的用户 DTO 对象 */
  const [detailUser, setDetailUser] = useState<UserResultDto | null>(null);

  /** Antd Form 表单实例句柄 */
  const [form] = Form.useForm<CreateUserInputDto>();

  // ---------------------------------------------------------------------------
  // API 网络请求处理函数 (Async API Handlers)
  // ---------------------------------------------------------------------------

  /**
   * 核心方法：异步调用 UserApiService 拉取分页用户数据
   */
  const fetchUsers = useCallback(async () => {
    setLoading(true);
    try {
      const res = await UserApiService.getPageList({
        pageIndex,
        pageSize,
        name: searchText.trim() || undefined,
      });

      if (res && Array.isArray(res.items)) {
        setUsers(res.items);
        setTotal(res.totalCount || 0);
      } else {
        setUsers([]);
        setTotal(0);
      }
    } catch {
      setUsers([]);
      setTotal(0);
    } finally {
      setLoading(false);
    }
  }, [pageIndex, pageSize, searchText]);

  /**
   * 监听页码、每页条数、搜索条件自动拉取 API 数据
   */
  useEffect(() => {
    // 延后到本轮提交完成后请求，避免 Effect 内同步触发级联渲染。
    const timeoutId = window.setTimeout(() => {
      void fetchUsers();
    }, 0);

    return () => window.clearTimeout(timeoutId);
  }, [fetchUsers]);

  /**
   * 删除指定用户账号 API
   * @param id 用户唯一 ID
   */
  const handleDelete = async (id: string) => {
    try {
      await UserApiService.deleteUser(id);
      message.success('用户已成功删除');
      fetchUsers();
    } catch {
      // 异常拦截
    }
  };

  /**
   * 打开新增或编辑 Modal
   * @param user 编辑时的目标 PagedUserRowDto，若不传则默认为新增操作
   */
  const openModal = (user?: PagedUserRowDto) => {
    if (user) {
      setEditingUser(user);
      form.setFieldsValue({
        name: user.name,
        email: user.email,
      });
    } else {
      setEditingUser(null);
      form.resetFields();
    }
    setIsModalOpen(true);
  };

  /**
   * 打开用户详情 Modal (通过 UserApiService 异步获取最新 UserResultDto 记录)
   * @param user 待查看详情的用户行数据
   */
  const openDetailModal = async (user: PagedUserRowDto) => {
    setDetailUser({ id: user.id, name: user.name, email: user.email });
    setIsDetailModalOpen(true);

    try {
      const res = await UserApiService.getById(user.id);
      if (res) {
        setDetailUser(res);
      }
    } catch {
      // 降级保留行数据
    }
  };

  /**
   * 提交新增或修改表单 API
   */
  const handleModalSubmit = async () => {
    try {
      const values = await form.validateFields();
      setSubmitting(true);

      if (editingUser) {
        const updateDto: UpdateUserInputDto = values;
        await UserApiService.updateUser(editingUser.id, updateDto);
        message.success('用户信息修改成功');
      } else {
        await UserApiService.createUser(values);
        message.success('新用户创建成功');
      }

      setIsModalOpen(false);
      form.resetFields();
      await fetchUsers();
    } catch {
      // 表单校验错误由 Form 展示，请求错误由统一请求层提示，并保留当前输入。
    } finally {
      setSubmitting(false);
    }
  };

  /**
   * 关闭新增或编辑弹窗；提交期间保持弹窗稳定，防止重复操作。
   */
  const handleModalCancel = () => {
    if (submitting) {
      return;
    }

    setIsModalOpen(false);
    form.resetFields();
  };

  /**
   * 点击重置查询条件
   */
  const handleResetSearch = () => {
    setSearchText('');
    setPageIndex(1);
  };

  // ---------------------------------------------------------------------------
  // Table 表格列定义
  // ---------------------------------------------------------------------------

  const columns: TableProps<PagedUserRowDto>['columns'] = [
    {
      title: '用户标识 (ID)',
      dataIndex: 'id',
      key: 'id',
      render: (text) => <span style={{ fontFamily: 'monospace', color: 'var(--color-muted)' }}>{text}</span>,
    },
    {
      title: '用户名称',
      dataIndex: 'name',
      key: 'name',
      render: (text) => (
        <Space size={12}>
          <Avatar style={{ backgroundColor: 'var(--color-primary)' }} icon={<UserOutlined />}>
            {text ? text[0] : 'U'}
          </Avatar>
          <span style={{ fontWeight: 600, color: 'var(--color-title)' }}>{text}</span>
        </Space>
      ),
    },
    {
      title: '电子邮箱',
      dataIndex: 'email',
      key: 'email',
      render: (text) => <span style={{ color: 'var(--color-text)' }}>{text}</span>,
    },
    {
      title: '操作',
      key: 'action',
      width: 200,
      render: (_, record) => (
        <Space size={4}>
          <Button
            type="text"
            icon={<EyeOutlined />}
            size="small"
            onClick={() => openDetailModal(record)}
            style={{ color: 'var(--color-primary)' }}
          >
            详情
          </Button>
          <Button
            type="text"
            icon={<EditOutlined />}
            size="small"
            onClick={() => openModal(record)}
            style={{ color: 'var(--color-primary)' }}
          >
            编辑
          </Button>
          <Popconfirm
            title="确认删除该用户？"
            description="删除后账号将无法恢复。"
            onConfirm={() => handleDelete(record.id)}
            okText="确定"
            cancelText="取消"
          >
            <Button type="text" danger icon={<DeleteOutlined />} size="small">
              删除
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  // ---------------------------------------------------------------------------
  // 视图渲染 (JSX Template)
  // ---------------------------------------------------------------------------

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
      {/* 1. 顶部操作栏与检索筛选卡片 */}
      <Card style={{ borderRadius: 12 }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 16 }}>
          <Space size={12}>
            <Input
              placeholder="按用户名称模糊搜索..."
              prefix={<SearchOutlined style={{ color: 'var(--color-placeholder)' }} />}
              value={searchText}
              onChange={(e) => setSearchText(e.target.value)}
              onPressEnter={() => {
                setPageIndex(1);
                fetchUsers();
              }}
              style={{ width: 280, borderRadius: 8 }}
              allowClear
            />
            <Button
              icon={<SearchOutlined />}
              onClick={() => {
                setPageIndex(1);
                fetchUsers();
              }}
              style={{ borderRadius: 8 }}
              type="primary"
            >
              查询
            </Button>
            <Button
              icon={<ReloadOutlined />}
              onClick={handleResetSearch}
              style={{ borderRadius: 8 }}
            >
              重置
            </Button>
          </Space>

          <Button
            type="primary"
            icon={<UserAddOutlined />}
            onClick={() => openModal()}
            className="create-primary-button"
          >
            新增用户
          </Button>
        </div>
      </Card>

      {/* 2. 远程 API 数据表格展示卡片 */}
      <Card style={{ borderRadius: 12 }} styles={{ body: { padding: '20px 24px 16px 24px' } }}>
        <Table
          columns={columns}
          dataSource={users}
          rowKey="id"
          loading={loading}
          pagination={{
            current: pageIndex,
            pageSize: pageSize,
            total: total,
            showTotal: (totalCount, range) => `显示第 ${range[0]} - ${range[1]} 条数据，共 ${totalCount} 条`,
            onChange: (page, size) => {
              setPageIndex(page);
              setPageSize(size);
            },
          }}
        />
      </Card>

      {/* 3. 新建 / 编辑用户对话框 Modal */}
      <Modal
        title={editingUser ? '编辑用户' : '新增用户'}
        open={isModalOpen}
        onOk={handleModalSubmit}
        confirmLoading={submitting}
        onCancel={handleModalCancel}
        cancelButtonProps={{ disabled: submitting }}
        keyboard={!submitting}
        maskClosable={!submitting}
        okText="确定"
        cancelText="取消"
        className={styles.userModal}
      >
        <Form form={form} layout="vertical" className={styles.userForm}>
          <Form.Item
            name="name"
            label="用户名称"
            rules={[{ required: true, message: '请输入用户名称' }]}
          >
            <Input
              className={styles.formControl}
              placeholder="例如：Alex Smith"
            />
          </Form.Item>

          <Form.Item
            name="email"
            label="电子邮箱"
            rules={[
              { required: true, message: '请输入邮箱' },
              { type: 'email', message: '邮箱格式不正确' },
            ]}
          >
            <Input
              className={styles.formControl}
              placeholder="alex.smith@dedsi.com"
            />
          </Form.Item>
        </Form>
      </Modal>

      {/* 4. 用户详细信息弹窗 Modal */}
      <Modal
        title="用户详细信息"
        open={isDetailModalOpen}
        onCancel={() => setIsDetailModalOpen(false)}
        footer={[
          <Button
            key="close"
            type="primary"
            onClick={() => setIsDetailModalOpen(false)}
            style={{ borderRadius: 8 }}
          >
            关闭
          </Button>,
        ]}
        width={500}
        style={{ borderRadius: 12 }}
      >
        <div style={{ paddingTop: 12 }}>
          {detailUser && (
            <Descriptions
              column={1}
              bordered
              size="small"
              labelStyle={{ width: 120, fontWeight: 600, color: 'var(--color-text)', backgroundColor: 'var(--color-surface-subtle)' }}
              contentStyle={{ color: 'var(--color-title)' }}
            >
              <Descriptions.Item label="用户唯一 ID">{detailUser.id}</Descriptions.Item>
              <Descriptions.Item label="用户名称">{detailUser.name}</Descriptions.Item>
              <Descriptions.Item label="电子邮箱">{detailUser.email}</Descriptions.Item>
            </Descriptions>
          )}
        </div>
      </Modal>
    </div>
  );
};

export default UserManagement;
