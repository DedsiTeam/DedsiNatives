# CRUD 页面与样式示例

本示例只展示标准分页页面的 UI 结构。业务页面通过 `useCrudTable` 提供数据、loading 和 pagination；完整数据与表单编排见 `dedsi-add-react-admin-feature` 的页面示例。

## 页面组件

文件：`src/pages/catalog/products/index.tsx`

```tsx
import { Button, Tag, type TablePaginationConfig, type TableProps } from 'antd';
import { ReloadOutlined } from '@ant-design/icons';
import type { ProductRowResultDto } from '../../../apiServices';
import {
  CopyableIdTag,
  CrudTable,
  CrudToolbar,
} from '../../../components';
import styles from './index.module.css';

/** 产品管理列表的展示属性。 */
interface ProductManagementViewProps {
  /** 当前页产品数据。 */
  items: ProductRowResultDto[];
  /** 是否正在加载远程数据。 */
  loading: boolean;
  /** 通用 Hook 生成的分页配置。 */
  pagination: TablePaginationConfig;
  /** 搜索框草稿值。 */
  draftKeyword: string;
  /** 更新搜索框草稿值。 */
  onDraftKeywordChange: (value: string) => void;
  /** 提交搜索条件。 */
  onSearch: () => void;
  /** 重置搜索条件。 */
  onReset: () => void;
  /** 刷新当前页。 */
  onReload: () => void;
  /** 打开新增产品弹窗。 */
  onCreate: () => void;
}

/** 展示产品列表、筛选工具栏和主要操作。 */
export function ProductManagementView({
  items,
  loading,
  pagination,
  draftKeyword,
  onDraftKeywordChange,
  onSearch,
  onReset,
  onReload,
  onCreate,
}: ProductManagementViewProps) {
  const columns: TableProps<ProductRowResultDto>['columns'] = [
    {
      title: '产品名称',
      dataIndex: 'name',
      key: 'name',
    },
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
      title: '状态',
      dataIndex: 'isEnabled',
      key: 'isEnabled',
      render: (isEnabled: boolean) => (
        <Tag color={isEnabled ? 'success' : 'default'}>
          {isEnabled ? '启用' : '停用'}
        </Tag>
      ),
    },
  ];

  return (
    <main className={styles.page}>
      <CrudToolbar
        searchPlaceholder="按产品名称搜索..."
        searchValue={draftKeyword}
        onSearchChange={onDraftKeywordChange}
        onSearch={onSearch}
        onReset={onReset}
        extraActions={(
          <Button
            icon={<ReloadOutlined spin={loading} />}
            onClick={onReload}
            disabled={loading}
          >
            刷新
          </Button>
        )}
        createButton={{ text: '新增产品', onClick: onCreate }}
      />

      <CrudTable<ProductRowResultDto>
        rowKey="id"
        columns={columns}
        dataSource={items}
        loading={loading}
        pagination={pagination}
        emptyText="暂无产品数据"
      />
    </main>
  );
}
```

## CSS Module

文件：`src/pages/catalog/products/index.module.css`

```css
.page {
  display: flex;
  flex-direction: column;
  gap: var(--space-20);
  width: 100%;
}
```

`CrudToolbar` 和 `CrudTable` 已拥有卡片、工具栏、空状态和响应式基础样式。页面 CSS Module 只添加业务页面确实需要的布局或单元格样式，不复制通用组件内部实现。

## 输出检查

- 页面使用统一 `components` 和 `apiServices` 出口，不重复声明网络 DTO。
- 筛选变化后的页码重置由 `useCrudTable` 处理；展示组件不维护另一套分页状态。
- ID 复制、工具栏、表格卡片、空状态和分页器均复用 CRUD 组件。
- CSS 只消费项目 Token；窄屏时通用组件和业务列仍可操作。
