using DedsiNative.Dictionaries;
using DedsiNative.Systems;
using FastEndpoints;

namespace DedsiNative.Endpoints.DictionaryEndpoints;

/// <summary>
/// 更新字典分组请求。
/// </summary>
/// <param name="SystemId">新的所属系统标识。</param>
/// <param name="Name">新的字典分组名称。</param>
public sealed record UpdateDictionaryRequest(string SystemId, string Name);

/// <summary>
/// 更新字典分组端点。
/// </summary>
public sealed class UpdateDictionaryEndpoint(
    IDictionaryRepository dictionaryRepository,
    ISystemRepository systemRepository)
    : Endpoint<UpdateDictionaryRequest, bool>
{
    /// <summary>
    /// 配置更新字典分组接口。
    /// </summary>
    public override void Configure()
    {
        Post("/api/dictionary/update/{id}");
        Policies(ManagementPermissions.Dictionaries.Update);
        Description(description => description.WithTags("字典管理"));
        Summary(summary =>
        {
            summary.Summary = "更新字典分组";
            summary.Description = "修改字典分组名称和系统归属。";
        });
    }

    /// <summary>
    /// 加载字典和目标系统，通过领域方法更新并持久化。
    /// </summary>
    /// <param name="req">更新请求。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task HandleAsync(UpdateDictionaryRequest req, CancellationToken ct)
    {
        var id = Route<string>("id")!;
        var dictionary = await dictionaryRepository.GetAsync(id, true, ct);
        var system = await systemRepository.GetAsync(req.SystemId, true, ct);

        dictionary.ChangeSystem(system.Id, system.Name).ChangeName(req.Name);
        await dictionaryRepository.UpdateAsync(dictionary, true, ct);
        await Send.OkAsync(true, ct);
    }
}
