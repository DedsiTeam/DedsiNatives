using DedsiNative.Dictionaries;
using DedsiNative.Systems;
using FastEndpoints;
using DictionaryAggregate = DedsiNative.Dictionaries.Dictionary;

namespace DedsiNative.Endpoints.DictionaryEndpoints;

/// <summary>
/// 创建字典分组请求。
/// </summary>
/// <param name="SystemId">所属系统标识。</param>
/// <param name="Name">字典分组名称。</param>
public sealed record CreateDictionaryRequest(string SystemId, string Name);

/// <summary>
/// 创建字典分组响应。
/// </summary>
/// <param name="Id">新字典分组标识。</param>
public sealed record CreateDictionaryResponse(string Id);

/// <summary>
/// 创建字典分组端点。
/// </summary>
/// <param name="dictionaryRepository">字典聚合仓储。</param>
/// <param name="systemRepository">系统聚合仓储。</param>
public sealed class CreateDictionaryEndpoint(
    IDictionaryRepository dictionaryRepository,
    ISystemRepository systemRepository)
    : Endpoint<CreateDictionaryRequest, CreateDictionaryResponse>
{
    /// <summary>
    /// 配置创建字典分组接口。
    /// </summary>
    public override void Configure()
    {
        Post("/api/dictionary/create");
        Policies(ManagementPermissions.Dictionaries.Create);
        Description(description => description.WithTags("字典管理"));
        Summary(summary =>
        {
            summary.Summary = "创建字典分组";
            summary.Description = "在指定系统下创建字典分组，并保存系统名称快照。";
        });
    }

    /// <summary>
    /// 校验系统存在后创建并持久化字典聚合。
    /// </summary>
    /// <param name="req">创建请求。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(CreateDictionaryRequest req, CancellationToken ct)
    {
        var system = await systemRepository.GetAsync(req.SystemId, true, ct);
        var id = Ulid.NewUlid().ToString();
        var dictionary = new DictionaryAggregate(id, system.Id, system.Name, req.Name);

        await dictionaryRepository.InsertAsync(dictionary, true, ct);
        await Send.OkAsync(new CreateDictionaryResponse(id), ct);
    }
}
