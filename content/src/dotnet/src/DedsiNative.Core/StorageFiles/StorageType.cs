namespace DedsiNative.StorageFiles;

/// <summary>
/// 底层对象与文件存储介质类型枚举。
/// </summary>
public enum StorageType
{
    /// <summary>
    /// 本地服务器磁盘持久化存储。
    /// </summary>
    Local = 1,

    /// <summary>
    /// MinIO 私有化对象存储集群。
    /// </summary>
    Minio = 2,

    /// <summary>
    /// 阿里云 OSS 对象存储。
    /// </summary>
    AliyunOss = 3,

    /// <summary>
    /// 腾讯云 COS 对象存储。
    /// </summary>
    TencentCos = 4,

    /// <summary>
    /// 亚马逊 AWS S3 对象存储。
    /// </summary>
    AwsS3 = 5,
}
