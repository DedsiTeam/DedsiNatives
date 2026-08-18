import { ConfigProvider } from 'antd';
import zhCN from 'antd/locale/zh_CN';
import { RouterProvider } from 'react-router-dom';
import { router } from './router';

export function App() {
  return (
    <ConfigProvider
      locale={zhCN}
      theme={{
        token: {
          // 主色调: 深黑极简体系 (Primary: #090A0F)
          colorPrimary: '#090A0F',         // 主色: 深黑
          colorPrimaryHover: '#1E293B',    // 深灰悬停
          colorPrimaryActive: '#000000',   // 纯黑激活
          colorPrimaryBg: '#F1F5F9',       // 浅灰背景
          colorPrimaryBgHover: '#E2E8F0',  // 浅灰悬停
          colorLink: '#090A0F',
          colorLinkHover: '#1E293B',
          colorLinkActive: '#000000',

          // 主色与中性色（黑白灰体系）
          colorBgBase: '#FFFFFF',          // 纯白 (Pure White)
          colorBgLayout: '#F8FAFC',        // 页面底层
          colorBgContainer: '#FFFFFF',     // 卡片/容器背景
          colorBgElevated: '#FFFFFF',      // 下拉框/弹窗
          colorBgSpotlight: '#090A0F',     // 深黑

          // 文本层级
          colorText: '#1E293B',            // 深灰 (Slate Gray): 正文文字
          colorTextBase: '#1E293B',
          colorTextHeading: '#090A0F',     // 深黑 (Deep Black): 主标题
          colorTextSecondary: '#64748B',   // 中灰 (Neutral Gray): 辅助说明
          colorTextTertiary: '#64748B',    // 占位符
          colorTextQuaternary: '#CBD5E1',  // 禁用态

          // 边框与描边
          colorBorder: '#E2E8F0',          // 浅灰 (Cool Gray): 分割线与描边
          colorBorderSecondary: '#F1F5F9',

          // 状态色
          colorSuccess: '#16A34A',
          colorSuccessBg: '#F0FDF4',
          colorWarning: '#D97706',
          colorWarningBg: '#FFFBEB',
          colorError: '#DC2626',
          colorErrorBg: '#FEF2F2',
          colorInfo: '#0284C7',
          colorInfoBg: '#F0F9FF',

          // 圆角与排版规范
          borderRadius: 6,
          borderRadiusSM: 4,
          borderRadiusLG: 8,
          borderRadiusXS: 2,
          fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
          fontSize: 13,

          // 光晕与焦点外圈
          controlOutline: 'rgba(9, 10, 15, 0.12)',
        },
        components: {
          Menu: {
            itemBg: '#FFFFFF',
            itemColor: '#64748B',
            itemHoverColor: '#090A0F',
            itemHoverBg: '#F8FAFC',
            itemSelectedBg: '#F1F5F9',
            itemSelectedColor: '#090A0F',
            itemBorderRadius: 6,
            itemMarginInline: 8,
            subMenuItemBg: '#FAFAFC',
          },
          Table: {
            headerBg: '#F8FAFC',
            headerColor: '#090A0F',
            rowHoverBg: '#F8FAFC',
            borderColor: '#E2E8F0',
            colorBgContainer: '#FFFFFF',
          },
          Card: {
            borderRadiusLG: 8,
            headerBg: '#FFFFFF',
            colorBgContainer: '#FFFFFF',
            colorBorderSecondary: '#E2E8F0',
          },
          Button: {
            borderRadius: 6,
            defaultColor: '#1E293B',
            defaultBg: '#FFFFFF',
            defaultBorderColor: '#E2E8F0',
            primaryShadow: '0 2px 8px rgba(9, 10, 15, 0.2)',
          },
          Input: {
            borderRadius: 6,
            colorBgContainer: '#FFFFFF',
            colorBorder: '#E2E8F0',
            hoverBorderColor: '#090A0F',
            activeBorderColor: '#090A0F',
            activeShadow: '0 0 0 2px rgba(9, 10, 15, 0.12)',
          },
          Select: {
            borderRadius: 6,
            colorBgContainer: '#FFFFFF',
            colorBorder: '#E2E8F0',
            hoverBorderColor: '#090A0F',
            activeBorderColor: '#090A0F',
            activeOutlineColor: 'rgba(9, 10, 15, 0.12)',
          },
          Modal: {
            borderRadiusLG: 12,
            contentBg: '#FFFFFF',
            headerBg: '#FFFFFF',
            titleColor: '#090A0F',
            boxShadow: '0 12px 32px -4px rgba(9, 10, 15, 0.12), 0 0 0 1px rgba(9, 10, 15, 0.05)',
          },
          Dropdown: {
            colorBgElevated: '#FFFFFF',
            boxShadowSecondary: '0 10px 30px -4px rgba(9, 10, 15, 0.1), 0 0 0 1px #E2E8F0',
          },
          Form: {
            labelColor: '#1E293B',
          },
          Pagination: {
            itemActiveBg: '#F1F5F9',
          },
          Tag: {
            borderRadiusSM: 4,
          },
        },
      }}
    >
      <RouterProvider router={router} />
    </ConfigProvider>
  );
}

export default App;
