using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FastEndpoints;
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
/// 验证用户凭证（当前为硬编码），签发 JWT Token。
/// </summary>
public class LoginEndpoint(IConfiguration configuration) : Endpoint<LoginRequest, LoginResponse>
{
    // ── 硬编码用户（临时，后续替换为数据库查询）──────────────────
    private const string HardcodedUsername = "admin";
    private const string HardcodedPassword = "Admin@123";

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
        // 验证凭证
        if (req.Username != HardcodedUsername || req.Password != HardcodedPassword)
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
            new Claim(JwtRegisteredClaimNames.Sub,  Guid.NewGuid().ToString()),
            new(AbpClaimTypes.Name, HardcodedUsername)
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
