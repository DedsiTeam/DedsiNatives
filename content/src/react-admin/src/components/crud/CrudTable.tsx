import React from 'react';
import { Card, Empty, Table, type TableProps } from 'antd';
import styles from './CrudTable.module.css';

export interface CrudTableProps<RecordType extends object = object>
  extends Omit<TableProps<RecordType>, 'className' | 'style'> {
  /** 自定义卡片包装器类名 */
  cardClassName?: string;
  /** 自定义卡片包装器样式 */
  cardStyle?: React.CSSProperties;
  /** 自定义表格类名 */
  className?: string;
  /** 自定义表格样式 */
  style?: React.CSSProperties;
  /** 空数据文案，默认为 '暂无数据' */
  emptyText?: string;
}

/**
 * 通用 CRUD 数据表格包装器组件
 */
export function CrudTable<RecordType extends object = object>({
  cardClassName,
  cardStyle,
  className,
  style,
  emptyText = '暂无数据',
  locale,
  scroll = { x: 900 },
  ...restTableProps
}: CrudTableProps<RecordType>) {
  return (
    <Card
      className={`${styles.tableCard} ${cardClassName || ''}`}
      styles={{ body: { padding: '16px 24px' } }}
      style={cardStyle}
    >
      <Table<RecordType>
        className={className}
        style={style}
        scroll={scroll}
        locale={{
          emptyText: <Empty description={emptyText} />,
          ...locale,
        }}
        {...restTableProps}
      />
    </Card>
  );
}
