import React from 'react';
import { Tooltip, message } from 'antd';
import { CopyOutlined } from '@ant-design/icons';
import styles from './CopyableIdTag.module.css';

export interface CopyableIdTagProps {
  /** 待展示与复制的唯一标识 ID */
  id: string;
  /** 复制成功提示中显示的实体类型名称，如 '系统 ID'、'用户 ID' */
  label?: string;
  /** 自定义类名 */
  className?: string;
  /** 自定义样式 */
  style?: React.CSSProperties;
}

/**
 * 通用带复制反馈的 ID 标签组件
 */
export const CopyableIdTag: React.FC<CopyableIdTagProps> = ({
  id,
  label = 'ID',
  className,
  style,
}) => {
  const handleCopy = async (e: React.MouseEvent) => {
    e.stopPropagation();
    if (!id) return;
    try {
      await navigator.clipboard.writeText(id);
      message.success(`已复制 ${label} 到剪贴板`);
    } catch {
      message.error('复制失败，请手动选择复制');
    }
  };

  if (!id) return <span style={{ color: 'var(--color-placeholder)' }}>-</span>;

  return (
    <Tooltip title={`点击复制 ${label}`}>
      <span
        className={`${styles.idTag} ${className || ''}`}
        style={style}
        onClick={handleCopy}
      >
        <span>{id}</span>
        <CopyOutlined className={styles.copyIcon} />
      </span>
    </Tooltip>
  );
};
