using DedsiNative.LoginAudits;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Xunit;

namespace DedsiNative.Core.Tests.LoginAudits;

/// <summary>
/// 登录审计聚合根的领域不变量测试。
/// </summary>
public sealed class LoginAuditTests
{
    /// <summary>
    /// 登录审计是独立实体，不应继承聚合根提供的审计与并发行为。
    /// </summary>
    [Fact]
    public void LoginAudit_Should_Directly_Inherit_Entity()
    {
        Assert.Equal(typeof(Entity<string>), typeof(LoginAudit).BaseType);
    }

    /// <summary>
    /// 成功登录应保存时间、可选用户信息，并清空失败说明。
    /// </summary>
    [Fact]
    public void Constructor_Should_Create_Successful_Audit()
    {
        var userId = Guid.NewGuid();
        var now = DateTime.Now;
        var audit = new LoginAudit(
            Ulid.NewUlid().ToString(),
            now,
            LoginResult.Success,
            LoginReason.SuccessfulAuthentication,
            "  zhangsan  ",
            "张三",
            userId,
            "127.0.0.1",
            null,
            "test-agent");

        Assert.Equal(26, audit.Id.Length);
        Assert.Equal(now, audit.LoginTimeUtc);
        Assert.Equal(LoginResult.Success, audit.Result);
        Assert.Equal(LoginReason.SuccessfulAuthentication, audit.Reason);
        Assert.Equal("zhangsan", audit.Account);
        Assert.Equal("张三", audit.UserName);
        Assert.Equal(userId, audit.UserId);
        Assert.Null(audit.FailureDescription);
    }

    /// <summary>
    /// 失败登录应保留非成功原因和安全失败说明。
    /// </summary>
    [Fact]
    public void Constructor_Should_Create_Failed_Audit_With_Failure_Description()
    {
        var audit = new LoginAudit(
            Ulid.NewUlid().ToString(),
            DateTime.Now,
            LoginResult.Failure,
            LoginReason.InvalidPassword,
            "zhangsan",
            failureDescription: "密码校验失败。");

        Assert.Equal(LoginResult.Failure, audit.Result);
        Assert.Equal(LoginReason.InvalidPassword, audit.Reason);
        Assert.Equal("密码校验失败。", audit.FailureDescription);
    }

    /// <summary>
    /// 长文本字段应安全截断而不阻断审计写入。
    /// </summary>
    [Fact]
    public void Constructor_Should_Safely_Truncate_Text_Fields()
    {
        var audit = new LoginAudit(
            Ulid.NewUlid().ToString(),
            DateTime.Now,
            LoginResult.Failure,
            LoginReason.SystemError,
            new string('a', LoginAuditConsts.MaxAccountLength + 1),
            new string('u', LoginAuditConsts.MaxUserNameLength + 1),
            clientIp: new string('i', LoginAuditConsts.MaxClientIpLength + 1),
            failureDescription: new string('f', LoginAuditConsts.MaxFailureDescriptionLength + 1),
            userAgent: new string('g', LoginAuditConsts.MaxUserAgentLength + 1));

        Assert.Equal(LoginAuditConsts.MaxAccountLength, audit.Account.Length);
        Assert.Equal(LoginAuditConsts.MaxUserNameLength, audit.UserName!.Length);
        Assert.Equal(LoginAuditConsts.MaxClientIpLength, audit.ClientIp!.Length);
        Assert.Equal(LoginAuditConsts.MaxFailureDescriptionLength, audit.FailureDescription!.Length);
        Assert.Equal(LoginAuditConsts.MaxUserAgentLength, audit.UserAgent!.Length);
    }

    /// <summary>
    /// 成功和失败结果必须使用匹配的固定原因码与失败说明边界。
    /// </summary>
    [Fact]
    public void Constructor_Should_Reject_Mismatched_Result_And_Reason()
    {
        Assert.Throws<BusinessException>(() => new LoginAudit(
            Ulid.NewUlid().ToString(),
            DateTime.Now,
            LoginResult.Success,
            LoginReason.InvalidPassword,
            "zhangsan"));

        Assert.Throws<BusinessException>(() => new LoginAudit(
            Ulid.NewUlid().ToString(),
            DateTime.Now,
            LoginResult.Failure,
            LoginReason.SuccessfulAuthentication,
            "zhangsan"));

        Assert.Throws<BusinessException>(() => new LoginAudit(
            Ulid.NewUlid().ToString(),
            DateTime.Now,
            LoginResult.Success,
            LoginReason.SuccessfulAuthentication,
            "zhangsan",
            failureDescription: "不应存在"));
    }

    /// <summary>
    /// 审计主键和登录账号必须有效。
    /// </summary>
    [Fact]
    public void Constructor_Should_Reject_Invalid_Id_Or_Blank_Account()
    {
        Assert.Throws<ArgumentException>(() => new LoginAudit(
            "invalid",
            DateTime.Now,
            LoginResult.Failure,
            LoginReason.AccountNotFound,
            "zhangsan"));

        Assert.Throws<ArgumentException>(() => new LoginAudit(
            Ulid.NewUlid().ToString(),
            DateTime.Now,
            LoginResult.Failure,
            LoginReason.AccountNotFound,
            " "));
    }
}
