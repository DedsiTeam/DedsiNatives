using System.Text.Json;
using System.Text.Json.Serialization;

namespace DedsiNative.WorkItemListener;

internal sealed record ListenerOptions(
    string Organization,
    string Project,
    string AssignedTo,
    string Token,
    string AuthenticationScheme,
    TimeSpan PollInterval,
    TimeSpan RetryCooldown,
    bool RunOnce,
    bool LoopEnabled,
    string RepositoryPath,
    string CopilotCommand)
{
    public static ListenerOptions FromLocalConfiguration()
    {
        var configurationPath = Path.Combine(AppContext.BaseDirectory, "appsettings.local.json");
        if (!File.Exists(configurationPath))
        {
            throw new InvalidOperationException(
                $"缺少本地配置文件 {configurationPath}。请复制 appsettings.example.json 并填写 ADO_TOKEN。");
        }

        var json = File.ReadAllText(configurationPath);
        var configuration = JsonSerializer.Deserialize<ConfigurationFile>(json)
            ?? throw new InvalidOperationException("appsettings.local.json 内容无效。");

        var organization = GetRequired(configuration.Organization, "ADO_ORG");
        var project = GetRequired(configuration.Project, "ADO_PROJECT");
        var assignedTo = GetRequired(configuration.AssignedTo, "ADO_ASSIGNED_TO");
        var token = GetRequired(configuration.Token, "ADO_TOKEN");
        var authenticationScheme = GetOptional(configuration.AuthenticationScheme, "Basic");

        if (authenticationScheme is not ("Basic" or "Bearer"))
        {
            throw new InvalidOperationException("配置项 ADO_AUTH_SCHEME 只能是 Basic 或 Bearer。");
        }

        return new ListenerOptions(
            organization,
            project,
            assignedTo,
            token,
            authenticationScheme,
            TimeSpan.FromSeconds(GetPositiveInteger(configuration.PollSeconds, 60, "WORK_ITEM_LISTENER_POLL_SECONDS")),
            TimeSpan.FromSeconds(GetPositiveInteger(configuration.RetrySeconds, 300, "WORK_ITEM_LISTENER_RETRY_SECONDS")),
            configuration.RunOnce ?? false,
            configuration.LoopEnabled ?? true,
            Path.GetFullPath(GetOptional(configuration.RepositoryPath, Directory.GetCurrentDirectory())),
            GetOptional(configuration.CopilotCommand, "copilot"));
    }

    private static string GetRequired(string? value, string name)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"本地配置文件缺少配置项 {name}。")
            : value.Trim();
    }

    private static string GetOptional(string? value, string defaultValue)
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
    }

    private static int GetPositiveInteger(int? value, int defaultValue, string name)
    {
        if (value is null)
        {
            return defaultValue;
        }

        return value > 0
            ? value.Value
            : throw new InvalidOperationException($"配置项 {name} 必须是正整数。");
    }

    private sealed class ConfigurationFile
    {
        [JsonPropertyName("ADO_ORG")]
        public string? Organization { get; init; }

        [JsonPropertyName("ADO_PROJECT")]
        public string? Project { get; init; }

        [JsonPropertyName("ADO_ASSIGNED_TO")]
        public string? AssignedTo { get; init; }

        [JsonPropertyName("ADO_TOKEN")]
        public string? Token { get; init; }

        [JsonPropertyName("ADO_AUTH_SCHEME")]
        public string? AuthenticationScheme { get; init; }

        [JsonPropertyName("WORK_ITEM_LISTENER_POLL_SECONDS")]
        public int? PollSeconds { get; init; }

        [JsonPropertyName("WORK_ITEM_LISTENER_RETRY_SECONDS")]
        public int? RetrySeconds { get; init; }

        [JsonPropertyName("WORK_ITEM_LISTENER_RUN_ONCE")]
        public bool? RunOnce { get; init; }

        [JsonPropertyName("WORK_ITEM_LOOP_ENABLED")]
        public bool? LoopEnabled { get; init; }

        [JsonPropertyName("WORK_ITEM_LOOP_REPOSITORY_PATH")]
        public string? RepositoryPath { get; init; }

        [JsonPropertyName("WORK_ITEM_LOOP_COPILOT_COMMAND")]
        public string? CopilotCommand { get; init; }
    }
}
