import React, { useState } from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import {
  Menu,
  Avatar,
  Dropdown,
  Breadcrumb,
  type MenuProps,
} from 'antd';
import {
  DashboardOutlined,
  UserOutlined,
  SettingOutlined,
  ShoppingOutlined,
  BarChartOutlined,
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  LogoutOutlined,
  LockOutlined,
  SafetyCertificateOutlined,
} from '@ant-design/icons';
import styles from './AdminLayout.module.css';
import './menu-compat.css';

interface AdminLayoutProps {
  children?: React.ReactNode;
}

const menuItems: MenuProps['items'] = [
  {
    key: '/dashboard',
    icon: <DashboardOutlined />,
    label: '仪表盘',
  },
  {
    key: '/system',
    icon: <SettingOutlined />,
    label: '系统管理',
    children: [
      { key: '/system/users', icon: <UserOutlined />, label: '用户管理' },
      { key: '/system/roles', icon: <SafetyCertificateOutlined />, label: '角色权限' },
    ],
  },
  {
    key: '/orders',
    icon: <ShoppingOutlined />,
    label: '订单中心',
  },
  {
    key: '/analytics',
    icon: <BarChartOutlined />,
    label: '数据分析',
  },
  {
    key: '/settings',
    icon: <SettingOutlined />,
    label: '偏好设置',
  },
];

export const AdminLayout: React.FC<AdminLayoutProps> = ({ children }) => {
  const [collapsed, setCollapsed] = useState(false);
  const navigate = useNavigate();
  const location = useLocation();

  // 当前选中的菜单项
  const selectedKeys = [location.pathname || '/dashboard'];

  const handleMenuClick: MenuProps['onClick'] = (e) => {
    navigate(e.key);
  };

  const handleUserMenuClick: MenuProps['onClick'] = (e) => {
    if (e.key === 'logout') {
      navigate('/login');
    }
  };

  // 用户下拉菜单项
  const userMenuItems: MenuProps['items'] = [
    {
      key: 'profile',
      icon: <UserOutlined />,
      label: '个人中心',
    },
    {
      key: 'password',
      icon: <LockOutlined />,
      label: '修改密码',
    },
    {
      type: 'divider',
    },
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      danger: true,
      label: '退出登录',
    },
  ];

  return (
    <div className={styles.layoutContainer}>
      {/* 左侧功能菜单 */}
      <aside
        className={`${styles.sider} ${collapsed ? 'admin-sider-collapsed' : ''}`}
        style={{ width: collapsed ? 64 : 240 }}
      >
        {/* Logo 区域 */}
        <div className={`${styles.logoArea} admin-logo-area`}>
          <div className={styles.logoBadge}>D</div>
          {!collapsed && <span className={styles.logoTitle}>Dedsi Admin</span>}
        </div>

        {/* 菜单树 */}
        <div className={`${styles.menuContainer} admin-sider-menu`}>
          <Menu
            mode="inline"
            inlineCollapsed={collapsed}
            selectedKeys={selectedKeys}
            items={menuItems}
            onClick={handleMenuClick}
            style={{ borderRight: 0 }}
          />
        </div>
      </aside>

      {/* 右侧主区域 */}
      <div className={styles.mainWrapper}>
        {/* 右侧上方功能栏 (固定 60px 高度) */}
        <header className={styles.headerBar}>
          <div className={styles.headerLeft}>
            <div
              className={styles.triggerBtn}
              onClick={() => setCollapsed(!collapsed)}
            >
              {collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
            </div>

            <Breadcrumb
              items={[
                { title: '首页' },
                { title: selectedKeys[0].replace('/', '') || '仪表盘' },
              ]}
            />
          </div>

          <div className={styles.headerRight}>
            <Dropdown menu={{ items: userMenuItems, onClick: handleUserMenuClick }} placement="bottomRight">
              <div className={styles.userInfo}>
                <Avatar
                  size="default"
                  style={{ backgroundColor: 'var(--color-primary)', cursor: 'pointer' }}
                >
                  Admin
                </Avatar>
                <div style={{ display: 'flex', flexDirection: 'column' }}>
                  <span className={styles.userName}>超级管理员</span>
                  <span className={styles.userRole}>admin@dedsi.com</span>
                </div>
              </div>
            </Dropdown>
          </div>
        </header>

        {/* 下方内容区域 */}
        <main className={styles.contentArea}>
          {children || <Outlet />}
        </main>
      </div>
    </div>
  );
};

export default AdminLayout;
