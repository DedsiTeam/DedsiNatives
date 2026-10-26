# DedsiNative 后端代码生成器

读取 DedsiWork 导出的 `.domain.json`，直接在后端 `src` 对应类库中新增领域模型、EF 配置、聚合仓储、查询及五类 Endpoint。首次写入自动创建模块文件夹。模板为可手工编辑的 `Templates/*.tpl`，不依赖 AI 或在线服务。

## 使用

先在 DedsiWork 完成领域设计并导出 `.domain.json`，再在 `content/src/dotnet` 执行。下列文件路径请替换为实际导出路径：

```powershell
dotnet run --project tools/DedsiNative.CodeGenerator -- validate "C:/Exports/Device.domain.json"
dotnet run --project tools/DedsiNative.CodeGenerator -- plan "C:/Exports/Device.domain.json" --module Devices
dotnet run --project tools/DedsiNative.CodeGenerator -- generate "C:/Exports/Device.domain.json" --module Devices
```

`plan` 不写文件；`generate` 先完整校验、渲染并检查冲突，再新增文件。已有文件一律报错，不提供覆盖选项。写入失败时删除本次新增文件，可能保留创建的空目录。返回码 0 表示成功，1 表示输入、模板或文件系统错误。

默认向上查找包含 `src/DedsiNative.Core` 的后端目录。也可显式使用 `--root C:\Githubs\DedsiNatives\content\src\dotnet`。模块目录默认是聚合根名称加 `s`，不做英文复数推断；推荐显式传入 `--module`。

上述示例生成：

```text
src/DedsiNative.Core/Devices/Device.cs
src/DedsiNative.Core/Devices/IDeviceRepository.cs
src/DedsiNative.Infrastructure/EntityFrameworkCore/Configurations/DeviceConfiguration.cs
src/DedsiNative.Infrastructure/EntityFrameworkCore/Repositories/DeviceRepository.cs
```

命名空间遵循类库的 `DedsiNative` 根命名空间和相对目录。新建文件自动被 SDK 项目包含。EF 配置由现有 DbContext 自动扫描，实体可以通过 `dbContext.Set<Device>()` 访问，无需为生成过程修改 DbSet。仓储沿用项目现有基类与接口约定。

## 支持范围与映射约定

- 支持 `format: dedsi-domain`、`formatVersion: 1.0`。模型、设计项、字段 ID 保留 UUID 语义，不成为业务主键。一个模型恰好有一个聚合根。
- 聚合根：根据显式 `Id` 生成 `DedsiAggregateRoot<string>` 或 `DedsiAggregateRoot<Guid>`、受保护 ORM 构造函数、公开创建构造函数、私有 setter、聚合仓储接口及实现。仓储泛型主键同步变化。
- 实体：普通类、与文档一致的主键、公开构造函数、私有 setter、独立 EF 配置，不生成独立仓储。
- 值对象：record，支持当前模型中的值对象引用（含可空和嵌套引用）；持久化引用采用 `OwnsOne` 表拆分，字段位于拥有者表，不生成独立表。生成无参构造供 EF 物化。
- 实体集合：`List<当前模型实体名>` 在持久化类中生成为 `ICollection<T>`，初始化为空集合，构造函数复制集合容器。一对多关系采用 `{父类型名}Id` 外键，默认 `Restrict` 删除并 `AutoInclude` 加载。未声明外键字段时使用强类型影子属性；已声明时必须与父主键类型一致且不可空。只做投影的查询可显式 `IgnoreAutoIncludes()`。
- 一个实体只能有一个持久化父集合；多重归属、循环引用、未知类型、可空集合、值对象内的实体集合会在写入前报错。不会把 `WarehouseId` 等普通标量字段自行推断为导航关系。
- 领域事件：不可变 record。事件处理器仅校验 `eventId` 并明确报告跳过，不生成或注册空处理器，也不会自动发布事件。
- 沿用 DedsiWork 的一个事件对应一个处理器校验。
- 领域事件中的实体集合生成数组并复制容器，元素仍是实体引用，不代表已经实现深拷贝业务快照。事件发布、快照与处理逻辑需在业务实现阶段完成。
- 生成创建、修改、删除、详情、条件分页 Endpoint，独立请求响应与结构校验器，Query 契约和实现，聚合 Update 方法。不推断文字业务约束，不生成索引、迁移或执行数据库更新。
- 名称要求为非保留英文 C# 标识符，不自动翻译或改名。模型展示名称与说明可使用中文。
- 支持 `string/bool/byte/short/int/long/float/double/decimal/Guid/DateTime/DateOnly/TimeOnly` 及这些类型的 `?` 可空形式。类型区分大小写；除上述明确的值对象/实体集合引用之外，不支持任意 C# 类型文本、枚举或直接实体导航。遇到不支持的字段整体失败，不静默丢字段。
- 遵循业务时间规范，导出 `DateTimeOffset` / `DateTimeOffset?` 按明确约定生成 `DateTime` / `DateTime?`，并逐字段提示。EF 映射为 `timestamp without time zone`。这是类型映射，不读取或转换任何运行时日期数据，也不修改原始导出文件；调用方仍须传入北京时间墙钟。
- 聚合根/实体未声明 `Id` 时默认字符串；显式声明支持 `Id:string` 或 `Id:Guid`，不可空，不重复生成。字符串主键直接赋值，不生成 ULID 格式校验；Guid 仍拒绝 Empty；主键由调用方提供并配置 `ValueGeneratedNever`。数据库字符串主键的 26 位长度约定保持不变。继承审计字段及生成成员冲突会被拒绝。
- EF 表名默认与设计项名称相同，不猜复数。字段可空性由 `?` 决定。没有长度和精度元数据时不猜测业务限制；生成后需补充业务要求。

生成的是新增模块骨架。完成业务规则、关联与索引配置后运行 `dotnet build DedsiNative.slnx`，有模型变化时使用 EF 工具生成并检查迁移。模型升级时可输出到独立后端副本比较，V1 不自动同步已有代码。

## 手动维护模板

默认模板在构建/发布时复制到工具输出目录。编辑源码 `Templates` 后重新 `dotnet run` 即可生效。要直接使用指定目录中的模板，可传 `--templates <目录>`，无需重编工具。该目录需包含本次生成所需的全部模板。

模板使用 `{{占位符}}`，只做一次文本替换，不执行脚本。未知占位符报错；模型说明作为 XML 注释转义，不会被当作模板再次解析。

注释格式：`summary` 保持多行，字段、属性说明换行；构造函数的 `param` 使用单行，并使用导出字段说明填充。多行字段说明用于参数注释时合并空白，避免生成空标签或多行参数标签。`ParameterComment.tpl` 可独立维护参数注释样式。

| 模板 | 用途 |
| --- | --- |
| Aggregate.tpl / Entity.tpl / ValueObject.tpl / DomainEvent.tpl | 聚合根、实体、值对象和事件整体结构 |
| Property.tpl / Assignment.tpl | 每个属性和构造赋值的重复片段 |
| Configuration.tpl / FieldConfiguration.tpl / AuditConfiguration.tpl | EF 配置与字段、审计片段 |
| OwnedConfiguration.tpl / CollectionConfiguration.tpl / ForeignKeyConfiguration.tpl | 值对象、一对多与外键映射 |
| GuidValidation.tpl / RecordOrmConstructor.tpl | Guid 主键校验和 record 的 EF 构造函数 |
| RepositoryInterface.tpl / Repository.tpl | 聚合仓储契约与实现 |

类型模板可用：`Namespace`、`Module`、`Name`、`Description`、`Properties`、`Parameters`、`ConstructorParameters`、`Assignments`、`FieldConfigurations`、`AuditConfiguration`、`KeyType`、`KeyLength`、`IdValidation`、`OrmConstructor`。

属性/赋值片段可用：`Name`、`Type`、`Description`、`Setter`、`Required`、`Value`、`Initializer`。EF 字段片段可用 `Builder`、`Name`、`Required`、`ColumnType`；owned 片段另有 `OwnedBuilder`、`Fields`；集合片段另有 `ForeignKey`；外键片段使用 `ForeignKey`、`KeyType`、`KeyLength`。审计和主键校验片段为固定文本。修改字段选择或支持的类型需要调整生成器代码；普通代码结构、注释、表名等可以直接调整模板。

## 验证

```powershell
dotnet test tests/DedsiNative.CodeGenerator.Tests/DedsiNative.CodeGenerator.Tests.csproj
powershell -NoProfile -File tools/DedsiNative.CodeGenerator/Verify-Generated.ps1
```

第二个命令在工具 `obj` 的独立目录生成样例，通过当前项目真实依赖编译并检查 EF 模型，不污染业务类库，不连接数据库。验证目录保留供检查，已被 Git 的 obj 忽略规则排除。

## 五类 Endpoint 的固定约定

不需要额外生成配置。目标根目录须包含 Core、Infrastructure、Endpoints 三个类库。
接口路径为聚合名的小驼峰，自动继承项目的 dedsinative-api 前缀。例如：

| HTTP | 相对路径 | 行为 |
| --- | --- | --- |
| POST | device/create | 服务端生成主键并创建完整聚合 |
| POST | device/update/{id} | 更新全部可写字段，整批替换明细 |
| POST | device/delete/{id} | 删除聚合及其所属明细 |
| GET | device/{id} | 返回含值对象及明细数组的详情 |
| POST | device/pagedQuery | 条件分页，返回聚合标量字段 |

创建和修改请求不包含实体主键；Guid 使用 Guid.NewGuid，字符串使用 ULID。
除主键外的普通业务字段由客户端提交，包括 Status、CreatedAt；不按字段名称猜测默认状态或时间。
修改是全量提交：可空字段传 null 会清空，集合传 [] 会删除全部旧明细；新明细生成新主键，不保留原明细身份。
仓储先显式标记原有明细删除，再替换集合，一次 SaveChanges 保存；删除也先处理所属明细。
外部引用继续受 Restrict 约束。数量、状态流转、外部仓库存在性等业务校验需要在领域中补充。

请求/响应嵌套对象与领域对象分离。请求校验器检查非空引用与嵌套结构，错误返回 400。
分页使用 PageNumber（从 1 开始）和 PageSize（1 至 200，默认 20），不提供无界导出。
筛选属性命名 Filter + 字段名，例如 FilterOrderNo、FilterWarehouseId、FilterStatus。
字符串先 Trim，空白忽略，再包含匹配；其他标量使用可空类型，非 null 时精确匹配。
暂不支持筛选 null、时间区间、明细字段及动态排序。按主键稳定排序，在数据库中计数、投影、分页，避免加载明细。

权限为 generated:{聚合名全小写}:{view|create|update|delete}。
模板使用端点内联授权策略检查 permission 声明，不依赖宿主的 ManagementPermissions.All 注册列表。
未登录返回 401，无对应权限返回 403。使用前在现有权限管理中创建这些权限并分配岗位，重新签发登录令牌。
不自动创建权限数据或授予任何岗位权限。

### 新增及拆分的模板

- CreateEndpoint / UpdateEndpoint / DeleteEndpoint / GetEndpoint / PagedEndpoint：五类接口及操作请求响应。
- EndpointContracts / Dto / DtoProperty / ToDomain / ToResponse：共用 DTO 与显式映射。
- RequestValidator / OperationValidator / RequiredRule / NestedRule / CollectionRule：请求结构校验。
- QueryInterface / Query / FilterString / FilterScalar：查询契约、EF 查询与筛选片段。
- EndpointAuthorization：权限规则。
- DomainUpdate / RemoveDetails：领域更新与旧明细删除。
- ConstructorParameter / NullGuard / CollectionCopy / ArrayCopy：参数排版、空值检查及集合复制。
- ValueObject / DomainEvent：分别维护值对象和领域事件；旧 Record.tpl 已不被生成流程使用。

验证包括真实生成、解决方案编译、HTTP TestServer 五接口流程与权限检查、
真实 Npgsql SQL 翻译、EF 变更跟踪中的明细删除与新增。HTTP 测试使用隔离仓储替身，
EF 测试不连接数据库；未执行迁移或真实 PostgreSQL 写入。

测试项目的 `Fixtures` 仅用于自动化回归测试，不是业务生成输入，也不随生成器构建或发布复制。`Verify-Generated.ps1` 使用该测试数据验证模板兼容性。

HTTP Request、Response（含嵌套契约、分页行）统一使用位置参数 record；参数每行一个，param 注释每条单行。创建和修改请求分别为独立的 sealed record，不生成共享 Request 基类；每类请求分别生成校验规则与映射重载。所有请求和响应均为 sealed record。保留默认分页值与现有 JSON 字段。Endpoint、校验器、仓储、EF 配置继续使用 class。

Query 的 Input、ItemDto、Output 使用 PositionalRecord.tpl 生成 sealed record 位置参数形式，每个参数一行。RecordParameterComment.tpl 为每个位置参数生成单行 XML param，summary 保持多行。Query 与分页 Endpoint 使用构造函数传参。

创建、修改操作的 Validator 与对应 Request、Endpoint 同文件生成；嵌套对象校验器作为对应操作 Validator 的私有嵌套 AbstractValidator 生成，不参与 Endpoint 请求校验器自动发现；Contracts 文件只保留数据契约及映射。
