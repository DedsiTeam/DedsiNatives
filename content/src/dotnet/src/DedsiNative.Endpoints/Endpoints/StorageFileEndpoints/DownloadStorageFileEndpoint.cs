using DedsiNative.StorageFiles;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Volo.Abp;

namespace DedsiNative.Endpoints.StorageFileEndpoints;

/// <summary>
/// 文件流下载端点。
/// </summary>
public sealed class DownloadStorageFileEndpoint(
    IStorageFileRepository storageFileRepository,
    IStorageProvider storageProvider)
    : EndpointWithoutRequest
{
    /// <inheritdoc />
    public override void Configure()
    {
        Get("/api/storage/download/{id}");
        AllowAnonymous();
        Description(d => d.WithTags("文件存储管理"));
        Summary(s =>
        {
            s.Summary = "下载文件";
            s.Description = "根据文件唯一标识获取文件流，响应 Content-Disposition 为附件下载模式。";
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id");
        var fileRecord = await storageFileRepository.GetAsync(id, true, ct);

        var stream = await storageProvider.OpenReadStreamAsync(fileRecord.RelativePath, ct);
        if (stream is null)
        {
            throw new BusinessException("DedsiNative:Storage:PhysicalFileNotFound", "底层物理文件不存在或已被清除。");
        }

        await Send.StreamAsync(
            stream,
            fileName: fileRecord.FileName,
            fileLengthBytes: fileRecord.SizeBytes,
            contentType: fileRecord.ContentType,
            cancellation: ct);
    }
}
