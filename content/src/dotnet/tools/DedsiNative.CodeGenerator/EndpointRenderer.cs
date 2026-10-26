using System.Security;
using System.Text.Json;
namespace DedsiNative.CodeGenerator;

/// <summary>
/// 固定约定的 HTTP 契约、查询及聚合明细生命周期模板编排。
/// </summary>
public sealed class EndpointRenderer(DomainElement[] elements, TemplateRenderer renderer, string module)
{
    private FieldType Resolve(DomainField field) => DomainReader.Resolve(field.DataType, elements, field.Name);
    private static string Xml(string value) => SecurityElement.Escape(value.Replace("\r", " ").Replace("\n", " ")) ?? "";
    private static string NewId(DomainElement e) => DomainReader.KeyType(e) == "Guid" ? "Guid.NewGuid()" : "Ulid.NewUlid().ToString()";
    private static DomainField Id(DomainElement e) => new("", "Id", DomainReader.KeyType(e), "业务主键");
    private static DomainField[] Fields(DomainElement e) => e.Fields.Where(f => e.Kind is not ("entity" or "aggregate_root") || f.Name != "Id").ToArray();

    /// <summary>
    /// 生成位置参数数据契约，参数逐行排列，XML 参数说明每条占一行。
    /// </summary>
    private string PositionalRecord(
        string name,
        string description,
        IEnumerable<(string Type, string Name, string Description, string Default)> fields)
    {
        var parameters = fields.ToArray();
        return renderer.Render("PositionalRecord", new()
        {
            ["Name"] = name,
            ["Description"] = description,
            ["ParameterComments"] = string.Concat(parameters.Select(f => renderer.Render("RecordParameterComment", new()
            {
                ["Name"] = f.Name,
                ["Description"] = Xml(string.IsNullOrWhiteSpace(f.Description) ? f.Name + " 字段。" : f.Description)
            }))),
            ["Parameters"] = string.Join(",\n", parameters.Select(f => "    " + f.Type + " " + f.Name + f.Default))
        });
    }

    /// <summary>
    /// 生成接口、独立 DTO 与查询实现，输出仍由统一预检负责。
    /// </summary>
    public void Add(DomainElement root, Action<string, string> add)
    {
        var scalar = new[] { Id(root) }.Concat(Fields(root).Where(f => Resolve(f).Target is null)).ToArray();
        var values = new Dictionary<string, string>
        {
            ["Namespace"] = "DedsiNative." + module, ["Name"] = root.Name,
            ["KeyType"] = DomainReader.KeyType(root), ["NewId"] = NewId(root),
            ["Route"] = JsonNamingPolicy.CamelCase.ConvertName(root.Name),
            ["UpdateArguments"] = JoinArguments(Fields(root).Select(f => "replacement." + f.Name)),
            ["PagedRequestContract"] = PositionalRecord("Paged" + root.Name + "Request", "条件分页请求。",
                new[] { ("int", "PageNumber", "从 1 开始的页码。", " = 1"), ("int", "PageSize", "每页数量，范围 1 至 200。", " = 20") }
                    .Concat(scalar.Select(f => (Resolve(f).Name.TrimEnd('?') + "?", "Filter" + f.Name, f.Description + "；为空时不筛选。", " = null")))),
            ["PagedResponseContract"] = PositionalRecord("Paged" + root.Name + "Response", "条件分页响应。",
                new[] { ("long", "TotalCount", "符合条件的总记录数。", ""), (root.Name + "RowResponse[]", "Items", "当前页明细数组。", "") }),
            ["CreateResponseContract"] = PositionalRecord("Create" + root.Name + "Response", "创建结果。",
                new[] { (DomainReader.KeyType(root), "Id", "服务端生成的主键。", "") }),
            ["RowContract"] = PositionalRecord(root.Name + "RowResponse", "分页列表行。",
                scalar.Select(f => (Resolve(f).Name, f.Name, f.Description, ""))),
            ["QueryRowContract"] = PositionalRecord(root.Name + "QueryItemDto", "分页查询结果明细。",
                scalar.Select(f => (Resolve(f).Name, f.Name, f.Description, ""))),
            ["QueryInputContract"] = PositionalRecord(root.Name + "PagedQueryInput", "分页查询条件。",
                new[] { ("int", "SkipCount", "跳过的行数，不能小于零。", ""), ("int", "MaxResultCount", "最多返回的行数，范围 1 至 200。", "") }
                    .Concat(scalar.Select(f => (Resolve(f).Name.TrimEnd('?') + "?", "Filter" + f.Name, f.Description + "；为空时不筛选。", " = null")))),
            ["QueryOutputContract"] = PositionalRecord(root.Name + "PagedQueryOutput", "分页查询结果。",
                new[] { ("long", "TotalCount", "符合条件的总记录数。", ""), (root.Name + "QueryItemDto[]", "Items", "当前页的明细数组。", "") }),
            ["QueryInputArguments"] = string.Join(",\n", new[] { "            (req.PageNumber - 1) * req.PageSize", "            req.PageSize" }.Concat(scalar.Select(f => $"            req.Filter{f.Name}"))),
            ["RowArguments"] = string.Join(",\n", scalar.Select(f => $"                item.{f.Name}")),
            ["QueryArguments"] = string.Join(",\n", scalar.Select(f => $"                entity.{f.Name}")),
            ["FilterLocals"] = string.Join("\n", scalar.Where(f => Resolve(f).Name.TrimEnd('?') == "string").Select(f => $"        var filter{f.Name} = input.Filter{f.Name}?.Trim();")),
            ["Filters"] = string.Join("\n", scalar.Select(f => renderer.Render(Resolve(f).Name.TrimEnd('?') == "string" ? "FilterString" : "FilterScalar",
                new() { ["Name"] = f.Name, ["Local"] = "filter" + f.Name })))
        };
        var reachable = new List<DomainElement>();
        void Visit(DomainElement e)
        {
            if (reachable.Contains(e)) return;
            reachable.Add(e);
            foreach (var f in Fields(e))
                if (Resolve(f).Target is { } target) Visit(target);
        }
        Visit(root);
        var contracts = new List<string>();
        var mappings = new List<string>();
        var nestedValidators = new List<string>();
        var operationRules = "";
        foreach (var e in reachable)
        {
            var fields = Fields(e);
            string DtoType(DomainField f, string suffix)
            {
                var t = Resolve(f);
                return t.Target is null ? t.Name : t.Target.Name + suffix + (t.Collection ? "[]" : t.Nullable ? "?" : "");
            }
            var requestParameters = fields.Select(f => (DtoType(f, "Request"), f.Name, f.Description, "")).ToArray();
            if (e != root)
                contracts.Add(PositionalRecord(e.Name + "Request", "写入请求数据。", requestParameters));
            if (e == root)
                foreach (var operation in new[] { "Create", "Update" })
                    values[operation + "RequestContract"] = PositionalRecord(operation + e.Name + "Request",
                        operation == "Create" ? "创建聚合请求。" : "全量修改聚合请求，原明细整批替换。",
                        requestParameters);
            var rules = new List<string>();
            foreach (var field in fields)
            {
                var t = Resolve(field);
                var ruleValues = new Dictionary<string, string> { ["Name"] = field.Name, ["Type"] = t.Target?.Name ?? "" };
                if (!t.Nullable && (t.Target is not null || t.Name == "string"))
                    rules.Add(renderer.Render("RequiredRule", ruleValues));
                if (t.Target is not null)
                    rules.Add(renderer.Render(t.Collection ? "CollectionRule" : "NestedRule", ruleValues));
            }
            if (e == root)
                operationRules = string.Join("\n", rules);
            else
                nestedValidators.Add(renderer.Render("RequestValidator", new() { ["Type"] = e.Name, ["Rules"] = string.Join("\n", rules) }));
            var responseFields = (e.Kind is "aggregate_root" or "entity" ? new[] { Id(e) } : []).Concat(fields).ToArray();
            contracts.Add(PositionalRecord(e.Name + "Response", "详情响应数据。", responseFields.Select(f => (DtoType(f, "Response"), f.Name, f.Description, ""))));
            string Map(DomainField f, string owner, bool toDomain)
            {
                var t = Resolve(f);
                var access = owner + "." + f.Name;
                if (t.Target is null) return access;
                var method = toDomain ? "ToDomain" : "ToResponse";
                if (t.Collection)
                    return $"({access} ?? throw new ArgumentNullException(nameof({access}))).Select(item => {method}(item)).ToArray()";
                return t.Nullable ? $"{access} is null ? null : {method}({access})" : $"{method}({access})";
            }
            var args = fields.Select(f => Map(f, "request", true));
            if (e.Kind is "aggregate_root" or "entity") args = new[] { e == root ? "id" : NewId(e) }.Concat(args);
            foreach (var requestType in e == root
                ? new[] { "Create" + e.Name + "Request", "Update" + e.Name + "Request" }
                : new[] { e.Name + "Request" })
                mappings.Add(renderer.Render("ToDomain", new()
                {
                    ["Type"] = e.Name,
                    ["RequestType"] = requestType,
                    ["IdParameter"] = e == root ? ", " + DomainReader.KeyType(e) + " id" : "",
                    ["Arguments"] = JoinArguments(args)
                }));
            mappings.Add(renderer.Render("ToResponse", new()
            {
                ["Type"] = e.Name,
                ["Arguments"] = JoinArguments(responseFields.Select(f => Map(f, "entity", false)))
            }));
        }
        foreach (var operation in new[] { "Create", "Update" })
            values[operation + "Validator"] = renderer.Render("OperationValidator", new()
            {
                ["Name"] = root.Name,
                ["Operation"] = operation,
                ["Rules"] = operationRules,
                ["NestedValidators"] = string.Join("\n", nestedValidators.Select(v =>
                    string.Join("\n", v.Split('\n').Select(line => line.Length == 0 ? "" : "    " + line))))
            });
        values["Contracts"] = string.Join("\n", contracts);
        values["Mappings"] = string.Join("\n", mappings);
        var directory = Path.Combine("src", "DedsiNative.Endpoints", "Endpoints", root.Name + "Endpoints");
        add(Path.Combine(directory, root.Name + "Contracts.cs"), renderer.Render("EndpointContracts", values));
        foreach (var operation in new[] { "Create", "Update", "Delete", "Get", "Paged" })
        {
            var permission = operation is "Get" or "Paged" ? "view" : operation.ToLowerInvariant();
            values["Authorization"] = renderer.Render("EndpointAuthorization", new()
            {
                ["Permission"] = "generated:" + root.Name.ToLowerInvariant() + ":" + permission
            });
            add(Path.Combine(directory, operation + root.Name + "Endpoint.cs"), renderer.Render(operation + "Endpoint", values));
        }
        add(Path.Combine("src", "DedsiNative.Core", module, "I" + root.Name + "Query.cs"), renderer.Render("QueryInterface", values));
        add(Path.Combine("src", "DedsiNative.Infrastructure", "EntityFrameworkCore", "Queries", root.Name + "Query.cs"), renderer.Render("Query", values));
    }

    private static string JoinArguments(IEnumerable<string> args)
    {
        var items = args.ToArray();
        return items.Length == 0 ? "" : "\n            " + string.Join(",\n            ", items);
    }

    /// <summary>
    /// 以子孙优先顺序删除聚合内部实体，避免替换和删除时遗留孤儿。
    /// </summary>
    public string RemoveDetails(DomainElement owner, string expression = "entity", int depth = 0)
    {
        return string.Join("", Fields(owner).Where(f => Resolve(f).Collection).Select(f =>
        {
            var variable = "detail" + depth;
            return renderer.Render("RemoveDetails", new()
            {
                ["Indent"] = new string(' ', 8 + depth * 4), ["Variable"] = variable,
                ["Owner"] = expression, ["Field"] = f.Name,
                ["Children"] = RemoveDetails(Resolve(f).Target!, variable, depth + 1)
            });
        }));
    }
}
