using DedsiNative.Systems;
using FastEndpoints;

namespace DedsiNative.Endpoints.SystemEndpoints;

/// <summary>删除系统端点，删除前由前端要求管理员明确确认。</summary>
/// <param name="systemRepository">系统聚合仓储。</param>
public sealed class DeleteSystemEndpoint(ISystemRepository systemRepository) : EndpointWithoutRequest<bool>
{
    /// <summary>配置删除系统接口的路由和 HTTP 方法。</summary>
    public override void Configure()
    {
        Post("/api/system/delete/{id}");
        Policies(ManagementPermissions.Systems.Delete);
        Description(x => x.WithTags("系统管理"));
        Summary(s =>
        {
            s.Summary = "删除系统";
            s.Description = "根据系统 ID 删除系统。";
        });
    }

    /// <summary>加载目标系统并执行删除。</summary>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id")!;
        var system = await systemRepository.GetAsync(id, true, ct);
        await systemRepository.DeleteAsync(system, true, ct);
        await Send.OkAsync(true, ct);
    }
}
