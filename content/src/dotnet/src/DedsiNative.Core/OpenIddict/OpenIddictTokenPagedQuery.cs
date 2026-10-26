namespace DedsiNative.OpenIddict;

/// <summary>
/// OpenIddict 令牌分页查询条件。
/// </summary>
/// <param name="Subject">
/// 用户主体标识。
/// </param>
/// <param name="ApplicationId">
/// 关联应用标识。
/// </param>
/// <param name="Type">
/// 类型。
/// </param>
/// <param name="Status">
/// 状态。
/// </param>
/// <param name="SkipCount">
/// 跳过的记录数。
/// </param>
/// <param name="MaxResultCount">
/// 最多返回的记录数。
/// </param>
public sealed record OpenIddictTokenPagedQueryInput(
    string? Subject,
    string? ApplicationId,
    string? Type,
    string? Status,
    int SkipCount,
    int MaxResultCount);

/// <summary>
/// OpenIddict 令牌查询明细。
/// </summary>
/// <param name="Id">
/// 记录唯一标识。
/// </param>
/// <param name="ApplicationId">
/// 关联应用标识。
/// </param>
/// <param name="ClientId">
/// 客户端标识。
/// </param>
/// <param name="Subject">
/// 用户主体标识。
/// </param>
/// <param name="Status">
/// 状态。
/// </param>
/// <param name="Type">
/// 类型。
/// </param>
/// <param name="CreationDate">
/// 创建时间，以北京时间墙钟表示。
/// </param>
/// <param name="ExpirationDate">
/// 过期时间，以北京时间墙钟表示。
/// </param>
/// <param name="RedemptionDate">
/// 兑换时间，以北京时间墙钟表示。
/// </param>
public sealed record OpenIddictTokenQueryItemDto(
    string Id,
    string? ApplicationId,
    string? ClientId,
    string? Subject,
    string? Status,
    string? Type,
    DateTime? CreationDate,
    DateTime? ExpirationDate,
    DateTime? RedemptionDate);

/// <summary>
/// OpenIddict 令牌分页查询结果。
/// </summary>
/// <param name="TotalCount">
/// 符合筛选条件的总记录数。
/// </param>
/// <param name="Items">
/// 当前页的查询明细。
/// </param>
public sealed record OpenIddictTokenPagedQueryOutput(
    long TotalCount,
    OpenIddictTokenQueryItemDto[] Items);
