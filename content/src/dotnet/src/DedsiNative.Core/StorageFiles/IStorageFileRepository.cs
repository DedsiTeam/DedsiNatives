using Dedsi.Ddd.Domain.Repositories;

namespace DedsiNative.StorageFiles;

/// <summary>
/// 文件与对象存储聚合仓储契约。
/// </summary>
public interface IStorageFileRepository : IDedsiCqrsRepository<StorageFile, string>
{
    /// <summary>
    /// 根据文件 MD5 哈希摘要查找已存在的有效文件记录（用于实现秒传及防重复存储）。
    /// </summary>
    /// <param name="md5Hash">
    /// 文件的 MD5 特征值。
    /// </param>
    /// <param name="cancellationToken">
    /// 异步操作取消令牌。
    /// </param>
    /// <returns>
    /// 匹配的文件聚合根，未找到返回 null。
    /// </returns>
    Task<StorageFile?> FindByMd5Async(string md5Hash, CancellationToken cancellationToken);
}
