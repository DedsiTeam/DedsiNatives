using DedsiNative.Systems;
using FastEndpoints;

namespace DedsiNative.Endpoints.SystemEndpoints;

/// <summary>系统详情响应。</summary>
/// <param name="Id">系统唯一标识。</param>
/// <param name="Name">系统名称。</param>
/// <param name="Description">系统说明。</param>
/// <param name="Sort">展示排序。</param>
public sealed record GetSystemResponse(string Id, string Name, string? Description, int Sort);

/// <summary>获取系统详情端点。</summary>
/// <param name="systemRepository">系统聚合仓储。</param>
public sealed class GetSystemEndpoint(ISystemRepository systemRepository)
    : EndpointWithoutRequest<GetSystemResponse>
{
    /// <summary>配置系统详情接口的路由和 HTTP 方法。</summary>
    public override void Configure()
    {
        Get("/api/system/{id}");
        Description(x => x.WithTags("系统管理"));
        Summary(s =>
        {
            s.Summary = "获取系统详情";
            s.Description = "根据系统 ID 获取系统名称、说明和排序信息。";
        });
    }

    /// <summary>通过仓储加载完整系统聚合并返回详情。</summary>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id")!;
        var system = await systemRepository.GetAsync(id, true, ct);
        await Send.OkAsync(new GetSystemResponse(system.Id, system.Name, system.Description, system.Sort), ct);
    }
}
