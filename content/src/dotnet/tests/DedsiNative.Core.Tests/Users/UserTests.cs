using DedsiNative.Users;
using DedsiNative.Users.Events;
using Xunit;

namespace DedsiNative.Core.Tests.Users;

/// <summary>
/// 用户聚合根的领域规则测试。
/// </summary>
public sealed class UserTests
{
    /// <summary>
    /// 创建用户时应保存合法字段并登记一次创建事件。
    /// </summary>
    [Fact]
    public void Constructor_Should_Set_Properties_And_Add_Created_Event()
    {
        var user = new User(
            Ulid.NewUlid().ToString(),
            "张三",
            "zhangsan@example.com");

        Assert.Equal("张三", user.Name);
        Assert.Equal("zhangsan@example.com", user.Email);
        Assert.Equal(26, user.Id.Length);

        var eventRecord = Assert.Single(user.GetLocalEvents());
        var createdEvent = Assert.IsType<UserCreatedEvent>(eventRecord.EventData);
        Assert.Equal(user.Id, createdEvent.UserId);
        Assert.Equal(user.Name, createdEvent.Name);
        Assert.Equal(user.Email, createdEvent.Email);
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
                Ulid.NewUlid().ToString(),
                name,
                email));
    }

    /// <summary>
    /// 用户名称超过领域上限时应拒绝创建。
    /// </summary>
    [Fact]
    public void Constructor_Should_Reject_Name_That_Is_Too_Long()
    {
        var name = new string('A', User.MaxNameLength + 1);

        Assert.Throws<ArgumentException>(
            () => new User(
                Ulid.NewUlid().ToString(),
                name,
                "user@example.com"));
    }

    /// <summary>
    /// 用户邮箱超过领域上限时应拒绝创建。
    /// </summary>
    [Fact]
    public void Constructor_Should_Reject_Email_That_Is_Too_Long()
    {
        var email = new string('a', User.MaxEmailLength + 1);

        Assert.Throws<ArgumentException>(
            () => new User(
                Ulid.NewUlid().ToString(),
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
            Ulid.NewUlid().ToString(),
            "张三",
            "zhangsan@example.com");

        Assert.Throws<ArgumentException>(() => user.ChangeName(" "));
        Assert.Throws<ArgumentException>(() => user.ChangeEmail(string.Empty));
        Assert.Throws<ArgumentException>(
            () => user.ChangeName(new string('A', User.MaxNameLength + 1)));
        Assert.Throws<ArgumentException>(
            () => user.ChangeEmail(new string('a', User.MaxEmailLength + 1)));
    }
}
