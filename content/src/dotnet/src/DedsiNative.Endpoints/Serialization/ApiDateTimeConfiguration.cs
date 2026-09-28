using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using FastEndpoints;
using Microsoft.Extensions.Primitives;

namespace DedsiNative.Serialization;

/// <summary>
/// API 时间格式的全局配置。
/// JSON 与 Query 中的时间字符串固定为北京时间 <c>yyyy-MM-dd HH:mm:ss.FFFFFFF</c>，
/// 解析后保持 <see cref="DateTimeKind.Unspecified"/>，不再转换为 UTC。
/// </summary>
public static class ApiDateTimeConfiguration
{
    /// <summary>
    /// API 中 <see cref="DateTime"/> 的固定传输格式。
    /// </summary>
    public const string Format = "yyyy-MM-dd HH:mm:ss.FFFFFFF";

    /// <summary>
    /// 配置 FastEndpoints 的 JSON 序列化和非 JSON 时间绑定规则。
    /// </summary>
    /// <param name="options">FastEndpoints 全局配置。</param>
    public static void Configure(Config options)
    {
        options.Serializer.Options.Converters.Add(new ApiDateTimeJsonConverter());
        options.Serializer.Options.Converters.Add(new ApiNullableDateTimeJsonConverter());
        options.Binding.ValueParserFor<DateTime>(ParseNonJsonDateTime);
        options.Binding.ValueParserFor<DateTime?>(ParseNonJsonNullableDateTime);
    }

    /// <summary>
    /// 解析 Query、Route、Form 和 Header 中的非空时间参数。
    /// </summary>
    /// <param name="value">待解析的原始参数值。</param>
    /// <returns>包含解析后时间或解析失败状态的结果。</returns>
    public static ParseResult ParseNonJsonDateTime(StringValues value)
    {
        var success = TryParseBeijing(value.ToString(), out var result);
        return new ParseResult(success, result);
    }

    /// <summary>
    /// 解析可空时间参数；空白值视为未提供。
    /// </summary>
    /// <param name="value">待解析的原始参数值。</param>
    /// <returns>包含解析后时间、空值或解析失败状态的结果。</returns>
    public static ParseResult ParseNonJsonNullableDateTime(StringValues value)
    {
        if (StringValues.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return new ParseResult(true, null);
        }

        return ParseNonJsonDateTime(value);
    }

    /// <summary>
    /// 按 API 固定格式解析北京时间，结果 Kind 为 <see cref="DateTimeKind.Unspecified"/>。
    /// </summary>
    /// <param name="value">待解析的时间文本（北京时间格式）。</param>
    /// <param name="result">解析成功后的北京时间。</param>
    /// <returns>格式正确时为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
    public static bool TryParseBeijing(string? value, out DateTime result)
    {
        if (DateTime.TryParseExact(
            value,
            Format,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed))
        {
            result = parsed;
            return true;
        }

        result = default;
        return false;
    }

}
/// <summary>
/// 将 API JSON 中的 <see cref="DateTime"/> 转换为固定格式北京时间文本的转换器。
/// </summary>
public sealed class ApiDateTimeJsonConverter : JsonConverter<DateTime>
{
    /// <summary>
    /// 从 JSON 字符串读取固定格式的北京时间。
    /// </summary>
    /// <param name="reader">JSON 读取器。</param>
    /// <param name="typeToConvert">要转换的目标类型。</param>
    /// <param name="options">JSON 序列化选项。</param>
    /// <returns>解析后的北京时间（Kind 为 Unspecified）。</returns>
    /// <exception cref="JsonException">JSON 令牌不是字符串或时间格式不符合约定时抛出。</exception>
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String ||
            !ApiDateTimeConfiguration.TryParseBeijing(reader.GetString(), out var value))
        {
            throw new JsonException(
                $"时间格式必须为 {ApiDateTimeConfiguration.Format}，且按北京时间解释。");
        }

        return value;
    }

    /// <summary>
    /// 将时间以固定北京时间格式写入 JSON，不再把 Unspecified 当作 UTC 转换。
    /// </summary>
    /// <param name="writer">JSON 写入器。</param>
    /// <param name="value">待写入的时间。</param>
    /// <param name="options">JSON 序列化选项。</param>
    public override void Write(
        Utf8JsonWriter writer,
        DateTime value,
        JsonSerializerOptions options)
    {
        if (value.Kind == DateTimeKind.Utc)
        {
            throw new JsonException("API 业务时间不得使用 UTC 值。");
        }

        writer.WriteStringValue(value.ToString(ApiDateTimeConfiguration.Format, CultureInfo.InvariantCulture));
    }
}

/// <summary>
/// 可空 <see cref="DateTime"/> 的北京时间 JSON 转换器。
/// </summary>
public sealed class ApiNullableDateTimeJsonConverter : JsonConverter<DateTime?>
{
    private static readonly ApiDateTimeJsonConverter Inner = new();

    /// <summary>
    /// 读取可空北京时间；JSON null 表示未提供。
    /// </summary>
    /// <param name="reader">JSON 读取器。</param>
    /// <param name="typeToConvert">要转换的目标类型。</param>
    /// <param name="options">JSON 序列化选项。</param>
    /// <returns>解析后的北京时间或 <see langword="null"/>。</returns>
    public override DateTime? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return Inner.Read(ref reader, typeof(DateTime), options);
    }

    /// <summary>
    /// 写入可空北京时间；空值输出 JSON null。
    /// </summary>
    /// <param name="writer">JSON 写入器。</param>
    /// <param name="value">待写入的时间。</param>
    /// <param name="options">JSON 序列化选项。</param>
    public override void Write(
        Utf8JsonWriter writer,
        DateTime? value,
        JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        Inner.Write(writer, value.Value, options);
    }
}
