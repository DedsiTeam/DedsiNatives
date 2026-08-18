# 页面与样式示例

以下示例展示后台列表页的结构和样式职责。业务请求由功能 skill 实现；纯 UI 任务不得借此更改原有 API 流程。

## 文档导航

- [页面组件](#页面组件)
- [CSS Module](#css-module)
- [输出要求](#输出要求)

## 页面组件

文件：`src/pages/catalog/products/index.tsx`

```tsx
import { Button, Card, Empty, Input, Space, Table, Typography } from 'antd';
import { PlusOutlined, ReloadOutlined, SearchOutlined } from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import type { ProductRowResultDto } from '../../../apiServices';
import styles from './index.module.css';

const { Text, Title } = Typography;

/**
 * 产品管理页面属性。
 */
interface ProductManagementProps {
  /** 当前页产品数据。 */
  items: ProductRowResultDto[];
  /** 是否正在加载远程数据。 */
  loading: boolean;
  /** 当前搜索关键词。 */
  keyword: string;
  /** 更新搜索关键词。 */
  onKeywordChange: (value: string) => void;
  /** 执行搜索。 */
  onSearch: () => void;
  /** 重新加载当前页。 */
  onReload: () => void;
  /** 打开新增产品弹窗。 */
  onCreate: () => void;
}

/**
 * 展示产品列表、筛选工具栏和主要操作。
 */
export function ProductManagement({
  items,
  loading,
  keyword,
  onKeywordChange,
  onSearch,
  onReload,
  onCreate,
}: ProductManagementProps) {
  const columns: ColumnsType<ProductRowResultDto> = [
    {
      title: '产品名称',
      dataIndex: 'name',
      key: 'name',
    },
    {
      title: '单价',
      dataIndex: 'price',
      key: 'price',
      render: (price: number) => `¥${price.toFixed(2)}`,
    },
  ];

  return (
    <main className={styles.page}>
      <header className={styles.header}>
        <div>
          <Title level={2} className={styles.title}>产品管理</Title>
          <Text type="secondary">维护产品资料、价格和启用状态。</Text>
        </div>
        <Button
          type="primary"
          className="create-primary-button"
          icon={<PlusOutlined />}
          onClick={onCreate}
        >
          新增产品
        </Button>
      </header>

      <Card className={styles.card}>
        <div className={styles.toolbar}>
          <Input
            allowClear
            className={styles.search}
            placeholder="按产品名称搜索"
            prefix={<SearchOutlined />}
            value={keyword}
            onChange={(event) => onKeywordChange(event.target.value)}
            onPressEnter={onSearch}
          />
          <Space>
            <Button onClick={onSearch}>查询</Button>
            <Button icon={<ReloadOutlined />} onClick={onReload}>
              刷新
            </Button>
          </Space>
        </div>

        <Table<ProductRowResultDto>
          rowKey="id"
          columns={columns}
          dataSource={items}
          loading={loading}
          locale={{ emptyText: <Empty description="暂无产品数据" /> }}
          scroll={{ x: 720 }}
        />
      </Card>
    </main>
  );
}
```

## CSS Module

文件：`src/pages/catalog/products/index.module.css`

```css
.page {
  display: grid;
  gap: 24px;
  width: 100%;
}

.header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.title {
  margin: 0 0 8px;
  color: var(--color-title);
}

.card {
  border: 1px solid var(--color-border);
  border-radius: 12px;
  box-shadow: var(--shadow-md);
}

.toolbar {
  display: flex;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 16px;
}

.search {
  width: min(360px, 100%);
}

@media (max-width: 768px) {
  .header,
  .toolbar {
    align-items: stretch;
    flex-direction: column;
  }

  .search {
    width: 100%;
  }
}
```

## 输出要求

- 组件从统一 `apiServices` 出口导入业务类型，不在页面重复声明网络 DTO。
- 纯展示组件通过具名 props 接收数据和事件，禁止偷偷发请求或改写业务状态。
- 样式名称表达语义，不使用 `box1`、`blueText` 等依赖视觉结果的名称。
- 色彩必须使用项目变量；字面色值只能在 `src/index.css` 的 `:root` 中定义，缺少语义色时先增加可复用 Token。
- 所有新增类主操作按钮直接使用全局 `create-primary-button` 类，不在页面 CSS Module 重复定义背景样式。
- 表格在窄屏允许横向滚动，工具栏自然换行，主要操作保持清晰可达。
