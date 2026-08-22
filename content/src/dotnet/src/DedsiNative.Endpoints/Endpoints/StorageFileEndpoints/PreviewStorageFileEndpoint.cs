using DedsiNative.StorageFiles;
using FastEndpoints;
using Volo.Abp;

namespace DedsiNative.Endpoints.StorageFileEndpoints;

/// <summary>
/// 文件在线预览端点，用于图片、PDF 等内联直接渲染展示。
/// </summary>
public sealed class PreviewStorageFileEndpoint(
    IStorageFileRepository storageFileRepository,
    IStorageProvider storageProvider)
    : EndpointWithoutRequest
{
    /// <inheritdoc />
    public override void Configure()
    {
        Get("/api/storage/preview/{id}");
        Policies(ManagementPermissions.Storage.View);
        Description(d => d.WithTags("文件存储管理"));
        Summary(s =>
        {
            s.Summary = "预览文件";
            s.Description = "按内联（inline）方式直接输出文件流，用于图片、PDF 及多媒体浏览器直接展示。";
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
            fileLengthBytes: fileRecord.SizeBytes,
            contentType: fileRecord.ContentType,
            cancellation: ct);
    }
}
