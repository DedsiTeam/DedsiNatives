import React from 'react';
import { Button, Result } from 'antd';
import { useNavigate } from 'react-router-dom';
import { HomeOutlined, RollbackOutlined } from '@ant-design/icons';

export const ForbiddenPage: React.FC = () => {
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
        status="403"
        title="403"
        subTitle="抱歉，您暂无权限访问当前页面。如需访问，请联系管理员为您分配对应权限。"
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
            key="back"
            icon={<RollbackOutlined />}
            onClick={() => navigate(-1)}
          >
            返回上一页
          </Button>,
        ]}
      />
    </div>
  );
};

export default ForbiddenPage;
