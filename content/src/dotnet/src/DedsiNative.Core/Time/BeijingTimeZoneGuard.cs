namespace DedsiNative.Time;

/// <summary>
/// 保证业务服务使用北京时间墙钟，避免把其他本地时区写入无时区数据库字段。
/// </summary>
public static class BeijingTimeZoneGuard
{
    /// <summary>
    /// 将协议规定的 UTC 时间转换为供业务界面展示的北京时间。
    /// </summary>
    /// <param name="value">协议返回的 UTC 时间。</param>
    /// <returns>北京时间墙钟值；输入为空时返回空。</returns>
    public static DateTime? FromProtocolUtc(DateTime? value)
    {
        return value is null ? null : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc).ToLocalTime();
    }

    /// <summary>
    /// 在启动业务服务前验证运行环境的时区。
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// 运行环境不是北京时间时抛出。
    /// </exception>
    public static void EnsureConfigured()
    {
        var local = TimeZoneInfo.Local;
        if ((local.Id != "Asia/Shanghai" && local.Id != "China Standard Time") ||
            local.BaseUtcOffset != TimeSpan.FromHours(8))
        {
            throw new InvalidOperationException("运行环境必须配置为北京时间时区（Asia/Shanghai）。");
        }
    }
}
