namespace DedsiNative.CodeGenerator;

/// <summary>
/// DedsiWork 导出协议；设计项 ID 仅用于引用，不作为业务实体主键。
/// </summary>
public sealed record DomainDocument(string Format, string FormatVersion, DomainModel Model);

/// <summary>
/// 一个包含单个聚合根的领域设计快照。
/// </summary>
public sealed record DomainModel(string Id, int Version, string Name, string Description, DomainElement[] Elements);

/// <summary>
/// 领域设计项；事件处理器通过 EventId 引用当前模型中的事件。
/// </summary>
public sealed record DomainElement(string Id, string Kind, string Name, string Description, DomainField[] Fields, string? EventId = null);

/// <summary>
/// 字段定义；DataType 可为标量、当前模型的值对象名称或 List&lt;实体名称&gt;。
/// </summary>
public sealed record DomainField(string Id, string Name, string DataType, string Description);
