using DedsiNative.Users;
using Xunit;

namespace DedsiNative.Core.Tests.Users;

/// <summary>
/// 用户密码哈希验证测试。
/// </summary>
public sealed class UserPasswordHasherTests
{
    /// <summary>
    /// 正确密码应匹配生成的哈希和盐值，错误密码不得匹配。
    /// </summary>
    [Fact]
    public void Verify_Should_Accept_Only_The_Original_Password()
    {
        var (passwordHash, passwordSalt) = UserPasswordHasher.Hash("TestPassword!123");

        Assert.True(UserPasswordHasher.Verify("TestPassword!123", passwordHash, passwordSalt));
        Assert.False(UserPasswordHasher.Verify("WrongPassword!123", passwordHash, passwordSalt));
    }
}
