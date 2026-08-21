import React, { useEffect, useRef } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { message } from 'antd';

interface AuthGuardProps {
  /** 访问该页面所需的权限编码（如 Users.View），不传则仅校验登录态 */
  permission?: string;
  /** 子页面组件 */
  children: React.ReactNode;
}

/**
 * 从 localstorage current_user 读取当前用户的权限列表
 */
function getCurrentUserPermissions(): string[] {
  try {
    const raw = localStorage.getItem('current_user');
    if (!raw) return [];
    const parsed = JSON.parse(raw);
    return Array.isArray(parsed?.permissions)
      ? parsed.permissions.filter((p: unknown): p is string => typeof p === 'string')
      : [];
  } catch {
    return [];
  }
}

/**
 * 路由权限鉴权守卫组件：
 * 若当前登录用户没有指定权限，弹出无权限提示并自动重定向至首页（/dashboard）。
 */
export const AuthGuard: React.FC<AuthGuardProps> = ({ permission, children }) => {
  const location = useLocation();
  const permissions = getCurrentUserPermissions();
  const hasNotifiedRef = useRef(false);

  const hasPermission = !permission || permissions.includes(permission);

  useEffect(() => {
    if (!hasPermission && !hasNotifiedRef.current) {
      hasNotifiedRef.current = true;
      message.warning('您没有权限访问该页面，已跳转至首页');
    }
  }, [hasPermission, location.pathname]);

  if (!hasPermission) {
    return <Navigate to="/dashboard" replace />;
  }

  return <>{children}</>;
};

export default AuthGuard;
