using DedsiNative.Positions;
using FastEndpoints;

namespace DedsiNative.Endpoints.PositionEndpoints;

/// <summary>删除岗位端点，删除前由前端要求管理员明确确认。</summary>
public sealed class DeletePositionEndpoint(IPositionRepository positionRepository)
    : EndpointWithoutRequest<bool>
{
    /// <summary>配置岗位删除接口。</summary>
    public override void Configure()
    {
        Post("/api/position/delete/{id}");
        Description(x => x.WithTags("岗位管理"));
        Summary(s =>
        {
            s.Summary = "删除岗位";
            s.Description = "根据岗位 ID 删除岗位及其权限、组织机构关联。";
        });
    }

    /// <summary>加载岗位并执行删除，子实体由级联关系一并删除。</summary>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id")!;
        var position = await positionRepository.GetAsync(id, true, ct);
        await positionRepository.DeleteAsync(position, true, ct);
        await Send.OkAsync(true, ct);
    }
}
