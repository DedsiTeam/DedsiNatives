namespace DedsiNative.Endpoints.AuthEndpoints;

/// <summary>
/// 普通账号密码登录请求参数。
/// </summary>
/// <param name="Username">用户名或登录账号。</param>
/// <param name="Password">登录密码。</param>
public sealed record LoginRequest(string Username, string Password);

/// <summary>
/// SSO 单点登录换取凭据请求参数。
/// </summary>
/// <param name="Token">SSO 认证中心返回的 ID Token 或 Access Token。</param>
public sealed record SsoLoginRequest(string Token);

/// <summary>
/// 登录响应模型，包含系统 JWT Bearer Token、过期时间及完整用户安全资料。
/// </summary>
/// <param name="Token">JWT Bearer Token。</param>
/// <param name="ExpiresAt">Token 过期时间（UTC）。</param>
/// <param name="User">当前登录用户的安全基本资料。</param>
public sealed record LoginResponse(string Token, DateTime ExpiresAt, LoginUserResponse User);

/// <summary>
/// 登录响应中用户所属岗位。
/// </summary>
/// <param name="PositionId">岗位唯一标识。</param>
/// <param name="PositionName">岗位名称。</param>
public sealed record LoginUserPositionResponse(
    string PositionId,
    string PositionName);

/// <summary>
/// 登录成功后返回的当前用户完整安全基本资料。
/// </summary>
/// <param name="Id">用户唯一标识。</param>
/// <param name="Name">用户姓名。</param>
/// <param name="Email">用户邮箱。</param>
/// <param name="Account">登录账号。</param>
/// <param name="Permissions">由用户岗位解析出的有效权限名称列表。</param>
/// <param name="Positions">用户所属岗位列表。</param>
public sealed record LoginUserResponse(
    Guid Id,
    string Name,
    string Email,
    string Account,
    string[] Permissions,
    LoginUserPositionResponse[] Positions);
