using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DedsiNative.LoginAudits;
using DedsiNative.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Volo.Abp.Security.Claims;
using Volo.Abp.Uow;

namespace DedsiNative.AuthServer.Pages.Account;

public class LoginModel(
    IUserRepository userRepository,
    ILoginAuditRepository loginAuditRepository,
    IUnitOfWorkManager unitOfWorkManager,
    ILogger<LoginModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "请输入登录账号")]
        [Display(Name = "账号")]
        public string Account { get; set; } = string.Empty;

        [Required(ErrorMessage = "请输入登录密码")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "记住我")]
        public bool RememberMe { get; set; } = true;
    }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        // 如果用户已登录，直接重定向
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(ReturnUrl);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var account = Input.Account?.Trim() ?? string.Empty;
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        User? user;
        try
        {
            user = await userRepository.FindByAccountAsync(account, HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "登录查询用户异常");
            ErrorMessage = "系统繁忙，请稍后重试。";
            return Page();
        }

        if (user is null || user.LoginInfo is null)
        {
            await PersistAuditAsync(LoginResult.Failure, LoginReason.AccountNotFound, account, null, clientIp, "未找到对应登录账号。", userAgent);
            ErrorMessage = "账号或密码错误。";
            return Page();
        }

        if (user.SoftDeletedAt is not null)
        {
            await PersistAuditAsync(LoginResult.Failure, LoginReason.UserSoftDeleted, account, user, clientIp, "用户已被删除。", userAgent);
            ErrorMessage = "账户已被删除。";
            return Page();
        }

        if (user.LoginInfo.Status != AccountStatus.Normal)
        {
            var (reason, message) = user.LoginInfo.Status switch
            {
                AccountStatus.Disabled => (LoginReason.AccountDisabled, "账户已被禁用。"),
                AccountStatus.Locked => (LoginReason.AccountLocked, "账户已被锁定。"),
                AccountStatus.Cancelled => (LoginReason.AccountCancelled, "账户已被注销。"),
                _ => (LoginReason.SystemError, "账户当前不可登录。")
            };
            await PersistAuditAsync(LoginResult.Failure, reason, account, user, clientIp, message, userAgent);
            ErrorMessage = message;
            return Page();
        }

        if (!UserPasswordHasher.Verify(Input.Password, user.LoginInfo.PasswordHash, user.LoginInfo.PasswordSalt))
        {
            await PersistAuditAsync(LoginResult.Failure, LoginReason.InvalidPassword, account, user, clientIp, "密码校验失败。", userAgent);
            ErrorMessage = "账号或密码错误。";
            return Page();
        }

        // 记录登录成功审计及最后登录信息
        try
        {
            using var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
            user.RecordLogin(DateTime.Now, clientIp);
            await userRepository.UpdateAsync(user, true, HttpContext.RequestAborted);

            var audit = CreateAudit(LoginResult.Success, LoginReason.SuccessfulAuthentication, account, user, clientIp, null, userAgent);
            await loginAuditRepository.InsertAsync(audit, true, HttpContext.RequestAborted);
            await uow.CompleteAsync(HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "成功登录审计写入失败");
            ErrorMessage = "登录状态写入失败，请重试。";
            return Page();
        }

        // 创建本地 Cookie 认证 Principal
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(AbpClaimTypes.Name, user.LoginInfo.Account),
            new(ClaimTypes.Email, user.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = Input.RememberMe,
            ExpiresUtc = Input.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProperties);

        logger.LogInformation("用户 {Account} 成功登录 AuthServer", account);

        if (Url.IsLocalUrl(ReturnUrl))
        {
            return Redirect(ReturnUrl);
        }

        return Redirect("~/");
    }

    private async Task PersistAuditAsync(
        LoginResult result,
        LoginReason reason,
        string account,
        User? user,
        string? clientIp,
        string? failureDescription,
        string? userAgent)
    {
        try
        {
            var audit = CreateAudit(result, reason, account, user, clientIp, failureDescription, userAgent);
            using var uow = unitOfWorkManager.Begin(requiresNew: true, isTransactional: true);
            await loginAuditRepository.InsertAsync(audit, true, HttpContext.RequestAborted);
            await uow.CompleteAsync(HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "审计记录写入异常");
        }
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
            id: Ulid.NewUlid().ToString(),
            loginTime: DateTime.Now,
            result: result,
            reason: reason,
            account: account,
            userName: user?.Name,
            userId: user?.Id,
            clientIp: clientIp,
            failureDescription: failureDescription,
            userAgent: userAgent);
    }
}
