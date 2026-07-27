import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Form, Input, Button, Checkbox, message, Typography } from 'antd';
import { UserOutlined, LockOutlined, ArrowRightOutlined } from '@ant-design/icons';
import { AuthApiService } from '../../apiServices';
import styles from './Login.module.css';

const { Link } = Typography;

export const LoginPage: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const [form] = Form.useForm();

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
        message.success({ content: `欢迎回来，${values.username}！`, key: 'login' });
        navigate('/dashboard');
      }
    } catch {
      // 捕获异常已由全局 Axios 拦截器处理
    } finally {
      setLoading(false);
    }
  };

  // 快捷填入测试演示数据 (对应后端硬编码凭证: admin / Admin@123)
  const fillDemoAccount = () => {
    form.setFieldsValue({
      username: 'admin',
      password: 'Admin@123',
      remember: true,
    });
    message.info('已自动填充测试管理员账号 (admin / Admin@123)');
  };

  return (
    <div className={styles.loginContainer}>
      {/* 动态科技背景网格与微流体动画 */}
      <div className={styles.gridPattern} />
      <div className={styles.orb1} />
      <div className={styles.orb2} />

      {/* 居中核心登录卡片 */}
      <div className={styles.loginCard}>
        {/* 顶部 Logo 与系统名称 */}
        <div className={styles.cardHeader}>
          <div className={styles.logoBadge}>D</div>
          <h1 className={styles.brandName}>Dedsi Admin</h1>
          <p className={styles.subTitle}>请输入您的系统账号与密码以登录</p>
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
            label={<span style={{ fontWeight: 600, color: '#374151', fontSize: 13 }}>账号名</span>}
            rules={[{ required: true, message: '请输入您的账号名' }]}
          >
            <Input
              prefix={<UserOutlined style={{ color: '#9ca3af', marginRight: 4 }} />}
              placeholder="例如: admin"
              style={{ borderRadius: 8 }}
            />
          </Form.Item>

          <Form.Item
            name="password"
            label={<span style={{ fontWeight: 600, color: '#374151', fontSize: 13 }}>登录密码</span>}
            rules={[{ required: true, message: '请输入您的登录密码' }]}
          >
            <Input.Password
              prefix={<LockOutlined style={{ color: '#9ca3af', marginRight: 4 }} />}
              placeholder="••••••••"
              style={{ borderRadius: 8 }}
            />
          </Form.Item>

          <div className={styles.extraOptions}>
            <Form.Item name="remember" valuePropName="checked" noStyle>
              <Checkbox style={{ color: '#6b7280', fontSize: 13 }}>记住登录状态</Checkbox>
            </Form.Item>
            <Link style={{ color: '#315efb', fontSize: 13 }} onClick={() => message.info('请联系系统管理员重置密码')}>
              忘记密码？
            </Link>
          </div>

          <Form.Item style={{ marginBottom: 16 }}>
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

          <div style={{ textAlign: 'center', marginTop: 8 }}>
            <Button type="link" size="small" onClick={fillDemoAccount} style={{ color: '#8b31fb', fontSize: 13 }}>
              一键填入演示账号
            </Button>
          </div>
        </Form>
      </div>

      {/* 页脚版权信息 */}
      <footer className={styles.footer}>
        © {new Date().getFullYear()} Dedsi Team. All Rights Reserved.
      </footer>
    </div>
  );
};

export default LoginPage;
