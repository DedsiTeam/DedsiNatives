using System.Security.Cryptography;

namespace DedsiNative.Users;

/// <summary>
/// 用户密码材料处理器，使用 PBKDF2-SHA512 生成和验证密码哈希。
/// </summary>
public static class UserPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    /// <summary>
    /// 为明文密码生成哈希及随机盐值。
    /// </summary>
    /// <param name="password">待保护的明文密码。</param>
    /// <returns>可安全持久化的密码哈希和盐值。</returns>
    public static (string PasswordHash, string PasswordSalt) Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA512, HashSize);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    /// <summary>
    /// 验证明文密码是否匹配已保存的密码材料。
    /// </summary>
    /// <param name="password">待验证的明文密码。</param>
    /// <param name="passwordHash">已保存的密码哈希。</param>
    /// <param name="passwordSalt">已保存的密码盐值。</param>
    /// <returns>匹配时返回 <see langword="true"/>。</returns>
    public static bool Verify(string password, string passwordHash, string passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(passwordHash)
            || string.IsNullOrWhiteSpace(passwordSalt)) return false;
        try
        {
            var expectedHash = Convert.FromBase64String(passwordHash);
            var salt = Convert.FromBase64String(passwordSalt);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA512, HashSize);
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException) { return false; }
    }
}
