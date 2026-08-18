using Dedsi.Ddd.Domain.Repositories;

namespace DedsiNative.Dictionaries;

/// <summary>
/// 字典聚合仓储，负责字典分组及其字典项的写侧持久化。
/// </summary>
public interface IDictionaryRepository : IDedsiCqrsRepository<Dictionary, string>;
