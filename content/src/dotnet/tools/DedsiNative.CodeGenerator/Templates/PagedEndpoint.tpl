using {{Namespace}};
using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
namespace DedsiNative.Endpoints.{{Name}}Endpoints;

{{PagedRequestContract}}
{{PagedResponseContract}}
{{RowContract}}
/// <summary>
/// 条件分页查询接口。
/// </summary>
public sealed class Paged{{Name}}Endpoint(I{{Name}}Query query)
    : Endpoint<Paged{{Name}}Request, Paged{{Name}}Response>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Post("{{Route}}/pagedQuery");
{{Authorization}}
        Description(x => x.WithTags("{{Name}}"));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(Paged{{Name}}Request req, CancellationToken ct)
    {
        if (req.PageNumber < 1 || req.PageSize is < 1 or > 200 ||
            (long)(req.PageNumber - 1) * req.PageSize > int.MaxValue)
        {
            ThrowError("页码必须大于零，每页数量为 1 至 200，偏移量不能超过整数上限。");
            return;
        }
        var result = await query.GetPagedAsync(new {{Name}}PagedQueryInput(
{{QueryInputArguments}}), ct);
        await Send.OkAsync(new Paged{{Name}}Response(
            result.TotalCount,
            result.Items.Select(item => new {{Name}}RowResponse(
{{RowArguments}})).ToArray()), ct);
    }
}
