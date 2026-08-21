using Dedsi.EntityFrameworkCore.Repositories;
using DedsiNative.StorageFiles;
using Volo.Abp.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Repositories;

/// <summary>
/// 文件与对象存储聚合仓储的 EF Core 实现。
/// </summary>
public sealed class StorageFileRepository(IDbContextProvider<DedsiNativeDbContext> dbContextProvider)
    : DedsiDddEfCoreRepository<DedsiNativeDbContext, StorageFile, string>(dbContextProvider), IStorageFileRepository;
