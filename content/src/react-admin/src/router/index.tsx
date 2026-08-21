import { createBrowserRouter, Navigate } from 'react-router-dom';
import AdminLayout from '../layouts/AdminLayout';
import AuthGuard from '../components/AuthGuard';
import Dashboard from '../pages/dashboard';
import UserManagement from '../pages/system/users';
import SystemManagement from '../pages/system/systems';
import PermissionManagement from '../pages/system/permissions';
import PositionManagement from '../pages/system/positions';
import OrganizationManagement from '../pages/system/organizations';
import StorageManagement from '../pages/system/storage';
import MenuManagement from '../pages/system/menus';
import DictionaryManagement from '../pages/system/dictionaries';
import LoginAuditManagement from '../pages/system/login-audits';
import SsoApplications from '../pages/sso/applications';
import SsoScopes from '../pages/sso/scopes';
import SsoAuthorizations from '../pages/sso/authorizations';
import SsoTokens from '../pages/sso/tokens';
import ProfilePage from '../pages/profile';
import ChangePasswordPage from '../pages/change-password';
import LoginPage from '../pages/login';
import CallbackPage from '../pages/callback';
import OrderDetail from '../pages/orders/detail';

export const router = createBrowserRouter([
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/callback',
    element: <CallbackPage />,
  },
  {
    path: '/',
    element: <AdminLayout />,
    children: [
      {
        index: true,
        element: <Navigate to="/dashboard" replace />,
      },
      {
        path: 'dashboard',
        element: <Dashboard />,
      },
      {
        path: 'system/users',
        element: (
          <AuthGuard permission="system:users:view">
            <UserManagement />
          </AuthGuard>
        ),
      },
      {
        path: 'system/systems',
        element: (
          <AuthGuard permission="system:systems:view">
            <SystemManagement />
          </AuthGuard>
        ),
      },
      {
        path: 'system/permissions',
        element: (
          <AuthGuard permission="system:permissions:view">
            <PermissionManagement />
          </AuthGuard>
        ),
      },
      {
        path: 'system/positions',
        element: (
          <AuthGuard permission="system:positions:view">
            <PositionManagement />
          </AuthGuard>
        ),
      },
      {
        path: 'system/organizations',
        element: (
          <AuthGuard permission="system:organizations:view">
            <OrganizationManagement />
          </AuthGuard>
        ),
      },
      {
        path: 'system/storage',
        element: (
          <AuthGuard permission="system:storage:view">
            <StorageManagement />
          </AuthGuard>
        ),
      },
      {
        path: 'system/menus',
        element: (
          <AuthGuard permission="system:menus:view">
            <MenuManagement />
          </AuthGuard>
        ),
      },
      {
        path: 'system/dictionaries',
        element: (
          <AuthGuard permission="system:dictionaries:view">
            <DictionaryManagement />
          </AuthGuard>
        ),
      },
      {
        path: 'system/login-audits',
        element: (
          <AuthGuard permission="system:login-audits:view">
            <LoginAuditManagement />
          </AuthGuard>
        ),
      },
      {
        path: 'sso/applications',
        element: (
          <AuthGuard permission="system:openiddict:view">
            <SsoApplications />
          </AuthGuard>
        ),
      },
      {
        path: 'sso/scopes',
        element: (
          <AuthGuard permission="system:openiddict:view">
            <SsoScopes />
          </AuthGuard>
        ),
      },
      {
        path: 'sso/authorizations',
        element: (
          <AuthGuard permission="system:openiddict:view">
            <SsoAuthorizations />
          </AuthGuard>
        ),
      },
      {
        path: 'sso/tokens',
        element: (
          <AuthGuard permission="system:openiddict:view">
            <SsoTokens />
          </AuthGuard>
        ),
      },
      { path: 'profile', element: <ProfilePage /> },
      { path: 'change-password', element: <ChangePasswordPage /> },
      {
        path: 'orders',
        element: <OrderDetail />,
      },
      {
        path: '*',
        element: <Navigate to="/dashboard" replace />,
      },
    ],
  },
]);
