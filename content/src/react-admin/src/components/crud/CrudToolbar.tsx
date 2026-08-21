import React from 'react';
import { Button, Card, Input, Space } from 'antd';
import { PlusOutlined, ReloadOutlined, SearchOutlined } from '@ant-design/icons';
import styles from './CrudToolbar.module.css';

export interface CreateButtonConfig {
  text?: string;
  icon?: React.ReactNode;
  onClick: () => void;
  disabled?: boolean;
  hidden?: boolean;
  type?: 'primary' | 'default' | 'dashed' | 'link' | 'text';
}

export interface CrudToolbarProps {
  /** 搜索输入框 placeholder */
  searchPlaceholder?: string;
  /** 搜索框草稿值 */
  searchValue?: string;
  /** 搜索框值变动回调 */
  onSearchChange?: (val: string) => void;
  /** 点击「查询」或在搜索框按回车触发的回调 */
  onSearch?: () => void;
  /** 点击「重置」按钮触发的回调 */
  onReset?: () => void;
  /** 是否展示查询与重置按钮（默认为 true） */
  showSearchButtons?: boolean;
  /** 额外的筛选表单项插槽（如系统下拉、组织树选择器、状态切换等） */
  extraFilters?: React.ReactNode;
  /** 快捷新建按钮配置 */
  createButton?: CreateButtonConfig;
  /** 顶部右侧自定义动作组插槽（如导出、批量操作、自定义按钮等） */
  extraActions?: React.ReactNode;
  /** 自定义卡片类名 */
  className?: string;
  /** 自定义样式 */
  style?: React.CSSProperties;
}

/**
 * 通用 CRUD 顶部检索与操作工具栏组件
 */
export const CrudToolbar: React.FC<CrudToolbarProps> = ({
  searchPlaceholder = '按关键字搜索...',
  searchValue,
  onSearchChange,
  onSearch,
  onReset,
  showSearchButtons = true,
  extraFilters,
  createButton,
  extraActions,
  className,
  style,
}) => {
  return (
    <Card
      className={`${styles.toolbarCard} ${className || ''}`}
      styles={{ body: { padding: '16px 24px' } }}
      style={style}
    >
      <div className={styles.toolbarWrapper}>
        <div className={styles.filterGroup}>
          {onSearchChange !== undefined && (
            <Input
              placeholder={searchPlaceholder}
              prefix={<SearchOutlined style={{ color: 'var(--color-placeholder)' }} />}
              value={searchValue}
              onChange={(e) => onSearchChange(e.target.value)}
              onPressEnter={onSearch}
              className={styles.searchInput}
              allowClear
            />
          )}

          {extraFilters}

          {showSearchButtons && (
            <Space size={8}>
              {onSearch && (
                <Button
                  type="primary"
                  icon={<SearchOutlined />}
                  onClick={onSearch}
                  className={styles.primaryButton}
                >
                  查询
                </Button>
              )}
              {onReset && (
                <Button
                  icon={<ReloadOutlined />}
                  onClick={onReset}
                  style={{ borderRadius: 'var(--radius-btn)' }}
                >
                  重置
                </Button>
              )}
            </Space>
          )}
        </div>

        <div className={styles.actionGroup}>
          {extraActions}

          {createButton && !createButton.hidden && (
            <Button
              type={createButton.type || 'primary'}
              className="create-primary-button"
              icon={createButton.icon ?? <PlusOutlined />}
              onClick={createButton.onClick}
              disabled={createButton.disabled}
            >
              {createButton.text || '新增'}
            </Button>
          )}
        </div>
      </div>
    </Card>
  );
};
