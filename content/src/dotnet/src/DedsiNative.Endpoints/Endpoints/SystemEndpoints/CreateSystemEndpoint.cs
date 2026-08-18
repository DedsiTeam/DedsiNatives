using DedsiNative.Systems;
using FastEndpoints;
using SystemEntity = DedsiNative.Systems.System;

namespace DedsiNative.Endpoints.SystemEndpoints;

/// <summary>创建系统的请求参数。</summary>
/// <param name="Name">系统名称，不能为空。</param>
/// <param name="Description">系统说明，可为空。</param>
/// <param name="Sort">展示排序，数值越小越靠前。</param>
public sealed record CreateSystemRequest(string Name, string? Description, int Sort);

/// <summary>创建系统的响应。</summary>
/// <param name="Id">新系统的 26 位 ULID 标识。</param>
public sealed record CreateSystemResponse(string Id);

/// <summary>创建系统端点，负责创建系统聚合并持久化。</summary>
/// <param name="systemRepository">系统聚合仓储。</param>
public sealed class CreateSystemEndpoint(ISystemRepository systemRepository)
    : Endpoint<CreateSystemRequest, CreateSystemResponse>
{
    /// <summary>配置创建系统接口的路由和 HTTP 方法。</summary>
    public override void Configure()
    {
        Post("/api/system/create");
        Description(x => x.WithTags("系统管理"));
        Summary(s =>
        {
            s.Summary = "创建系统";
            s.Description = "创建系统并返回服务端生成的系统标识。";
        });
    }

    /// <summary>创建系统并返回服务端生成的标识。</summary>
    /// <param name="req">创建系统请求。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(CreateSystemRequest req, CancellationToken ct)
    {
        var id = Ulid.NewUlid().ToString();
        var system = new SystemEntity(id, req.Name, req.Description, req.Sort);

        await systemRepository.InsertAsync(system, true, ct);
        await Send.OkAsync(new CreateSystemResponse(id), ct);
    }
}
