using Dedsi.Ddd.Domain.Repositories;

namespace DedsiNative.Menus;

/// <summary>
/// 菜单聚合仓储，提供菜单写入与持久化能力。
/// </summary>
public interface IMenuRepository : IDedsiCqrsRepository<Menu, string>;
