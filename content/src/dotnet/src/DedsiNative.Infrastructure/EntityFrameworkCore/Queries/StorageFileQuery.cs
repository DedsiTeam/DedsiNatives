using DedsiNative.StorageFiles;
using Microsoft.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Queries;

/// <summary>
/// 文件与对象存储只读查询契约的 EF Core 实现。
/// </summary>
public sealed class StorageFileQuery(DedsiNativeDbContext dbContext) : IStorageFileQuery
{
    /// <inheritdoc />
    public async Task<StorageFilePagedQueryOutput> GetPagedAsync(
        StorageFilePagedQueryInput query,
        CancellationToken cancellationToken)
    {
        var keyword = query.Keyword?.Trim();
        var category = query.Category?.Trim();
        var extension = query.Extension?.Trim();
        // 扩展名统一补齐前导点，空筛选条件仍保持为空。
        if (!string.IsNullOrEmpty(extension) && !extension.StartsWith('.'))
        {
            extension = "." + extension;
        }

        var dbQuery = dbContext.StorageFiles.AsNoTracking()
            .WhereIf(!string.IsNullOrEmpty(keyword), f =>
                f.FileName.Contains(keyword!) ||
                f.StorageName.Contains(keyword!) ||
                (f.Description != null && f.Description.Contains(keyword!)))
            .WhereIf(!string.IsNullOrEmpty(category), f => f.Category == category)
            .WhereIf(!string.IsNullOrEmpty(extension), f => f.Extension == extension)
            .WhereIf(query.StorageType.HasValue, f => f.StorageType == query.StorageType!.Value)
            .WhereIf(query.IsPublic.HasValue, f => f.IsPublic == query.IsPublic!.Value)
            .WhereIf(query.StartTime.HasValue, f => f.CreationTime >= query.StartTime!.Value)
            .WhereIf(query.EndTime.HasValue, f => f.CreationTime <= query.EndTime!.Value);

        var totalCount = await dbQuery.LongCountAsync(cancellationToken);

        dbQuery = dbQuery.OrderByDescending(f => f.CreationTime);

        if (!query.IsExport)
        {
            dbQuery = dbQuery.Skip(query.SkipCount).Take(query.MaxResultCount);
        }

        var items = await dbQuery
            .Select(f => new StorageFileQueryItemDto(
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
                f.CreationTime))
            .ToArrayAsync(cancellationToken);

        return new StorageFilePagedQueryOutput(totalCount, items);
    }

    /// <inheritdoc />
    public async Task<StorageFile?> FindByMd5Async(string md5Hash, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(md5Hash))
        {
            return null;
        }

        return await dbContext.StorageFiles
            .Where(f => f.Md5Hash == md5Hash)
            .OrderByDescending(f => f.CreationTime)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
