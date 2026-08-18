/**
 * @file 系统管理页面 (SystemManagement)
 * @description 直连 SystemApiService 与对应 DTO 类型 (SystemResultDto, CreateSystemInputDto, UpdateSystemInputDto)。
 * 遵循共享 Skill dedsi-style-react-admin-ui 的 UI/UX 规范。
 */

import { useCallback, useEffect, useState } from 'react';
import {
  Button,
  Card,
  Descriptions,
  Empty,
  Form,
  Input,
  InputNumber,
  Modal,
  Popconfirm,
  Space,
  Table,
  Tag,
  Tooltip,
  Avatar,
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
  AppstoreOutlined,
} from '@ant-design/icons';
import {
  SystemApiService,
  type CreateSystemInputDto,
  type SystemResultDto,
  type SystemRowResultDto,
  type UpdateSystemInputDto,
} from '../../../apiServices';
import styles from './index.module.css';

/** 根据系统名称生成固定的头像背景色 */
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

/** 系统管理页面，负责系统列表查询及基础资料维护。 */
export default function SystemManagement() {
  const [items, setItems] = useState<SystemRowResultDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [pageIndex, setPageIndex] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [draftName, setDraftName] = useState('');
  const [name, setName] = useState('');
  const [editing, setEditing] = useState<SystemRowResultDto | null>(null);
  const [detail, setDetail] = useState<SystemResultDto | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);
  const [form] = Form.useForm<CreateSystemInputDto>();
  const [modalOpen, setModalOpen] = useState(false);
  const [detailOpen, setDetailOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  /** 按当前筛选条件加载系统列表。 */
  const loadSystems = useCallback(async () => {
    setLoading(true);
    try {
      const result = await SystemApiService.getPageList({
        pageIndex,
        pageSize,
        name: name || undefined,
      });
      setItems(result.items || []);
      setTotalCount(result.totalCount || 0);
    } catch {
      setItems([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  }, [name, pageIndex, pageSize]);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => {
      void loadSystems();
    }, 0);

    return () => window.clearTimeout(timeoutId);
  }, [loadSystems]);

  /** 提交名称筛选并回到第一页。 */
  const handleSearch = () => {
    setPageIndex(1);
    setName(draftName.trim());
  };

  /** 重置搜索条件 */
  const handleResetSearch = () => {
    setDraftName('');
    setName('');
    setPageIndex(1);
  };

  /** 打开新增或编辑系统表单。 */
  const openForm = (item?: SystemRowResultDto) => {
    setEditing(item ?? null);
    form.setFieldsValue(
      item
        ? { name: item.name, description: item.description ?? '', sort: item.sort }
        : { name: '', description: '', sort: 0 }
    );
    setModalOpen(true);
  };

  /** 提交系统创建或更新请求。 */
  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      setSubmitting(true);
      if (editing) {
        const input: UpdateSystemInputDto = values;
        await SystemApiService.update(editing.id, input);
        message.success('系统信息已更新');
      } else {
        const input: CreateSystemInputDto = values;
        await SystemApiService.create(input);
        message.success('系统已创建');
      }
      setModalOpen(false);
      form.resetFields();
      await loadSystems();
    } catch {
      // 表单校验失败由 AntD 展示
    } finally {
      setSubmitting(false);
    }
  };

  /** 删除系统并处理删除当前页最后一条记录的边界。 */
  const handleDelete = async (id: string) => {
    try {
      await SystemApiService.delete(id);
      message.success('系统已删除');
      if (items.length === 1 && pageIndex > 1) {
        setPageIndex((current) => current - 1);
      } else {
        await loadSystems();
      }
    } catch {
      // 拦截器统一处理
    }
  };

  /** 加载系统详情 */
  const openDetail = async (item: SystemRowResultDto) => {
    setDetailOpen(true);
    setDetail(null);
    setDetailLoading(true);
    try {
      setDetail(await SystemApiService.getById(item.id));
    } catch {
      setDetail(null);
    } finally {
      setDetailLoading(false);
    }
  };

  const columns: TableProps<SystemRowResultDto>['columns'] = [
    {
      title: '系统标识与名称',
      key: 'name',
      width: 240,
      render: (_, record) => {
        return (
          <div className={styles.cellWrapper}>
            <div className={styles.cellInfo}>
              <span className={styles.cellTitle}>{record.name}</span>
            </div>
          </div>
        );
      },
    },
    {
      title: '系统 ID',
      dataIndex: 'id',
      key: 'id',
      width: 240,
      render: (id: string) => (
        <Tooltip title="点击复制 ID">
          <span className={styles.idTag} onClick={() => void copyToClipboard(id, '系统 ID')}>
            {id}
            <CopyOutlined style={{ fontSize: 11, opacity: 0.6 }} />
          </span>
        </Tooltip>
      ),
    },
    {
      title: '系统说明',
      dataIndex: 'description',
      key: 'description',
      render: (value: string | null) =>
        value ? (
          <span style={{ color: 'var(--color-body)' }}>{value}</span>
        ) : (
          <Tag bordered={false} style={{ color: 'var(--color-placeholder)' }}>
            未填写
          </Tag>
        ),
    },
    {
      title: '展示排序',
      dataIndex: 'sort',
      key: 'sort',
      width: 110,
      render: (sort: number) => (
        <Tag color="blue" style={{ borderRadius: 10, fontWeight: 600 }}>
          {sort}
        </Tag>
      ),
    },
    {
      title: '操作',
      key: 'actions',
      width: 220,
      fixed: 'right',
      render: (_, record) => (
        <Space size={4}>
          <Tooltip title="查看系统详细信息">
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
          <Tooltip title="编辑系统资料">
            <Button
              type="text"
              icon={<EditOutlined />}
              size="small"
              onClick={() => openForm(record)}
              style={{ color: 'var(--color-primary)', fontWeight: 500 }}
            >
              编辑
            </Button>
          </Tooltip>
          <Popconfirm
            title="确认删除该系统？"
            description="删除后系统数据将无法恢复。"
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
            <Input
              allowClear
              className={styles.searchInput}
              prefix={<SearchOutlined style={{ color: 'var(--color-placeholder)' }} />}
              placeholder="按系统名称搜索..."
              value={draftName}
              onChange={(event) => setDraftName(event.target.value)}
              onPressEnter={handleSearch}
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
              onClick={() => void loadSystems()}
              style={{ borderRadius: 'var(--radius-btn)' }}
            >
              刷新
            </Button>
            <Button
              type="primary"
              className="create-primary-button"
              icon={<PlusOutlined />}
              onClick={() => openForm()}
            >
              新增系统
            </Button>
          </Space>
        </div>
      </Card>

      {/* 2. 数据表格卡片 */}
      <Card className={styles.tableCard} styles={{ body: { padding: '16px 24px' } }}>
        <Table<SystemRowResultDto>
          rowKey="id"
          columns={columns}
          dataSource={items}
          loading={loading}
          locale={{ emptyText: <Empty description="暂无系统数据" /> }}
          scroll={{ x: 800 }}
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

      {/* 3. 新增 / 编辑系统弹窗 Modal */}
      <Modal
        title={
          <Space size={8}>
            <AppstoreOutlined style={{ color: 'var(--color-primary)' }} />
            <span style={{ fontWeight: 700, fontSize: 16 }}>
              {editing ? `编辑系统: ${editing.name}` : '新增系统'}
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
        width={540}
      >
        <Form form={form} layout="vertical" style={{ marginTop: 12 }}>
          <div className={styles.sectionCard}>
            <div className={styles.sectionTitle}>
              <AppstoreOutlined style={{ color: 'var(--color-primary)' }} />
              <span>系统基础信息</span>
            </div>
            <Form.Item
              name="name"
              label="系统名称"
              rules={[{ required: true, message: '请输入系统名称' }]}
            >
              <Input className={styles.formControl} placeholder="例如：统一身份认证" />
            </Form.Item>
            <Form.Item name="description" label="系统说明">
              <Input.TextArea
                rows={3}
                placeholder="请输入系统说明及业务使用场景"
                style={{ borderRadius: 'var(--radius-btn)' }}
              />
            </Form.Item>
            <Form.Item
              name="sort"
              label="展示排序"
              rules={[{ required: true, message: '请输入排序数值' }]}
              style={{ marginBottom: 0 }}
            >
              <InputNumber
                precision={0}
                style={{ width: '100%', borderRadius: 'var(--radius-btn)' }}
                placeholder="数值越小排序越靠前"
              />
            </Form.Item>
          </div>
        </Form>
      </Modal>

      {/* 4. 系统详情 Modal */}
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
                const row: SystemRowResultDto = {
                  id: detail.id,
                  name: detail.name,
                  description: detail.description,
                  sort: detail.sort,
                };
                openForm(row);
              }
            }}
            style={{ borderRadius: 'var(--radius-btn)', backgroundColor: 'var(--color-primary)' }}
          >
            编辑此系统
          </Button>,
          <Button
            key="close"
            onClick={() => setDetailOpen(false)}
            style={{ borderRadius: 'var(--radius-btn)' }}
          >
            关闭
          </Button>,
        ]}
        width={500}
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
                {detail.name ? detail.name.charAt(0).toUpperCase() : 'S'}
              </Avatar>
              <div className={styles.detailHeaderInfo}>
                <span className={styles.detailHeaderName}>{detail.name}</span>
                <Tag color="blue" style={{ borderRadius: 10, width: 'fit-content' }}>
                  排序权重: {detail.sort}
                </Tag>
              </div>
            </div>

            <Descriptions
              column={1}
              bordered
              size="small"
              labelStyle={{
                width: 120,
                fontWeight: 600,
                color: 'var(--color-text-secondary)',
                backgroundColor: 'var(--color-surface-subtle)',
              }}
              contentStyle={{ color: 'var(--color-title)' }}
            >
              <Descriptions.Item label="系统 ID">
                <span
                  style={{ fontFamily: 'monospace', color: 'var(--color-body)', cursor: 'pointer' }}
                  onClick={() => void copyToClipboard(detail.id, '系统 ID')}
                >
                  {detail.id} <CopyOutlined style={{ color: 'var(--color-placeholder)', marginLeft: 4 }} />
                </span>
              </Descriptions.Item>
              <Descriptions.Item label="系统名称">{detail.name}</Descriptions.Item>
              <Descriptions.Item label="展示排序">{detail.sort}</Descriptions.Item>
              <Descriptions.Item label="系统说明">
                {detail.description || '暂无说明'}
              </Descriptions.Item>
            </Descriptions>
          </div>
        ) : (
          <Empty description="无法加载系统详情" />
        )}
      </Modal>
    </div>
  );
}
