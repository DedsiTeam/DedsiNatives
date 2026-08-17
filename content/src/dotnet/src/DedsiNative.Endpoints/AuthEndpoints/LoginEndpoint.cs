using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DedsiNative.Users;
using FastEndpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp.Security.Claims;
using Volo.Abp.Users;

namespace DedsiNative.Endpoints.AuthEndpoints;

/// <summary>
/// 登录请求参数。
/// </summary>
/// <param name="Username">用户名。</param>
/// <param name="Password">密码。</param>
public sealed record LoginRequest(string Username, string Password);

/// <summary>
/// 登录响应，包含 JWT Token 及过期时间。
/// </summary>
/// <param name="Token">JWT Bearer Token。</param>
/// <param name="ExpiresAt">Token 过期时间（UTC）。</param>
public sealed record LoginResponse(string Token, DateTime ExpiresAt);

/// <summary>
/// 登录端点，处理 POST /api/auth/login 请求。
/// 验证数据库用户凭证并签发 JWT Token。
/// </summary>
/// <param name="configuration">
/// JWT 签发配置。
/// </param>
/// <param name="userQuery">
/// 用户认证只读查询服务。
/// </param>
public sealed class LoginEndpoint(
    IConfiguration configuration,
    IUserQuery userQuery)
    : Endpoint<LoginRequest, LoginResponse>
{
    /// <summary>
    /// 配置端点路由，匿名访问。
    /// </summary>
    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
    }

    /// <summary>
    /// 验证用户凭证，成功则返回签发的 JWT Token。
    /// </summary>
    /// <param name="req">登录请求，包含用户名和密码。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var account = req.Username?.Trim() ?? string.Empty;
        var user = await userQuery.FindLoginByAccountAsync(account, ct);

        // 账号不存在与密码错误保持相同响应，避免对外暴露账号是否存在。
        if (user is null
            || !UserPasswordHasher.Verify(
                req.Password,
                user.PasswordHash,
                user.PasswordSalt))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var jwtSection         = configuration.GetSection("Jwt");
        var secret             = jwtSection["Secret"]!;
        var issuer             = jwtSection["Issuer"]!;
        var audience           = jwtSection["Audience"]!;
        var expirationMinutes  = jwtSection.GetValue<int>("ExpirationMinutes");

        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        // 构建 Claims
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new(AbpClaimTypes.Name, user.Name)
        };

        // 签发 Token
        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token       = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            expires:            expiresAt,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        await Send.OkAsync(new LoginResponse(tokenString, expiresAt), ct);
    }
}
