using Dedsi.Ddd.Domain.Queries;
namespace {{Namespace}};

{{QueryInputContract}}
{{QueryRowContract}}
{{QueryOutputContract}}
/// <summary>
/// 聚合的只读查询契约。
/// </summary>
public interface I{{Name}}Query : IDedsiQuery
{
    /// <summary>
    /// 根据标量字段筛选并按主键稳定分页。
    /// </summary>
    Task<{{Name}}PagedQueryOutput> GetPagedAsync(
        {{Name}}PagedQueryInput input,
        CancellationToken ct);
}
