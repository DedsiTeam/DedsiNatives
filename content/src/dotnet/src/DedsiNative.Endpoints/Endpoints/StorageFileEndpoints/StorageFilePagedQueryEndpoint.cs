using DedsiNative.StorageFiles;
using FastEndpoints;

namespace DedsiNative.Endpoints.StorageFileEndpoints;

/// <summary>
/// 文件分页检索请求模型。
/// </summary>
/// <param name="Keyword">文件名或存储名模糊搜索关键字（可选）。</param>
/// <param name="Category">业务分类筛选（可选）。</param>
/// <param name="Extension">扩展名筛选（可选）。</param>
/// <param name="StorageType">存储类型筛选（可选）。</param>
/// <param name="IsPublic">是否公开筛选（可选）。</param>
/// <param name="StartTimeUtc">上传起始时间（可选）。</param>
/// <param name="EndTimeUtc">上传截止时间（可选）。</param>
/// <param name="PageIndex">当前页码（默认 1）。</param>
/// <param name="PageSize">每页条数（默认 10）。</param>
public sealed record StorageFilePagedRequest(
    string? Keyword,
    string? Category,
    string? Extension,
    StorageType? StorageType,
    bool? IsPublic,
    DateTime? StartTimeUtc,
    DateTime? EndTimeUtc,
    int PageIndex = 1,
    int PageSize = 10);

/// <summary>
/// 文件分页检索响应模型。
/// </summary>
/// <param name="TotalCount">总记录数。</param>
/// <param name="Items">文件记录列表。</param>
public sealed record StorageFilePagedResponse(
    long TotalCount,
    StorageFileQueryItem[] Items);

/// <summary>
/// 分页查询文件列表端点。
/// </summary>
public sealed class StorageFilePagedQueryEndpoint(IStorageFileQuery storageFileQuery)
    : Endpoint<StorageFilePagedRequest, StorageFilePagedResponse>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Post("/api/storage/pagedQuery");
        Policies(ManagementPermissions.Storage.View);
        Description(d => d.WithTags("文件存储管理"));
        Summary(s =>
        {
            s.Summary = "分页查询文件列表";
            s.Description = "按文件名、分类、扩展名及上传时间等多条件分页筛选文件元数据列表。";
        });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(StorageFilePagedRequest req, CancellationToken ct)
    {
        var pageIndex = Math.Max(1, req.PageIndex);
        var pageSize = Math.Clamp(req.PageSize, 1, 1000);
        var skipCount = (pageIndex - 1) * pageSize;

        var result = await storageFileQuery.GetPagedAsync(
            new StorageFilePagedQuery(
                req.Keyword,
                req.Category,
                req.Extension,
                req.StorageType,
                req.IsPublic,
                req.StartTimeUtc,
                req.EndTimeUtc,
                skipCount,
                pageSize,
                false),
            ct);

        await Send.OkAsync(new StorageFilePagedResponse(result.TotalCount, result.Items), ct);
    }
}
