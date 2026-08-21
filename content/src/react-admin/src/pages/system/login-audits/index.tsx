/**
 * @file 登录审计管理页面 (LoginAuditManagement)
 * @description 提供筛选、分页浏览与单条审计详情查看，基于通用 CrudToolbar / CrudTable / useCrudTable / CopyableIdTag 组件。
 */

import { useState } from 'react';
import {
  Button,
  DatePicker,
  Descriptions,
  Empty,
  Input,
  Modal,
  Select,
  Skeleton,
  Tag,
  Tooltip,
  Typography,
  message,
  type TableProps,
} from 'antd';
import {
  CheckCircleOutlined,
  CloseCircleOutlined,
  EyeOutlined,
} from '@ant-design/icons';
import type { Dayjs } from 'dayjs';
import {
  LoginAuditApiService,
  LoginReason,
  LoginResult,
  type LoginAuditQueryInputDto,
  type LoginAuditResultDto,
  type LoginAuditRowResultDto,
} from '../../../apiServices';
import {
  CopyableIdTag,
  CrudTable,
  CrudToolbar,
  useCrudTable,
} from '../../../components';
import styles from './index.module.css';

const { RangePicker } = DatePicker;
const { Text } = Typography;

/** 登录结果的可读标签与语义展示。 */
const loginResultLabels: Record<LoginResult, string> = {
  [LoginResult.Success]: '成功',
  [LoginResult.Failure]: '失败',
};

/** 登录原因的可读标签。 */
const loginReasonLabels: Record<LoginReason, string> = {
  [LoginReason.SuccessfulAuthentication]: '认证成功',
  [LoginReason.AccountNotFound]: '账号不存在',
  [LoginReason.InvalidPassword]: '密码错误',
  [LoginReason.UserSoftDeleted]: '用户已删除',
  [LoginReason.AccountDisabled]: '账户禁用',
  [LoginReason.AccountLocked]: '账户锁定',
  [LoginReason.AccountCancelled]: '账户注销',
  [LoginReason.SystemError]: '系统异常',
};

/** 用于查询和详情展示的空值占位。 */
function displayValue(value: string | null | undefined): string {
  return value?.trim() || '-';
}

/** 登录审计管理页面，提供筛选、分页浏览与单条审计详情查看。 */
export default function LoginAuditManagement() {
  // 1. 筛选条件的草稿值
  const [draftTimeRange, setDraftTimeRange] = useState<[Dayjs, Dayjs] | null>(null);
  const [draftResult, setDraftResult] = useState<LoginResult>();
  const [draftReason, setDraftReason] = useState<LoginReason>();
  const [draftAccount, setDraftAccount] = useState('');
  const [draftUserName, setDraftUserName] = useState('');
  const [draftClientIp, setDraftClientIp] = useState('');

  // 2. 已生效筛选条件
  const [query, setQuery] = useState<Omit<LoginAuditQueryInputDto, 'pageIndex' | 'pageSize'>>({});

  // 3. 详情弹窗状态
  const [detailOpen, setDetailOpen] = useState(false);
  const [detailLoading, setDetailLoading] = useState(false);
  const [detail, setDetail] = useState<LoginAuditResultDto | null>(null);

  // 4. 通用 CRUD Hook
  const {
    items,
    loading,
    pagination,
  } = useCrudTable<LoginAuditRowResultDto, typeof query>({
    fetchApi: LoginAuditApiService.getPageList,
    filters: query,
  });

  /** 提交草稿筛选条件 */
  const handleSearch = () => {
    setQuery({
      startTimeUtc: draftTimeRange?.[0]?.format('YYYY-MM-DD HH:mm:ss'),
      endTimeUtc: draftTimeRange?.[1]?.format('YYYY-MM-DD HH:mm:ss'),
      result: draftResult,
      reason: draftReason,
      account: draftAccount.trim() || undefined,
      userName: draftUserName.trim() || undefined,
      clientIp: draftClientIp.trim() || undefined,
    });
  };

  /** 重置条件 */
  const handleReset = () => {
    setDraftTimeRange(null);
    setDraftResult(undefined);
    setDraftReason(undefined);
    setDraftAccount('');
    setDraftUserName('');
    setDraftClientIp('');
    setQuery({});
  };

  /** 打开并加载单条审计详情。 */
  const openDetail = async (id: string) => {
    setDetailOpen(true);
    setDetailLoading(true);
    setDetail(null);
    try {
      setDetail(await LoginAuditApiService.getById(id));
    } catch {
      message.error('登录审计详情加载失败，请稍后重试。');
    } finally {
      setDetailLoading(false);
    }
  };

  const columns: TableProps<LoginAuditRowResultDto>['columns'] = [
    {
      title: '登录时间',
      dataIndex: 'loginTimeUtc',
      key: 'loginTimeUtc',
      width: 190,
    },
    {
      title: '结果',
      dataIndex: 'result',
      key: 'result',
      width: 100,
      render: (result: LoginResult) => (
        <Tag
          color={result === LoginResult.Success ? 'success' : 'error'}
          icon={result === LoginResult.Success ? <CheckCircleOutlined /> : <CloseCircleOutlined />}
        >
          {loginResultLabels[result] ?? '未知'}
        </Tag>
      ),
    },
    {
      title: '登录原因',
      dataIndex: 'reason',
      key: 'reason',
      width: 140,
      render: (reason: LoginReason) => loginReasonLabels[reason] ?? '未知原因',
    },
    {
      title: '账号 / 用户名',
      key: 'identity',
      width: 210,
      render: (_, record) => (
        <div className={styles.identityCell}>
          <Text strong>{record.account}</Text>
          <Text type="secondary">{displayValue(record.userName)}</Text>
        </div>
      ),
    },
    {
      title: '客户端 IP',
      dataIndex: 'clientIp',
      key: 'clientIp',
      width: 160,
      render: (value: string | null) => (value ? <Text code>{value}</Text> : '-'),
    },
    {
      title: '失败说明',
      dataIndex: 'failureDescription',
      key: 'failureDescription',
      width: 220,
      ellipsis: true,
      render: (value: string | null) => displayValue(value),
    },
    {
      title: '操作',
      key: 'actions',
      width: 90,
      fixed: 'right',
      render: (_, record) => (
        <Tooltip title="查看详情">
          <Button
            type="text"
            size="small"
            icon={<EyeOutlined />}
            onClick={() => void openDetail(record.id)}
            style={{ color: 'var(--color-primary)', fontWeight: 500 }}
          >
            详情
          </Button>
        </Tooltip>
      ),
    },
  ];

  return (
    <main className={styles.pageContainer}>
      {/* 1. 顶部检索工具栏 */}
      <CrudToolbar
        onSearch={handleSearch}
        onReset={handleReset}
        extraFilters={
          <>
            <RangePicker
              allowClear
              showTime
              value={draftTimeRange}
              className={styles.timeRange}
              placeholder={['开始时间', '结束时间']}
              onChange={(values) =>
                setDraftTimeRange(values?.[0] && values[1] ? [values[0], values[1]] : null)
              }
            />
            <Select<LoginResult>
              allowClear
              className={styles.selectFilter}
              placeholder="登录结果"
              value={draftResult}
              options={Object.entries(loginResultLabels).map(([value, label]) => ({
                value: Number(value),
                label,
              }))}
              onChange={setDraftResult}
            />
            <Select<LoginReason>
              allowClear
              className={styles.selectFilter}
              placeholder="登录原因"
              value={draftReason}
              options={Object.entries(loginReasonLabels).map(([value, label]) => ({
                value: Number(value),
                label,
              }))}
              onChange={setDraftReason}
            />
            <Input
              allowClear
              className={styles.textFilter}
              placeholder="登录账号"
              value={draftAccount}
              onChange={(event) => setDraftAccount(event.target.value)}
              onPressEnter={handleSearch}
            />
            <Input
              allowClear
              className={styles.textFilter}
              placeholder="用户名"
              value={draftUserName}
              onChange={(event) => setDraftUserName(event.target.value)}
              onPressEnter={handleSearch}
            />
            <Input
              allowClear
              className={styles.ipFilter}
              placeholder="客户端 IP"
              value={draftClientIp}
              onChange={(event) => setDraftClientIp(event.target.value)}
              onPressEnter={handleSearch}
            />
          </>
        }
      />

      {/* 2. 数据表格卡片 */}
      <CrudTable<LoginAuditRowResultDto>
        rowKey="id"
        columns={columns}
        dataSource={items}
        loading={loading}
        pagination={pagination}
        emptyText="暂无登录审计记录"
        scroll={{ x: 1100 }}
      />

      {/* 3. 详情 Modal */}
      <Modal
        title="登录审计详情"
        open={detailOpen}
        width={720}
        footer={null}
        onCancel={() => setDetailOpen(false)}
      >
        {detailLoading ? (
          <Skeleton active paragraph={{ rows: 8 }} />
        ) : detail ? (
          <>
            <div className={styles.detailSummary}>
              <div>
                <Text strong>{detail.account}</Text>
                <Text type="secondary">{detail.loginTimeUtc}</Text>
              </div>
              <Tag
                color={detail.result === LoginResult.Success ? 'success' : 'error'}
                icon={detail.result === LoginResult.Success ? <CheckCircleOutlined /> : <CloseCircleOutlined />}
              >
                {loginResultLabels[detail.result] ?? '未知'}
              </Tag>
            </div>
            <Descriptions
              bordered
              size="small"
              column={1}
              labelStyle={{ width: 130, fontWeight: 600, background: 'var(--color-surface-subtle)' }}
            >
              <Descriptions.Item label="审计标识">
                <CopyableIdTag id={detail.id} label="审计 ID" />
              </Descriptions.Item>
              <Descriptions.Item label="登录时间">{detail.loginTimeUtc}</Descriptions.Item>
              <Descriptions.Item label="登录结果">{loginResultLabels[detail.result] ?? '未知'}</Descriptions.Item>
              <Descriptions.Item label="登录原因">{loginReasonLabels[detail.reason] ?? '未知原因'}</Descriptions.Item>
              <Descriptions.Item label="登录账号">
                <Text code>{detail.account}</Text>
              </Descriptions.Item>
              <Descriptions.Item label="用户名">{displayValue(detail.userName)}</Descriptions.Item>
              <Descriptions.Item label="用户标识">
                {detail.userId ? <CopyableIdTag id={detail.userId} label="用户 ID" /> : '-'}
              </Descriptions.Item>
              <Descriptions.Item label="客户端 IP">
                {detail.clientIp ? <Text code>{detail.clientIp}</Text> : '-'}
              </Descriptions.Item>
              <Descriptions.Item label="失败说明">{displayValue(detail.failureDescription)}</Descriptions.Item>
              <Descriptions.Item label="User-Agent">
                <span className={styles.userAgent}>{displayValue(detail.userAgent)}</span>
              </Descriptions.Item>
            </Descriptions>
          </>
        ) : (
          <Empty description="暂无详情数据" />
        )}
      </Modal>
    </main>
  );
}
