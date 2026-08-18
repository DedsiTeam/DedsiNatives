using Dedsi.Ddd.Domain.Repositories;

namespace DedsiNative.LoginAudits;

/// <summary>
/// 登录审计实体的写侧仓储，只用于追加记录和按标识读取。
/// </summary>
public interface ILoginAuditRepository : IDedsiCqrsRepository<LoginAudit, string>;
