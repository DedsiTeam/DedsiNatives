using DedsiNative.StorageFiles;
using FastEndpoints;

namespace DedsiNative.Endpoints.StorageFileEndpoints;

/// <summary>
/// 删除文件响应模型。
/// </summary>
/// <param name="Success">是否删除成功。</param>
public sealed record DeleteStorageFileResponse(bool Success);

/// <summary>
/// 删除文件端点，物理清除存储介质中的实体文件并软删除数据库元数据。
/// </summary>
public sealed class DeleteStorageFileEndpoint(
    IStorageFileRepository storageFileRepository,
    IStorageProvider storageProvider)
    : EndpointWithoutRequest<DeleteStorageFileResponse>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Post("/api/storage/delete/{id}");
        Description(d => d.WithTags("文件存储管理"));
        Summary(s =>
        {
            s.Summary = "删除文件";
            s.Description = "删除指定文件记录，并同步清除底层存储介质中的物理文件。";
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id");
        var fileRecord = await storageFileRepository.GetAsync(id, true, ct);

        // 尝试从物理存储中删除文件
        await storageProvider.DeleteAsync(fileRecord.RelativePath, ct);

        // 删除数据库记录
        await storageFileRepository.DeleteAsync(fileRecord, true, ct);

        await Send.OkAsync(new DeleteStorageFileResponse(true), ct);
    }
}
