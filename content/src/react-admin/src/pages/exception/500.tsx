import React from 'react';
import { Button, Result } from 'antd';
import { useNavigate } from 'react-router-dom';
import { HomeOutlined, ReloadOutlined } from '@ant-design/icons';

export const ServerErrorPage: React.FC = () => {
  const navigate = useNavigate();

  return (
    <div
      style={{
        height: '100%',
        minHeight: 480,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 32,
      }}
    >
      <Result
        status="500"
        title="500"
        subTitle="抱歉，服务器出现异常，请稍后重试或联系技术人员协助排查。"
        extra={[
          <Button
            type="primary"
            key="home"
            icon={<HomeOutlined />}
            onClick={() => navigate('/dashboard')}
          >
            返回仪表盘
          </Button>,
          <Button
            key="reload"
            icon={<ReloadOutlined />}
            onClick={() => window.location.reload()}
          >
            刷新重试
          </Button>,
        ]}
      />
    </div>
  );
};

export default ServerErrorPage;
