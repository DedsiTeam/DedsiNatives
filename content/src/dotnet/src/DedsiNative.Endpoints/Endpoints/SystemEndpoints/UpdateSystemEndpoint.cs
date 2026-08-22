using DedsiNative.Systems;
using FastEndpoints;

namespace DedsiNative.Endpoints.SystemEndpoints;

/// <summary>更新系统的请求参数。</summary>
/// <param name="Name">新的系统名称。</param>
/// <param name="Description">新的系统说明。</param>
/// <param name="Sort">新的展示排序。</param>
public sealed record UpdateSystemRequest(string Name, string? Description, int Sort);

/// <summary>更新系统端点，通过领域方法修改系统聚合。</summary>
/// <param name="systemRepository">系统聚合仓储。</param>
public sealed class UpdateSystemEndpoint(ISystemRepository systemRepository)
    : Endpoint<UpdateSystemRequest, bool>
{
    /// <summary>配置更新系统接口的路由和 HTTP 方法。</summary>
    public override void Configure()
    {
        Post("/api/system/update/{id}");
        Policies(ManagementPermissions.Systems.Update);
        Description(x => x.WithTags("系统管理"));
        Summary(s =>
        {
            s.Summary = "更新系统";
            s.Description = "修改系统名称、说明和排序信息。";
        });
    }

    /// <summary>加载系统聚合、执行领域变更并持久化。</summary>
    /// <param name="req">更新系统请求。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(UpdateSystemRequest req, CancellationToken ct)
    {
        var id = Route<string>("id")!;
        var system = await systemRepository.GetAsync(id, true, ct);

        system
            .ChangeName(req.Name)
            .ChangeDescription(req.Description)
            .ChangeSort(req.Sort);

        await systemRepository.UpdateAsync(system, true, ct);
        await Send.OkAsync(true, ct);
    }
}
