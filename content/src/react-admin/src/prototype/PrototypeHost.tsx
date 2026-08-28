import { Result } from 'antd';
import { useParams } from 'react-router-dom';
import { prototypeRegistry } from './prototypeRegistry';
import { PrototypeShell } from './PrototypeShell';

function PrototypeHost() {
  const { workItemId = '' } = useParams();
  const normalizedWorkItemId = workItemId.trim().toUpperCase();
  const definition = prototypeRegistry[normalizedWorkItemId];

  if (!definition) {
    return (
      <PrototypeShell
        workItemId={normalizedWorkItemId || '未指定'}
        title="未找到已注册的静态原型"
        description="请检查工作项 ID，或在 prototypeRegistry.ts 中注册已进入评审的页面。"
      >
        <Result
          status="404"
          title="Prototype not registered"
          subTitle="原型页面必须位于最终生产目录，注册表只用于提供临时评审入口。"
        />
      </PrototypeShell>
    );
  }

  const PrototypePage = definition.component;

  return (
    <PrototypeShell
      workItemId={normalizedWorkItemId}
      title={definition.title}
      description={definition.description}
    >
      <PrototypePage />
    </PrototypeShell>
  );
}

export default PrototypeHost;
