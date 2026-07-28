# 完整功能示例

以下示例说明新增 `Product` 管理功能时必须输出的纵向结构。实际编码前还要完整读取 `$dedsi-build-react-admin-api` 与 `$dedsi-style-react-admin-ui` 的示例。

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

API 文件严格采用 `$dedsi-build-react-admin-api` 的 `references/api-example.md`。页面视觉严格采用 `$dedsi-style-react-admin-ui` 的 `references/ui-example.md`。

## 页面业务示例

文件：`src/pages/catalog/products/index.tsx`。以下代码突出查询、分页和搜索状态；新增、编辑、删除应沿用相同的类型和错误处理方式。

```tsx
import { useCallback, useEffect, useState } from 'react';
import { Button, Card, Input, Space, Table, message } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  ProductApiService,
  type ProductRowResultDto,
} from '../../../apiServices';
import styles from './index.module.css';

/**
 * 产品管理页面，负责产品查询、分页和业务操作编排。
 */
export default function ProductManagement() {
  /** 当前页产品数据。 */
  const [items, setItems] = useState<ProductRowResultDto[]>([]);
  /** 符合条件的产品总数。 */
  const [totalCount, setTotalCount] = useState(0);
  /** 接口请求是否正在执行。 */
  const [loading, setLoading] = useState(false);
  /** 输入框中的临时搜索文本。 */
  const [draftKeyword, setDraftKeyword] = useState('');
  /** 已提交给接口的搜索关键词。 */
  const [keyword, setKeyword] = useState('');
  /** 当前页码，从 1 开始。 */
  const [pageIndex, setPageIndex] = useState(1);
  /** 每页记录数。 */
  const [pageSize, setPageSize] = useState(10);

  /**
   * 按当前已提交条件加载产品列表。
   */
  const loadProducts = useCallback(async () => {
    setLoading(true);

    try {
      const result = await ProductApiService.getPageList({
        pageIndex,
        pageSize,
        name: keyword || undefined,
      });
      setItems(result.items);
      setTotalCount(result.totalCount);
    } catch {
      // 通用错误由请求层提示；页面清空旧数据，避免继续展示过期结果。
      setItems([]);
      setTotalCount(0);
    } finally {
      setLoading(false);
    }
  }, [keyword, pageIndex, pageSize]);

  useEffect(() => {
    void loadProducts();
  }, [loadProducts]);

  /**
   * 提交搜索条件，并回到第一页。
   */
  const handleSearch = () => {
    setPageIndex(1);
    setKeyword(draftKeyword.trim());
  };

  const columns: ColumnsType<ProductRowResultDto> = [
    { title: '产品名称', dataIndex: 'name', key: 'name' },
    {
      title: '单价',
      dataIndex: 'price',
      key: 'price',
      render: (price: number) => `¥${price.toFixed(2)}`,
    },
  ];

  return (
    <main className={styles.page}>
      <Card title="产品管理">
        <div className={styles.toolbar}>
          <Space wrap>
            <Input
              allowClear
              placeholder="请输入产品名称"
              value={draftKeyword}
              onChange={(event) => setDraftKeyword(event.target.value)}
              onPressEnter={handleSearch}
            />
            <Button type="primary" onClick={handleSearch}>查询</Button>
            <Button onClick={() => void loadProducts()}>刷新</Button>
          </Space>
        </div>

        <Table<ProductRowResultDto>
          rowKey="id"
          columns={columns}
          dataSource={items}
          loading={loading}
          scroll={{ x: 720 }}
          pagination={{
            current: pageIndex,
            pageSize,
            total: totalCount,
            showSizeChanger: true,
            onChange: (nextPage, nextPageSize) => {
              // 改变每页条数时回到第一页，避免请求不存在的页码。
              setPageIndex(nextPageSize === pageSize ? nextPage : 1);
              setPageSize(nextPageSize);
            },
          }}
        />
      </Card>
    </main>
  );
}
```

删除成功后应重新加载当前页；若删除的是末页最后一条记录，应先将页码回退一页。创建或更新成功后关闭弹窗、重置表单并刷新列表。失败时保留用户输入，方便修正后重试。

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
- API、页面组件、关键状态和复杂分支具备中文注释。
- 样式来自同目录 CSS Module，并满足 UI skill 的状态和响应式要求。
