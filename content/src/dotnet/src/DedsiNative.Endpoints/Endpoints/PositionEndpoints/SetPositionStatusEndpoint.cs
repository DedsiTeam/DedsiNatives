using DedsiNative.Positions;
using FastEndpoints;

namespace DedsiNative.Endpoints.PositionEndpoints;

/// <summary>岗位启用状态请求。</summary>
/// <param name="IsEnabled">目标启用状态。</param>
public sealed record SetPositionStatusRequest(bool IsEnabled);

/// <summary>岗位状态端点，通过聚合行为启用或停用岗位。</summary>
public sealed class SetPositionStatusEndpoint(IPositionRepository positionRepository)
    : Endpoint<SetPositionStatusRequest, bool>
{
    /// <summary>配置岗位状态接口。</summary>
    public override void Configure()
    {
        Post("/api/position/status/{id}");
        Description(x => x.WithTags("岗位管理"));
        Summary(s =>
        {
            s.Summary = "设置岗位状态";
            s.Description = "启用或停用指定岗位。";
        });
    }

    /// <summary>执行岗位启用或停用行为。</summary>
    public override async Task HandleAsync(SetPositionStatusRequest req, CancellationToken ct)
    {
        var id = Route<string>("id")!;
        var position = await positionRepository.GetAsync(id, true, ct);
        if (req.IsEnabled) position.Enable(); else position.Disable();

        await positionRepository.UpdateAsync(position, true, ct);
        await Send.OkAsync(true, ct);
    }
}
