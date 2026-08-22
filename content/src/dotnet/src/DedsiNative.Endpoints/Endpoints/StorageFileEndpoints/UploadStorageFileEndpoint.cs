using System.Security.Cryptography;
using DedsiNative.StorageFiles;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Volo.Abp;

namespace DedsiNative.Endpoints.StorageFileEndpoints;

/// <summary>
/// 上传文件请求模型。
/// </summary>
public sealed class UploadStorageFileRequest
{
    /// <summary>
    /// 上传的文件。
    /// </summary>
    public IFormFile File { get; set; } = default!;

    /// <summary>
    /// 业务分类标识（如 avatar, document, attachment，默认 general）。
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// 是否公开可读（默认 false）。
    /// </summary>
    public bool? IsPublic { get; set; }

    /// <summary>
    /// 备注说明（可选）。
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// 上传文件响应模型。
/// </summary>
/// <param name="Id">文件唯一标识，26 位 ULID。</param>
/// <param name="FileName">原始文件名。</param>
/// <param name="StorageName">物理存储文件名。</param>
/// <param name="Extension">扩展名。</param>
/// <param name="ContentType">MIME 类型。</param>
/// <param name="SizeBytes">文件大小（字节）。</param>
/// <param name="RelativePath">相对存储路径。</param>
/// <param name="Url">访问直链或预览地址。</param>
/// <param name="Md5Hash">MD5 摘要。</param>
/// <param name="Category">业务分类。</param>
/// <param name="IsPublic">是否公开。</param>
public sealed record UploadStorageFileResponse(
    string Id,
    string FileName,
    string StorageName,
    string Extension,
    string ContentType,
    long SizeBytes,
    string RelativePath,
    string? Url,
    string? Md5Hash,
    string Category,
    bool IsPublic);

/// <summary>
/// 文件上传端点，支持单文件流式接收、计算 MD5 校验和并持久化到存储引擎。
/// </summary>
public sealed class UploadStorageFileEndpoint(
    IStorageFileRepository storageFileRepository,
    IStorageProvider storageProvider)
    : Endpoint<UploadStorageFileRequest, UploadStorageFileResponse>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Post("/api/storage/upload");
        Policies(ManagementPermissions.Storage.Upload);
        AllowFileUploads();
        Description(d => d.WithTags("文件存储管理"));
        Summary(s =>
        {
            s.Summary = "上传文件";
            s.Description = "接收前端或客户端上传的文件，自动持久化存储、提取元数据并生成访问凭证。";
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UploadStorageFileRequest req, CancellationToken ct)
    {
        if (req.File is null || req.File.Length == 0)
        {
            throw new BusinessException("DedsiNative:Storage:EmptyFile", "请选择有效的文件进行上传。");
        }

        var id = Ulid.NewUlid().ToString();
        var originalFileName = Path.GetFileName(req.File.FileName);
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".bin";
        }

        var storageName = $"{id}{extension}";
        var yearMonth = DateTime.UtcNow.ToString("yyyy/MM");
        var relativePath = $"uploads/{yearMonth}/{storageName}";
        var contentType = string.IsNullOrWhiteSpace(req.File.ContentType)
            ? "application/octet-stream"
            : req.File.ContentType;

        using var memoryStream = new MemoryStream();
        await req.File.CopyToAsync(memoryStream, ct);
        memoryStream.Seek(0, SeekOrigin.Begin);

        // 计算 MD5
        using var md5 = MD5.Create();
        var md5Bytes = md5.ComputeHash(memoryStream);
        var md5Hash = Convert.ToHexString(md5Bytes).ToLowerInvariant();
        memoryStream.Seek(0, SeekOrigin.Begin);

        // 保存到物理介质
        await storageProvider.SaveAsync(memoryStream, relativePath, contentType, ct);

        var isPublic = req.IsPublic ?? false;
        var category = string.IsNullOrWhiteSpace(req.Category) ? "general" : req.Category.Trim();
        var previewUrl = $"/api/storage/preview/{id}";

        var storageFile = new StorageFile(
            id,
            originalFileName,
            storageName,
            extension,
            contentType,
            req.File.Length,
            storageProvider.ProviderType,
            relativePath,
            previewUrl,
            md5Hash,
            category,
            isPublic,
            req.Description);

        await storageFileRepository.InsertAsync(storageFile, true, ct);

        await Send.OkAsync(new UploadStorageFileResponse(
            storageFile.Id,
            storageFile.FileName,
            storageFile.StorageName,
            storageFile.Extension,
            storageFile.ContentType,
            storageFile.SizeBytes,
            storageFile.RelativePath,
            storageFile.Url,
            storageFile.Md5Hash,
            storageFile.Category,
            storageFile.IsPublic), ct);
    }
}
