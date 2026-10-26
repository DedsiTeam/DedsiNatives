using {{Namespace}};
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
namespace DedsiNative.Endpoints.{{Name}}Endpoints;

/// <summary>
/// 查询聚合详情，包含值对象及明细数组。
/// </summary>
public sealed class Get{{Name}}Endpoint(I{{Name}}Repository repository)
    : EndpointWithoutRequest<{{Name}}Response>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Get("{{Route}}/{id}");
{{Authorization}}
        Description(x => x.WithTags("{{Name}}"));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        var entity = await repository.GetAsync(Route<{{KeyType}}>("id")!, true, ct);
        await Send.OkAsync({{Name}}EndpointMappings.ToResponse(entity), ct);
    }
}
