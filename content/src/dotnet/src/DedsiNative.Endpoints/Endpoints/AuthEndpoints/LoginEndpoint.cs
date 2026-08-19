using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DedsiNative.LoginAudits;
using DedsiNative.Permissions;
using DedsiNative.Positions;
using DedsiNative.Users;
using FastEndpoints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp;
using Volo.Abp.Security.Claims;
using Volo.Abp.Uow;

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
/// <param name="User">当前登录用户的安全基本资料。</param>
public sealed record LoginResponse(string Token, DateTime ExpiresAt, LoginUserResponse User);

/// <summary>
/// 登录响应中岗位包含的有效权限响应模型。
/// </summary>
/// <param name="PermissionId">权限唯一标识。</param>
/// <param name="PermissionName">权限名称快照。</param>
/// <param name="SystemId">权限所属系统标识。</param>
/// <param name="SystemName">权限所属系统名称快照。</param>
public sealed record LoginPositionPermissionResponse(
    string PermissionId,
    string PermissionName,
    string SystemId,
    string SystemName);

/// <summary>
/// 登录响应中用户所属岗位及其有效权限。
/// </summary>
/// <param name="PositionId">岗位唯一标识。</param>
/// <param name="PositionName">岗位名称。</param>
/// <param name="Permissions">岗位包含的有效权限列表。</param>
public sealed record LoginUserPositionResponse(
    string PositionId,
    string PositionName,
    IReadOnlyList<LoginPositionPermissionResponse> Permissions);

/// <summary>
/// 登录成功后返回的当前用户基本资料。
/// </summary>
/// <param name="Id">用户唯一标识。</param>
/// <param name="Name">用户姓名。</param>
/// <param name="Email">用户邮箱。</param>
/// <param name="Account">登录账号。</param>
/// <param name="Permissions">由用户岗位解析出的有效权限名称。</param>
/// <param name="Positions">用户所属岗位及其对应权限列表。</param>
public sealed record LoginUserResponse(
    Guid Id,
    string Name,
    string Email,
    string Account,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<LoginUserPositionResponse> Positions);

/// <summary>
/// 登录端点，验证数据库用户凭证、独立持久化登录审计并签发 JWT Token。
/// </summary>
/// <param name="configuration">JWT 配置。</param>
/// <param name="userRepository">用户聚合仓储。</param>
/// <param name="positionRepository">岗位聚合仓储，用于解析权限名称。</param>
/// <param name="permissionQuery">权限只读查询，用于过滤当前已启用的权限。</param>
/// <param name="loginAuditRepository">登录审计写侧仓储。</param>
/// <param name="unitOfWorkManager">用于创建独立审计提交单元的工作单元管理器。</param>
/// <param name="logger">不含密码和令牌的安全服务端日志。</param>
public sealed class LoginEndpoint(
    IConfiguration configuration,
    IUserRepository userRepository,
    IPositionRepository positionRepository,
    IPermissionQuery permissionQuery,
    ILoginAuditRepository loginAuditRepository,
    IUnitOfWorkManager unitOfWorkManager,
    ILogger<LoginEndpoint> logger)
    : Endpoint<LoginRequest, LoginResponse>
{
    /// <summary>
    /// 配置端点路由，允许匿名访问。
    /// </summary>
    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
        Description(description => description.WithTags("认证管理"));
        Summary(summary =>
        {
            summary.Summary = "用户登录";
            summary.Description = "验证用户登录账号和密码，登录成功后返回访问令牌。";
        });
    }

    /// <summary>
    /// 验证用户凭证，独立保存审计记录，并在成功后返回 JWT Token。
    /// </summary>
    /// <param name="req">登录请求，包含用户名和密码。</param>
    /// <param name="ct">用于取消异步操作的令牌。</param>
    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var account = req.Username?.Trim() ?? string.Empty;
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        User? user;

        try
        {
            user = await userRepository.FindByAccountAsync(account, ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            await RecordSystemFailureAndThrowAsync(
                account,
                clientIp,
                userAgent,
                exception,
                ct);
            return;
        }

        if (user is null || user.LoginInfo is null)
        {
            await RecordFailureAndThrowAsync(
                LoginReason.AccountNotFound,
                "未找到对应登录账号。",
                account,
                null,
                clientIp,
                userAgent,
                ct);
            return;
        }

        if (user.SoftDeletedAt is not null)
        {
            await RecordFailureAndThrowAsync(
                LoginReason.UserSoftDeleted,
                "用户已被删除。",
                account,
                user,
                clientIp,
                userAgent,
                ct);
            return;
        }

        var loginInfo = user.LoginInfo;
        LoginReason? statusReason = loginInfo.Status switch
        {
            AccountStatus.Normal => null,
            AccountStatus.Disabled => LoginReason.AccountDisabled,
            AccountStatus.Locked => LoginReason.AccountLocked,
            AccountStatus.Cancelled => LoginReason.AccountCancelled,
            _ => LoginReason.SystemError
        };

        if (statusReason.HasValue)
        {
            var statusMessage = loginInfo.Status switch
            {
                AccountStatus.Disabled => "账户已被禁用。",
                AccountStatus.Locked => "账户已被锁定。",
                AccountStatus.Cancelled => "账户已被注销。",
                _ => "账户当前不可登录。"
            };
            await RecordFailureAndThrowAsync(
                statusReason.Value,
                statusMessage,
                account,
                user,
                clientIp,
                userAgent,
                ct);
            return;
        }

        try
        {
            if (!UserPasswordHasher.Verify(req.Password, loginInfo.PasswordHash, loginInfo.PasswordSalt))
            {
                await RecordFailureAndThrowAsync(
                    LoginReason.InvalidPassword,
                    "密码校验失败。",
                    account,
                    user,
                    clientIp,
                    userAgent,
                    ct);
                return;
            }
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            await RecordSystemFailureAndThrowAsync(
                account,
                clientIp,
                userAgent,
                exception,
                ct,
                user);
            return;
        }

        IReadOnlyList<LoginUserPositionResponse> positions;
        IReadOnlyList<string> permissionNames;
        JwtSettings jwtSettings;
        try
        {
            (positions, permissionNames) = await GetUserPositionsAndPermissionsAsync(user, ct);
            jwtSettings = GetJwtSettings();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            await RecordSystemFailureAndThrowAsync(
                account,
                clientIp,
                userAgent,
                exception,
                ct,
                user);
            return;
        }

        try
        {
            await PersistSuccessfulLoginAsync(
                account,
                clientIp,
                userAgent,
                ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            // 成功审计未持久化时绝不签发 Token，日志中不记录账号、密码或令牌。
            logger.LogError(exception, "成功登录审计写入失败，已拒绝签发访问令牌。");
            throw new UserFriendlyException("登录状态写入失败，已拒绝签发访问令牌。");
        }

        // 只有成功审计及最后登录信息都独立提交后，才实际签发 JWT。
        var (token, expiresAt) = CreateToken(user, permissionNames, jwtSettings);

        await Send.OkAsync(new LoginResponse(
            token,
            expiresAt,
            new LoginUserResponse(
                user.Id,
                user.Name,
                user.Email,
                loginInfo.Account,
                permissionNames,
                positions)), ct);
    }

    private async Task RecordFailureAndThrowAsync(
        LoginReason reason,
        string safeFailureDescription,
        string account,
        User? user,
        string? clientIp,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        try
        {
            await PersistAuditAsync(
                LoginResult.Failure,
                reason,
                account,
                user,
                clientIp,
                safeFailureDescription,
                userAgent,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            // 失败审计写入失败不改变统一认证失败语义，也不得泄露认证材料。
            logger.LogError(exception, "失败登录审计写入失败。");
        }

        throw new UserFriendlyException(safeFailureDescription);
    }

    private async Task RecordSystemFailureAndThrowAsync(
        string account,
        string? clientIp,
        string? userAgent,
        Exception exception,
        CancellationToken cancellationToken,
        User? user = null)
    {
        logger.LogError(exception, "登录认证处理发生系统异常。");
        await RecordFailureAndThrowAsync(
            LoginReason.SystemError,
            "认证处理发生系统异常。",
            account,
            user,
            clientIp,
            userAgent,
            cancellationToken);
    }

    private async Task PersistAuditAsync(
        LoginResult result,
        LoginReason reason,
        string account,
        User? user,
        string? clientIp,
        string? failureDescription,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var audit = CreateAudit(
            result,
            reason,
            account,
            user,
            clientIp,
            failureDescription,
            userAgent);

        // 认证失败会抛异常；使用 requiresNew 保证已完成的审计不随外层工作单元回滚。
        using var unitOfWork = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        await loginAuditRepository.InsertAsync(audit, true, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    private async Task PersistSuccessfulLoginAsync(
        string account,
        string? clientIp,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        // 重新在独立工作单元读取用户，避免外层异常回滚最后登录信息或成功审计。
        using var unitOfWork = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
        var persistentUser = await userRepository.FindByAccountAsync(account, cancellationToken)
            ?? throw new InvalidOperationException("成功认证的用户在提交登录信息时不存在。");
        persistentUser.RecordLogin(DateTime.Now, clientIp);
        await userRepository.UpdateAsync(persistentUser, true, cancellationToken);

        var audit = CreateAudit(
            LoginResult.Success,
            LoginReason.SuccessfulAuthentication,
            account,
            persistentUser,
            clientIp,
            null,
            userAgent);
        await loginAuditRepository.InsertAsync(audit, true, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
    }

    private static LoginAudit CreateAudit(
        LoginResult result,
        LoginReason reason,
        string account,
        User? user,
        string? clientIp,
        string? failureDescription,
        string? userAgent)
    {
        return new LoginAudit(
            Ulid.NewUlid().ToString(),
            DateTime.Now,
            result,
            reason,
            account,
            user?.Name,
            user?.Id,
            clientIp,
            failureDescription,
            userAgent);
    }

    private async Task<(IReadOnlyList<LoginUserPositionResponse> Positions, IReadOnlyList<string> PermissionNames)> GetUserPositionsAndPermissionsAsync(
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

        var userPositions = new List<LoginUserPositionResponse>();
        foreach (var position in activePositions)
        {
            var positionPermissions = position.Permissions
                .Where(permission => enabledPermissions.ContainsKey(permission.PermissionId))
                .Select(permission => new LoginPositionPermissionResponse(
                    permission.PermissionId,
                    enabledPermissions[permission.PermissionId],
                    permission.SystemId,
                    permission.SystemName))
                .ToList();

            userPositions.Add(new LoginUserPositionResponse(
                position.Id,
                position.Name,
                positionPermissions));
        }

        var permissionNames = userPositions
            .SelectMany(p => p.Permissions)
            .Select(p => p.PermissionName)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(permissionName => permissionName, StringComparer.Ordinal)
            .ToList();

        return (userPositions, permissionNames);
    }

    private (string Token, DateTime ExpiresAt) CreateToken(
        User user,
        IReadOnlyList<string> permissionNames,
        JwtSettings jwtSettings)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(AbpClaimTypes.Name, user.LoginInfo!.Account)
        };
        claims.AddRange(permissionNames.Select(permissionName =>
            new Claim(LoginAuditPermissions.ClaimType, permissionName)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
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
