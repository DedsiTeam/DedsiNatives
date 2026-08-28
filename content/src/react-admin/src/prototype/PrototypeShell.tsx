import { Layout, Space, Tag, Typography } from 'antd';
import type { ReactNode } from 'react';
import styles from './PrototypeShell.module.css';

interface PrototypeShellProps {
  workItemId: string;
  title: string;
  description?: string;
  children: ReactNode;
}

export function PrototypeShell({
  workItemId,
  title,
  description,
  children,
}: PrototypeShellProps) {
  return (
    <Layout className={styles.shell}>
      <header className={styles.header}>
        <Space direction="vertical" size={4}>
          <Space wrap>
            <Tag color="blue">静态原型</Tag>
            <Typography.Text code>{workItemId}</Typography.Text>
          </Space>
          <Typography.Title level={3} className={styles.title}>
            {title}
          </Typography.Title>
          {description ? (
            <Typography.Text type="secondary">{description}</Typography.Text>
          ) : null}
        </Space>
      </header>
      <Layout.Content className={styles.content}>{children}</Layout.Content>
    </Layout>
  );
}
