using Dedsi.Ddd.Domain.Repositories;

namespace DedsiNative.Permissions;

/// <summary>权限聚合仓储，提供权限聚合根的持久化操作。</summary>
public interface IPermissionRepository : IDedsiCqrsRepository<Permission, string>;
