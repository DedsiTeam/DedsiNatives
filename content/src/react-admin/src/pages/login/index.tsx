import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Form, Input, Button, Checkbox, message, Typography } from 'antd';
import { UserOutlined, LockOutlined, ArrowRightOutlined, SafetyCertificateOutlined } from '@ant-design/icons';
import { AuthApiService } from '../../apiServices';
import { SsoAuthService } from '../../auth/authService';
import styles from './Login.module.css';

const { Link } = Typography;

// 自动动态扫描 assets/login-bg 下的所有背景图（支持 jpeg/jpg/png/webp）
const bgModules = import.meta.glob<{ default: string }>(
  '../../assets/login-bg/*.{jpeg,jpg,png,webp}',
  { eager: true }
);

const ALL_BG_IMAGES = Object.values(bgModules).map((mod) => mod.default);

/**
 * Fisher-Yates 随机打乱数组算法
 */
function shuffleImages<T>(array: T[]): T[] {
  const result = [...array];
  for (let i = result.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [result[i], result[j]] = [result[j], result[i]];
  }
  return result;
}

export const LoginPage: React.FC = () => {
  const [loading, setLoading] = useState(false);
  // 每次进入页面随机乱序图片列表，确保第一张和后续轮播顺序均随机
  const [bgImages] = useState<string[]>(() => shuffleImages(ALL_BG_IMAGES));
  const [currentBgIndex, setCurrentBgIndex] = useState(0);
  const navigate = useNavigate();
  const [form] = Form.useForm();

  // 背景图片自动平滑轮播切换
  useEffect(() => {
    if (bgImages.length <= 1) return;
    const timer = setInterval(() => {
      setCurrentBgIndex((prev) => (prev + 1) % bgImages.length);
    }, 6000);
    return () => clearInterval(timer);
  }, [bgImages.length]);

  // 提交登录 API 请求
  const onFinish = async (values: { username: string; password: string; remember?: boolean }) => {
    setLoading(true);
    message.loading({ content: '正在验证身份...', key: 'login' });

    try {
      const res = await AuthApiService.login({
        username: values.username,
        password: values.password,
      });

      if (res && res.token) {
        localStorage.setItem('access_token', res.token);
        localStorage.setItem('current_user', JSON.stringify(res.user));
        message.success({ content: `欢迎回来，${res.user.name}！`, key: 'login' });
        navigate('/dashboard');
      }
    } catch {
      // 捕获异常已由全局 Axios 拦截器处理
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className={styles.loginContainer}>
      {/* 动态全屏背景图随机轮播与缓动效果 */}
      <div className={styles.bgWrapper}>
        {bgImages.map((img, index) => (
          <div
            key={img}
            className={`${styles.bgSlide} ${index === currentBgIndex ? styles.bgSlideActive : ''}`}
            style={{ backgroundImage: `url(${img})` }}
          />
        ))}
        {/* 背景遮罩层，提升卡片对比度与高端质感 */}
        <div className={styles.bgOverlay} />
      </div>

      {/* 上下左右居中核心登录卡片 */}
      <div className={styles.loginCard}>
        {/* 顶部 Logo 与系统名称 */}
        <div className={styles.cardHeader}>
          <div className={styles.logoBadge}>D</div>
          <h1 className={styles.brandName}>Dedsi Admin</h1>
          <p className={styles.subTitle}>统一身份与访问控制管理中台</p>
        </div>

        {/* 表单 */}
        <Form
          form={form}
          name="login_form"
          initialValues={{ remember: true }}
          onFinish={onFinish}
          layout="vertical"
          size="large"
        >
          <Form.Item
            name="username"
            label={<span style={{ fontWeight: 600, color: 'var(--color-body)', fontSize: 13 }}>账号名</span>}
            rules={[{ required: true, message: '请输入您的账号名' }]}
          >
            <Input
              prefix={<UserOutlined style={{ color: 'var(--color-placeholder)', marginRight: 4 }} />}
              placeholder="请输入登录账号"
              style={{ borderRadius: 8 }}
            />
          </Form.Item>

          <Form.Item
            name="password"
            label={<span style={{ fontWeight: 600, color: 'var(--color-body)', fontSize: 13 }}>登录密码</span>}
            rules={[{ required: true, message: '请输入您的登录密码' }]}
          >
            <Input.Password
              prefix={<LockOutlined style={{ color: 'var(--color-placeholder)', marginRight: 4 }} />}
              placeholder="请输入登录密码"
              style={{ borderRadius: 8 }}
            />
          </Form.Item>

          <div className={styles.extraOptions}>
            <Form.Item name="remember" valuePropName="checked" noStyle>
              <Checkbox style={{ color: 'var(--color-muted)', fontSize: 13 }}>记住登录状态</Checkbox>
            </Form.Item>
            <Link style={{ color: 'var(--color-primary)', fontSize: 13 }} onClick={() => message.info('请联系系统管理员重置密码')}>
              忘记密码？
            </Link>
          </div>

          <Form.Item style={{ marginBottom: 12 }}>
            <Button
              type="primary"
              htmlType="submit"
              loading={loading}
              className={styles.submitBtn}
              icon={!loading && <ArrowRightOutlined />}
            >
              立即登录
            </Button>
          </Form.Item>

          <div style={{ display: 'flex', alignItems: 'center', margin: '16px 0', gap: 12 }}>
            <div style={{ flex: 1, height: 1, background: 'var(--color-border)' }} />
            <span style={{ fontSize: 12, color: 'var(--color-muted)' }}>或使用统一身份认证</span>
            <div style={{ flex: 1, height: 1, background: 'var(--color-border)' }} />
          </div>

          <Button
            block
            size="large"
            icon={<SafetyCertificateOutlined style={{ color: 'var(--color-primary)' }} />}
            onClick={() => {
              message.loading({ content: '正在跳转至 SSO 统一认证中心...', key: 'sso' });
              SsoAuthService.loginViaSso().catch((err) => {
                message.error({ content: err?.message || '发起 SSO 登录失败', key: 'sso' });
              });
            }}
            style={{
              borderRadius: 8,
              height: 42,
              border: '1px solid var(--color-border)',
              fontWeight: 600,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: 6,
            }}
          >
            DedsiNative SSO 单点登录
          </Button>
        </Form>

        {/* 页脚版权信息 */}
        <footer className={styles.footer}>
          © {new Date().getFullYear()} Dedsi Team. All Rights Reserved.
        </footer>
      </div>
    </div>
  );
};

export default LoginPage;
