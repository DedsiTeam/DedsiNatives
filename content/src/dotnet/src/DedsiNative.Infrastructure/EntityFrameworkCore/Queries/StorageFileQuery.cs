using DedsiNative.StorageFiles;
using Microsoft.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Queries;

/// <summary>
/// 文件与对象存储只读查询契约的 EF Core 实现。
/// </summary>
public sealed class StorageFileQuery(IDedsiNativeDbContext dbContext) : IStorageFileQuery
{
    /// <inheritdoc />
    public async Task<StorageFilePagedQueryResult> GetPagedAsync(
        StorageFilePagedQuery query,
        CancellationToken cancellationToken)
    {
        var dbQuery = dbContext.StorageFiles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var trimmedKeyword = query.Keyword.Trim();
            dbQuery = dbQuery.Where(f =>
                f.FileName.Contains(trimmedKeyword) ||
                f.StorageName.Contains(trimmedKeyword) ||
                (f.Description != null && f.Description.Contains(trimmedKeyword)));
        }

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var trimmedCategory = query.Category.Trim();
            dbQuery = dbQuery.Where(f => f.Category == trimmedCategory);
        }

        if (!string.IsNullOrWhiteSpace(query.Extension))
        {
            var trimmedExtension = query.Extension.Trim();
            if (!trimmedExtension.StartsWith('.'))
            {
                trimmedExtension = "." + trimmedExtension;
            }
            dbQuery = dbQuery.Where(f => f.Extension == trimmedExtension);
        }

        if (query.StorageType.HasValue)
        {
            dbQuery = dbQuery.Where(f => f.StorageType == query.StorageType.Value);
        }

        if (query.IsPublic.HasValue)
        {
            dbQuery = dbQuery.Where(f => f.IsPublic == query.IsPublic.Value);
        }

        if (query.StartTimeUtc.HasValue)
        {
            dbQuery = dbQuery.Where(f => f.CreationTime >= query.StartTimeUtc.Value);
        }

        if (query.EndTimeUtc.HasValue)
        {
            dbQuery = dbQuery.Where(f => f.CreationTime <= query.EndTimeUtc.Value);
        }

        var totalCount = await dbQuery.LongCountAsync(cancellationToken);

        dbQuery = dbQuery.OrderByDescending(f => f.CreationTime);

        if (!query.IsExport)
        {
            dbQuery = dbQuery.Skip(query.SkipCount).Take(query.MaxResultCount);
        }

        var items = await dbQuery
            .Select(f => new StorageFileQueryItem(
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

        return new StorageFilePagedQueryResult(totalCount, items);
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
