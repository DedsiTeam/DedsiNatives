import { createBrowserRouter, Navigate } from 'react-router-dom';
import AdminLayout from '../layouts/AdminLayout';
import Dashboard from '../pages/dashboard';
import UserManagement from '../pages/system/users';
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
