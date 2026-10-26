using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace DedsiNative.LoginAudits;

/// <summary>
/// 登录审计实体，每个实例仅代表一次不可变的账号密码登录尝试。
/// </summary>
public class LoginAudit : Entity<string>
{
    /// <summary>
    /// 供 ORM 框架反射创建实体的受保护构造函数。
    /// </summary>
    protected LoginAudit()
    {
    }

    /// <summary>
    /// 创建一次登录审计记录。
    /// </summary>
    /// <param name="id">本次登录审计的 26 位 ULID 标识。</param>
    /// <param name="loginTime">登录尝试发生时间（北京时间）。</param>
    /// <param name="result">认证结果。</param>
    /// <param name="reason">认证原因码。</param>
    /// <param name="account">提交并去除首尾空格后的登录账号。</param>
    /// <param name="userName">可识别用户的名称。</param>
    /// <param name="userId">可识别用户的可选标识。</param>
    /// <param name="clientIp">经可信转发链解析后的客户端 IP。</param>
    /// <param name="failureDescription">经脱敏的失败说明。</param>
    /// <param name="userAgent">请求提供的 User-Agent。</param>
    public LoginAudit(
        string id,
        DateTime loginTime,
        LoginResult result,
        LoginReason reason,
        string account,
        string? userName = null,
        Guid? userId = null,
        string? clientIp = null,
        string? failureDescription = null,
        string? userAgent = null)
        : base(id)
    {
        LoginTime = loginTime;
        ValidateResultAndReason(
            result,
            reason,
            failureDescription);
        Result = result;
        Reason = reason;
        Account = RequiredAndTruncate(
            account,
            LoginAuditConsts.MaxAccountLength,
            nameof(account));
        UserName = OptionalAndTruncate(userName, LoginAuditConsts.MaxUserNameLength);
        UserId = userId;
        ClientIp = OptionalAndTruncate(clientIp, LoginAuditConsts.MaxClientIpLength);
        FailureDescription = result == LoginResult.Success
            ? null
            : OptionalAndTruncate(failureDescription, LoginAuditConsts.MaxFailureDescriptionLength);
        UserAgent = OptionalAndTruncate(userAgent, LoginAuditConsts.MaxUserAgentLength);
    }

    /// <summary>
    /// 登录尝试发生时间。
    /// </summary>
    public DateTime LoginTime { get; private set; }

    /// <summary>
    /// 本次尝试的认证结果。
    /// </summary>
    public LoginResult Result { get; private set; }

    /// <summary>
    /// 本次尝试的固定认证原因码。
    /// </summary>
    public LoginReason Reason { get; private set; }

    /// <summary>
    /// 提交的登录账号，已去除首尾空格并安全截断。
    /// </summary>
    public string Account { get; private set; } = string.Empty;

    /// <summary>
    /// 可识别用户的名称快照。
    /// </summary>
    public string? UserName { get; private set; }

    /// <summary>
    /// 可识别用户的可选标识。
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// 经可信代理链处理后的客户端 IP。
    /// </summary>
    public string? ClientIp { get; private set; }

    /// <summary>
    /// 经脱敏后的失败说明；成功记录始终为空。
    /// </summary>
    public string? FailureDescription { get; private set; }

    /// <summary>
    /// 请求提供的 User-Agent。
    /// </summary>
    public string? UserAgent { get; private set; }


    /// <summary>
    /// 校验登录结果、原因码及失败说明之间的一致性。
    /// </summary>
    /// <param name="result">登录结果，必须与原因码一致。</param>
    /// <param name="reason">登录原因码。</param>
    /// <param name="failureDescription">失败说明；成功登录不得携带失败说明。</param>
    private static void ValidateResultAndReason(
        LoginResult result,
        LoginReason reason,
        string? failureDescription)
    {
        if (!Enum.IsDefined(result) || !Enum.IsDefined(reason))
        {
            throw new ArgumentOutOfRangeException(nameof(reason), "登录结果或原因码无效。");
        }
        if (result == LoginResult.Success && reason != LoginReason.SuccessfulAuthentication)
        {
            throw new BusinessException("LoginAudit:SuccessfulLoginMustUseSuccessfulAuthentication");
        }
        if (result == LoginResult.Failure && reason == LoginReason.SuccessfulAuthentication)
        {
            throw new BusinessException("LoginAudit:FailedLoginCannotUseSuccessfulAuthentication");
        }
        if (result == LoginResult.Success && !string.IsNullOrWhiteSpace(failureDescription))
        {
            throw new BusinessException("LoginAudit:SuccessfulLoginCannotContainFailureDescription");
        }
    }

    /// <summary>
    /// 校验必填文本，去除首尾空白后按最大长度截断。
    /// </summary>
    /// <param name="value">待处理的文本。</param>
    /// <param name="maxLength">允许保留的最大字符数。</param>
    /// <param name="parameterName">校验失败时报告的参数名。</param>
    /// <returns>去除首尾空白且不超过最大长度的非空文本。</returns>
    private static string RequiredAndTruncate(
        string value,
        int maxLength,
        string parameterName)
    {
        var normalizedValue = Check.NotNullOrWhiteSpace(value, parameterName).Trim();
        return Truncate(normalizedValue, maxLength);
    }

    /// <summary>
    /// 将空白文本转换为 null，其他文本去除首尾空白后截断。
    /// </summary>
    /// <param name="value">待处理的文本。</param>
    /// <param name="maxLength">允许保留的最大字符数。</param>
    /// <returns>归一化并截断后的文本；空白输入返回 null。</returns>
    private static string? OptionalAndTruncate(
        string? value,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        return Truncate(value.Trim(), maxLength);
    }

    /// <summary>
    /// 超过最大长度时截断文本，未超长时保留原值。
    /// </summary>
    /// <param name="value">待处理的文本。</param>
    /// <param name="maxLength">允许保留的最大字符数。</param>
    /// <returns>不超过最大长度的文本。</returns>
    private static string Truncate(
        string value,
        int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
