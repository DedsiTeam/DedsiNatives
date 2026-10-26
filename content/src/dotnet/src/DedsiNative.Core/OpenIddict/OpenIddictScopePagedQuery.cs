namespace DedsiNative.OpenIddict;

/// <summary>
/// OpenIddict 作用域分页查询条件。
/// </summary>
/// <param name="Name">
/// 作用域名称。
/// </param>
/// <param name="DisplayName">
/// 显示名称。
/// </param>
/// <param name="SkipCount">
/// 跳过的记录数。
/// </param>
/// <param name="MaxResultCount">
/// 最多返回的记录数。
/// </param>
public sealed record OpenIddictScopePagedQueryInput(
    string? Name,
    string? DisplayName,
    int SkipCount,
    int MaxResultCount);

/// <summary>
/// OpenIddict 作用域查询明细。
/// </summary>
/// <param name="Id">
/// 记录唯一标识。
/// </param>
/// <param name="Name">
/// 作用域名称。
/// </param>
/// <param name="DisplayName">
/// 显示名称。
/// </param>
/// <param name="Description">
/// 作用域描述。
/// </param>
/// <param name="Resources">
/// 关联资源。
/// </param>
public sealed record OpenIddictScopeQueryItemDto(
    string Id,
    string? Name,
    string? DisplayName,
    string? Description,
    string[] Resources);

/// <summary>
/// OpenIddict 作用域分页查询结果。
/// </summary>
/// <param name="TotalCount">
/// 符合筛选条件的总记录数。
/// </param>
/// <param name="Items">
/// 当前页的查询明细。
/// </param>
public sealed record OpenIddictScopePagedQueryOutput(
    long TotalCount,
    OpenIddictScopeQueryItemDto[] Items);
