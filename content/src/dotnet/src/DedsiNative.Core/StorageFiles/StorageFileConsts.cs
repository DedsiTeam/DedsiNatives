namespace DedsiNative.StorageFiles;

/// <summary>
/// 文件与对象存储聚合字段约束常量。
/// </summary>
public static class StorageFileConsts
{
    /// <summary>
    /// ULID 字符串长度。
    /// </summary>
    public const int UlidLength = 26;

    /// <summary>
    /// 原始文件名最大长度。
    /// </summary>
    public const int MaxFileNameLength = 256;

    /// <summary>
    /// 存储物理文件名最大长度。
    /// </summary>
    public const int MaxStorageNameLength = 256;

    /// <summary>
    /// 文件扩展名最大长度。
    /// </summary>
    public const int MaxExtensionLength = 32;

    /// <summary>
    /// 内容 MIME 类型最大长度。
    /// </summary>
    public const int MaxContentTypeLength = 128;

    /// <summary>
    /// 相对存储路径最大长度。
    /// </summary>
    public const int MaxRelativePathLength = 512;

    /// <summary>
    /// 访问直链 URL 最大长度。
    /// </summary>
    public const int MaxUrlLength = 1024;

    /// <summary>
    /// MD5 文件哈希特征值最大长度。
    /// </summary>
    public const int MaxMd5HashLength = 64;

    /// <summary>
    /// 业务分类标识最大长度。
    /// </summary>
    public const int MaxCategoryLength = 64;

    /// <summary>
    /// 文件说明最大长度。
    /// </summary>
    public const int MaxDescriptionLength = 512;
}
