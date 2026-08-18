import { useEffect, useState } from 'react';
import { Alert, Avatar, Card, Descriptions, Spin, Tag, Typography } from 'antd';
import { MailOutlined, UserOutlined } from '@ant-design/icons';
import styles from './index.module.css';
import { ProfileApiService, type ProfileResultDto } from '../../apiServices';

const { Paragraph, Title } = Typography;

/** 个人中心静态页面，展示当前登录账户的资料样式。 */
export default function ProfilePage() {
  const [profile, setProfile] = useState<ProfileResultDto>();
  const [error, setError] = useState(false);
  useEffect(() => { ProfileApiService.get().then(setProfile).catch(() => setError(true)); }, []);
  if (!profile && !error) return <main className={styles.page}><Spin /></main>;
  if (!profile) return <main className={styles.page}><Alert type="error" message="个人资料加载失败" /></main>;
  return (
    <main className={styles.page}>
      <Card className={styles.profileCard}>
        <div className={styles.banner}>
          <Avatar size={64} icon={<UserOutlined />} className={styles.avatar}>{profile.name.charAt(0)}</Avatar>
          <div>
            <Title level={3} className={styles.name}>{profile.name}</Title><Paragraph className={styles.email}><MailOutlined /> {profile.email}</Paragraph>
          </div>
          <Tag color="success">当前账户</Tag>
        </div>
      </Card>
      <Card title="基本资料" className={styles.card}>
        <Descriptions bordered column={{ xs: 1, sm: 2 }} size="small">
          <Descriptions.Item label="姓名">{profile.name}</Descriptions.Item><Descriptions.Item label="登录账号">{profile.account}</Descriptions.Item><Descriptions.Item label="邮箱">{profile.email}</Descriptions.Item>
          <Descriptions.Item label="账户状态"><Tag color="success">正常</Tag></Descriptions.Item>
        </Descriptions>
      </Card>
    </main>
  );
}
