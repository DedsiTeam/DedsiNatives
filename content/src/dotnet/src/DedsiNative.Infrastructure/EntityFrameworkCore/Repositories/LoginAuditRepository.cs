using Dedsi.EntityFrameworkCore.Repositories;
using DedsiNative.LoginAudits;
using Volo.Abp.EntityFrameworkCore;

namespace DedsiNative.EntityFrameworkCore.Repositories;

/// <summary>
/// 登录审计写侧仓储的 EF Core 实现。
/// </summary>
/// <param name="dbContextProvider">用于获取登录审计数据库上下文的提供者。</param>
public sealed class LoginAuditRepository(
    IDbContextProvider<DedsiNativeDbContext> dbContextProvider)
    : DedsiEfCoreRepository<DedsiNativeDbContext, LoginAudit, string>(
        dbContextProvider),
      ILoginAuditRepository;
