import { Component, type ErrorInfo, type ReactNode } from 'react';
import { Button, Result, Collapse } from 'antd';
import { ReloadOutlined, BugOutlined } from '@ant-design/icons';

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
  errorInfo: ErrorInfo | null;
}

/**
 * 全局 React 渲染错误边界组件，捕获组件树渲染异常并展示友好的错误提示页面，防止全站白屏。
 */
export class ErrorBoundary extends Component<Props, State> {
  public state: State = {
    hasError: false,
    error: null,
    errorInfo: null,
  };

  public static getDerivedStateFromError(error: Error): Partial<State> {
    return { hasError: true, error };
  }

  public componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('ErrorBoundary 捕获到未处理的渲染异常:', error, errorInfo);
    this.setState({ errorInfo });
  }

  private handleReload = () => {
    window.location.reload();
  };

  private handleReset = () => {
    this.setState({ hasError: false, error: null, errorInfo: null });
  };

  public render() {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback;
      }

      return (
        <div
          style={{
            padding: 32,
            minHeight: 400,
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
          }}
        >
          <Result
            status="error"
            title="页面渲染发生异常"
            subTitle="当前模块在加载或渲染时遇到了未处理的错误，我们已记录此异常。"
            extra={[
              <Button
                type="primary"
                key="reload"
                icon={<ReloadOutlined />}
                onClick={this.handleReload}
              >
                刷新页面
              </Button>,
              <Button key="retry" onClick={this.handleReset}>
                重试加载
              </Button>,
            ]}
          >
            {this.state.error && (
              <Collapse
                ghost
                items={[
                  {
                    key: 'details',
                    label: (
                      <span style={{ color: 'var(--color-neutral-gray)', fontSize: 13 }}>
                        <BugOutlined style={{ marginRight: 6 }} />
                        查看错误详情 (Developer Details)
                      </span>
                    ),
                    children: (
                      <pre
                        style={{
                          background: 'var(--color-bg)',
                          border: '1px solid var(--color-border)',
                          borderRadius: 'var(--radius-md)',
                          padding: 16,
                          fontSize: 12,
                          color: 'var(--color-error)',
                          overflowX: 'auto',
                          maxHeight: 240,
                          lineHeight: 1.5,
                        }}
                      >
                        {this.state.error.toString()}
                        {'\n\n'}
                        {this.state.errorInfo?.componentStack}
                      </pre>
                    ),
                  },
                ]}
              />
            )}
          </Result>
        </div>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;
