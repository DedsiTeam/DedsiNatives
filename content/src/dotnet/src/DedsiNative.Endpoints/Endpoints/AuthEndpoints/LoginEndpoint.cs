using DedsiNative.LoginAudits;
using DedsiNative.Users;
using FastEndpoints;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Uow;

namespace DedsiNative.Endpoints.AuthEndpoints;

/// <summary>
/// 登录端点，验证数据库用户账号密码、记录失败审计，并通过统一登录服务签发 JWT Token 与完整用户模型。
/// </summary>
/// <param name="userQuery">用户只读查询服务。</param>
/// <param name="userLoginService">统一用户登录服务。</param>
/// <param name="loginAuditRepository">登录审计写侧仓储。</param>
/// <param name="unitOfWorkManager">工作单元管理器。</param>
/// <param name="logger">安全日志记录器。</param>
public sealed class LoginEndpoint(
    IUserQuery userQuery,
    IUserLoginService userLoginService,
    ILoginAuditRepository loginAuditRepository,
    IUnitOfWorkManager unitOfWorkManager,
    ILogger<LoginEndpoint> logger)
    : Endpoint<LoginRequest, LoginResponse>
{
    public override void Configure()
    {
        Post("/api/auth/login");
        AllowAnonymous();
        Description(description => description.WithTags("认证管理"));
        Summary(summary =>
        {
            summary.Summary = "用户登录";
            summary.Description = "验证用户登录账号和密码，登录成功后返回访问令牌与用户权限数据。";
        });
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var account = req.Username?.Trim() ?? string.Empty;
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        User? user;

        try
        {
            user = await userQuery.FindByAccountAsync(account, ct);
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

        // 统一调用用户登录服务执行持久化和凭据签发
        var response = await userLoginService.GenerateLoginResponseAsync(
            user,
            clientIp,
            userAgent,
            "账号密码登录",
            ct);

        await Send.OkAsync(response, ct);
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
            using var unitOfWork = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
            var audit = new LoginAudit(
                Ulid.NewUlid().ToString(),
                DateTime.Now,
                LoginResult.Failure,
                reason,
                account,
                user?.Name,
                user?.Id,
                clientIp,
                safeFailureDescription,
                userAgent);

            await loginAuditRepository.InsertAsync(audit, true, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
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
}
