using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DedsiNative.LoginAudits;
using DedsiNative.Permissions;
using DedsiNative.Positions;
using DedsiNative.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;
using Volo.Abp.Uow;

namespace DedsiNative.Endpoints.AuthEndpoints;

/// <summary>
/// 用户登录与凭据签发统一服务接口。
/// </summary>
public interface IUserLoginService
{
    /// <summary>
    /// 为已认证的用户执行登录后持久化操作（记录登录时间、写入审计日志）并签发标准 JWT Token 与完整用户响应模型。
    /// </summary>
    Task<LoginResponse> GenerateLoginResponseAsync(
        User user,
        string? clientIp,
        string? userAgent,
        string loginMethodDescription,
        CancellationToken cancellationToken);
}

/// <summary>
/// 用户登录与凭据签发统一服务实现。
/// </summary>
public sealed class UserLoginService(
    IConfiguration configuration,
    IUserRepository userRepository,
    IPositionRepository positionRepository,
    IPermissionQuery permissionQuery,
    ILoginAuditRepository loginAuditRepository,
    IUnitOfWorkManager unitOfWorkManager,
    ILogger<UserLoginService> logger)
    : IUserLoginService, ITransientDependency
{
    public async Task<LoginResponse> GenerateLoginResponseAsync(
        User user,
        string? clientIp,
        string? userAgent,
        string loginMethodDescription,
        CancellationToken cancellationToken)
    {
        var account = user.LoginInfo?.Account ?? user.Name;

        // 1. 独立工作单元：更新用户最后登录时间与持久化成功审计记录
        try
        {
            using var unitOfWork = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
            var persistentUser = await userRepository.GetAsync(user.Id, true, cancellationToken);
            persistentUser.RecordLogin(DateTime.Now, clientIp);
            await userRepository.UpdateAsync(persistentUser, true, cancellationToken);

            var audit = new LoginAudit(
                Ulid.NewUlid().ToString(),
                DateTime.Now,
                LoginResult.Success,
                LoginReason.SuccessfulAuthentication,
                account,
                persistentUser.Name,
                persistentUser.Id,
                clientIp,
                null,
                userAgent);

            await loginAuditRepository.InsertAsync(audit, true, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "登录成功状态或审计记录持久化失败。");
        }

        // 2. 统一解析用户有效岗位与细粒度权限列表
        var (positions, permissionNames) = await GetUserPositionsAndPermissionsAsync(user, cancellationToken);

        // 3. 统一签发系统 JWT 访问令牌
        var jwtSettings = GetJwtSettings();
        var (token, expiresAt) = CreateToken(user, permissionNames, jwtSettings);

        return new LoginResponse(
            token,
            expiresAt,
            new LoginUserResponse(
                user.Id,
                user.Name,
                user.Email,
                account,
                permissionNames,
                positions));
    }

    private async Task<(LoginUserPositionResponse[] Positions, string[] PermissionNames)> GetUserPositionsAndPermissionsAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var activePositions = new List<Position>();
        foreach (var userPosition in user.Positions)
        {
            var position = await positionRepository.GetAsync(
                userPosition.PositionId,
                true,
                cancellationToken);
            if (!position.IsEnabled)
            {
                continue;
            }

            activePositions.Add(position);
        }

        var allPositionPermissions = activePositions
            .SelectMany(p => p.Permissions)
            .ToList();

        var enabledPermissions = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var systemId in allPositionPermissions
                     .Select(permission => permission.SystemId)
                     .Distinct(StringComparer.Ordinal))
        {
            var queryResult = await permissionQuery.GetPagedAsync(
                new PermissionPagedQuery(systemId, null, true, 0, 1, true),
                cancellationToken);
            foreach (var permission in queryResult.Items)
            {
                enabledPermissions[permission.Id] = permission.Name;
            }
        }

        var userPositions = activePositions
            .Select(position => new LoginUserPositionResponse(
                position.Id,
                position.Name))
            .ToArray();

        var permissionNames = allPositionPermissions
            .Where(permission => enabledPermissions.ContainsKey(permission.PermissionId))
            .Select(permission => enabledPermissions[permission.PermissionId])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(permissionName => permissionName, StringComparer.Ordinal)
            .ToArray();

        return (userPositions, permissionNames);
    }

    private (string Token, DateTime ExpiresAt) CreateToken(
        User user,
        string[] permissionNames,
        JwtSettings jwtSettings)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(AbpClaimTypes.Name, user.LoginInfo?.Account ?? user.Name)
        };
        claims.AddRange(permissionNames.Select(permissionName =>
            new Claim(OpenIddict.OpenIddictPermissions.ClaimType, permissionName)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private JwtSettings GetJwtSettings()
    {
        var jwtSection = configuration.GetSection("Jwt");
        var secret = jwtSection["Secret"];
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var expirationMinutes = jwtSection.GetValue<int>("ExpirationMinutes");
        if (string.IsNullOrWhiteSpace(secret)
            || string.IsNullOrWhiteSpace(issuer)
            || string.IsNullOrWhiteSpace(audience)
            || expirationMinutes <= 0)
        {
            throw new InvalidOperationException("JWT 配置不完整或无效。");
        }

        return new JwtSettings(secret, issuer, audience, expirationMinutes);
    }

    private sealed record JwtSettings(
        string Secret,
        string Issuer,
        string Audience,
        int ExpirationMinutes);
}
