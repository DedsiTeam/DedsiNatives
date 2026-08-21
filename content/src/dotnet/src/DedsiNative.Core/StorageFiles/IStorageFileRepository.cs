using Dedsi.Ddd.Domain.Repositories;

namespace DedsiNative.StorageFiles;

/// <summary>
/// 文件与对象存储聚合仓储契约。
/// </summary>
public interface IStorageFileRepository : IDedsiCqrsRepository<StorageFile, string>;
