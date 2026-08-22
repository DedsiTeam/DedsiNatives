using DedsiNative.StorageFiles;
using FastEndpoints;

namespace DedsiNative.Endpoints.StorageFileEndpoints;

/// <summary>
/// 文件详情响应模型。
/// </summary>
/// <param name="Id">文件唯一标识，26 位 ULID。</param>
/// <param name="FileName">原始文件名。</param>
/// <param name="StorageName">物理存储文件名。</param>
/// <param name="Extension">扩展名。</param>
/// <param name="ContentType">MIME 类型。</param>
/// <param name="SizeBytes">文件大小（字节）。</param>
/// <param name="StorageType">存储介质类型。</param>
/// <param name="RelativePath">相对存储路径。</param>
/// <param name="Url">访问直链。</param>
/// <param name="Md5Hash">MD5 摘要。</param>
/// <param name="Category">业务分类。</param>
/// <param name="IsPublic">是否公开。</param>
/// <param name="Description">文件说明。</param>
/// <param name="CreatedAtUtc">创建时间（UTC）。</param>
public sealed record StorageFileDetailResponse(
    string Id,
    string FileName,
    string StorageName,
    string Extension,
    string ContentType,
    long SizeBytes,
    StorageType StorageType,
    string RelativePath,
    string? Url,
    string? Md5Hash,
    string Category,
    bool IsPublic,
    string? Description,
    DateTime CreatedAtUtc);

/// <summary>
/// 获取文件详情端点。
/// </summary>
public sealed class GetStorageFileDetailEndpoint(IStorageFileRepository storageFileRepository)
    : EndpointWithoutRequest<StorageFileDetailResponse>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Get("/api/storage/{id}");
        Policies(ManagementPermissions.Storage.View);
        Description(d => d.WithTags("文件存储管理"));
        Summary(s =>
        {
            s.Summary = "获取文件详情";
            s.Description = "根据文件唯一标识获取文件元数据详情。";
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id");
        var f = await storageFileRepository.GetAsync(id, true, ct);

        await Send.OkAsync(new StorageFileDetailResponse(
            f.Id,
            f.FileName,
            f.StorageName,
            f.Extension,
            f.ContentType,
            f.SizeBytes,
            f.StorageType,
            f.RelativePath,
            f.Url,
            f.Md5Hash,
            f.Category,
            f.IsPublic,
            f.Description,
            f.CreationTime), ct);
    }
}
