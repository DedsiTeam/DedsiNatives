/**
 * @file 用户管理页面 (UserManagement)
 * @description 直连 UserApiService 与对应 DTO 类型 (UserResultDto, CreateUserInputDto, UpdateUserInputDto)。
 * 严格按照后端 OpenAPI / FastEndpoints 契约处理数据展现与交互，遵循 Modern UI/UX 规范。
 */

import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
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
  TreeSelect,
  Tree,
  Tabs,
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
  MailOutlined,
  PhoneOutlined,
  IdcardOutlined,
  CheckCircleOutlined,
  StopOutlined,
  ApartmentOutlined,
} from '@ant-design/icons';

// 统一直接从 apiServices 导入 Service 与服务对应的 DTO 类型
import {
  UserApiService,
  PositionApiService,
  OrganizationApiService,
  type UserResultDto,
  type PagedUserRowDto,
  type PositionRowResultDto,
  type UserOrganizationOptionNodeDto,
  type CreateUserInputDto,
  type UpdateUserInputDto,
} from '../../../apiServices';
import {
  CopyableIdTag,
  CrudTable,
  CrudToolbar,
} from '../../../components';
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
                className={`${styles.positionItem} ${selected ? styles.positionItemSelected : styles.positionItemDefault
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
 * 组织机构选择树组件（支持逐级展开、搜索高亮、快捷全选/折叠与已选清单）
 */
interface SelectableOrganizationTreeProps {
  value?: string[];
  onChange?: (value: string[]) => void;
  options: UserOrganizationOptionNodeDto[];
}

const SelectableOrganizationTree: React.FC<SelectableOrganizationTreeProps> = ({
  value = [],
  onChange,
  options,
}) => {
  const [filterKeyword, setFilterKeyword] = useState<string>('');
  const [expandedKeys, setExpandedKeys] = useState<React.Key[]>([]);
  const [autoExpandParent, setAutoExpandParent] = useState<boolean>(true);

  // 组织 ID 到名称的映射字典
  const orgNameMap = useMemo(() => {
    const map = new Map<string, string>();
    const traverse = (nodes: UserOrganizationOptionNodeDto[]) => {
      for (const node of nodes) {
        map.set(node.value, node.title);
        if (node.children) {
          traverse(node.children);
        }
      }
    };
    traverse(options);
    return map;
  }, [options]);

  // 全量 Key 列表
  const allKeys = useMemo(() => {
    const keys: string[] = [];
    const traverse = (nodes: UserOrganizationOptionNodeDto[]) => {
      for (const node of nodes) {
        keys.push(node.value);
        if (node.children) traverse(node.children);
      }
    };
    traverse(options);
    return keys;
  }, [options]);

  // 初始自动展开全部节点
  useEffect(() => {
    if (allKeys.length > 0 && expandedKeys.length === 0) {
      setExpandedKeys(allKeys);
    }
  }, [allKeys]);

  const onExpand = (newExpandedKeys: React.Key[]) => {
    setExpandedKeys(newExpandedKeys);
    setAutoExpandParent(false);
  };

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value;
    setFilterKeyword(val);
    if (!val.trim()) {
      setAutoExpandParent(false);
      return;
    }
    const kw = val.trim().toLowerCase();
    const matchedKeys: string[] = [];
    const findMatches = (nodes: UserOrganizationOptionNodeDto[]) => {
      for (const node of nodes) {
        if (node.title.toLowerCase().includes(kw)) {
          matchedKeys.push(node.value);
        }
        if (node.children) findMatches(node.children);
      }
    };
    findMatches(options);
    setExpandedKeys(matchedKeys);
    setAutoExpandParent(true);
  };

  const handleCheck = (checked: any) => {
    const keys = Array.isArray(checked) ? checked : checked.checked;
    onChange?.(keys as string[]);
  };

  const handleRemove = (keyToRemove: string) => {
    onChange?.(value.filter((k) => k !== keyToRemove));
  };

  const handleClearAll = () => {
    onChange?.([]);
  };

  const handleExpandAll = () => {
    setExpandedKeys(allKeys);
  };

  const handleCollapseAll = () => {
    setExpandedKeys([]);
  };

  // 格式化树结构并支持关键词高亮
  const formattedTreeData = useMemo(() => {
    const loop = (data: UserOrganizationOptionNodeDto[]): any[] =>
      data.map((item) => {
        const kw = filterKeyword.trim().toLowerCase();
        const strTitle = item.title;
        const index = strTitle.toLowerCase().indexOf(kw);
        const beforeStr = strTitle.substring(0, index);
        const matchStr = strTitle.substring(index, index + kw.length);
        const afterStr = strTitle.substring(index + kw.length);
        const titleNode =
          index > -1 && kw ? (
            <span>
              {beforeStr}
              <span style={{ color: 'var(--color-primary)', fontWeight: 700 }}>{matchStr}</span>
              {afterStr}
            </span>
          ) : (
            <span>{strTitle}</span>
          );

        if (item.children && item.children.length > 0) {
          return {
            title: titleNode,
            key: item.value,
            children: loop(item.children),
          };
        }

        return {
          title: titleNode,
          key: item.value,
        };
      });

    return loop(options);
  }, [options, filterKeyword]);

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <div className={styles.treeToolbar}>
        <Input
          placeholder="搜索组织机构名称..."
          prefix={<SearchOutlined style={{ color: 'var(--color-placeholder)' }} />}
          value={filterKeyword}
          onChange={handleSearchChange}
          allowClear
          className={styles.positionFilterInput}
          style={{ flex: 1, marginBottom: 0 }}
        />
        <Space size={6}>
          <Button size="small" onClick={handleExpandAll}>
            全部展开
          </Button>
          <Button size="small" onClick={handleCollapseAll}>
            全部折叠
          </Button>
          {value.length > 0 && (
            <Button size="small" danger onClick={handleClearAll}>
              清空
            </Button>
          )}
        </Space>
      </div>

      <div className={styles.treeContainer}>
        {options.length === 0 ? (
          <div style={{ color: 'var(--color-muted)', fontSize: 13, padding: '36px 0', textAlign: 'center' }}>
            暂无可分配组织机构
          </div>
        ) : (
          <Tree
            checkable
            checkStrictly={false}
            onExpand={onExpand}
            expandedKeys={expandedKeys}
            autoExpandParent={autoExpandParent}
            onCheck={handleCheck}
            checkedKeys={value}
            treeData={formattedTreeData}
          />
        )}
      </div>

      {value.length > 0 && (
        <div className={styles.selectedSummary}>
          <div className={styles.selectedSummaryHeader}>
            <span>已选组织机构 ({value.length})</span>
            <Button type="link" size="small" danger onClick={handleClearAll} style={{ padding: 0, height: 'auto' }}>
              清空全选
            </Button>
          </div>
          <div className={styles.selectedTagsList}>
            {value.map((id) => (
              <Tag
                key={id}
                color="geekblue"
                closable
                onClose={() => handleRemove(id)}
                icon={<ApartmentOutlined />}
                style={{ borderRadius: 6, margin: 0 }}
              >
                {orgNameMap.get(id) || id}
              </Tag>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

/**
 * 用户管理主页面组件
 */
import { checkPermission } from '../../../components/Auth';
import { PERMISSIONS } from '../../../auth/permissions';

export const UserManagement: React.FC = () => {
  const canCreate = checkPermission(PERMISSIONS.users.create);
  const canUpdate = checkPermission(PERMISSIONS.users.update);
  const canDelete = checkPermission(PERMISSIONS.users.delete);
  const canResetPassword = checkPermission(PERMISSIONS.users.resetPassword);
  const canAssignPosition = checkPermission(PERMISSIONS.users.assignPosition);
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

  /** 组织机构筛选草稿选中值 */
  const [draftSelectedOrgId, setDraftSelectedOrgId] = useState<string | undefined>(undefined);

  /** 实际生效的组织机构筛选 */
  const [selectedOrgId, setSelectedOrgId] = useState<string | undefined>(undefined);

  /** 组织机构选项树数据 */
  const [orgTreeOptions, setOrgTreeOptions] = useState<UserOrganizationOptionNodeDto[]>([]);

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
   * 加载组织机构选项下拉树
   */
  const loadOrgOptions = useCallback(async () => {
    try {
      const res = await OrganizationApiService.getUserOrganizationOptions();
      if (res && Array.isArray(res)) {
        setOrgTreeOptions(res);
      }
    } catch {
      setOrgTreeOptions([]);
    }
  }, []);

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
        organizationId: selectedOrgId || undefined,
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
  }, [pageIndex, pageSize, searchText, selectedOrgId]);

  /**
   * 初始化加载组织机构选项
   */
  useEffect(() => {
    void loadOrgOptions();
  }, [loadOrgOptions]);

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
          loadOrgOptions(),
        ]);
        setRequiresLoginPassword(detail.loginInfo === null);
        form.setFieldsValue({
          name: detail.name,
          email: detail.email,
          phone: detail.phone ?? undefined,
          idCardNumber: detail.idCardNumber ?? undefined,
          positionIds: detail.positions.map((position) => position.positionId),
          organizationIds: detail.organizations.map((org) => org.organizationId),
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
        organizationIds: [],
        loginInfo: {
          status: 1,
          password: generateRandomPasswordString(),
        },
      });
      await Promise.all([loadAssignablePositions(), loadOrgOptions()]);
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
    setSelectedOrgId(draftSelectedOrgId);
  };

  /**
   * 重置查询条件
   */
  const handleResetSearch = () => {
    setDraftSearchText('');
    setSearchText('');
    setDraftSelectedOrgId(undefined);
    setSelectedOrgId(undefined);
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
      width: 260,
      render: (id: string) => <CopyableIdTag id={id} label="用户 ID" />,
    },
    {
      title: '联系电话',
      dataIndex: 'phone',
      key: 'phone',
      width: 140,
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
      width: 170,
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
              hidden={!canUpdate || !canAssignPosition}
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
                hidden={!canResetPassword}
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
            <Button type="text" danger icon={<DeleteOutlined />} size="small" hidden={!canDelete} style={{ fontWeight: 500 }}>
              删除
            </Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  // Selected position & organization count helpers for form
  const selectedPositionIds = Form.useWatch('positionIds', form) ?? [];
  const selectedOrgIds = Form.useWatch('organizationIds', form) ?? [];

  // ---------------------------------------------------------------------------
  // 视图渲染 (JSX Template)
  // ---------------------------------------------------------------------------

  return (
    <div className={styles.pageContainer}>
      {/* 1. 顶部检索筛选卡片 */}
      <CrudToolbar
        searchPlaceholder="按用户名称搜索..."
        searchValue={draftSearchText}
        onSearchChange={setDraftSearchText}
        onSearch={handleSearch}
        onReset={handleResetSearch}
        createButton={{
          text: '新增用户',
          icon: <UserAddOutlined />,
          hidden: !canCreate,
          onClick: () => void openModal(),
        }}
        extraFilters={
          <TreeSelect
            style={{ minWidth: 200 }}
            placeholder="按组织机构筛选..."
            treeData={orgTreeOptions}
            value={draftSelectedOrgId}
            onChange={(val) => setDraftSelectedOrgId(val)}
            allowClear
            treeDefaultExpandAll
          />
        }
      />

      {/* 2. 表格数据展示区 */}
      <CrudTable<PagedUserRowDto>
        columns={columns}
        dataSource={users}
        rowKey="id"
        loading={loading}
        scroll={{ x: 1100 }}
        emptyText="暂无用户数据"
        pagination={{
          current: pageIndex,
          pageSize: pageSize,
          total: total,
          showTotal: (totalCount, range) =>
            `显示第 ${range[0]} - ${range[1]} 条，共 ${totalCount} 条记录`,
          showSizeChanger: true,
          pageSizeOptions: ['10', '20', '50', '100', '500', '1000'],
          onChange: (page, size) => {
            setPageIndex(page);
            setPageSize(size);
          },
        }}
      />

      {/* 3. 新建 / 编辑用户 Modal（Tab 选项卡化） */}
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
        width={720}
        className={styles.userModal}
      >
        <Form form={form} layout="vertical" className={styles.userForm}>
          <Tabs
            defaultActiveKey="basic"
            items={[
              {
                key: 'basic',
                label: (
                  <Space size={6}>
                    <UserOutlined />
                    <span>基本资料与认证</span>
                  </Space>
                ),
                children: (
                  <div style={{ paddingTop: 8 }}>
                    {/* 基本信息 */}
                    <div className={styles.sectionCard}>
                      <div className={styles.sectionTitle}>
                        <div className={styles.sectionTitleLeft}>
                          <UserOutlined style={{ color: 'var(--color-primary)' }} />
                          <span>基本资料</span>
                        </div>
                      </div>
                      <Row gutter={16}>
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
                          <Form.Item name="idCardNumber" label="身份证号码" style={{ marginBottom: 0 }}>
                            <Input
                              prefix={<IdcardOutlined style={{ color: 'var(--color-placeholder)' }} />}
                              className={styles.formControl}
                              placeholder="18位身份证号"
                            />
                          </Form.Item>
                        </Col>
                      </Row>
                    </div>

                    {/* 账户安全与认证 */}
                    <div className={styles.sectionCard} style={{ marginBottom: 0 }}>
                      <div className={styles.sectionTitle}>
                        <div className={styles.sectionTitleLeft}>
                          <LockOutlined style={{ color: 'var(--color-primary)' }} />
                          <span>账户安全与认证</span>
                        </div>
                      </div>
                      <Row gutter={16}>
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
                  </div>
                ),
              },
              {
                key: 'organizations',
                label: (
                  <Space size={6}>
                    <ApartmentOutlined />
                    <span>所属组织机构</span>
                    {selectedOrgIds.length > 0 && (
                      <Tag color="geekblue" style={{ borderRadius: 10, marginInlineStart: 4 }}>
                        {selectedOrgIds.length}
                      </Tag>
                    )}
                  </Space>
                ),
                children: (
                  <div style={{ paddingTop: 8 }}>
                    <Form.Item name="organizationIds" style={{ marginBottom: 0 }}>
                      <SelectableOrganizationTree options={orgTreeOptions} />
                    </Form.Item>
                  </div>
                ),
              },
              {
                key: 'positions',
                label: (
                  <Space size={6}>
                    <SolutionOutlined />
                    <span>分配岗位</span>
                    {selectedPositionIds.length > 0 && (
                      <Tag color="blue" style={{ borderRadius: 10, marginInlineStart: 4 }}>
                        {selectedPositionIds.length}
                      </Tag>
                    )}
                  </Space>
                ),
                children: (
                  <div style={{ paddingTop: 8 }}>
                    <Form.Item name="positionIds" style={{ marginBottom: 0 }}>
                      <SelectablePositionList options={positionOptions} />
                    </Form.Item>
                  </div>
                ),
              },
            ]}
          />
        </Form>
      </Modal>

      {/* 4. 用户详细信息弹窗 Modal */}
      <Modal
        title={
          <Space size={8}>
            <UserOutlined style={{ color: 'var(--color-primary)' }} />
            <span style={{ fontWeight: 700, fontSize: 16 }}>用户详情</span>
          </Space>
        }
        open={isDetailModalOpen}
        onCancel={() => setIsDetailModalOpen(false)}
        footer={null}
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
                <CopyableIdTag id={detailUser.id} label="用户 ID" />
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
              <Descriptions.Item label="所属组织">
                {detailUser.organizations && detailUser.organizations.length > 0 ? (
                  <Space wrap size={[0, 6]}>
                    {detailUser.organizations.map((org) => (
                      <Tag key={org.organizationId} color="geekblue" icon={<ApartmentOutlined />}>
                        {org.organizationName}
                      </Tag>
                    ))}
                  </Space>
                ) : (
                  <span style={{ color: 'var(--color-placeholder)' }}>未分配组织机构</span>
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
