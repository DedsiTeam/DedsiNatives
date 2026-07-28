# 功能完成清单

## Core

- [ ] 聚合根放入独立业务目录。
- [ ] 主键和构造方式与相邻聚合一致，默认使用 ULID 字符串。
- [ ] 属性使用私有设置器，修改通过领域方法完成。
- [ ] 业务输入使用 ABP `Check` 或明确的领域校验。
- [ ] 仓储接口继承项目使用的 Dedsi CQRS 仓储基接口。
- [ ] 查询接口继承 `IDedsiQuery`，不暴露 EF Core 类型。
- [ ] 领域事件由聚合注册，处理器不反向污染聚合。
- [ ] 所有公共 API 具有准确的中文 XML 文档。

## Infrastructure

- [ ] 创建独立 `IEntityTypeConfiguration<T>`。
- [ ] 配置表、Schema、主键、长度、必填性、审计字段和并发令牌。
- [ ] DbContext 接口与实现同步增加 DbSet。
- [ ] 仓储和查询实现放在约定目录。
- [ ] 外部服务实现放在 Infrastructure。
- [ ] 模型变化通过 `dotnet ef migrations add` 生成迁移。

## Host

- [ ] Endpoint 位于 `Endpoints/{Feature}Endpoints/`。
- [ ] 每个 Endpoint 独立文件；其 Request、Response 和 Endpoint 默认写在同一文件。
- [ ] 同一 Endpoint 的相关 DTO、Validator 和实现至少位于同一功能目录。
- [ ] 命令使用仓储，查询使用查询契约，未注入 DbContext。
- [ ] 路由、HTTP 方法、认证、响应和取消令牌处理完整。
- [ ] Request、Response、Endpoint 具有中文 XML 文档。

## 验证

- [ ] 搜索并移除未使用变量、占位实现和硬编码凭证。
- [ ] 检查筛选字段、排序字段和 DTO 映射是否一一对应。
- [ ] 从 `content/src/dotnet` 运行 `dotnet build`。
- [ ] 检查迁移只包含当前业务模型的预期变化。
- [ ] 说明未执行的数据库更新、集成测试或外部服务验证。
