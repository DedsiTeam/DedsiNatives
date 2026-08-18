using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using FastEndpoints;
using Microsoft.Extensions.Primitives;

namespace DedsiNative.Serialization;

/// <summary>
/// API 时间格式的全局配置。
/// </summary>
public static class ApiDateTimeConfiguration
{
    /// <summary>
    /// API 中 <see cref="DateTime"/> 的固定传输格式。
    /// </summary>
    public const string Format = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// 配置 FastEndpoints 的 JSON 序列化和非 JSON 时间绑定规则。
    /// </summary>
    /// <param name="options">FastEndpoints 全局配置。</param>
    public static void Configure(Config options)
    {
        options.Serializer.Options.Converters.Add(new ApiDateTimeJsonConverter());
        options.Binding.ValueParserFor<DateTime>(ParseNonJsonValue);
    }

    /// <summary>
    /// 解析 Query、Route、Form 和 Header 中的时间参数。
    /// </summary>
    /// <param name="value">待解析的原始参数值。</param>
    /// <returns>包含 UTC 时间或解析失败状态的结果。</returns>
    public static ParseResult ParseNonJsonValue(StringValues value)
    {
        var success = TryParseUtc(value.ToString(), out var result);

        return new ParseResult(success, result);
    }

    /// <summary>
    /// 按 API 固定格式解析 UTC 时间。
    /// </summary>
    /// <param name="value">待解析的时间文本。</param>
    /// <param name="result">解析成功后的 UTC 时间。</param>
    /// <returns>格式正确时为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public static bool TryParseUtc(string? value, out DateTime result)
    {
        return DateTime.TryParseExact(
            value,
            Format,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out result);
    }

    /// <summary>
    /// 将时间转换为 UTC；未指定时区的值按 UTC 墙上时间解释。
    /// </summary>
    /// <param name="value">待规范化的时间。</param>
    /// <returns>UTC 时间。</returns>
    public static DateTime ToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            _ => value
        };
    }
}

/// <summary>
/// 将 API JSON 中的 <see cref="DateTime"/> 严格转换为固定 UTC 文本格式的转换器。
/// </summary>
public sealed class ApiDateTimeJsonConverter : JsonConverter<DateTime>
{
    /// <summary>
    /// 从 JSON 字符串读取固定格式的 UTC 时间。
    /// </summary>
    /// <param name="reader">JSON 读取器。</param>
    /// <param name="typeToConvert">要转换的目标类型。</param>
    /// <param name="options">JSON 序列化选项。</param>
    /// <returns>解析后的 UTC 时间。</returns>
    /// <exception cref="JsonException">JSON 令牌不是字符串或时间格式不符合约定时抛出。</exception>
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String ||
            !ApiDateTimeConfiguration.TryParseUtc(reader.GetString(), out var value))
        {
            throw new JsonException(
                $"时间格式必须为 {ApiDateTimeConfiguration.Format}，且按 UTC 解释。");
        }

        return value;
    }

    /// <summary>
    /// 将时间以固定格式写入 JSON，并确保输出值为 UTC。
    /// </summary>
    /// <param name="writer">JSON 写入器。</param>
    /// <param name="value">待写入的时间。</param>
    /// <param name="options">JSON 序列化选项。</param>
    public override void Write(
        Utf8JsonWriter writer,
        DateTime value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(
            ApiDateTimeConfiguration.ToUtc(value)
                .ToString(ApiDateTimeConfiguration.Format, CultureInfo.InvariantCulture));
    }
}
