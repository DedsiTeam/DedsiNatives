import React, { useEffect, useMemo, useState } from 'react';
import { Outlet, useNavigate, useLocation, Navigate } from 'react-router-dom';
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
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  LogoutOutlined,
  LockOutlined,
  SafetyCertificateOutlined,
  BookOutlined,
  AuditOutlined,
  AppstoreOutlined,
  SolutionOutlined,
  ApartmentOutlined,
  FolderOpenOutlined,
  KeyOutlined,
} from '@ant-design/icons';
import styles from './AdminLayout.module.css';
import './menu-compat.css';

import { MenuApiService, type CurrentUserMenuResultDto, type LoginUserPositionResultDto } from '../../apiServices';

interface AdminLayoutProps {
  children?: React.ReactNode;
}

/**
 * 布局展示所需的当前用户资料。
 */
interface CurrentUser {
  /** 用户姓名。 */
  name: string;
  /** 用户邮箱。 */
  email: string;
  /** 用户登录账号。 */
  account: string;
  /** 当前用户拥有的权限编码。 */
  permissions: string[];
  /** 用户关联的岗位列表。 */
  positions: LoginUserPositionResultDto[];
}

/**
 * 图标映射字典，支持后端动态配置图标名称。
 */
const iconMap: Record<string, React.ReactNode> = {
  DashboardOutlined: <DashboardOutlined />,
  SettingOutlined: <SettingOutlined />,
  AppstoreOutlined: <AppstoreOutlined />,
  SafetyCertificateOutlined: <SafetyCertificateOutlined />,
  MenuOutlined: <SettingOutlined />,
  SolutionOutlined: <SolutionOutlined />,
  UserOutlined: <UserOutlined />,
  BookOutlined: <BookOutlined />,
  AuditOutlined: <AuditOutlined />,
  ApartmentOutlined: <ApartmentOutlined />,
  FolderOpenOutlined: <FolderOpenOutlined />,
  KeyOutlined: <KeyOutlined />,
};

function getMenuIcon(iconName: string | null | undefined): React.ReactNode {
  if (!iconName) return <SettingOutlined />;
  return iconMap[iconName] ?? <SettingOutlined />;
}

/**
 * 从本地登录会话读取布局可展示的用户资料。
 */
function getCurrentUser(): CurrentUser {
  try {
    const storedUser = localStorage.getItem('current_user');
    const parsedUser: unknown = storedUser ? JSON.parse(storedUser) : null;
    if (typeof parsedUser !== 'object' || parsedUser === null) {
      return { name: '', email: '', account: '', permissions: [], positions: [] };
    }

    const user = parsedUser as Partial<CurrentUser>;
    return {
      name: typeof user.name === 'string' ? user.name : '',
      email: typeof user.email === 'string' ? user.email : '',
      account: typeof user.account === 'string' ? user.account : '',
      permissions: Array.isArray(user.permissions)
        ? user.permissions.filter((permission): permission is string => typeof permission === 'string')
        : [],
      positions: Array.isArray(user.positions) ? user.positions : [],
    };
  } catch {
    return { name: '', email: '', account: '', permissions: [], positions: [] };
  }
}

/**
 * 将后端返回的多级动态菜单树转换为 Ant Design Menu 所需的数据结构（已保持 Sort 升序）。
 */
function transformToMenuItems(menus: CurrentUserMenuResultDto[]): MenuProps['items'] {
  return menus.map((menu) => {
    const key = menu.routePath || menu.code || menu.id;
    const hasChildren = menu.children && menu.children.length > 0;

    if (hasChildren) {
      return {
        key,
        icon: getMenuIcon(menu.icon),
        label: menu.name,
        children: transformToMenuItems(menu.children),
      };
    }

    return {
      key,
      icon: getMenuIcon(menu.icon),
      label: menu.name,
    };
  });
}

/**
 * 递归收集所有路由路径对应的页面标题。
 */
function collectPageTitles(menus: CurrentUserMenuResultDto[], acc: Record<string, string> = {}): Record<string, string> {
  for (const menu of menus) {
    if (menu.routePath) {
      acc[menu.routePath] = menu.name;
    }
    if (menu.children && menu.children.length > 0) {
      collectPageTitles(menu.children, acc);
    }
  }
  return acc;
}

const defaultPageTitles: Record<string, string> = {
  '/dashboard': '仪表盘',
  '/system/users': '用户管理',
  '/system/systems': '系统管理',
  '/system/permissions': '权限管理',
  '/system/positions': '岗位管理',
  '/system/menus': '菜单管理',
  '/system/dictionaries': '字典管理',
  '/system/login-audits': '登录审计',
  '/sso/applications': '客户端应用',
  '/sso/scopes': '权限作用域',
  '/sso/authorizations': '用户授权记录',
  '/sso/tokens': '活跃令牌审计',
  '/profile': '个人中心',
  '/change-password': '修改密码',
};

export const AdminLayout: React.FC<AdminLayoutProps> = ({ children }) => {
  const [collapsed, setCollapsed] = useState(false);
  const [userMenus, setUserMenus] = useState<CurrentUserMenuResultDto[]>([]);
  const navigate = useNavigate();
  const location = useLocation();

  const token = localStorage.getItem('access_token');
  const currentUser = getCurrentUser();

  // 加载当前用户可访问的动态菜单
  useEffect(() => {
    let active = true;
    if (token) {
      MenuApiService.getCurrentUserMenus()
        .then((menus) => {
          if (active && Array.isArray(menus)) {
            setUserMenus(menus);
          }
        })
        .catch(() => {
          // 降级使用默认静态菜单处理
        });
    }
    return () => {
      active = false;
    };
  }, [token]);

  // 未登录或没有有效用户信息时，自动重定向至登录页
  if (!token || !currentUser.account) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  // 动态生成菜单项与标题映射表
  const menuItems = useMemo(() => {
    if (userMenus.length > 0) {
      return transformToMenuItems(userMenus);
    }
    // 降级兜底静态菜单
    return [
      { key: '/dashboard', icon: <DashboardOutlined />, label: '仪表盘' },
      {
        key: '/system',
        icon: <SettingOutlined />,
        label: '系统管理',
        children: [
          { key: '/system/systems', icon: <AppstoreOutlined />, label: '系统管理' },
          { key: '/system/permissions', icon: <SafetyCertificateOutlined />, label: '权限管理' },
          { key: '/system/menus', icon: <SettingOutlined />, label: '菜单管理' },
          { key: '/system/positions', icon: <SolutionOutlined />, label: '岗位管理' },
          { key: '/system/users', icon: <UserOutlined />, label: '用户管理' },
          { key: '/system/dictionaries', icon: <BookOutlined />, label: '字典管理' },
          ...(currentUser.permissions.includes('system:login-audits:view')
            ? [{ key: '/system/login-audits', icon: <AuditOutlined />, label: '登录审计' }]
            : []),
        ],
      },
      {
        key: '/sso',
        icon: <SafetyCertificateOutlined />,
        label: 'SSO 认证管理',
        children: [
          { key: '/sso/applications', icon: <AppstoreOutlined />, label: '客户端应用' },
          { key: '/sso/scopes', icon: <SafetyCertificateOutlined />, label: '权限作用域' },
          { key: '/sso/authorizations', icon: <KeyOutlined />, label: '用户授权记录' },
          { key: '/sso/tokens', icon: <AuditOutlined />, label: '活跃令牌审计' },
        ],
      },
    ];
  }, [userMenus, currentUser.permissions]);

  const pageTitles = useMemo(() => {
    const dynamicTitles = collectPageTitles(userMenus);
    return { ...defaultPageTitles, ...dynamicTitles };
  }, [userMenus]);

  // 当前选中的菜单项
  const selectedKeys = [location.pathname || '/dashboard'];
  const currentPageTitle = pageTitles[selectedKeys[0]] ?? '页面';

  const handleMenuClick: MenuProps['onClick'] = (e) => {
    if (e.key.startsWith('/')) {
      navigate(e.key);
    }
  };

  const handleUserMenuClick: MenuProps['onClick'] = (e) => {
    if (e.key === 'profile') navigate('/profile');
    else if (e.key === 'password') navigate('/change-password');
    else if (e.key === 'logout') {
      localStorage.removeItem('access_token');
      localStorage.removeItem('current_user');
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
        style={{ width: collapsed ? 64 : 200 }}
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
                { title: currentPageTitle },
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
                  {(currentUser.name || currentUser.account || 'U').charAt(0).toUpperCase()}
                </Avatar>
                <div style={{ display: 'flex', flexDirection: 'column' }}>
                  <span className={styles.userName}>{currentUser.name || currentUser.account || '未登录用户'}</span>
                  <span className={styles.userRole}>{currentUser.email || currentUser.account || '-'}</span>
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
