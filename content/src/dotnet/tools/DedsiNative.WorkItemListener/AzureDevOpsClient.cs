using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DedsiNative.WorkItemListener;

internal sealed class AzureDevOpsClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly ListenerOptions _options;

    public AzureDevOpsClient(ListenerOptions options)
    {
        _options = options;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(
                $"https://dev.azure.com/{Uri.EscapeDataString(options.Organization)}/{Uri.EscapeDataString(options.Project)}/")
        };

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = CreateAuthorizationHeader(options);
    }

    public async Task<IReadOnlyList<WorkItemCandidate>> GetCandidatesAsync(CancellationToken cancellationToken)
    {
        var query = BuildCandidateQuery(_options.AssignedTo);
        using var request = new HttpRequestMessage(HttpMethod.Post, "_apis/wit/wiql?$top=10&api-version=7.1")
        {
            Content = JsonContent.Create(new { query })
        };
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<WiqlResponse>(JsonOptions, cancellationToken);
        if (result?.WorkItems is not { Count: > 0 })
        {
            return [];
        }

        var candidates = new List<WorkItemCandidate>(result.WorkItems.Count);
        foreach (var reference in result.WorkItems)
        {
            var candidate = await GetWorkItemAsync(reference.Id, cancellationToken);
            if (candidate is not null && IsCandidate(candidate))
            {
                candidates.Add(candidate);
            }
        }

        return candidates;
    }

    public void Dispose() => _httpClient.Dispose();

    internal static string BuildCandidateQuery(string assignedTo)
    {
        var escapedAssignedTo = assignedTo.Replace("'", "''", StringComparison.Ordinal);
        return $$"""
                 SELECT [System.Id]
                 FROM WorkItems
                 WHERE [System.TeamProject] = @project
                   AND [System.AssignedTo] = '{{escapedAssignedTo}}'
                   AND [System.Tags] CONTAINS 'copilot-loop'
                   AND (
                       [System.Tags] CONTAINS 'copilot-ready'
                       OR [System.Tags] CONTAINS 'copilot-in-progress'
                       OR [System.Tags] CONTAINS 'copilot-failed'
                   )
                 ORDER BY [Microsoft.VSTS.Common.Priority] ASC, [System.Id] ASC
                 """;
    }

    private async Task<WorkItemCandidate?> GetWorkItemAsync(int id, CancellationToken cancellationToken)
    {
        var fields = string.Join(',',
            "System.Id",
            "System.Title",
            "System.State",
            "System.Tags");
        using var response = await _httpClient.GetAsync(
            $"_apis/wit/workitems/{id}?fields={Uri.EscapeDataString(fields)}&api-version=7.1",
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var workItem = await response.Content.ReadFromJsonAsync<WorkItemResponse>(JsonOptions, cancellationToken);
        if (workItem is null)
        {
            return null;
        }

        return new WorkItemCandidate(
            workItem.Id,
            workItem.Revision,
            workItem.Fields.GetValueOrDefault("System.Title") ?? string.Empty,
            workItem.Fields.GetValueOrDefault("System.State") ?? string.Empty,
            workItem.Fields.GetValueOrDefault("System.Tags") ?? string.Empty);
    }

    private static bool IsCandidate(WorkItemCandidate candidate)
    {
        var tags = candidate.Tags
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return tags.Contains("copilot-loop") &&
               (tags.Contains("copilot-ready") ||
                tags.Contains("copilot-in-progress") ||
                tags.Contains("copilot-failed"));
    }

    private static AuthenticationHeaderValue CreateAuthorizationHeader(ListenerOptions options)
    {
        if (options.AuthenticationScheme == "Bearer")
        {
            return new AuthenticationHeaderValue("Bearer", options.Token);
        }

        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($":{options.Token}"));
        return new AuthenticationHeaderValue("Basic", encoded);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (body.Length > 500)
        {
            body = body[..500];
        }

        throw new HttpRequestException(
            $"Azure DevOps 请求失败：{(int)response.StatusCode} {response.ReasonPhrase}。{body}",
            null,
            response.StatusCode);
    }

    private sealed record WiqlResponse(
        [property: JsonPropertyName("workItems")] List<WorkItemReference> WorkItems);

    private sealed record WorkItemReference(
        [property: JsonPropertyName("id")] int Id);

    private sealed record WorkItemResponse(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("rev")] int Revision,
        [property: JsonPropertyName("fields")] Dictionary<string, string> Fields);
}
