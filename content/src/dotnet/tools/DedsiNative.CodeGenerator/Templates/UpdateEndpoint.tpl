using {{Namespace}};
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
namespace DedsiNative.Endpoints.{{Name}}Endpoints;

{{UpdateRequestContract}}
{{UpdateValidator}}
/// <summary>
/// 修改聚合字段及明细。
/// </summary>
public sealed class Update{{Name}}Endpoint(I{{Name}}Repository repository)
    : Endpoint<Update{{Name}}Request, bool>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Post("{{Route}}/update/{id}");
{{Authorization}}
        Description(x => x.WithTags("{{Name}}"));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(Update{{Name}}Request req, CancellationToken ct)
    {
        var id = Route<{{KeyType}}>("id")!;
        var replacement = {{Name}}EndpointMappings.ToDomain(req, id);
        await repository.ReplaceAsync(id, entity => entity.Update({{UpdateArguments}}), ct);
        await Send.OkAsync(true, ct);
    }
}
