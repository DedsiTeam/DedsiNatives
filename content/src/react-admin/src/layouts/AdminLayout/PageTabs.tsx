import React, { useEffect, useState, useMemo } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { Dropdown, type MenuProps } from 'antd';
import {
  ReloadOutlined,
  CloseOutlined,
  CloseCircleOutlined,
  VerticalLeftOutlined,
  ClearOutlined,
  MoreOutlined,
} from '@ant-design/icons';
import styles from './AdminLayout.module.css';

export interface TabItem {
  key: string;
  title: string;
  closable: boolean;
}

interface PageTabsProps {
  pageTitles: Record<string, string>;
  onReload?: () => void;
}

const DASHBOARD_TAB: TabItem = {
  key: '/dashboard',
  title: '仪表盘',
  closable: false,
};

export const PageTabs: React.FC<PageTabsProps> = ({ pageTitles, onReload }) => {
  const location = useLocation();
  const navigate = useNavigate();
  const currentPath = location.pathname || '/dashboard';

  // 已打开的标签页列表（持久化或状态管理，默认包含固定仪表盘）
  const [tabs, setTabs] = useState<TabItem[]>([DASHBOARD_TAB]);

  // 监听路由变化，自动追加或激活 Tab
  useEffect(() => {
    // 过滤登录、回调或未知根路径
    if (!currentPath || currentPath === '/' || currentPath === '/login' || currentPath === '/callback') {
      return;
    }

    const title = pageTitles[currentPath] || '页面';

    setTabs((prevTabs) => {
      const exists = prevTabs.some((tab) => tab.key === currentPath);
      if (exists) {
        // 如果标题有更新则同步更新
        return prevTabs.map((tab) => (tab.key === currentPath ? { ...tab, title } : tab));
      }
      return [
        ...prevTabs,
        {
          key: currentPath,
          title,
          closable: currentPath !== '/dashboard',
        },
      ];
    });
  }, [currentPath, pageTitles]);

  // 切换选中标签
  const handleTabClick = (key: string) => {
    if (key !== currentPath) {
      navigate(key);
    }
  };

  // 关闭单个标签核心逻辑
  const closeTabByKey = (targetKey: string) => {
    const targetIndex = tabs.findIndex((tab) => tab.key === targetKey);
    if (targetIndex === -1) return;

    const newTabs = tabs.filter((tab) => tab.key !== targetKey);
    setTabs(newTabs);

    // 如果关闭的是当前激活页，自动跳转到相邻页
    if (targetKey === currentPath) {
      const nextTab = newTabs[targetIndex] || newTabs[targetIndex - 1] || DASHBOARD_TAB;
      navigate(nextTab.key);
    }
  };

  // 点击标签上的关闭图标
  const handleCloseTab = (e: React.MouseEvent, targetKey: string) => {
    e.stopPropagation();
    closeTabByKey(targetKey);
  };

  // 关闭其他标签
  const handleCloseOthers = () => {
    const newTabs = tabs.filter((tab) => !tab.closable || tab.key === currentPath);
    setTabs(newTabs);
  };

  // 关闭右侧标签
  const handleCloseRight = () => {
    const currentIndex = tabs.findIndex((tab) => tab.key === currentPath);
    if (currentIndex === -1) return;
    const newTabs = tabs.filter((tab, index) => !tab.closable || index <= currentIndex);
    setTabs(newTabs);
  };

  // 全部关闭（保留仪表盘）
  const handleCloseAll = () => {
    setTabs([DASHBOARD_TAB]);
    navigate('/dashboard');
  };

  // 快捷操作下拉菜单
  const dropdownMenuItems: MenuProps['items'] = useMemo(() => [
    {
      key: 'reload',
      icon: <ReloadOutlined />,
      label: '刷新当前页面',
      onClick: () => {
        if (onReload) {
          onReload();
        } else {
          window.location.reload();
        }
      },
    },
    {
      type: 'divider',
    },
    {
      key: 'close-current',
      icon: <CloseOutlined />,
      label: '关闭当前标签',
      disabled: currentPath === '/dashboard',
      onClick: () => closeTabByKey(currentPath),
    },
    {
      key: 'close-others',
      icon: <CloseCircleOutlined />,
      label: '关闭其他标签',
      disabled: tabs.length <= 1,
      onClick: handleCloseOthers,
    },
    {
      key: 'close-right',
      icon: <VerticalLeftOutlined />,
      label: '关闭右侧标签',
      onClick: handleCloseRight,
    },
    {
      type: 'divider',
    },
    {
      key: 'close-all',
      icon: <ClearOutlined />,
      danger: true,
      label: '全部关闭 (返回仪表盘)',
      disabled: tabs.length <= 1 && currentPath === '/dashboard',
      onClick: handleCloseAll,
    },
  ], [currentPath, tabs, onReload]);

  return (
    <div className={styles.pageTabsBar}>
      <div className={styles.tabsScrollContainer}>
        {tabs.map((tab) => {
          const isActive = tab.key === currentPath;
          return (
            <div
              key={tab.key}
              className={`${styles.tabItem} ${isActive ? styles.tabItemActive : ''}`}
              onClick={() => handleTabClick(tab.key)}
            >
              <span className={styles.tabDot} />
              <span className={styles.tabTitle}>{tab.title}</span>
              {tab.closable && (
                <span
                  className={styles.tabCloseIcon}
                  onClick={(e) => handleCloseTab(e, tab.key)}
                  title="关闭标签"
                >
                  <CloseOutlined style={{ fontSize: 10 }} />
                </span>
              )}
            </div>
          );
        })}
      </div>

      <div className={styles.tabsActionWrapper}>
        <Dropdown menu={{ items: dropdownMenuItems }} placement="bottomRight" arrow>
          <div className={styles.tabActionBtn} title="更多操作">
            <MoreOutlined />
          </div>
        </Dropdown>
      </div>
    </div>
  );
};

export default PageTabs;
