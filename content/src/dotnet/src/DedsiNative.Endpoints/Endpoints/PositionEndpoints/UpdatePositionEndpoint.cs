using DedsiNative.Positions;
using DedsiNative.Systems;
using FastEndpoints;

namespace DedsiNative.Endpoints.PositionEndpoints;

/// <summary>更新岗位的请求参数。</summary>
/// <param name="Name">新的岗位名称。</param>
/// <param name="SystemId">新的所属系统的 26 位 ULID。</param>
/// <param name="Description">新的岗位说明，可为空。</param>
public sealed record UpdatePositionRequest(string Name, string SystemId, string? Description);

/// <summary>更新岗位端点，通过领域方法修改岗位聚合。</summary>
public sealed class UpdatePositionEndpoint(
    IPositionRepository positionRepository,
    ISystemRepository systemRepository)
    : Endpoint<UpdatePositionRequest, bool>
{
    /// <summary>配置岗位更新接口。</summary>
    public override void Configure()
    {
        Post("/api/position/update/{id}");
        Description(x => x.WithTags("岗位管理"));
        Summary(s =>
        {
            s.Summary = "更新岗位";
            s.Description = "修改岗位名称、所属系统、说明和启用状态。";
        });
    }

    /// <summary>加载岗位和系统，执行领域变更并持久化。</summary>
    public override async Task HandleAsync(UpdatePositionRequest req, CancellationToken ct)
    {
        var id = Route<string>("id")!;
        var position = await positionRepository.GetAsync(id, true, ct);
        var system = await systemRepository.GetAsync(req.SystemId, true, ct);

        position
            .ChangeName(req.Name)
            .ChangeSystem(system.Id, system.Name)
            .ChangeDescription(req.Description);

        await positionRepository.UpdateAsync(position, true, ct);
        await Send.OkAsync(true, ct);
    }
}
