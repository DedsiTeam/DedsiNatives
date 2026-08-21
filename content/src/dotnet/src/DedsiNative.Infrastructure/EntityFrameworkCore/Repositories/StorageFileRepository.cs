using Dedsi.EntityFrameworkCore.Repositories;
using DedsiNative.StorageFiles;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Repositories;

/// <summary>
/// 文件与对象存储聚合仓储的 EF Core 实现。
/// </summary>
public sealed class StorageFileRepository(IDbContextProvider<DedsiNativeDbContext> dbContextProvider)
    : DedsiDddEfCoreRepository<DedsiNativeDbContext, StorageFile, string>(dbContextProvider), IStorageFileRepository
{
    /// <inheritdoc />
    public async Task<StorageFile?> FindByMd5Async(string md5Hash, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(md5Hash))
        {
            return null;
        }

        var dbContext = await dbContextProvider.GetDbContextAsync();
        return await dbContext.StorageFiles
            .Where(f => f.Md5Hash == md5Hash)
            .OrderByDescending(f => f.CreationTime)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
