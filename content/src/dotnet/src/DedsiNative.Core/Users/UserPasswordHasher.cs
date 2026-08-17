using System.Security.Cryptography;

namespace DedsiNative.Users;

/// <summary>
/// 用户密码材料处理器，使用 PBKDF2-SHA512 生成和验证密码哈希。
/// </summary>
public static class UserPasswordHasher
{
    /// <summary>
    /// 随机盐字节数，在防止相同密码产生相同哈希的同时控制持久化体积。
    /// </summary>
    private const int SaltSize = 16;

    /// <summary>
    /// 派生密码哈希的字节数。
    /// </summary>
    private const int HashSize = 32;

    /// <summary>
    /// PBKDF2 迭代次数，用于提高离线穷举成本。
    /// </summary>
    private const int Iterations = 100_000;

    /// <summary>
    /// 为明文密码生成哈希及随机盐值。
    /// </summary>
    /// <param name="password">
    /// 待保护的明文密码，不能为空或纯空白字符。
    /// </param>
    /// <returns>
    /// 可安全持久化的密码哈希和盐值。
    /// </returns>
    public static (string PasswordHash, string PasswordSalt) Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA512,
            HashSize);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    /// <summary>
    /// 验证明文密码是否匹配已保存的密码材料。
    /// </summary>
    /// <param name="password">
    /// 待验证的明文密码。
    /// </param>
    /// <param name="passwordHash">
    /// 已保存的 Base64 密码哈希。
    /// </param>
    /// <param name="passwordSalt">
    /// 已保存的 Base64 密码盐值。
    /// </param>
    /// <returns>
    /// 密码材料格式有效且密码匹配时返回 <see langword="true"/>。
    /// </returns>
    public static bool Verify(
        string password,
        string passwordHash,
        string passwordSalt)
    {
        if (string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(passwordHash)
            || string.IsNullOrWhiteSpace(passwordSalt))
        {
            return false;
        }

        try
        {
            var expectedHash = Convert.FromBase64String(passwordHash);
            var salt = Convert.FromBase64String(passwordSalt);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA512,
                HashSize);

            // 使用固定时间比较，避免密码哈希比较泄露可利用的时间差异。
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
