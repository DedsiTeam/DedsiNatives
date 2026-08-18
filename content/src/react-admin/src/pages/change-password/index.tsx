import { useState } from 'react';
import { Button, Card, Form, Input, Typography, message } from 'antd';
import { useNavigate } from 'react-router-dom';
import styles from './index.module.css';
import { ProfileApiService, type ChangePasswordInputDto } from '../../apiServices';

const { Paragraph, Title } = Typography;

/** 修改密码静态页面，仅展示未来接入接口前的表单交互形态。 */
export default function ChangePasswordPage() {
  const [form] = Form.useForm<ChangePasswordInputDto>();
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const submit = async (values: ChangePasswordInputDto) => {
    setLoading(true);
    try {
      await ProfileApiService.changePassword(values);
      form.resetFields();
      localStorage.removeItem('access_token');
      localStorage.removeItem('current_user');
      message.success('密码修改成功，请使用新密码重新登录');
      navigate('/login');
    } finally {
      setLoading(false);
    }
  };
  return (
    <main className={styles.page}>
      <Card className={styles.card}>
        <div className={styles.content}>
          <div className={styles.heading}><Title level={3}>修改密码</Title><Paragraph>请设置安全的新密码，接口接入后才能提交修改。</Paragraph></div>
          <Form form={form} layout="vertical" className={styles.form} onFinish={submit}>
            <Form.Item name="currentPassword" label="当前密码" rules={[{ required: true }]}><Input.Password placeholder="请输入当前密码" /></Form.Item>
            <Form.Item name="newPassword" label="新密码" rules={[{ required: true }]}><Input.Password placeholder="请输入新密码" /></Form.Item>
            <Form.Item name="confirmPassword" label="确认新密码" dependencies={['newPassword']} rules={[{ required: true }, ({ getFieldValue }) => ({ validator(_, value) { return !value || getFieldValue('newPassword') === value ? Promise.resolve() : Promise.reject(new Error('两次输入的密码不一致')); } })]}><Input.Password placeholder="请再次输入新密码" /></Form.Item>
            <Button type="primary" htmlType="submit" loading={loading}>确认修改</Button>
          </Form>
        </div>
      </Card>
    </main>
  );
}
