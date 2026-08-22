using DedsiNative.Systems;
using FastEndpoints;

namespace DedsiNative.Endpoints.SystemEndpoints;

/// <summary>
/// 获取全部系统选项端点。
/// </summary>
/// <param name="systemQuery">系统只读查询服务。</param>
public sealed class GetAllSystemEndpoint(ISystemQuery systemQuery)
    : EndpointWithoutRequest<SystemQueryItem[]>
{
    /// <summary>
    /// 配置获取全部系统选项接口。
    /// </summary>
    public override void Configure()
    {
        Get("/api/system/getAll");
        Policies(ManagementPermissions.Systems.View);
    }

    /// <summary>
    /// 返回按展示顺序排列的全部系统轻量选项。
    /// </summary>
    /// <param name="ct">用于取消异步操作的令牌。</param>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await systemQuery.GetPagedAsync(new SystemPagedQuery(null, 0, 0, true), ct);
        await Send.OkAsync(result.Items, ct);
    }
}
