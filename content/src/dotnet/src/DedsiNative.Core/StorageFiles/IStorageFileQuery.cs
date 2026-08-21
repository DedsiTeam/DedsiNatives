using Dedsi.Ddd.Domain.Queries;

namespace DedsiNative.StorageFiles;

/// <summary>
/// 文件分页检索条件参数模型。
/// </summary>
/// <param name="Keyword">
/// 文件名或存储名模糊搜索关键字，为空时不筛选。
/// </param>
/// <param name="Category">
/// 业务分类筛选（如 avatar, document, attachment，为空时不筛选）。
/// </param>
/// <param name="Extension">
/// 文件扩展名筛选（如 .png, .pdf，为空时不筛选）。
/// </param>
/// <param name="StorageType">
/// 存储介质筛选（为空时不筛选）。
/// </param>
/// <param name="IsPublic">
/// 是否公开筛选（为空时不筛选）。
/// </param>
/// <param name="StartTimeUtc">
/// 上传起始时间（UTC）。
/// </param>
/// <param name="EndTimeUtc">
/// 上传截止时间（UTC）。
/// </param>
/// <param name="SkipCount">
/// 需要跳过的记录数。
/// </param>
/// <param name="MaxResultCount">
/// 单页最多返回的记录数。
/// </param>
/// <param name="IsExport">
/// 是否为导出查询；导出时不分页。
/// </param>
public sealed record StorageFilePagedQuery(
    string? Keyword,
    string? Category,
    string? Extension,
    StorageType? StorageType,
    bool? IsPublic,
    DateTime? StartTimeUtc,
    DateTime? EndTimeUtc,
    int SkipCount,
    int MaxResultCount,
    bool IsExport);

/// <summary>
/// 文件分页查询投影项 DTO。
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
/// <param name="CreatedAtUtc">上传时间（UTC）。</param>
public sealed record StorageFileQueryItem(
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
/// 文件分页查询结果集。
/// </summary>
/// <param name="TotalCount">符合条件的总记录数。</param>
/// <param name="Items">当前页文件记录列表。</param>
public sealed record StorageFilePagedQueryResult(
    long TotalCount,
    StorageFileQueryItem[] Items);

/// <summary>
/// 文件与对象存储只读查询契约。
/// </summary>
public interface IStorageFileQuery : IDedsiQuery
{
    /// <summary>
    /// 分页检索文件记录列表。
    /// </summary>
    /// <param name="query">
    /// 分页筛选条件。
    /// </param>
    /// <param name="cancellationToken">
    /// 异步操作取消令牌。
    /// </param>
    /// <returns>
    /// 分页查询结果集。
    /// </returns>
    Task<StorageFilePagedQueryResult> GetPagedAsync(
        StorageFilePagedQuery query,
        CancellationToken cancellationToken);

    /// <summary>
    /// 根据文件 MD5 哈希摘要查找已存在的有效文件记录（用于实现秒传及防重复存储）。
    /// </summary>
    /// <param name="md5Hash">
    /// 文件的 MD5 特征值。
    /// </param>
    /// <param name="cancellationToken">
    /// 异步操作取消令牌。
    /// </param>
    /// <returns>
    /// 匹配的文件聚合根，未找到返回 null。
    /// </returns>
    Task<StorageFile?> FindByMd5Async(string md5Hash, CancellationToken cancellationToken);
}
