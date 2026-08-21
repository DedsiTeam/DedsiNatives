namespace DedsiNative.StorageFiles;

/// <summary>
/// 底层物理文件与对象存储提供者契约。
/// </summary>
public interface IStorageProvider
{
    /// <summary>
    /// 获取当前存储提供者的介质类型。
    /// </summary>
    StorageType ProviderType { get; }

    /// <summary>
    /// 将文件流持久化保存至目标相对路径。
    /// </summary>
    /// <param name="stream">
    /// 待写入的文件流。
    /// </param>
    /// <param name="relativePath">
    /// 目标存储相对路径（如 uploads/2026/08/xxx.png）。
    /// </param>
    /// <param name="contentType">
    /// 文件的 MIME 内容类型。
    /// </param>
    /// <param name="cancellationToken">
    /// 异步操作取消令牌。
    /// </param>
    /// <returns>
    /// 成功保存后的相对路径。
    /// </returns>
    Task<string> SaveAsync(
        Stream stream,
        string relativePath,
        string contentType,
        CancellationToken cancellationToken);

    /// <summary>
    /// 打开指定相对路径的只读文件流。
    /// </summary>
    /// <param name="relativePath">
    /// 物理存储相对路径。
    /// </param>
    /// <param name="cancellationToken">
    /// 异步操作取消令牌。
    /// </param>
    /// <returns>
    /// 可读的文件数据流，若文件不存在返回 null。
    /// </returns>
    Task<Stream?> OpenReadStreamAsync(
        string relativePath,
        CancellationToken cancellationToken);

    /// <summary>
    /// 删除指定物理存储相对路径下的文件。
    /// </summary>
    /// <param name="relativePath">
    /// 存储相对路径。
    /// </param>
    /// <param name="cancellationToken">
    /// 异步操作取消令牌。
    /// </param>
    /// <returns>
    /// 删除成功返回 true，文件不存在返回 false。
    /// </returns>
    Task<bool> DeleteAsync(
        string relativePath,
        CancellationToken cancellationToken);

    /// <summary>
    /// 检查指定相对路径的文件在物理介质中是否存在。
    /// </summary>
    /// <param name="relativePath">
    /// 存储相对路径。
    /// </param>
    /// <param name="cancellationToken">
    /// 异步操作取消令牌。
    /// </param>
    /// <returns>
    /// 若存在返回 true，否则返回 false。
    /// </returns>
    Task<bool> ExistsAsync(
        string relativePath,
        CancellationToken cancellationToken);
}
