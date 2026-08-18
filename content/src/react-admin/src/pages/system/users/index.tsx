/**
 * @file 用户管理页面 (UserManagement)
 * @description 直连 UserApiService 与对应 DTO 类型 (UserResultDto, CreateUserInputDto, UpdateUserInputDto)。
 * 严格按照后端 OpenAPI / FastEndpoints 契约处理数据展现与交互，遵循 Modern UI/UX 规范。
 */

import React, { useState, useEffect, useCallback, useMemo } from 'react';
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
  Select,
  Checkbox,
  Tag,
  Row,
  Col,
  Tooltip,
  Typography,
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
  SolutionOutlined,
  LockOutlined,
  CopyOutlined,
  MailOutlined,
  PhoneOutlined,
  IdcardOutlined,
  CheckCircleOutlined,
  StopOutlined,
} from '@ant-design/icons';

// 统一直接从 apiServices 导入 Service 与服务对应的 DTO 类型
import {
  UserApiService,
  PositionApiService,
  type UserResultDto,
  type PagedUserRowDto,
  type PositionRowResultDto,
  type CreateUserInputDto,
  type UpdateUserInputDto,
} from '../../../apiServices';
import styles from './index.module.css';

const { Text } = Typography;

/** 状态颜色映射字典 */
const ACCOUNT_STATUS_MAP: Record<number, { text: string; color: string; icon: React.ReactNode }> = {
  1: { text: '正常', color: 'success', icon: <CheckCircleOutlined /> },
  2: { text: '禁用', color: 'error', icon: <StopOutlined /> },
  3: { text: '锁定', color: 'warning', icon: <LockOutlined /> },
  4: { text: '注销', color: 'default', icon: <StopOutlined /> },
};

/** 根据用户名称生成固定的头像背景色 */
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

/**
 * 岗位选择列表组件（支持逐项整行点击选中、关键字检索过滤与状态高亮）
 */
interface SelectablePositionListProps {
  value?: string[];
  onChange?: (value: string[]) => void;
  options: PositionRowResultDto[];
}

const SelectablePositionList: React.FC<SelectablePositionListProps> = ({
  value = [],
  onChange,
  options,
}) => {
  const [filterKeyword, setFilterKeyword] = useState<string>('');

  const toggleSelect = (id: string) => {
    const nextValue = value.includes(id)
      ? value.filter((item) => item !== id)
      : [...value, id];
    onChange?.(nextValue);
  };

  const filteredOptions = useMemo(() => {
    if (!filterKeyword.trim()) return options;
    const kw = filterKeyword.trim().toLowerCase();
    return options.filter(
      (opt) =>
        opt.name.toLowerCase().includes(kw) ||
        opt.systemName.toLowerCase().includes(kw)
    );
  }, [options, filterKeyword]);

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <Input
        placeholder="搜索岗位名称或所属系统..."
        prefix={<SearchOutlined style={{ color: 'var(--color-placeholder)' }} />}
        value={filterKeyword}
        onChange={(e) => setFilterKeyword(e.target.value)}
        allowClear
        className={styles.positionFilterInput}
        size="small"
      />

      {options.length === 0 ? (
        <div style={{ color: 'var(--color-muted)', fontSize: 13, padding: '24px 0', textAlign: 'center' }}>
          暂无可分配岗位
        </div>
      ) : filteredOptions.length === 0 ? (
        <div style={{ color: 'var(--color-muted)', fontSize: 13, padding: '24px 0', textAlign: 'center' }}>
          未找到匹配岗位
        </div>
      ) : (
        <div className={styles.positionListContainer}>
          {filteredOptions.map((position) => {
            const selected = value.includes(position.id);
            return (
              <div
                key={position.id}
                onClick={() => toggleSelect(position.id)}
                className={`${styles.positionItem} ${
                  selected ? styles.positionItemSelected : styles.positionItemDefault
                }`}
              >
                <div style={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                  <span
                    style={{
                      fontWeight: selected ? 600 : 500,
                      color: selected ? 'var(--color-primary)' : 'var(--color-title)',
                      fontSize: 13,
                    }}
                  >
                    {position.name}
                  </span>
                  <span style={{ fontSize: 12, color: selected ? 'var(--color-body)' : 'var(--color-muted)' }}>
                    系统：{position.systemName}
                  </span>
                </div>
                <Checkbox checked={selected} onChange={() => toggleSelect(position.id)} />
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
};

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

  /** 搜索草稿框输入值 */
  const [draftSearchText, setDraftSearchText] = useState<string>('');

  /** 实际生效的搜索关键字 */
  const [searchText, setSearchText] = useState<string>('');

  /** 控制新增/编辑 Modal 显示隐藏状态 */
  const [isModalOpen, setIsModalOpen] = useState<boolean>(false);

  /** 当前正在编辑的用户 DTO 对象 (若为 null 则表示当前为新增模式) */
  const [editingUser, setEditingUser] = useState<PagedUserRowDto | null>(null);

  /** 编辑目标是否尚未创建登录信息；首次设置账户时必须提交密码。 */
  const [requiresLoginPassword, setRequiresLoginPassword] = useState<boolean>(false);

  /** 表单提交中的按钮 Loading 状态 */
  const [submitting, setSubmitting] = useState<boolean>(false);

  /** 控制用户详情 Modal 显示隐藏状态 */
  const [isDetailModalOpen, setIsDetailModalOpen] = useState<boolean>(false);

  /** 当前查看详情的用户 DTO 对象 */
  const [detailUser, setDetailUser] = useState<UserResultDto | null>(null);

  /** 详情 Modal 加载状态 */
  const [detailLoading, setDetailLoading] = useState<boolean>(false);

  /** 可分配的启用岗位列表。 */
  const [positionOptions, setPositionOptions] = useState<PositionRowResultDto[]>([]);

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
   * 监听页码、每页条数、检索条件变化，触发列表重新加载
   */
  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      void fetchUsers();
    }, 0);

    return () => window.clearTimeout(timeoutId);
  }, [fetchUsers]);

  /**
   * 删除指定用户账号 API
   */
  const handleDelete = async (id: string) => {
    try {
      await UserApiService.deleteUser(id);
      message.success('用户已成功删除');
      if (users.length === 1 && pageIndex > 1) {
        setPageIndex((prev) => prev - 1);
      } else {
        void fetchUsers();
      }
    } catch {
      // 异常由拦截器统一提示
    }
  };

  /**
   * 将指定用户的密码恢复为系统默认密码，并在成功后刷新列表。
   */
  const handleResetPassword = async (id: string) => {
    try {
      await UserApiService.resetPassword(id);
      message.success('密码已成功重置为系统默认密码');
      await fetchUsers();
    } catch {
      // 客户端拦截器已处理错误
    }
  };

  /**
   * 加载岗位选项
   */
  const loadAssignablePositions = async () => {
    try {
      const positions = await PositionApiService.getPageList({
        pageIndex: 1,
        pageSize: 100,
        isEnabled: true,
        isExport: true,
      });
      setPositionOptions(positions.items || []);
    } catch {
      setPositionOptions([]);
    }
  };

  /**
   * 打开新增或编辑 Modal
   */
  const openModal = async (user?: PagedUserRowDto) => {
    if (user) {
      setEditingUser(user);
      try {
        const [detail] = await Promise.all([
          UserApiService.getById(user.id),
          loadAssignablePositions(),
        ]);
        setRequiresLoginPassword(detail.loginInfo === null);
        form.setFieldsValue({
          name: detail.name,
          email: detail.email,
          phone: detail.phone ?? undefined,
          idCardNumber: detail.idCardNumber ?? undefined,
          positionIds: detail.positions.map((position) => position.positionId),
          loginInfo: detail.loginInfo
            ? { account: detail.loginInfo.account, status: detail.loginInfo.status }
            : undefined,
        });
      } catch {
        setRequiresLoginPassword(true);
        form.setFieldsValue({
          name: user.name,
          email: user.email,
          phone: user.phone ?? undefined,
        });
      }
    } else {
      setEditingUser(null);
      setRequiresLoginPassword(false);
      form.resetFields();
      form.setFieldsValue({
        positionIds: [],
        loginInfo: {
          status: 1,
          password: generateRandomPasswordString(),
        },
      });
      await loadAssignablePositions();
    }
    setIsModalOpen(true);
  };

  /**
   * 打开用户详情 Modal
   */
  const openDetailModal = async (user: PagedUserRowDto) => {
    setDetailUser(null);
    setDetailLoading(true);
    setIsDetailModalOpen(true);

    try {
      const res = await UserApiService.getById(user.id);
      if (res) {
        setDetailUser(res);
      }
    } catch {
      // 降级保留行数据
    } finally {
      setDetailLoading(false);
    }
  };

  /**
   * 生成 20 位高强度随机密码字符串
   */
  const generateRandomPasswordString = (): string => {
    const uppercase = 'ABCDEFGHJKLMNPQRSTUVWXYZ';
    const lowercase = 'abcdefghijkmnpqrstuvwxyz';
    const numbers = '23456789';
    const symbols = '!@#$%^&*';
    const all = uppercase + lowercase + numbers + symbols;

    let password = '';
    password += uppercase[Math.floor(Math.random() * uppercase.length)];
    password += lowercase[Math.floor(Math.random() * lowercase.length)];
    password += numbers[Math.floor(Math.random() * numbers.length)];
    password += symbols[Math.floor(Math.random() * symbols.length)];

    for (let i = 0; i < 16; i++) {
      password += all[Math.floor(Math.random() * all.length)];
    }

    return password
      .split('')
      .sort(() => 0.5 - Math.random())
      .join('');
  };

  /**
   * 自动生成 20 位高强度随机密码并填入表单
   */
  const handleGeneratePassword = () => {
    const shuffledPassword = generateRandomPasswordString();
    const currentLoginInfo = form.getFieldValue('loginInfo') || {};
    form.setFieldsValue({
      loginInfo: {
        ...currentLoginInfo,
        password: shuffledPassword,
      },
    });
    void form.validateFields([['loginInfo', 'password']]);
    message.success('已自动生成 20 位高强度随机密码');
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
      // 验证失败由 AntD Form 显示
    } finally {
      setSubmitting(false);
    }
  };

  /**
   * 关闭新增或编辑弹窗
   */
  const handleModalCancel = () => {
    if (submitting) return;
    setIsModalOpen(false);
    form.resetFields();
  };

  /**
   * 执行搜索
   */
  const handleSearch = () => {
    setPageIndex(1);
    setSearchText(draftSearchText);
  };

  /**
   * 重置查询条件
   */
  const handleResetSearch = () => {
    setDraftSearchText('');
    setSearchText('');
    setPageIndex(1);
  };

  // ---------------------------------------------------------------------------
  // Table 表格列定义
  // ---------------------------------------------------------------------------

  const columns: TableProps<PagedUserRowDto>['columns'] = [
    {
      title: '用户信息',
      key: 'userInfo',
      width: 260,
      render: (_, record) => {
        return (
          <div className={styles.userCell}>
            <div className={styles.userInfo}>
              <span className={styles.userName}>{record.name}</span>
              <span className={styles.userEmail}>{record.email}</span>
            </div>
          </div>
        );
      },
    },
    {
      title: '用户唯一标识 (ID)',
      dataIndex: 'id',
      key: 'id',
      width: 240,
      render: (id: string) => (
        <Tooltip title="点击复制 ID">
          <span className={styles.idTag} onClick={() => void copyToClipboard(id, '用户 ID')}>
            {id}
            <CopyOutlined style={{ fontSize: 11, opacity: 0.6 }} />
          </span>
        </Tooltip>
      ),
    },
    {
      title: '联系电话',
      dataIndex: 'phone',
      key: 'phone',
      width: 150,
      render: (phone: string | null) =>
        phone ? (
          <span style={{ color: 'var(--color-body)', fontSize: 13 }}>
            <PhoneOutlined style={{ marginRight: 6, color: 'var(--color-placeholder)' }} />
            {phone}
          </span>
        ) : (
          <Tag bordered={false} style={{ color: 'var(--color-placeholder)' }}>
            未填写
          </Tag>
        ),
    },
    {
      title: '最近更新时间',
      dataIndex: 'lastUpdatedAt',
      key: 'lastUpdatedAt',
      width: 180,
      render: (value: string) => (
        <span style={{ color: 'var(--color-muted)', fontSize: 13 }}>
          {value}
        </span>
      ),
    },
    {
      title: '操作',
      key: 'action',
      width: 280,
      fixed: 'right',
      render: (_, record) => (
        <Space size={4}>
          <Tooltip title="查看完整详情与关联岗位">
            <Button
              type="text"
              icon={<EyeOutlined />}
              size="small"
              onClick={() => void openDetailModal(record)}
              style={{ color: 'var(--color-primary)', fontWeight: 500 }}
            >
              详情
            </Button>
          </Tooltip>
          <Tooltip title="编辑基本资料与岗位">
            <Button
              type="text"
              icon={<EditOutlined />}
              size="small"
              onClick={() => void openModal(record)}
              style={{ color: 'var(--color-primary)', fontWeight: 500 }}
            >
              编辑
            </Button>
          </Tooltip>
          <Popconfirm
            title="确认重置该用户的密码？"
            description="确认后密码将恢复为系统默认初始密码，此操作不可撤销。"
            onConfirm={() => void handleResetPassword(record.id)}
            okText="确认重置"
            cancelText="取消"
            okButtonProps={{ danger: true }}
          >
            <Tooltip title="恢复初始密码">
              <Button
                type="text"
                icon={<LockOutlined />}
                size="small"
                style={{ color: 'var(--color-warning-strong)', fontWeight: 500 }}
              >
                重置密码
              </Button>
            </Tooltip>
          </Popconfirm>
          <Popconfirm
            title="确认删除该用户？"
            description="删除后账号将无法恢复并从系统中移除。"
            onConfirm={() => void handleDelete(record.id)}
            okText="确定删除"
            cancelText="取消"
            okButtonProps={{ danger: true }}
          >
            <Button type="text" danger icon={<DeleteOutlined />} size="small" style={{ fontWeight: 500 }}>
              删除
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  // Selected position count helper for form
  const selectedPositionIds = Form.useWatch('positionIds', form) ?? [];

  // ---------------------------------------------------------------------------
  // 视图渲染 (JSX Template)
  // ---------------------------------------------------------------------------

  return (
    <div className={styles.pageContainer}>
      {/* 1. 顶部检索筛选卡片 */}
      <Card className={styles.headerCard} styles={{ body: { padding: '16px 24px' } }}>
        <div className={styles.toolbarWrapper}>
          <div className={styles.searchGroup}>
            <Input
              placeholder="按用户名称搜索..."
              prefix={<SearchOutlined style={{ color: 'var(--color-placeholder)' }} />}
              value={draftSearchText}
              onChange={(e) => setDraftSearchText(e.target.value)}
              onPressEnter={handleSearch}
              className={styles.searchInput}
              allowClear
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
              onClick={() => void fetchUsers()}
              style={{ borderRadius: 'var(--radius-btn)' }}
            >
              刷新
            </Button>
            <Button
              type="primary"
              className="create-primary-button"
              icon={<UserAddOutlined />}
              onClick={() => void openModal()}
            >
              新增用户
            </Button>
          </Space>
        </div>
      </Card>

      {/* 2. 表格数据展示区 */}
      <Card className={styles.tableCard} styles={{ body: { padding: '16px 24px' } }}>
        <Table
          columns={columns}
          dataSource={users}
          rowKey="id"
          loading={loading}
          scroll={{ x: 1100 }}
          pagination={{
            current: pageIndex,
            pageSize: pageSize,
            total: total,
            showTotal: (totalCount, range) =>
              `显示第 ${range[0]} - ${range[1]} 条，共 ${totalCount} 条记录`,
            onChange: (page, size) => {
              setPageIndex(page);
              setPageSize(size);
            },
          }}
        />
      </Card>

      {/* 3. 新建 / 编辑用户 Modal */}
      <Modal
        title={
          <Space size={8}>
            {editingUser ? (
              <EditOutlined style={{ color: 'var(--color-primary)' }} />
            ) : (
              <UserAddOutlined style={{ color: 'var(--color-primary)' }} />
            )}
            <span style={{ fontWeight: 700, fontSize: 16 }}>
              {editingUser ? `编辑用户: ${editingUser.name}` : '新增用户账号'}
            </span>
          </Space>
        }
        open={isModalOpen}
        onOk={handleModalSubmit}
        confirmLoading={submitting}
        onCancel={handleModalCancel}
        cancelButtonProps={{ disabled: submitting }}
        keyboard={!submitting}
        maskClosable={!submitting}
        okText="保存提交"
        cancelText="取消"
        width={860}
        className={styles.userModal}
      >
        <Form form={form} layout="vertical" className={styles.userForm}>
          <Row gutter={20}>
            {/* 左半部分：基本信息 + 账户信息 */}
            <Col span={14} style={{ display: 'flex', flexDirection: 'column' }}>
              {/* 1. 基本信息卡片 */}
              <div className={styles.sectionCard}>
                <div className={styles.sectionTitle}>
                  <div className={styles.sectionTitleLeft}>
                    <UserOutlined style={{ color: 'var(--color-primary)' }} />
                    <span>基本资料</span>
                  </div>
                </div>
                <Row gutter={12}>
                  <Col span={12}>
                    <Form.Item
                      name="name"
                      label="用户名称"
                      rules={[{ required: true, message: '请输入用户姓名' }]}
                    >
                      <Input
                        prefix={<UserOutlined style={{ color: 'var(--color-placeholder)' }} />}
                        className={styles.formControl}
                        placeholder="例如：张三"
                      />
                    </Form.Item>
                  </Col>
                  <Col span={12}>
                    <Form.Item
                      name="email"
                      label="电子邮箱"
                      rules={[
                        { required: true, message: '请输入邮箱' },
                        { type: 'email', message: '邮箱格式不正确' },
                      ]}
                    >
                      <Input
                        prefix={<MailOutlined style={{ color: 'var(--color-placeholder)' }} />}
                        className={styles.formControl}
                        placeholder="user@dedsi.com"
                      />
                    </Form.Item>
                  </Col>
                  <Col span={12}>
                    <Form.Item name="phone" label="联系电话">
                      <Input
                        prefix={<PhoneOutlined style={{ color: 'var(--color-placeholder)' }} />}
                        className={styles.formControl}
                        placeholder="例如：13800138000"
                      />
                    </Form.Item>
                  </Col>
                  <Col span={12}>
                    <Form.Item name="idCardNumber" label="身份证号码">
                      <Input
                        prefix={<IdcardOutlined style={{ color: 'var(--color-placeholder)' }} />}
                        className={styles.formControl}
                        placeholder="18位身份证号"
                      />
                    </Form.Item>
                  </Col>
                </Row>
              </div>

              {/* 2. 账户凭据卡片 */}
              <div className={styles.sectionCard} style={{ flex: 1, marginBottom: 0 }}>
                <div className={styles.sectionTitle}>
                  <div className={styles.sectionTitleLeft}>
                    <LockOutlined style={{ color: 'var(--color-primary)' }} />
                    <span>账户安全与认证</span>
                  </div>
                </div>
                <Row gutter={12}>
                  <Col span={14}>
                    <Form.Item
                      name={['loginInfo', 'account']}
                      label="登录账号"
                      rules={[{ required: true, message: '请输入登录账号' }]}
                    >
                      <Input className={styles.formControl} placeholder="设置登录账号" />
                    </Form.Item>
                  </Col>
                  <Col span={10}>
                    <Form.Item name={['loginInfo', 'status']} label="账户状态" initialValue={1}>
                      <Select
                        className={styles.formControl}
                        options={[
                          { value: 1, label: '正常' },
                          { value: 2, label: '禁用' },
                          { value: 3, label: '锁定' },
                          { value: 4, label: '注销' },
                        ]}
                      />
                    </Form.Item>
                  </Col>
                  <Col span={24}>
                    <Form.Item
                      label={
                        <span>
                          {editingUser ? '登录密码（留空则不修改）' : '初始登录密码'}
                        </span>
                      }
                      style={{ marginBottom: 0 }}
                    >
                      <Space.Compact style={{ width: '100%' }}>
                        <Form.Item
                          name={['loginInfo', 'password']}
                          noStyle
                          rules={
                            editingUser && !requiresLoginPassword
                              ? []
                              : [{ required: true, message: '请设置登录密码' }]
                          }
                        >
                          <Input.Password
                            className={styles.formControl}
                            placeholder={
                              editingUser && !requiresLoginPassword
                                ? '留空则保持原密码不变'
                                : '请输入密码'
                            }
                          />
                        </Form.Item>
                        <Tooltip title="自动生成 20 位高强度随机密码">
                          <Button
                            icon={<ReloadOutlined />}
                            onClick={handleGeneratePassword}
                            className={styles.randomBtn}
                          >
                            随机生成
                          </Button>
                        </Tooltip>
                      </Space.Compact>
                    </Form.Item>
                  </Col>
                </Row>
              </div>
            </Col>

            {/* 右半部分：用户岗位 */}
            <Col span={10}>
              <div
                className={styles.sectionCard}
                style={{
                  height: '100%',
                  display: 'flex',
                  flexDirection: 'column',
                  marginBottom: 0,
                }}
              >
                <div className={styles.sectionTitle}>
                  <div className={styles.sectionTitleLeft}>
                    <SolutionOutlined style={{ color: 'var(--color-primary)' }} />
                    <span>分配岗位</span>
                  </div>
                  <Tag color="blue" style={{ borderRadius: 10 }}>
                    已选 {selectedPositionIds.length} 个
                  </Tag>
                </div>
                <Form.Item name="positionIds" style={{ marginBottom: 0, flex: 1 }}>
                  <SelectablePositionList options={positionOptions} />
                </Form.Item>
              </div>
            </Col>
          </Row>
        </Form>
      </Modal>

      {/* 4. 用户详细信息弹窗 Modal */}
      <Modal
        title={null}
        open={isDetailModalOpen}
        onCancel={() => setIsDetailModalOpen(false)}
        footer={[
          <Button
            key="edit"
            type="primary"
            icon={<EditOutlined />}
            onClick={() => {
              setIsDetailModalOpen(false);
              if (detailUser) {
                const userRow: PagedUserRowDto = {
                  id: detailUser.id,
                  name: detailUser.name,
                  email: detailUser.email,
                  phone: detailUser.phone,
                  lastUpdatedAt: detailUser.lastUpdatedAt,
                };
                void openModal(userRow);
              }
            }}
            style={{ borderRadius: 'var(--radius-btn)', backgroundColor: 'var(--color-primary)' }}
          >
            编辑此用户
          </Button>,
          <Button
            key="close"
            onClick={() => setIsDetailModalOpen(false)}
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
        ) : detailUser ? (
          <div style={{ paddingTop: 8 }}>
            {/* Top User Summary Card */}
            <div className={styles.detailHeader}>
              <Avatar
                size={54}
                style={{
                  background: getAvatarColor(detailUser.name),
                  fontSize: 22,
                  fontWeight: 700,
                }}
              >
                {detailUser.name ? detailUser.name.charAt(0).toUpperCase() : 'U'}
              </Avatar>
              <div className={styles.detailHeaderInfo}>
                <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                  <span className={styles.detailHeaderName}>{detailUser.name}</span>
                  {detailUser.loginInfo ? (
                    <Tag
                      color={ACCOUNT_STATUS_MAP[detailUser.loginInfo.status]?.color || 'default'}
                      icon={ACCOUNT_STATUS_MAP[detailUser.loginInfo.status]?.icon}
                      style={{ borderRadius: 10 }}
                    >
                      {ACCOUNT_STATUS_MAP[detailUser.loginInfo.status]?.text || '未知'}
                    </Tag>
                  ) : (
                    <Tag color="warning" icon={<LockOutlined />}>
                      未初始化账号
                    </Tag>
                  )}
                </div>
                <span className={styles.detailHeaderEmail}>{detailUser.email}</span>
              </div>
            </div>

            {/* Structured Details */}
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
              <Descriptions.Item label="用户标识 (ID)">
                <span
                  style={{
                    fontFamily: 'monospace',
                    color: 'var(--color-body)',
                    cursor: 'pointer',
                  }}
                  onClick={() => void copyToClipboard(detailUser.id, '用户 ID')}
                >
                  {detailUser.id} <CopyOutlined style={{ color: 'var(--color-placeholder)', marginLeft: 4 }} />
                </span>
              </Descriptions.Item>
              <Descriptions.Item label="联系电话">{detailUser.phone || '-'}</Descriptions.Item>
              <Descriptions.Item label="身份证号码">
                {detailUser.idCardNumber || '-'}
              </Descriptions.Item>
              <Descriptions.Item label="登录账号">
                {detailUser.loginInfo?.account ? (
                  <Text code>{detailUser.loginInfo.account}</Text>
                ) : (
                  '-'
                )}
              </Descriptions.Item>
              <Descriptions.Item label="关联岗位">
                {detailUser.positions.length > 0 ? (
                  <Space wrap size={[0, 6]}>
                    {detailUser.positions.map((pos) => (
                      <Tag key={pos.positionId} color="blue" icon={<SolutionOutlined />}>
                        {pos.positionName}
                      </Tag>
                    ))}
                  </Space>
                ) : (
                  <span style={{ color: 'var(--color-placeholder)' }}>未分配岗位</span>
                )}
              </Descriptions.Item>
              <Descriptions.Item label="最后登录时间">
                {detailUser.lastLoginTime
                  ? detailUser.lastLoginTime
                  : '-'}
              </Descriptions.Item>
              <Descriptions.Item label="最后登录 IP">
                {detailUser.lastLoginIp ? <Text code>{detailUser.lastLoginIp}</Text> : '-'}
              </Descriptions.Item>
              <Descriptions.Item label="最后更新时间">
                {detailUser.lastUpdatedAt}
              </Descriptions.Item>
            </Descriptions>
          </div>
        ) : (
          <div style={{ padding: '20px 0', textAlign: 'center', color: 'var(--color-error)' }}>
            无法加载用户详细数据
          </div>
        )}
      </Modal>
    </div>
  );
};

export default UserManagement;
