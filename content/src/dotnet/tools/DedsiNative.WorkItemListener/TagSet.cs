namespace DedsiNative.WorkItemListener;

/// <summary>
/// 以不区分大小写方式维护 Azure DevOps 标签，并集中保证自动化标签唯一性。
/// </summary>
public sealed class TagSet
{
    private static readonly HashSet<string> StatusTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "copilot-draft",
        "copilot-ready",
        "copilot-in-progress",
        "copilot-failed",
        "copilot-blocked",
        "copilot-pr",
        "copilot-completed",
        "copilot-cancelled",
    };

    private readonly List<string> _values;

    private TagSet(IEnumerable<string> values)
    {
        _values = values
            .Select(value => value.Trim())
            .Where(value => value.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// 当前标签数组。
    /// </summary>
    public string[] Values => _values.ToArray();

    /// <summary>
    /// 从 Azure DevOps 的分号分隔字段创建标签集合。
    /// </summary>
    /// <param name="value">
    /// System.Tags 字段值。
    /// </param>
    /// <returns>
    /// 标签集合。
    /// </returns>
    public static TagSet Parse(string? value)
    {
        return new TagSet((value ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// 判断指定标签是否存在。
    /// </summary>
    /// <param name="value">
    /// 完整标签名。
    /// </param>
    /// <returns>
    /// 标签存在时为 true。
    /// </returns>
    public bool Contains(string value)
    {
        return _values.Contains(value, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 获取指定前缀的首个标签。
    /// </summary>
    /// <param name="prefix">
    /// 不区分大小写的标签前缀。
    /// </param>
    /// <returns>
    /// 找到的标签，否则为 null。
    /// </returns>
    public string? FindByPrefix(string prefix)
    {
        return _values.FirstOrDefault(value => value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 返回当前 attempt；不存在或格式无效时返回 0。
    /// </summary>
    /// <returns>
    /// 当前自动尝试次数。
    /// </returns>
    public int GetAttempt()
    {
        var value = FindByPrefix("copilot-attempt-");
        return value is not null && int.TryParse(value["copilot-attempt-".Length..], out var attempt)
            ? attempt
            : 0;
    }

    /// <summary>
    /// 返回租约到期时间；不存在或格式无效时返回 null。
    /// </summary>
    /// <returns>
    /// UTC 租约到期时间。
    /// </returns>
    public DateTimeOffset? GetLeaseUntil()
    {
        var value = FindByPrefix("copilot-lease-");
        if (value is null || !long.TryParse(value["copilot-lease-".Length..], out var unixSeconds))
        {
            return null;
        }

        return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
    }

    /// <summary>
    /// 返回当前 run ID。
    /// </summary>
    /// <returns>
    /// run ID，不存在时为 null。
    /// </returns>
    public string? GetRunId()
    {
        var value = FindByPrefix("copilot-run-");
        return value?["copilot-run-".Length..];
    }

    /// <summary>
    /// 返回当前 PR ID。
    /// </summary>
    /// <returns>
    /// PR ID，不存在或格式无效时为 null。
    /// </returns>
    public int? GetPullRequestId()
    {
        var value = FindByPrefix("copilot-pull-request-");
        return value is not null && int.TryParse(value["copilot-pull-request-".Length..], out var id)
            ? id
            : null;
    }

    /// <summary>
    /// 创建领取后的唯一状态、阶段、attempt、run 与租约标签。
    /// </summary>
    /// <param name="runId">
    /// 本次运行身份。
    /// </param>
    /// <param name="attempt">
    /// 当前尝试次数。
    /// </param>
    /// <param name="leaseUntil">
    /// 租约到期时间。
    /// </param>
    /// <returns>
    /// 新标签集合。
    /// </returns>
    public TagSet Claim(string runId, int attempt, DateTimeOffset leaseUntil)
    {
        return ReplaceAutomationTags(
            "copilot-in-progress",
            "copilot-stage-implementing",
            $"copilot-attempt-{attempt}",
            $"copilot-run-{runId}",
            $"copilot-lease-{leaseUntil.ToUnixTimeSeconds()}");
    }

    /// <summary>
    /// 延长当前租约，并保持其他标签不变。
    /// </summary>
    /// <param name="leaseUntil">
    /// 新的租约到期时间。
    /// </param>
    /// <returns>
    /// 新标签集合。
    /// </returns>
    public TagSet RenewLease(DateTimeOffset leaseUntil)
    {
        var values = WithoutPrefixes(_values, "copilot-lease-");
        values.Add($"copilot-lease-{leaseUntil.ToUnixTimeSeconds()}");
        return new TagSet(values);
    }

    /// <summary>
    /// 创建等待 PR 合并的状态标签。
    /// </summary>
    /// <param name="pullRequestId">
    /// Azure Repos PR ID。
    /// </param>
    /// <returns>
    /// 新标签集合。
    /// </returns>
    public TagSet WaitingForPullRequest(int pullRequestId)
    {
        return ReplaceAutomationTags(
            "copilot-pr",
            "copilot-stage-integrating",
            FindByPrefix("copilot-attempt-"),
            FindByPrefix("copilot-run-"),
            $"copilot-pull-request-{pullRequestId}");
    }

    /// <summary>
    /// 创建终态标签，并移除租约、run 和 PR 控制标签。
    /// </summary>
    /// <param name="status">
    /// 完整的 copilot 状态标签。
    /// </param>
    /// <param name="stage">
    /// 完整的 copilot 阶段标签。
    /// </param>
    /// <param name="keepRunMetadata">
    /// 是否保留 attempt、run 和 PR 标签供恢复或审计。
    /// </param>
    /// <returns>
    /// 新标签集合。
    /// </returns>
    public TagSet Terminal(string status, string stage, bool keepRunMetadata)
    {
        return ReplaceAutomationTags(
            status,
            stage,
            keepRunMetadata ? FindByPrefix("copilot-attempt-") : null,
            keepRunMetadata ? FindByPrefix("copilot-run-") : null,
            keepRunMetadata ? FindByPrefix("copilot-pull-request-") : null);
    }

    /// <summary>
    /// 转换回可重试失败状态并保留 attempt 和 run 证据。
    /// </summary>
    /// <returns>
    /// 新标签集合。
    /// </returns>
    public TagSet Failed()
    {
        return Terminal("copilot-failed", "copilot-stage-backlog", true);
    }

    /// <summary>
    /// 转换为人工阻塞状态并保留运行证据。
    /// </summary>
    /// <returns>
    /// 新标签集合。
    /// </returns>
    public TagSet Blocked()
    {
        return Terminal("copilot-blocked", "copilot-stage-backlog", true);
    }

    /// <summary>
    /// 转换为完成状态。
    /// </summary>
    /// <returns>
    /// 新标签集合。
    /// </returns>
    public TagSet Completed()
    {
        return Terminal("copilot-completed", "copilot-stage-done", false);
    }

    /// <summary>
    /// 序列化为 Azure DevOps System.Tags 字段格式。
    /// </summary>
    /// <returns>
    /// 分号分隔标签。
    /// </returns>
    public override string ToString()
    {
        return string.Join("; ", _values);
    }

    private TagSet ReplaceAutomationTags(
        string status,
        string stage,
        string? attempt,
        string? run,
        string? finalControlTag)
    {
        var values = _values
            .Where(value => !StatusTags.Contains(value))
            .Where(value => !value.StartsWith("copilot-stage-", StringComparison.OrdinalIgnoreCase))
            .Where(value => !value.StartsWith("copilot-attempt-", StringComparison.OrdinalIgnoreCase))
            .Where(value => !value.StartsWith("copilot-run-", StringComparison.OrdinalIgnoreCase))
            .Where(value => !value.StartsWith("copilot-lease-", StringComparison.OrdinalIgnoreCase))
            .Where(value => !value.StartsWith("copilot-pull-request-", StringComparison.OrdinalIgnoreCase))
            .ToList();

        values.Add(status);
        values.Add(stage);
        AddIfPresent(values, attempt);
        AddIfPresent(values, run);
        AddIfPresent(values, finalControlTag);
        return new TagSet(values);
    }

    private static List<string> WithoutPrefixes(IEnumerable<string> values, params string[] prefixes)
    {
        return values
            .Where(value => prefixes.All(prefix => !value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    private static void AddIfPresent(ICollection<string> values, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            values.Add(value);
        }
    }
}
