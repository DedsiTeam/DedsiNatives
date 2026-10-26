namespace DedsiNative.OpenIddict;

/// <summary>
/// OpenIddict 客户端分页查询条件。
/// </summary>
/// <param name="ClientId">
/// 客户端标识。
/// </param>
/// <param name="DisplayName">
/// 显示名称。
/// </param>
/// <param name="ClientType">
/// 客户端类型。
/// </param>
/// <param name="SkipCount">
/// 跳过的记录数。
/// </param>
/// <param name="MaxResultCount">
/// 最多返回的记录数。
/// </param>
public sealed record OpenIddictApplicationPagedQueryInput(
    string? ClientId,
    string? DisplayName,
    string? ClientType,
    int SkipCount,
    int MaxResultCount);

/// <summary>
/// OpenIddict 客户端查询明细。
/// </summary>
/// <param name="Id">
/// 记录唯一标识。
/// </param>
/// <param name="ClientId">
/// 客户端标识。
/// </param>
/// <param name="DisplayName">
/// 显示名称。
/// </param>
/// <param name="ClientType">
/// 客户端类型。
/// </param>
/// <param name="ConsentType">
/// 授权确认类型。
/// </param>
/// <param name="RedirectUris">
/// 登录回调地址。
/// </param>
/// <param name="PostLogoutRedirectUris">
/// 登出回调地址。
/// </param>
/// <param name="Permissions">
/// 客户端权限。
/// </param>
public sealed record OpenIddictApplicationQueryItemDto(
    string Id,
    string? ClientId,
    string? DisplayName,
    string? ClientType,
    string? ConsentType,
    string[] RedirectUris,
    string[] PostLogoutRedirectUris,
    string[] Permissions);

/// <summary>
/// OpenIddict 客户端分页查询结果。
/// </summary>
/// <param name="TotalCount">
/// 符合筛选条件的总记录数。
/// </param>
/// <param name="Items">
/// 当前页的查询明细。
/// </param>
public sealed record OpenIddictApplicationPagedQueryOutput(
    long TotalCount,
    OpenIddictApplicationQueryItemDto[] Items);
