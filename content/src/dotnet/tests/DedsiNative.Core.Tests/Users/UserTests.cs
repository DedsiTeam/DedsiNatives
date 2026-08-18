using DedsiNative.Users;
using Xunit;

namespace DedsiNative.Core.Tests.Users;

/// <summary>
/// 用户聚合根的领域规则测试。
/// </summary>
public sealed class UserTests
{
    /// <summary>
    /// 创建用户时应保存合法字段。
    /// </summary>
    [Fact]
    public void Constructor_Should_Set_Properties()
    {
        var user = new User(
            Guid.NewGuid(),
            "张三",
            "zhangsan@example.com");

        Assert.Equal("张三", user.Name);
        Assert.Equal("zhangsan@example.com", user.Email);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    /// <summary>
    /// 创建用户时应拒绝空白名称或邮箱。
    /// </summary>
    /// <param name="name">待验证的用户名称。</param>
    /// <param name="email">待验证的用户邮箱。</param>
    [Theory]
    [InlineData("", "user@example.com")]
    [InlineData(" ", "user@example.com")]
    [InlineData("张三", "")]
    [InlineData("张三", " ")]
    public void Constructor_Should_Reject_Blank_Name_Or_Email(
        string name,
        string email)
    {
        Assert.Throws<ArgumentException>(
            () => new User(
                Guid.NewGuid(),
                name,
                email));
    }

    /// <summary>
    /// 用户名称超过领域上限时应拒绝创建。
    /// </summary>
    [Fact]
    public void Constructor_Should_Reject_Name_That_Is_Too_Long()
    {
        var name = new string('A', UserConsts.MaxNameLength + 1);

        Assert.Throws<ArgumentException>(
            () => new User(
                Guid.NewGuid(),
                name,
                "user@example.com"));
    }

    /// <summary>
    /// 用户邮箱超过领域上限时应拒绝创建。
    /// </summary>
    [Fact]
    public void Constructor_Should_Reject_Email_That_Is_Too_Long()
    {
        var email = new string('a', UserConsts.MaxEmailLength + 1);

        Assert.Throws<ArgumentException>(
            () => new User(
                Guid.NewGuid(),
                "张三",
                email));
    }

    /// <summary>
    /// 修改用户信息时应应用相同的非空和长度规则。
    /// </summary>
    [Fact]
    public void Change_Methods_Should_Enforce_Domain_Rules()
    {
        var user = new User(
            Guid.NewGuid(),
            "张三",
            "zhangsan@example.com");

        Assert.Throws<ArgumentException>(() => user.ChangeName(" "));
        Assert.Throws<ArgumentException>(() => user.ChangeEmail(string.Empty));
        Assert.Throws<ArgumentException>(
            () => user.ChangeName(new string('A', UserConsts.MaxNameLength + 1)));
        Assert.Throws<ArgumentException>(
            () => user.ChangeEmail(new string('a', UserConsts.MaxEmailLength + 1)));
    }

    /// <summary>
    /// 重置密码应只替换登录信息中的密码材料。
    /// </summary>
    [Fact]
    public void ResetPassword_Should_Replace_Login_Credentials()
    {
        var user = new User(Guid.NewGuid(), "张三", "zhangsan@example.com");
        user.SetLoginInfo(new UserLoginInfo(user.Id, "zhangsan", "old-hash", "old-salt"));

        user.ResetPassword("new-hash", "new-salt");

        Assert.NotNull(user.LoginInfo);
        Assert.Equal("new-hash", user.LoginInfo.PasswordHash);
        Assert.Equal("new-salt", user.LoginInfo.PasswordSalt);
    }

    /// <summary>
    /// 未设置登录信息或已软删除的用户不能重置密码。
    /// </summary>
    [Fact]
    public void ResetPassword_Should_Reject_User_Without_Valid_Login_State()
    {
        var userWithoutLoginInfo = new User(Guid.NewGuid(), "张三", "zhangsan@example.com");
        Assert.Throws<Volo.Abp.BusinessException>(
            () => userWithoutLoginInfo.ResetPassword("new-hash", "new-salt"));

        var softDeletedUser = new User(Guid.NewGuid(), "李四", "lisi@example.com");
        softDeletedUser.SetLoginInfo(new UserLoginInfo(softDeletedUser.Id, "lisi", "old-hash", "old-salt"));
        softDeletedUser.MarkAsSoftDeleted();

        Assert.Throws<Volo.Abp.BusinessException>(
            () => softDeletedUser.ResetPassword("new-hash", "new-salt"));
    }
}
