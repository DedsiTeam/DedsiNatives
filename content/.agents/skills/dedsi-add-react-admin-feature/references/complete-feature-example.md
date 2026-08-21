# 完整功能示例

以下示例说明从零新增 `Product` 管理功能时可采用的纵向结构。API 和 UI 的具体约束以对应 Skill 为准；只有当前任务需要其完整示例时才读取，不要仅因使用本示例而连带加载全部参考文件。

## 文档导航

- [文件结构](#文件结构)
- [页面业务示例](#页面业务示例)
- [路由示例](#路由示例)
- [输出检查](#输出检查)

## 文件结构

```text
src/
├─ apiServices/
│  ├─ modules/product/
│  │  ├─ dtos/
│  │  │  ├─ product-input.dto.ts
│  │  │  └─ product-result.dto.ts
│  │  └─ product.service.ts
│  └─ index.ts
├─ pages/catalog/products/
│  ├─ index.tsx
│  └─ index.module.css
└─ router/index.tsx
```

API 文件遵循 `$dedsi-build-react-admin-api`，页面视觉遵循 `$dedsi-style-react-admin-ui`；优先参考真实契约、现有相邻模块和当前组件实现。

## 页面业务示例

文件：`src/pages/catalog/products/index.tsx`。标准分页管理页面必须优先复用 `src/components/crud/`，页面只保留筛选条件、列定义与业务操作。

```tsx
import { useMemo, useState } from 'react';
import {
  Button,
  Form,
  Input,
  InputNumber,
  Modal,
  Popconfirm,
  Space,
  message,
  type TableProps,
} from 'antd';
import { DeleteOutlined, EditOutlined } from '@ant-design/icons';
import {
  ProductApiService,
  type CreateProductInputDto,
  type ProductRowResultDto,
  type UpdateProductInputDto,
} from '../../../apiServices';
import {
  CopyableIdTag,
  CrudTable,
  CrudToolbar,
  useCrudTable,
} from '../../../components';
import styles from './index.module.css';

/** 产品管理页面，负责产品筛选及资料维护。 */
export default function ProductManagement() {
  const [draftKeyword, setDraftKeyword] = useState('');
  const [keyword, setKeyword] = useState('');
  const [editing, setEditing] = useState<ProductRowResultDto | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [form] = Form.useForm<CreateProductInputDto>();

  // filters 必须保持引用稳定；条件变化后 useCrudTable 自动回到第一页。
  const queryFilters = useMemo(
    () => ({ name: keyword || undefined }),
    [keyword]
  );

  const {
    items,
    loading,
    pagination,
    loadData,
    handleDelete,
  } = useCrudTable<ProductRowResultDto, { name?: string }>({
    fetchApi: ProductApiService.getPageList,
    deleteApi: ProductApiService.delete,
    filters: queryFilters,
  });

  /** 提交搜索条件；页码重置由通用 Hook 负责。 */
  const handleSearch = () => {
    setKeyword(draftKeyword.trim());
  };

  /** 清空草稿和已提交条件。 */
  const handleReset = () => {
    setDraftKeyword('');
    setKeyword('');
  };

  /** 打开新增或编辑表单。 */
  const openForm = (item?: ProductRowResultDto) => {
    setEditing(item ?? null);
    form.setFieldsValue(
      item ? { name: item.name, price: item.price } : { name: '', price: 0 }
    );
    setModalOpen(true);
  };

  /** 保存产品并刷新当前列表。 */
  const handleSubmit = async () => {
    let values: CreateProductInputDto;
    try {
      values = await form.validateFields();
    } catch {
      return;
    }

    setSubmitting(true);
    try {

      if (editing) {
        const input: UpdateProductInputDto = values;
        await ProductApiService.update(editing.id, input);
        message.success('产品已更新');
      } else {
        await ProductApiService.create(values);
        message.success('产品已创建');
      }

      setModalOpen(false);
      form.resetFields();
      await loadData();
    } catch {
      // 通用请求失败由请求客户端统一提示，保留表单内容供用户修正后重试。
    } finally {
      setSubmitting(false);
    }
  };

  const columns: TableProps<ProductRowResultDto>['columns'] = [
    { title: '产品名称', dataIndex: 'name', key: 'name' },
    {
      title: '产品 ID',
      dataIndex: 'id',
      key: 'id',
      render: (id: string) => <CopyableIdTag id={id} label="产品 ID" />,
    },
    {
      title: '单价',
      dataIndex: 'price',
      key: 'price',
      render: (price: number) => `¥${price.toFixed(2)}`,
    },
    {
      title: '操作',
      key: 'actions',
      render: (_, record) => (
        <Space>
          <Button
            type="text"
            icon={<EditOutlined />}
            onClick={() => openForm(record)}
          >
            编辑
          </Button>
          <Popconfirm
            title="确认删除该产品？"
            onConfirm={() => void handleDelete(record.id, '产品已删除')}
          >
            <Button type="text" danger icon={<DeleteOutlined />}>删除</Button>
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <main className={styles.page}>
      <CrudToolbar
        searchPlaceholder="按产品名称搜索..."
        searchValue={draftKeyword}
        onSearchChange={setDraftKeyword}
        onSearch={handleSearch}
        onReset={handleReset}
        createButton={{ text: '新增产品', onClick: () => openForm() }}
      />

      <CrudTable<ProductRowResultDto>
        rowKey="id"
        columns={columns}
        dataSource={items}
        loading={loading}
        pagination={pagination}
        emptyText="暂无产品数据"
      />

      <Modal
        title={editing ? '编辑产品' : '新增产品'}
        open={modalOpen}
        onOk={() => void handleSubmit()}
        onCancel={() => !submitting && setModalOpen(false)}
        confirmLoading={submitting}
        cancelButtonProps={{ disabled: submitting }}
      >
        <Form form={form} layout="vertical">
          <Form.Item
            name="name"
            label="产品名称"
            rules={[{ required: true, message: '请输入产品名称' }]}
          >
            <Input />
          </Form.Item>
          <Form.Item
            name="price"
            label="单价"
            rules={[{ required: true, message: '请输入产品单价' }]}
          >
            <InputNumber min={0} precision={2} style={{ width: '100%' }} />
          </Form.Item>
        </Form>
      </Modal>
    </main>
  );
}
```

创建或更新成功后关闭弹窗、重置表单并调用 `loadData()`；标准删除必须交给 `handleDelete()` 处理刷新和末页回退。失败时保留用户输入，方便修正后重试。实际字段、校验和弹窗内容必须按真实 DTO 与业务契约调整。

## 路由示例

在 `src/router/index.tsx` 中静态导入并注册：

```tsx
import ProductManagement from '../pages/catalog/products';

{
  path: 'catalog/products',
  element: <ProductManagement />,
}
```

若菜单中增加入口，其 key 和导航地址都使用 `/catalog/products`。不要只加菜单不加路由，也不要让菜单路径与路由大小写不一致。

## 输出检查

- 输入 DTO、结果 DTO、Service、页面和路由均已创建并从统一出口导出。
- 页面只使用后端真实字段，不把临时展示字段混入 DTO。
- 搜索状态与输入状态分离，避免每输入一个字符立即请求。
- 标准分页页面复用 CRUD 组件，`filters` 引用稳定，页面没有重复维护分页状态。
- API、页面组件、关键状态和复杂分支具备中文注释。
- 样式来自同目录 CSS Module，并满足 UI skill 的状态和响应式要求。
