using Dedsi.Ddd.Domain.Repositories;

namespace DedsiNative.Positions;

/// <summary>岗位聚合仓储，提供岗位及其子实体的持久化操作。</summary>
public interface IPositionRepository : IDedsiCqrsRepository<Position, string>;
