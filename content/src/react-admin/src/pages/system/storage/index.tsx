import { useCallback, useEffect, useState } from 'react';
import {
  CopyOutlined,
  DeleteOutlined,
  DownloadOutlined,
  EyeOutlined,
  FileExcelOutlined,
  FileImageOutlined,
  FileOutlined,
  FilePdfOutlined,
  FileTextOutlined,
  FileWordOutlined,
  FileZipOutlined,
  InboxOutlined,
  PlusOutlined,
  SearchOutlined,
} from '@ant-design/icons';
import {
  Button,
  Card,
  Col,
  Descriptions,
  Form,
  Input,
  Modal,
  Popconfirm,
  Row,
  Select,
  Space,
  Switch,
  Table,
  Tag,
  Tooltip,
  Typography,
  Upload,
  message,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import type { UploadFile } from 'antd/es/upload/interface';
import {
  StorageApiService,
  type StorageFileResultDto,
} from '../../../apiServices';
import styles from './index.module.css';

const { Text } = Typography;
const { Dragger } = Upload;

/** 格式化文件大小为可读单位 */
function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 B';
  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(2))} ${sizes[i]}`;
}

function ImageThumbnail({ src, alt }: { src: string; alt: string }) {
  const [hasError, setHasError] = useState(false);
  if (hasError) {
    return <FileImageOutlined style={{ color: '#13c2c2' }} className={styles.fileIcon} />;
  }
  return (
    <img
      src={src}
      alt={alt}
      className={styles.fileImagePreview}
      onError={() => setHasError(true)}
    />
  );
}

/** 获取对应文件类型的图标 */
function getFileIcon(ext: string, previewUrl?: string) {
  const e = ext.toLowerCase();
  if (['.png', '.jpg', '.jpeg', '.gif', '.svg', '.webp'].includes(e) && previewUrl) {
    return <ImageThumbnail src={previewUrl} alt="preview" />;
  }
  if (['.png', '.jpg', '.jpeg', '.gif', '.svg', '.webp'].includes(e)) {
    return <FileImageOutlined style={{ color: '#13c2c2' }} className={styles.fileIcon} />;
  }
  if (['.pdf'].includes(e)) {
    return <FilePdfOutlined style={{ color: '#ff4d4f' }} className={styles.fileIcon} />;
  }
  if (['.doc', '.docx'].includes(e)) {
    return <FileWordOutlined style={{ color: '#1677ff' }} className={styles.fileIcon} />;
  }
  if (['.xls', '.xlsx', '.csv'].includes(e)) {
    return <FileExcelOutlined style={{ color: '#52c41a' }} className={styles.fileIcon} />;
  }
  if (['.zip', '.rar', '.7z', '.tar', '.gz'].includes(e)) {
    return <FileZipOutlined style={{ color: '#fa8c16' }} className={styles.fileIcon} />;
  }
  if (['.txt', '.json', '.xml', '.md'].includes(e)) {
    return <FileTextOutlined style={{ color: '#722ed1' }} className={styles.fileIcon} />;
  }
  return <FileOutlined style={{ color: '#8c8c8c' }} className={styles.fileIcon} />;
}

export default function StorageManagement() {
  const [tableItems, setTableItems] = useState<StorageFileResultDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [pageIndex, setPageIndex] = useState(1);
  const pageSize = 10;

  // 检索草稿与实际条件
  const [draftKeyword, setDraftKeyword] = useState('');
  const [keyword, setKeyword] = useState('');
  const [category, setCategory] = useState<string | undefined>();
  const [extension, setExtension] = useState<string | undefined>();

  // 弹窗状态
  const [uploadModalOpen, setUploadModalOpen] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [fileList, setFileList] = useState<UploadFile[]>([]);
  const [uploadForm] = Form.useForm();

  // 预览与详情
  const [detailFile, setDetailFile] = useState<StorageFileResultDto | undefined>();
  const [previewFile, setPreviewFile] = useState<StorageFileResultDto | undefined>();

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const res = await StorageApiService.getStorageFilesPaged({
        keyword: keyword || undefined,
        category: category || undefined,
        extension: extension || undefined,
        pageIndex,
        pageSize,
      });
      setTableItems(res.items || []);
      setTotalCount(res.totalCount || 0);
    } catch {
      setTableItems([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  }, [keyword, category, extension, pageIndex]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  const submitSearch = () => {
    setPageIndex(1);
    setKeyword(draftKeyword.trim());
  };

  const resetSearch = () => {
    setDraftKeyword('');
    setKeyword('');
    setCategory(undefined);
    setExtension(undefined);
    setPageIndex(1);
  };

  // 执行上传
  const handleUploadSubmit = async () => {
    if (fileList.length === 0) {
      message.warning('请先选择要上传的文件。');
      return;
    }

    const values = await uploadForm.validateFields();
    const targetItem = fileList[0];
    const rawFile = (targetItem.originFileObj || (targetItem as unknown as File)) as File;
    if (!rawFile || !(rawFile instanceof Blob)) {
      message.error('未找到有效文件数据。');
      return;
    }

    setUploading(true);
    try {
      await StorageApiService.uploadFile(
        rawFile,
        values.category || 'general',
        values.isPublic || false,
        values.description
      );
      message.success('文件上传成功！');
      setUploadModalOpen(false);
      setFileList([]);
      uploadForm.resetFields();
      void loadData();
    } catch {
      // 错误由全局拦截器处理
    } finally {
      setUploading(false);
    }
  };

  // 删除文件
  const handleDelete = async (id: string) => {
    await StorageApiService.deleteStorageFile(id);
    message.success('文件已成功删除。');
    if (tableItems.length === 1 && pageIndex > 1) {
      setPageIndex(pageIndex - 1);
    } else {
      void loadData();
    }
  };

  // 复制访问直链
  const copyFileUrl = (item: StorageFileResultDto) => {
    const fullUrl = StorageApiService.getPreviewUrl(item.id);
    void navigator.clipboard.writeText(fullUrl);
    message.success('文件直链已复制到剪贴板。');
  };

  const columns: ColumnsType<StorageFileResultDto> = [
    {
      title: '文件名',
      dataIndex: 'fileName',
      key: 'fileName',
      width: 260,
      render: (name: string, record) => (
        <div className={styles.fileNameWrapper}>
          {getFileIcon(record.extension, StorageApiService.getPreviewUrl(record.id))}
          <div className={styles.fileMeta}>
            <Tooltip title={name}>
              <span className={styles.fileName}>{name}</span>
            </Tooltip>
            <span className={styles.fileSubMeta}>
              {record.extension} · {formatFileSize(record.sizeBytes)}
            </span>
          </div>
        </div>
      ),
    },
    {
      title: '存储标识 (ULID)',
      dataIndex: 'id',
      key: 'id',
      width: 170,
      render: (id: string) => (
        <Text code copyable={{ text: id }}>
          {id.slice(0, 10)}...
        </Text>
      ),
    },
    {
      title: '业务分类',
      dataIndex: 'category',
      key: 'category',
      width: 110,
      render: (cat: string) => <Tag color="geekblue">{cat || 'general'}</Tag>,
    },
    {
      title: '公开性',
      dataIndex: 'isPublic',
      key: 'isPublic',
      width: 90,
      render: (isPub: boolean) =>
        isPub ? <Tag color="green">公开</Tag> : <Tag color="default">私有</Tag>,
    },
    {
      title: 'MIME 类型',
      dataIndex: 'contentType',
      key: 'contentType',
      width: 140,
      ellipsis: true,
    },
    {
      title: '上传时间',
      dataIndex: 'createdAtUtc',
      key: 'createdAtUtc',
      width: 170,
      render: (time: string) => time?.replace('T', ' ').slice(0, 19) || '-',
    },
    {
      title: '操作',
      key: 'actions',
      fixed: 'right',
      width: 320,
      render: (_, record) => (
        <Space size={4}>
          <Tooltip title="查看文件元数据详情">
            <Button
              type="text"
              size="small"
              icon={<EyeOutlined />}
              onClick={() => setDetailFile(record)}
              style={{ color: 'var(--color-primary)', fontWeight: 500 }}
            >
              详情
            </Button>
          </Tooltip>
          {['.png', '.jpg', '.jpeg', '.gif', '.svg', '.webp', '.pdf'].includes(
            record.extension.toLowerCase()
          ) && (
            <Tooltip title="在线预览文件">
              <Button
                type="text"
                size="small"
                icon={<FileImageOutlined />}
                onClick={() => setPreviewFile(record)}
                style={{ color: '#13c2c2', fontWeight: 500 }}
              >
                预览
              </Button>
            </Tooltip>
          )}
          <Tooltip title="复制文件预览直链">
            <Button
              type="text"
              size="small"
              icon={<CopyOutlined />}
              onClick={() => copyFileUrl(record)}
              style={{ color: '#722ed1', fontWeight: 500 }}
            >
              直链
            </Button>
          </Tooltip>
          <Tooltip title="下载文件至本地">
            <Button
              type="text"
              size="small"
              icon={<DownloadOutlined />}
              href={StorageApiService.getDownloadUrl(record.id)}
              target="_blank"
              style={{ color: '#52c41a', fontWeight: 500 }}
            >
              下载
            </Button>
          </Tooltip>
          <Popconfirm
            title="确认删除该文件？"
            description="删除后将同步清除底层存储介质中的物理文件，不可恢复。"
            okText="确定删除"
            cancelText="取消"
            okButtonProps={{ danger: true }}
            onConfirm={() => void handleDelete(record.id)}
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
    <main className={styles.pageContainer}>
      {/* 检索卡片 */}
      <Card className={styles.headerCard} bordered={false}>
        <div className={styles.searchHeader}>
          <div className={styles.searchForm}>
            <Input
              style={{ width: 220 }}
              placeholder="按文件名搜索..."
              prefix={<SearchOutlined style={{ color: 'var(--color-placeholder)' }} />}
              value={draftKeyword}
              onChange={(e) => setDraftKeyword(e.target.value)}
              onPressEnter={submitSearch}
              allowClear
            />
            <Select
              style={{ width: 140 }}
              placeholder="业务分类"
              value={category}
              onChange={setCategory}
              allowClear
              options={[
                { label: '通用 (general)', value: 'general' },
                { label: '用户头像 (avatar)', value: 'avatar' },
                { label: '业务附件 (attachment)', value: 'attachment' },
                { label: '归档文档 (document)', value: 'document' },
              ]}
            />
            <Select
              style={{ width: 130 }}
              placeholder="文件类型"
              value={extension}
              onChange={setExtension}
              allowClear
              options={[
                { label: '图片 (.png/.jpg)', value: '.png' },
                { label: '文档 (.pdf)', value: '.pdf' },
                { label: '表格 (.xlsx)', value: '.xlsx' },
                { label: '压缩包 (.zip)', value: '.zip' },
              ]}
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
            onClick={() => {
              setFileList([]);
              uploadForm.resetFields();
              uploadForm.setFieldsValue({ category: 'general', isPublic: false });
              setUploadModalOpen(true);
            }}
          >
            上传文件
          </Button>
        </div>
      </Card>

      {/* 表格卡片 */}
      <Card className={styles.tableCard} bordered={false}>
        <Table<StorageFileResultDto>
          rowKey="id"
          loading={loading}
          columns={columns}
          dataSource={tableItems}
          pagination={{
            current: pageIndex,
            pageSize,
            total: totalCount,
            showTotal: (total) => `共 ${total} 个文件`,
            onChange: (p) => setPageIndex(p),
          }}
          scroll={{ x: 1000 }}
        />
      </Card>

      {/* 上传文件弹窗 */}
      <Modal
        title="上传文件至对象存储"
        open={uploadModalOpen}
        onOk={() => void handleUploadSubmit()}
        confirmLoading={uploading}
        onCancel={() => setUploadModalOpen(false)}
        width={600}
        destroyOnClose
      >
        <Form form={uploadForm} layout="vertical" style={{ paddingTop: 12 }}>
          <Form.Item label="选择文件" required>
            <Dragger
              fileList={fileList}
              beforeUpload={(file) => {
                setFileList([file]);
                return false;
              }}
              onRemove={() => {
                setFileList([]);
              }}
              maxCount={1}
            >
              <p className="ant-upload-drag-icon">
                <InboxOutlined style={{ color: 'var(--color-primary)' }} />
              </p>
              <p className="ant-upload-text">点击或将文件拖拽到此区域上传</p>
              <p className="ant-upload-hint">支持单文件流式持久化存储与哈希校验</p>
            </Dragger>
          </Form.Item>

          <Row gutter={16}>
            <Col span={12}>
              <Form.Item
                name="category"
                label="业务分类"
                initialValue="general"
                rules={[{ required: true, message: '请选择业务分类' }]}
              >
                <Select
                  options={[
                    { label: '通用分类 (general)', value: 'general' },
                    { label: '用户头像 (avatar)', value: 'avatar' },
                    { label: '业务附件 (attachment)', value: 'attachment' },
                    { label: '系统文档 (document)', value: 'document' },
                  ]}
                />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item
                name="isPublic"
                label="访问策略"
                valuePropName="checked"
                initialValue={false}
              >
                <Switch checkedChildren="公开" unCheckedChildren="私有" />
              </Form.Item>
            </Col>
            <Col span={24}>
              <Form.Item name="description" label="备注说明">
                <Input.TextArea rows={2} placeholder="关于该文件的说明或备注..." maxLength={512} />
              </Form.Item>
            </Col>
          </Row>
        </Form>
      </Modal>

      {/* 详情弹窗 */}
      <Modal
        title="文件元数据详情"
        open={Boolean(detailFile)}
        onCancel={() => setDetailFile(undefined)}
        footer={[
          <Button key="close" type="primary" onClick={() => setDetailFile(undefined)}>
            关闭
          </Button>,
        ]}
        width={680}
      >
        {detailFile && (
          <Descriptions bordered size="small" column={2}>
            <Descriptions.Item label="文件名" span={2}>
              {detailFile.fileName}
            </Descriptions.Item>
            <Descriptions.Item label="文件标识 (ULID)">
              <Text code copyable>
                {detailFile.id}
              </Text>
            </Descriptions.Item>
            <Descriptions.Item label="存储文件名">
              <Text code>{detailFile.storageName}</Text>
            </Descriptions.Item>
            <Descriptions.Item label="扩展名">{detailFile.extension}</Descriptions.Item>
            <Descriptions.Item label="文件大小">
              {formatFileSize(detailFile.sizeBytes)} ({detailFile.sizeBytes} 字节)
            </Descriptions.Item>
            <Descriptions.Item label="MIME 类型">{detailFile.contentType}</Descriptions.Item>
            <Descriptions.Item label="业务分类">
              <Tag color="geekblue">{detailFile.category}</Tag>
            </Descriptions.Item>
            <Descriptions.Item label="访问直链" span={2}>
              <Text code copyable>
                {StorageApiService.getPreviewUrl(detailFile.id)}
              </Text>
            </Descriptions.Item>
            <Descriptions.Item label="MD5 哈希" span={2}>
              <Text code>{detailFile.md5Hash || '-'}</Text>
            </Descriptions.Item>
            <Descriptions.Item label="上传时间" span={2}>
              {detailFile.createdAtUtc?.replace('T', ' ').slice(0, 19)}
            </Descriptions.Item>
            <Descriptions.Item label="说明备注" span={2}>
              {detailFile.description || '-'}
            </Descriptions.Item>
          </Descriptions>
        )}
      </Modal>

      {/* 图片/PDF 在线预览弹窗 */}
      <Modal
        title={previewFile?.fileName || '文件在线预览'}
        open={Boolean(previewFile)}
        onCancel={() => setPreviewFile(undefined)}
        footer={[
          <Button
            key="download"
            icon={<DownloadOutlined />}
            href={previewFile ? StorageApiService.getDownloadUrl(previewFile.id) : undefined}
            target="_blank"
          >
            下载原文件
          </Button>,
          <Button key="close" type="primary" onClick={() => setPreviewFile(undefined)}>
            关闭
          </Button>,
        ]}
        width={720}
      >
        {previewFile && (
          <div className={styles.previewModalContent}>
            {['.png', '.jpg', '.jpeg', '.gif', '.svg', '.webp'].includes(
              previewFile.extension.toLowerCase()
            ) ? (
              <img
                src={StorageApiService.getPreviewUrl(previewFile.id)}
                alt={previewFile.fileName}
                className={styles.previewImage}
              />
            ) : previewFile.extension.toLowerCase() === '.pdf' ? (
              <iframe
                src={StorageApiService.getPreviewUrl(previewFile.id)}
                title={previewFile.fileName}
                style={{ width: '100%', height: '500px', border: 'none' }}
              />
            ) : (
              <p>该文件格式不支持内联渲染预览，请点击下方按钮直接下载。</p>
            )}
          </div>
        )}
      </Modal>
    </main>
  );
}
