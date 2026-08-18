using Dedsi.Ddd.Domain.Repositories;

namespace DedsiNative.Systems;

/// <summary>系统聚合仓储，提供系统聚合根的持久化操作。</summary>
public interface ISystemRepository : IDedsiCqrsRepository<System, string>;
