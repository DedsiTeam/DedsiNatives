using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace DedsiNative.StorageFiles;

/// <summary>
/// 文件与对象存储聚合根，维护系统统一文件资产元数据、存储位置、访问策略与摘要校验。
/// </summary>
public class StorageFile : FullAuditedAggregateRoot<string>
{
    /// <summary>
    /// EF Core 所需的无参构造函数。
    /// </summary>
    protected StorageFile()
    {
    }

    /// <summary>
    /// 初始化文件与对象存储聚合根实例。
    /// </summary>
    /// <param name="id">
    /// 文件唯一标识，26 位有序 ULID 字符串。
    /// </param>
    /// <param name="fileName">
    /// 用户上传时的原始文件名。
    /// </param>
    /// <param name="storageName">
    /// 物理存储文件名（防重名唯一标识）。
    /// </param>
    /// <param name="extension">
    /// 文件扩展名（含点号，如 .png, .pdf）。
    /// </param>
    /// <param name="contentType">
    /// 文件标准 MIME 内容类型。
    /// </param>
    /// <param name="sizeBytes">
    /// 文件大小（单位：字节）。
    /// </param>
    /// <param name="storageType">
    /// 存储提供者介质类型。
    /// </param>
    /// <param name="relativePath">
    /// 存储相对路径或对象 Key。
    /// </param>
    /// <param name="url">
    /// 公开访问直链 URL（可选）。
    /// </param>
    /// <param name="md5Hash">
    /// 文件 MD5 哈希摘要（可选，用于秒传查重）。
    /// </param>
    /// <param name="category">
    /// 业务分类标识（如 avatar, document, attachment）。
    /// </param>
    /// <param name="isPublic">
    /// 是否允许免鉴权公开访问。
    /// </param>
    /// <param name="description">
    /// 文件备注说明。
    /// </param>
    public StorageFile(
        string id,
        string fileName,
        string storageName,
        string extension,
        string contentType,
        long sizeBytes,
        StorageType storageType,
        string relativePath,
        string? url = null,
        string? md5Hash = null,
        string category = "general",
        bool isPublic = false,
        string? description = null)
        : base(ValidateUlid(id, nameof(id)))
    {
        FileName = Check.NotNullOrWhiteSpace(fileName, nameof(fileName), StorageFileConsts.MaxFileNameLength);
        StorageName = Check.NotNullOrWhiteSpace(storageName, nameof(storageName), StorageFileConsts.MaxStorageNameLength);
        Extension = Check.NotNullOrWhiteSpace(extension, nameof(extension), StorageFileConsts.MaxExtensionLength);
        ContentType = Check.NotNullOrWhiteSpace(contentType, nameof(contentType), StorageFileConsts.MaxContentTypeLength);
        SizeBytes = Math.Max(0, sizeBytes);
        StorageType = storageType;
        RelativePath = Check.NotNullOrWhiteSpace(relativePath, nameof(relativePath), StorageFileConsts.MaxRelativePathLength);
        Url = Check.Length(url, nameof(url), StorageFileConsts.MaxUrlLength);
        Md5Hash = Check.Length(md5Hash, nameof(md5Hash), StorageFileConsts.MaxMd5HashLength);
        Category = Check.NotNullOrWhiteSpace(category, nameof(category), StorageFileConsts.MaxCategoryLength);
        IsPublic = isPublic;
        Description = Check.Length(description, nameof(description), StorageFileConsts.MaxDescriptionLength);
    }

    /// <summary>
    /// 用户上传时的原始文件名。
    /// </summary>
    public string FileName { get; private set; } = string.Empty;

    /// <summary>
    /// 物理存储文件名（带唯一后缀防重名）。
    /// </summary>
    public string StorageName { get; private set; } = string.Empty;

    /// <summary>
    /// 文件扩展名（小写带点，如 .jpg, .pdf）。
    /// </summary>
    public string Extension { get; private set; } = string.Empty;

    /// <summary>
    /// 标准 MIME 类型（如 image/png, application/pdf）。
    /// </summary>
    public string ContentType { get; private set; } = string.Empty;

    /// <summary>
    /// 文件大小（单位：字节 Bytes）。
    /// </summary>
    public long SizeBytes { get; private set; }

    /// <summary>
    /// 底层物理存储介质类型。
    /// </summary>
    public StorageType StorageType { get; private set; }

    /// <summary>
    /// 物理存储相对路径或存储桶 Object Key。
    /// </summary>
    public string RelativePath { get; private set; } = string.Empty;

    /// <summary>
    /// 公开访问直链 URL（若未配置或为私有文件可为空）。
    /// </summary>
    public string? Url { get; private set; }

    /// <summary>
    /// 文件 MD5 校验哈希特征值。
    /// </summary>
    public string? Md5Hash { get; private set; }

    /// <summary>
    /// 业务分类标识（如 avatar, attachment, document, general）。
    /// </summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>
    /// 是否允许免鉴权公开预览/下载。
    /// </summary>
    public bool IsPublic { get; private set; }

    /// <summary>
    /// 文件描述或业务备注。
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 更新文件业务元数据信息。
    /// </summary>
    /// <param name="fileName">
    /// 新的文件名。
    /// </param>
    /// <param name="category">
    /// 业务分类标识。
    /// </param>
    /// <param name="isPublic">
    /// 是否公开。
    /// </param>
    /// <param name="description">
    /// 备注说明。
    /// </param>
    /// <returns>
    /// 当前文件聚合根实例。
    /// </returns>
    public StorageFile UpdateInfo(
        string fileName,
        string category,
        bool isPublic,
        string? description)
    {
        FileName = Check.NotNullOrWhiteSpace(fileName, nameof(fileName), StorageFileConsts.MaxFileNameLength);
        Category = Check.NotNullOrWhiteSpace(category, nameof(category), StorageFileConsts.MaxCategoryLength);
        IsPublic = isPublic;
        Description = Check.Length(description, nameof(description), StorageFileConsts.MaxDescriptionLength);
        return this;
    }

    /// <summary>
    /// 设置或更新文件的公开访问直链。
    /// </summary>
    /// <param name="url">
    /// 访问直链 URL。
    /// </param>
    /// <returns>
    /// 当前文件聚合根实例。
    /// </returns>
    public StorageFile SetUrl(string? url)
    {
        Url = Check.Length(url, nameof(url), StorageFileConsts.MaxUrlLength);
        return this;
    }

    private static string ValidateUlid(string value, string paramName)
    {
        Check.NotNullOrWhiteSpace(value, paramName);
        if (value.Length != StorageFileConsts.UlidLength)
        {
            throw new ArgumentException($"'{paramName}' 必须是长度为 26 位的 ULID 格式字符串。", paramName);
        }

        return value;
    }
}
