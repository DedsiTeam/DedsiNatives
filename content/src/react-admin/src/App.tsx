import { ConfigProvider } from 'antd';
import { RouterProvider } from 'react-router-dom';
import { router } from './router';
import zhCN from 'antd/locale/zh_CN';

export function App() {
  return (
    <ConfigProvider
      locale={zhCN}
      theme={{
        token: {
          colorPrimary: '#315efb',
          colorLink: '#315efb',
          borderRadius: 8,
          borderRadiusLG: 12,
          fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
          colorBgLayout: '#f7f9fc',
          colorTextBase: '#374151',
          colorTextHeading: '#111827',
        },
        components: {
          Menu: {
            itemBorderRadius: 8,
            itemSelectedBg: 'rgba(49, 94, 251, 0.08)',
            itemSelectedColor: '#315efb',
            itemHoverBg: 'rgba(49, 94, 251, 0.05)',
            itemHoverColor: '#315efb',
          },
          Card: {
            borderRadiusLG: 12,
          },
          Button: {
            borderRadius: 8,
          },
          Input: {
            borderRadius: 8,
          },
        },
      }}
    >
      <RouterProvider router={router} />
    </ConfigProvider>
  );
}

export default App;
