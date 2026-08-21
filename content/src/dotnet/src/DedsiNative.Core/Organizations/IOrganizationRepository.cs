using Dedsi.Ddd.Domain.Repositories;

namespace DedsiNative.Organizations;

/// <summary>
/// 组织机构聚合仓储契约。
/// </summary>
public interface IOrganizationRepository : IDedsiCqrsRepository<Organization, string>;
