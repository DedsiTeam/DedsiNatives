using Dedsi.EntityFrameworkCore.Repositories;
using DedsiNative.Dictionaries;
using Volo.Abp.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Repositories;

/// <summary>
/// 字典聚合仓储的 EF Core 实现。
/// </summary>
/// <param name="dbContextProvider">数据库上下文提供者。</param>
public sealed class DictionaryRepository(
    IDbContextProvider<DedsiNativeDbContext> dbContextProvider)
    : DedsiDddEfCoreRepository<DedsiNativeDbContext, Dictionary, string>(
        dbContextProvider),
      IDictionaryRepository;
