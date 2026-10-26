using {{Namespace}};
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
namespace DedsiNative.Endpoints.{{Name}}Endpoints;

{{CreateRequestContract}}
{{CreateValidator}}
{{CreateResponseContract}}
/// <summary>
/// 创建并保存完整聚合。
/// </summary>
public sealed class Create{{Name}}Endpoint(I{{Name}}Repository repository)
    : Endpoint<Create{{Name}}Request, Create{{Name}}Response>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Post("{{Route}}/create");
{{Authorization}}
        Description(x => x.WithTags("{{Name}}"));
        Summary(x => x.Summary = "创建{{Name}}");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(Create{{Name}}Request req, CancellationToken ct)
    {
        var id = {{NewId}};
        var entity = {{Name}}EndpointMappings.ToDomain(req, id);
        await repository.InsertAsync(entity, true, ct);
        await Send.OkAsync(new Create{{Name}}Response(id), ct);
    }
}
