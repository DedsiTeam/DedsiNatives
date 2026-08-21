/**
 * @file OIDC 登录回调处理页面 (CallbackPage)
 * @description 处理授权码兑换 Token，并将认证信息同步至本地存储后重定向到主页。
 */

import React, { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Spin, Result, Button, Typography } from 'antd';
import { SafetyCertificateOutlined } from '@ant-design/icons';
import { SsoAuthService } from '../../auth/authService';

const { Paragraph } = Typography;

export const CallbackPage: React.FC = () => {
  const navigate = useNavigate();
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const executedRef = useRef(false);

  useEffect(() => {
    if (executedRef.current) return;
    executedRef.current = true;

    SsoAuthService.handleCallback()
      .then(() => {
        // 清除 URL 中的 code/state 查询参数，防止刷新重复触发
        window.history.replaceState({}, document.title, window.location.pathname);
        navigate('/dashboard', { replace: true });
      })
      .catch((err) => {
        // 如果本地已成功持有 Token，直接放行
        if (localStorage.getItem('access_token')) {
          window.history.replaceState({}, document.title, window.location.pathname);
          navigate('/dashboard', { replace: true });
          return;
        }
        setErrorMsg(err?.message || 'SSO 单点登录认证失败，请重试。');
      });
  }, [navigate]);

  if (errorMsg) {
    return (
      <div
        style={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          minHeight: '100vh',
          background: 'var(--color-bg-layout)',
          padding: 'var(--space-24)',
        }}
      >
        <Result
          status="error"
          title="SSO 认证失败"
          subTitle={errorMsg}
          extra={[
            <Button
              type="primary"
              key="login"
              onClick={() => navigate('/login', { replace: true })}
              style={{ borderRadius: 'var(--radius-btn)' }}
            >
              返回登录页
            </Button>,
          ]}
        >
          <Paragraph type="secondary" style={{ textAlign: 'center', maxWidth: 480 }}>
            请确认您的浏览器允许与 SSO 认证中心进行跨域 Cookie/Token 通信，并且当前客户端应用已正确配置 Redirect URI。
          </Paragraph>
        </Result>
      </div>
    );
  }

  return (
    <div
      style={{
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'center',
        alignItems: 'center',
        minHeight: '100vh',
        background: 'var(--color-bg-layout)',
        gap: 'var(--space-16)',
      }}
    >
      <Spin size="large" />
      <div style={{ display: 'flex', alignItems: 'center', gap: 8, color: 'var(--color-primary)', fontWeight: 600 }}>
        <SafetyCertificateOutlined style={{ fontSize: 18 }} />
        <span>正在完成 SSO 统一安全认证，请稍候...</span>
      </div>
    </div>
  );
};

export default CallbackPage;
