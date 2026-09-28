# 前端静态原型晋级流程

本流程用于产品经理根据 Azure DevOps 工作项，在 Codex 中使用 `prototype` Agent 绘制可直接晋级为正式实现的 React 静态页面。`docs/product/` 可保留需求编写草稿和发布快照；发布后的工作项、审批状态和执行记录以 Azure DevOps 为远程事实来源。

## 责任边界

- 产品经理负责原型范围、静态交互、评审和批准。
- Prototype Agent 只在最终前端目录绘制静态页面，不调用后端，不使用 Mock 数据，不操作 Azure DevOps 或 Git。
- Frontend Agent 在原型批准后继续使用原页面，补全 DTO、API Service、鉴权、数据状态、路由和菜单。
- 原型批准后，由用户在 Codex 中发起正式实现和验收。

## 一次工作项流程

1. 上游完善工作项的业务目标、页面字段、操作、权限表现和验收标准。
2. 标明原型已准备好供评审。
3. 产品经理在 Codex 中使用 `prototype` Agent，把页面、组件和 CSS Module 写入最终生产目录。
4. 在 `src/react-admin/src/prototype/prototypeRegistry.ts` 使用 `WI-<id>` 登记页面。
5. 运行 `bun run dev:prototype`，通过 `/__prototype/WI-<id>` 评审。
6. 原型 PR 通过 `bun run build:prototype`；当前项目的 lint 工具链可用时同时运行 `bun run lint`，然后进入 `prototype-review`。
7. 产品评审通过后标记 `prototype-approved`，合并原型 PR。
8. 编码工作项回填页面路径、Prototype PR、Approved Commit、必须保留的交互和允许调整范围。
9. 接口契约和 Ready 检查全部满足后，由用户在 Codex 中发起正式实现。
10. `frontend` Agent 在原页面上接入真实数据，完成后删除该工作项的原型登记项，再启用正式路由、菜单和权限。

## 静态原型约束

- 不创建 `mocks/`、fixture、Mock Server 或 Mock Adapter。
- 不调用 API Service、`fetch` 或 `axios`。
- 列表显示空状态；表单显示空值、占位符和已确认枚举，不编造用户、订单、金额等业务记录。
- 可以使用本地 UI 状态演示弹窗、抽屉、页签和表单展开/关闭，但不模拟提交成功或服务端错误。
- 产品构建只用于评审；正式构建中 `VITE_PROTOTYPE_MODE=false`，不注册 `/__prototype/*`。

## 运行

```bash
cd src/react-admin
bun run dev:prototype
```

打开：

```text
http://localhost:11026/__prototype/WI-<id>
```

验证：

```bash
bun run build:prototype
bun run build
```

当前项目的 lint 工具链可用时再补充运行 `bun run lint`。
