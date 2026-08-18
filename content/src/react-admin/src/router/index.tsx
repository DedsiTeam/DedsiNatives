import { createBrowserRouter, Navigate } from 'react-router-dom';
import AdminLayout from '../layouts/AdminLayout';
import Dashboard from '../pages/dashboard';
import UserManagement from '../pages/system/users';
import SystemManagement from '../pages/system/systems';
import PermissionManagement from '../pages/system/permissions';
import PositionManagement from '../pages/system/positions';
import MenuManagement from '../pages/system/menus';
import DictionaryManagement from '../pages/system/dictionaries';
import LoginAuditManagement from '../pages/system/login-audits';
import ProfilePage from '../pages/profile';
import ChangePasswordPage from '../pages/change-password';
import LoginPage from '../pages/login';
import OrderDetail from '../pages/orders/detail';

export const router = createBrowserRouter([
  {
    path: '/login',
    element: <LoginPage />,
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
        element: <UserManagement />,
      },
      {
        path: 'system/systems',
        element: <SystemManagement />,
      },
      {
        path: 'system/permissions',
        element: <PermissionManagement />,
      },
      {
        path: 'system/positions',
        element: <PositionManagement />,
      },
      { path: 'system/menus', element: <MenuManagement /> },
      { path: 'system/dictionaries', element: <DictionaryManagement /> },
      { path: 'system/login-audits', element: <LoginAuditManagement /> },
      { path: 'profile', element: <ProfilePage /> },
      { path: 'change-password', element: <ChangePasswordPage /> },
      {
        path: 'orders',
        element: <OrderDetail />,
      },
      {
        path: '*',
        element: <Dashboard />,
      },
    ],
  },
]);
