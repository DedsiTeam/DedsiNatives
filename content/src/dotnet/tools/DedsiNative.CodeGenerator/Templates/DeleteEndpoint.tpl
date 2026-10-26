using {{Namespace}};
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
namespace DedsiNative.Endpoints.{{Name}}Endpoints;

/// <summary>
/// 删除聚合及所属明细，外部引用仍受数据库约束保护。
/// </summary>
public sealed class Delete{{Name}}Endpoint(I{{Name}}Repository repository)
    : EndpointWithoutRequest<bool>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Post("{{Route}}/delete/{id}");
{{Authorization}}
        Description(x => x.WithTags("{{Name}}"));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        await repository.DeleteWithDetailsAsync(Route<{{KeyType}}>("id")!, ct);
        await Send.OkAsync(true, ct);
    }
}
