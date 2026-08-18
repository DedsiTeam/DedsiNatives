using DedsiNative.Menus;
using FastEndpoints;

namespace DedsiNative.Endpoints.MenuEndpoints;

/// <summary>
/// 获取指定系统全部菜单选项端点。
/// </summary>
/// <param name="menuQuery">菜单只读查询服务。</param>
public sealed class GetAllMenuEndpoint(IMenuQuery menuQuery)
    : EndpointWithoutRequest<IReadOnlyList<MenuQueryItem>>
{
    /// <summary>
    /// 配置按系统获取菜单选项接口。
    /// </summary>
    public override void Configure()
    {
        Get("/api/menu/getAll/{systemId}");
    }

    /// <summary>
    /// 返回指定系统全部扁平菜单选项，不构建树形结构。
    /// </summary>
    /// <param name="ct">用于取消异步操作的令牌。</param>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var systemId = Route<string>("systemId")!;
        var result = await menuQuery.GetPagedAsync(
            new MenuPagedQuery(systemId, null, null, null, null, null, null, null, 0, 0, true),
            ct);
        await Send.OkAsync(result.Items, ct);
    }
}
