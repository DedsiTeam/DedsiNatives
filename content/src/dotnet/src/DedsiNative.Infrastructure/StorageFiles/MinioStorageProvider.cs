using DedsiNative.StorageFiles;
using Microsoft.Extensions.Logging;
using Volo.Abp.BlobStoring;

namespace DedsiNative.Infrastructure.StorageFiles;

/// <summary>
/// 基于 ABP Blob Storing 框架的 MinIO 对象存储提供者实现。
/// </summary>
public sealed class MinioStorageProvider(
    IBlobContainer blobContainer,
    ILogger<MinioStorageProvider> logger) : IStorageProvider
{
    /// <inheritdoc />
    public StorageType ProviderType => StorageType.Minio;

    /// <inheritdoc />
    public async Task<string> SaveAsync(
        Stream stream,
        string relativePath,
        string contentType,
        CancellationToken cancellationToken)
    {
        var blobName = NormalizeBlobName(relativePath);

        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        await blobContainer.SaveAsync(
            blobName,
            stream,
            overrideExisting: true,
            cancellationToken: cancellationToken);

        logger.LogInformation("文件已通过 ABP Blob Storing 成功持久化至 MinIO，Blob 名称: {BlobName}", blobName);

        return blobName;
    }

    /// <inheritdoc />
    public async Task<Stream?> OpenReadStreamAsync(
        string relativePath,
        CancellationToken cancellationToken)
    {
        var blobName = NormalizeBlobName(relativePath);

        try
        {
            return await blobContainer.GetOrNullAsync(blobName, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "通过 ABP Blob Storing 读取 MinIO 对象 {BlobName} 失败", blobName);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(
        string relativePath,
        CancellationToken cancellationToken)
    {
        var blobName = NormalizeBlobName(relativePath);

        try
        {
            return await blobContainer.DeleteAsync(blobName, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "通过 ABP Blob Storing 从 MinIO 删除对象 {BlobName} 失败", blobName);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        string relativePath,
        CancellationToken cancellationToken)
    {
        var blobName = NormalizeBlobName(relativePath);

        try
        {
            return await blobContainer.ExistsAsync(blobName, cancellationToken: cancellationToken);
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizeBlobName(string relativePath)
    {
        return relativePath.TrimStart('/', '\\').Replace('\\', '/');
    }
}
