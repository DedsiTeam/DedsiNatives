using DedsiNative.StorageFiles;

namespace DedsiNative.Infrastructure.StorageFiles;

/// <summary>
/// 空对象存储提供者实现（占位桩）。
/// 用于在未配置具体云存储（如 MinIO、阿里云 OSS、腾讯云 COS）驱动时作为空实现。
/// </summary>
public sealed class NullStorageProvider : IStorageProvider
{
    /// <inheritdoc />
    public StorageType ProviderType => StorageType.Local;

    /// <inheritdoc />
    public Task<string> SaveAsync(
        Stream stream,
        string relativePath,
        string contentType,
        CancellationToken cancellationToken)
    {
        // 存储方式暂空，仅返回相对路径
        return Task.FromResult(relativePath);
    }

    /// <inheritdoc />
    public Task<Stream?> OpenReadStreamAsync(
        string relativePath,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<Stream?>(null);
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(
        string relativePath,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(
        string relativePath,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }
}
