import type { ComponentType } from 'react';

export interface PrototypeDefinition {
  title: string;
  description?: string;
  component: ComponentType;
}

/**
 * 已进入评审的静态原型注册表。
 *
 * 原型页面必须位于最终生产目录，例如
 * `src/pages/system/users/index.tsx`；这里只保存评审期间的临时路由关系。
 * 正式研发完成时删除对应注册项，但保留并继续完善页面组件。
 */
export const prototypeRegistry: Readonly<Record<string, PrototypeDefinition>> = {};
