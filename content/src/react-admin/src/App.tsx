import { ConfigProvider } from 'antd';
import zhCN from 'antd/locale/zh_CN';
import { RouterProvider } from 'react-router-dom';
import { router } from './router';

const readColorToken = (name: string) =>
  getComputedStyle(document.documentElement).getPropertyValue(name).trim();

export function App() {
  const colors = {
    primary: readColorToken('--color-primary'),
    primaryHover: readColorToken('--color-primary-hover'),
    primaryActive: readColorToken('--color-primary-active'),
    primaryLight: readColorToken('--color-primary-light'),
    primarySoft: readColorToken('--color-primary-soft'),
    title: readColorToken('--color-title'),
    text: readColorToken('--color-text'),
    muted: readColorToken('--color-muted'),
    placeholder: readColorToken('--color-placeholder'),
    disabled: readColorToken('--color-disabled'),
    page: readColorToken('--color-bg'),
    surface: readColorToken('--color-surface'),
    tableHeader: readColorToken('--color-table-header'),
    tableHover: readColorToken('--color-table-hover'),
    border: readColorToken('--color-border'),
    borderStrong: readColorToken('--color-border-strong'),
    success: readColorToken('--color-success'),
    warning: readColorToken('--color-warning'),
    error: readColorToken('--color-error'),
    info: readColorToken('--color-info'),
    focusRing: readColorToken('--color-focus-ring'),
    primaryShadow: readColorToken('--shadow-primary-sm'),
    modalShadow: readColorToken('--shadow-lg'),
  };

  return (
    <ConfigProvider
      locale={zhCN}
      theme={{
        token: {
          colorPrimary: colors.primary,
          colorPrimaryHover: colors.primaryHover,
          colorPrimaryActive: colors.primaryActive,
          colorPrimaryBg: colors.primaryLight,
          colorPrimaryBgHover: colors.primarySoft,
          colorLink: colors.primary,
          colorLinkHover: colors.primaryHover,
          colorLinkActive: colors.primaryActive,
          borderRadius: 8,
          borderRadiusLG: 12,
          fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
          colorBgBase: colors.surface,
          colorBgLayout: colors.page,
          colorBgContainer: colors.surface,
          colorBgElevated: colors.surface,
          colorText: colors.text,
          colorTextBase: colors.text,
          colorTextHeading: colors.title,
          colorTextSecondary: colors.muted,
          colorTextTertiary: colors.placeholder,
          colorTextQuaternary: colors.disabled,
          colorBorder: colors.borderStrong,
          colorBorderSecondary: colors.border,
          colorSuccess: colors.success,
          colorWarning: colors.warning,
          colorError: colors.error,
          colorInfo: colors.info,
          controlOutline: colors.focusRing,
        },
        components: {
          Menu: {
            itemBg: colors.surface,
            itemBorderRadius: 8,
            itemSelectedBg: colors.primaryLight,
            itemSelectedColor: colors.primary,
            itemHoverBg: colors.primarySoft,
            itemHoverColor: colors.primary,
            itemActiveBg: colors.primaryLight,
          },
          Table: {
            headerBg: colors.tableHeader,
            headerColor: colors.title,
            rowHoverBg: colors.tableHover,
            borderColor: colors.border,
          },
          Card: {
            borderRadiusLG: 12,
            headerBg: colors.surface,
            colorBorderSecondary: colors.border,
          },
          Button: {
            borderRadius: 8,
            primaryShadow: colors.primaryShadow,
            defaultColor: colors.text,
            defaultBorderColor: colors.borderStrong,
          },
          Input: {
            borderRadius: 8,
            activeBorderColor: colors.primary,
            hoverBorderColor: colors.primaryHover,
            activeShadow: `0 0 0 3px ${colors.focusRing}`,
          },
          Select: {
            activeBorderColor: colors.primary,
            hoverBorderColor: colors.primaryHover,
            activeOutlineColor: colors.focusRing,
          },
          Modal: {
            borderRadiusLG: 12,
            contentBg: colors.surface,
            headerBg: colors.surface,
            titleColor: colors.title,
            boxShadow: colors.modalShadow,
          },
          Form: {
            labelColor: colors.text,
          },
          Pagination: {
            itemActiveBg: colors.primaryLight,
          },
        },
      }}
    >
      <RouterProvider router={router} />
    </ConfigProvider>
  );
}

export default App;
