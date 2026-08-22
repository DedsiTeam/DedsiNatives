import React from 'react';
import { Tooltip } from 'antd';

/**
 * 从本地持久化会话中读取当前用户的权限编码列表。
 */
export function getCurrentUserPermissions(): string[] {
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
 * 校验当前用户是否拥有指定的权限。
 * @param permission 待校验的权限编码（单项或数组）
 * @param logical 多个权限时的逻辑运算符：'OR'（满足其一）或 'AND'（满足全部），默认为 'OR'
 */
export function checkPermission(
  permission?: string | string[],
  logical: 'OR' | 'AND' = 'OR'
): boolean {
  if (!permission) return true;
  const userPermissions = getCurrentUserPermissions();
  
  if (Array.isArray(permission)) {
    if (permission.length === 0) return true;
    return logical === 'AND'
      ? permission.every((p) => userPermissions.includes(p))
      : permission.some((p) => userPermissions.includes(p));
  }
  
  return userPermissions.includes(permission);
}

/**
 * 权限状态与检查工具 Hook。
 */
export function useAuth() {
  const permissions = getCurrentUserPermissions();

  const hasPermission = (permission?: string | string[], logical: 'OR' | 'AND' = 'OR') =>
    checkPermission(permission, logical);

  return {
    permissions,
    hasPermission,
  };
}

export interface AuthProps {
  /** 权限编码（支持单个字符串或权限数组） */
  permission?: string | string[];
  /** 多权限校验逻辑，默认为 'OR' */
  logical?: 'OR' | 'AND';
  /** 无权限时的展示模式：'hide' (隐藏节点) | 'disable' (禁用子元素)，默认为 'hide' */
  mode?: 'hide' | 'disable';
  /** 无权限被隐藏时渲染的兜底内容（仅在 mode='hide' 下生效） */
  fallback?: React.ReactNode;
  /** 无权限被禁用时悬浮提示的文本（仅在 mode='disable' 下生效） */
  disabledTooltip?: string;
  /** 子节点 */
  children: React.ReactNode;
}

/**
 * 细粒度按钮/组件级权限控制组件。
 *
 * @example
 * // 1. 无权限时直接隐藏
 * <Auth permission="system:users:create">
 *   <Button type="primary">新建用户</Button>
 * </Auth>
 *
 * // 2. 无权限时禁用并提示
 * <Auth permission="system:users:delete" mode="disable" disabledTooltip="暂无删除权限">
 *   <Button danger>删除</Button>
 * </Auth>
 */
export const Auth: React.FC<AuthProps> = ({
  permission,
  logical = 'OR',
  mode = 'hide',
  fallback = null,
  disabledTooltip = '暂无操作权限',
  children,
}) => {
  const isAllowed = checkPermission(permission, logical);

  if (isAllowed) {
    return <>{children}</>;
  }

  if (mode === 'hide') {
    return <>{fallback}</>;
  }

  if (mode === 'disable') {
    if (React.isValidElement(children)) {
      const disabledChild = React.cloneElement(children as React.ReactElement<{ disabled?: boolean; style?: React.CSSProperties }>, {
        disabled: true,
        style: {
          ...((children as React.ReactElement<{ style?: React.CSSProperties }>).props.style || {}),
          pointerEvents: 'none',
        },
      });

      return (
        <Tooltip title={disabledTooltip}>
          <span style={{ display: 'inline-block', cursor: 'not-allowed' }}>
            {disabledChild}
          </span>
        </Tooltip>
      );
    }
  }

  return <>{fallback}</>;
};

export default Auth;
