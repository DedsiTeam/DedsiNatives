using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace DedsiNative.WorkItemListener;

/// <summary>
/// 通过 Azure DevOps REST API 提供队列查询、原子状态转换和 Pull Request 操作。
/// </summary>
public sealed class AzureDevOpsClient
{
    private static readonly string[] WorkItemFields =
    {
        "System.Id",
        "System.Rev",
        "System.Title",
        "System.Description",
        "System.State",
        "System.Tags",
        "Microsoft.VSTS.Common.AcceptanceCriteria",
    };

    private readonly HttpClient _httpClient;
    private readonly ListenerOptions _options;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private readonly string _projectBaseUrl;

    /// <summary>
    /// 初始化 Azure DevOps REST 客户端。
    /// </summary>
    /// <param name="httpClient">
    /// 共享 HTTP 客户端。
    /// </param>
    /// <param name="options">
    /// 监听器配置。
    /// </param>
    public AzureDevOpsClient(HttpClient httpClient, ListenerOptions options)
    {
        _httpClient = httpClient;
        _options = options;
        _projectBaseUrl = $"https://dev.azure.com/{Uri.EscapeDataString(options.Organization)}/{Uri.EscapeDataString(options.Project)}";

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = CreateAuthorization(options);
    }

    /// <summary>
    /// 查询所有需要监听的工作项快照。
    /// </summary>
    /// <param name="cancellationToken">
    /// 取消令牌。
    /// </param>
    /// <returns>
    /// 按优先级和 ID 排序的工作项数组。
    /// </returns>
    public async Task<WorkItemSnapshot[]> GetQueueAsync(CancellationToken cancellationToken)
    {
        const string query = """
            SELECT [System.Id]
            FROM WorkItems
            WHERE [System.TeamProject] = @project
              AND [System.Tags] CONTAINS 'copilot-loop'
              AND (
                    [System.Tags] CONTAINS 'copilot-ready'
                 OR [System.Tags] CONTAINS 'copilot-failed'
                 OR [System.Tags] CONTAINS 'copilot-in-progress'
                 OR [System.Tags] CONTAINS 'copilot-pr'
              )
            ORDER BY [Microsoft.VSTS.Common.Priority], [System.Id]
            """;

        using var wiqlResponse = await _httpClient.PostAsJsonAsync(
            $"{_projectBaseUrl}/_apis/wit/wiql?api-version=7.1",
            new { query },
            _jsonOptions,
            cancellationToken);
        using var wiqlJson = await ReadSuccessJsonAsync(wiqlResponse, cancellationToken);
        var ids = wiqlJson.RootElement.GetProperty("workItems")
            .EnumerateArray()
            .Select(item => item.GetProperty("id").GetInt32())
            .Take(200)
            .ToArray();

        if (ids.Length == 0)
        {
            return Array.Empty<WorkItemSnapshot>();
        }

        using var batchResponse = await _httpClient.PostAsJsonAsync(
            $"{_projectBaseUrl}/_apis/wit/workitemsbatch?api-version=7.1",
            new
            {
                ids,
                fields = WorkItemFields,
                errorPolicy = "Omit",
            },
            _jsonOptions,
            cancellationToken);
        using var batchJson = await ReadSuccessJsonAsync(batchResponse, cancellationToken);
        return batchJson.RootElement.GetProperty("value")
            .EnumerateArray()
            .Select(ParseWorkItem)
            .ToArray();
    }

    /// <summary>
    /// 读取单个工作项的最新快照。
    /// </summary>
    /// <param name="id">
    /// 工作项 ID。
    /// </param>
    /// <param name="cancellationToken">
    /// 取消令牌。
    /// </param>
    /// <returns>
    /// 最新工作项快照。
    /// </returns>
    public async Task<WorkItemSnapshot> GetWorkItemAsync(int id, CancellationToken cancellationToken)
    {
        var fields = string.Join(',', WorkItemFields.Select(Uri.EscapeDataString));
        using var response = await _httpClient.GetAsync(
            $"{_projectBaseUrl}/_apis/wit/workitems/{id}?fields={fields}&api-version=7.1",
            cancellationToken);
        using var json = await ReadSuccessJsonAsync(response, cancellationToken);
        return ParseWorkItem(json.RootElement);
    }

    /// <summary>
    /// 使用 revision test 原子领取工作项。
    /// </summary>
    /// <param name="workItem">
    /// 查询时的工作项快照。
    /// </param>
    /// <param name="runId">
    /// 本次运行身份。
    /// </param>
    /// <param name="attempt">
    /// 新的尝试次数。
    /// </param>
    /// <param name="leaseUntil">
    /// 领取租约到期时间。
    /// </param>
    /// <param name="cancellationToken">
    /// 取消令牌。
    /// </param>
    /// <returns>
    /// 成功时返回领取后的快照；并发冲突时返回 null。
    /// </returns>
    public async Task<ClaimedWorkItem?> TryClaimAsync(
        WorkItemSnapshot workItem,
        string runId,
        int attempt,
        DateTimeOffset leaseUntil,
        CancellationToken cancellationToken)
    {
        var tags = workItem.Tags.Claim(runId, attempt, leaseUntil);
        var updated = await TryUpdateTagsAsync(
            workItem,
            tags,
            $"WorkItemListener claimed WI #{workItem.Id}; run={runId}; attempt={attempt}; leaseUntil={leaseUntil:O}.",
            cancellationToken);
        return updated is null ? null : new ClaimedWorkItem(updated, runId, attempt, leaseUntil);
    }

    /// <summary>
    /// 为属于指定 run 的工作项续租。
    /// </summary>
    /// <param name="id">
    /// 工作项 ID。
    /// </param>
    /// <param name="runId">
    /// 当前运行身份。
    /// </param>
    /// <param name="leaseUntil">
    /// 新租约到期时间。
    /// </param>
    /// <param name="cancellationToken">
    /// 取消令牌。
    /// </param>
    /// <returns>
    /// 仍持有租约且更新成功时为 true。
    /// </returns>
    public async Task<bool> RenewLeaseAsync(
        int id,
        string runId,
        DateTimeOffset leaseUntil,
        CancellationToken cancellationToken)
    {
        var latest = await GetWorkItemAsync(id, cancellationToken);
        if (!latest.Tags.Contains("copilot-in-progress") ||
            !string.Equals(latest.Tags.GetRunId(), runId, StringComparison.OrdinalIgnoreCase) ||
            latest.Tags.GetLeaseUntil() is not { } currentLease ||
            currentLease <= DateTimeOffset.UtcNow)
        {
            return false;
        }

        var updated = await TryUpdateTagsAsync(latest, latest.Tags.RenewLease(leaseUntil), null, cancellationToken);
        return updated is not null;
    }

    /// <summary>
    /// 把当前 run 转换为 PR 等待状态。
    /// </summary>
    /// <param name="id">
    /// 工作项 ID。
    /// </param>
    /// <param name="runId">
    /// 当前运行身份。
    /// </param>
    /// <param name="pullRequest">
    /// 已创建的 PR。
    /// </param>
    /// <param name="cancellationToken">
    /// 取消令牌。
    /// </param>
    /// <returns>
    /// 转换成功时为 true。
    /// </returns>
    public Task<bool> MarkPullRequestAsync(
        int id,
        string runId,
        PullRequestSnapshot pullRequest,
        CancellationToken cancellationToken)
    {
        return TransitionOwnedAsync(
            id,
            runId,
            tags => tags.WaitingForPullRequest(pullRequest.Id),
            $"Copilot implementation pushed; PR #{pullRequest.Id}: {pullRequest.WebUrl ?? "URL unavailable"}.",
            cancellationToken);
    }

    /// <summary>
    /// 把当前 run 标记为可重试失败。
    /// </summary>
    public Task<bool> MarkFailedAsync(
        int id,
        string runId,
        string evidence,
        CancellationToken cancellationToken)
    {
        return TransitionOwnedAsync(id, runId, tags => tags.Failed(), evidence, cancellationToken);
    }

    /// <summary>
    /// 把当前 run 标记为需要人工处理的阻塞状态。
    /// </summary>
    public Task<bool> MarkBlockedAsync(
        int id,
        string runId,
        string evidence,
        CancellationToken cancellationToken)
    {
        return TransitionOwnedAsync(id, runId, tags => tags.Blocked(), evidence, cancellationToken);
    }

    /// <summary>
    /// 把已合并 PR 对应工作项转换为完成状态。
    /// </summary>
    public async Task<bool> MarkCompletedAsync(
        WorkItemSnapshot workItem,
        PullRequestSnapshot pullRequest,
        CancellationToken cancellationToken)
    {
        var updated = await TryUpdateTagsAsync(
            workItem,
            workItem.Tags.Completed(),
            $"PR #{pullRequest.Id} merged successfully. Branch policies are the Pipeline completion gate.",
            cancellationToken);
        return updated is not null;
    }

    /// <summary>
    /// 把过期租约转换为可重试失败状态。
    /// </summary>
    public async Task<bool> RecoverExpiredLeaseAsync(
        WorkItemSnapshot workItem,
        CancellationToken cancellationToken)
    {
        var updated = await TryUpdateTagsAsync(
            workItem,
            workItem.Tags.Failed(),
            "WorkItemListener recovered an expired lease. The previous worktree is preserved for evidence.",
            cancellationToken);
        return updated is not null;
    }

    /// <summary>
    /// 达到自动尝试上限后转为人工阻塞，避免失败项永久留在可重试语义中。
    /// </summary>
    public async Task<bool> MarkRetryLimitReachedAsync(
        WorkItemSnapshot workItem,
        CancellationToken cancellationToken)
    {
        var updated = await TryUpdateTagsAsync(
            workItem,
            workItem.Tags.Blocked(),
            $"Automatic retry limit reached after {workItem.Tags.GetAttempt()} attempts.",
            cancellationToken);
        return updated is not null;
    }

    /// <summary>
    /// 创建 Azure Repos Pull Request 并关联工作项。
    /// </summary>
    public async Task<PullRequestSnapshot> CreatePullRequestAsync(
        string repository,
        WorkItemSnapshot workItem,
        WorktreeContext worktree,
        CancellationToken cancellationToken)
    {
        var repositorySegment = Uri.EscapeDataString(repository);
        var workItemUrl = $"{_projectBaseUrl}/_apis/wit/workItems/{workItem.Id}";
        using var response = await _httpClient.PostAsJsonAsync(
            $"{_projectBaseUrl}/_apis/git/repositories/{repositorySegment}/pullrequests?api-version=7.1",
            new
            {
                sourceRefName = $"refs/heads/{worktree.Branch}",
                targetRefName = $"refs/heads/{_options.TargetBranch}",
                title = $"WI #{workItem.Id} - {workItem.Title}",
                description = $"Automated by DedsiNative.WorkItemListener and GitHub Copilot CLI.\n\nAB#{workItem.Id}",
                workItemRefs = new[]
                {
                    new { id = workItem.Id.ToString(), url = workItemUrl },
                },
            },
            _jsonOptions,
            cancellationToken);
        using var json = await ReadSuccessJsonAsync(response, cancellationToken);
        var pullRequest = ParsePullRequest(json.RootElement);

        if (_options.EnableAutoComplete && !string.IsNullOrWhiteSpace(pullRequest.CreatedById))
        {
            pullRequest = await EnableAutoCompleteAsync(repository, pullRequest, cancellationToken);
        }

        return pullRequest;
    }

    /// <summary>
    /// 读取 Azure Repos Pull Request 当前状态。
    /// </summary>
    public async Task<PullRequestSnapshot> GetPullRequestAsync(
        string repository,
        int pullRequestId,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            $"{_projectBaseUrl}/_apis/git/repositories/{Uri.EscapeDataString(repository)}/pullrequests/{pullRequestId}?api-version=7.1",
            cancellationToken);
        using var json = await ReadSuccessJsonAsync(response, cancellationToken);
        return ParsePullRequest(json.RootElement);
    }

    private async Task<PullRequestSnapshot> EnableAutoCompleteAsync(
        string repository,
        PullRequestSnapshot pullRequest,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"{_projectBaseUrl}/_apis/git/repositories/{Uri.EscapeDataString(repository)}/pullrequests/{pullRequest.Id}?api-version=7.1")
        {
            Content = JsonContent.Create(new
            {
                autoCompleteSetBy = new { id = pullRequest.CreatedById },
                completionOptions = new
                {
                    deleteSourceBranch = true,
                    mergeStrategy = "squash",
                    transitionWorkItems = false,
                },
            }, options: _jsonOptions),
        };
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        using var json = await ReadSuccessJsonAsync(response, cancellationToken);
        return ParsePullRequest(json.RootElement);
    }

    private async Task<bool> TransitionOwnedAsync(
        int id,
        string runId,
        Func<TagSet, TagSet> transition,
        string history,
        CancellationToken cancellationToken)
    {
        var latest = await GetWorkItemAsync(id, cancellationToken);
        if (!latest.Tags.Contains("copilot-in-progress") ||
            !string.Equals(latest.Tags.GetRunId(), runId, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return await TryUpdateTagsAsync(latest, transition(latest.Tags), history, cancellationToken) is not null;
    }

    private async Task<WorkItemSnapshot?> TryUpdateTagsAsync(
        WorkItemSnapshot expected,
        TagSet tags,
        string? history,
        CancellationToken cancellationToken)
    {
        var operations = new List<object>
        {
            new { op = "test", path = "/rev", value = expected.Revision },
            new { op = "add", path = "/fields/System.Tags", value = tags.ToString() },
        };
        if (!string.IsNullOrWhiteSpace(history))
        {
            operations.Add(new { op = "add", path = "/fields/System.History", value = Truncate(history, 3500) });
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"{_projectBaseUrl}/_apis/wit/workitems/{expected.Id}?api-version=7.1")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(operations, _jsonOptions),
                Encoding.UTF8,
                "application/json-patch+json"),
        };
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            using var json = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);
            return ParseWorkItem(json.RootElement);
        }

        // Azure DevOps 对 revision test 冲突可能返回 400、409 或 412；重读 revision 后再区分竞争与真实错误。
        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Conflict or HttpStatusCode.PreconditionFailed)
        {
            var latest = await GetWorkItemAsync(expected.Id, cancellationToken);
            if (latest.Revision != expected.Revision)
            {
                return null;
            }
        }

        await ThrowForFailureAsync(response, cancellationToken);
        return null;
    }

    private static AuthenticationHeaderValue CreateAuthorization(ListenerOptions options)
    {
        if (options.AuthenticationScheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
        {
            return new AuthenticationHeaderValue("Bearer", options.Token);
        }

        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($"listener:{options.Token}"));
        return new AuthenticationHeaderValue("Basic", encoded);
    }

    private static WorkItemSnapshot ParseWorkItem(JsonElement item)
    {
        var fields = item.GetProperty("fields");
        return new WorkItemSnapshot(
            item.GetProperty("id").GetInt32(),
            item.GetProperty("rev").GetInt32(),
            GetString(fields, "System.Title"),
            GetString(fields, "System.Description"),
            GetString(fields, "Microsoft.VSTS.Common.AcceptanceCriteria"),
            GetString(fields, "System.State"),
            TagSet.Parse(GetString(fields, "System.Tags")));
    }

    private static PullRequestSnapshot ParsePullRequest(JsonElement item)
    {
        var createdById = item.TryGetProperty("createdBy", out var createdBy) &&
                          createdBy.TryGetProperty("id", out var identityId)
            ? identityId.GetString()
            : null;
        string? webUrl = null;
        if (item.TryGetProperty("_links", out var links) &&
            links.TryGetProperty("web", out var web) &&
            web.TryGetProperty("href", out var href))
        {
            webUrl = href.GetString();
        }

        return new PullRequestSnapshot(
            item.GetProperty("pullRequestId").GetInt32(),
            GetString(item, "status"),
            createdById,
            webUrl);
    }

    private static string GetString(JsonElement parent, string propertyName)
    {
        if (!parent.TryGetProperty(propertyName, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return string.Empty;
        }

        return value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static async Task<JsonDocument> ReadSuccessJsonAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            await ThrowForFailureAsync(response, cancellationToken);
        }

        return await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);
    }

    private static async Task ThrowForFailureAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"Azure DevOps returned {(int)response.StatusCode} {response.ReasonPhrase}: {Truncate(body, 2000)}",
            null,
            response.StatusCode);
    }
}
