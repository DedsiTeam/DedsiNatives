using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace DedsiNative.CodeGenerator;

/// <summary>
/// 预渲染后的文件，路径只能位于所选后端的目标类库中。
/// </summary>
public sealed record GeneratedFile(string Path, string Content);

/// <summary>
/// 全部校验与渲染完成后得到的写入计划。
/// </summary>
public sealed record GenerationPlan(string Root, GeneratedFile[] Files, string[] Warnings);

/// <summary>
/// 领域模型生成流程；只新增文件，不修改 DbContext、权限、迁移或已有业务代码。
/// </summary>
public static class Generator
{
    /// <summary>
    /// 合并项目约定与领域设计，渲染所有模板并检查输出冲突。
    /// </summary>
    public static GenerationPlan Plan(DomainDocument document, string root, string module, string templates)
    {
        DomainReader.Validate(document);
        DomainReader.Identifier(module, "module");
        root = Path.GetFullPath(root);
        foreach (var project in new[] { "DedsiNative.Core", "DedsiNative.Infrastructure", "DedsiNative.Endpoints" })
            if (!File.Exists(Path.Combine(root, "src", project, project + ".csproj")))
                throw new InvalidDataException($"目标类库不存在：{project}，--root 必须指向后端 dotnet 目录。");
        var renderer = new TemplateRenderer(templates);
        var files = new List<GeneratedFile>();
        var warnings = new List<string>();
        var generatedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var mapping = new MappingRenderer(document.Model.Elements, renderer);
        var endpoints = new EndpointRenderer(document.Model.Elements, renderer, module);
        foreach (var element in document.Model.Elements)
        {
            if (element.Kind == "event_handler")
            {
                warnings.Add($"未生成事件处理器 {element.Name}：第一版不生成应用层处理逻辑，eventId={element.EventId}。");
                continue;
            }
            var entity = element.Kind is "aggregate_root" or "entity";
            var aggregate = element.Kind == "aggregate_root";
            var fields = element.Fields.Where(f => !entity || f.Name != "Id").ToArray();
            Dictionary<string, string> Field(DomainField field) => FieldValues(field, entity, document.Model.Elements, module, renderer);
            var keyType = DomainReader.KeyType(element);
            var values = new Dictionary<string, string>
            {
                ["Namespace"] = "DedsiNative." + module,
                ["Module"] = module,
                ["Name"] = element.Name,
                ["Description"] = Xml(element.Description),
                ["Properties"] = string.Join("\n", fields.Select(f => renderer.Render("Property", Field(f)))),
                ["Parameters"] = fields.Length == 0 ? "" : "\n" + string.Join(",\n", fields.Select(f => renderer.Render("ConstructorParameter", new() { ["Type"] = Field(f)["Type"], ["Parameter"] = DomainReader.ParameterName(f.Name) }).TrimEnd())),
                ["Assignments"] = string.Join("\n", fields.Select(f => renderer.Render("Assignment", Field(f))).Select(s => s.TrimEnd('\n'))),
                ["FieldConfigurations"] = entity ? mapping.Fields(element, "builder") + mapping.ForeignKey(element) : "",
                ["AuditConfiguration"] = aggregate ? renderer.Render("AuditConfiguration", []) : "",
                ["KeyType"] = DomainReader.TypeName(keyType, element.Name + ".Id"),
                ["KeyLength"] = keyType == "string" ? ".HasMaxLength(26)" : "",
                ["IdValidation"] = keyType == "Guid" ? renderer.Render("GuidValidation", []) : "",
                ["RemoveDetails"] = aggregate ? endpoints.RemoveDetails(element) : "",
                ["OrmConstructor"] = fields.Length == 0 ? "" : renderer.Render("RecordOrmConstructor", new() { ["Name"] = element.Name })
            };
            values["ConstructorParameters"] = entity
                ? "\n        " + values["KeyType"] + " id" + (fields.Length > 0 ? "," + values["Parameters"] : "")
                : values["Parameters"];
            var parameterComments = fields.Select(field => renderer.Render("ParameterComment", new()
            {
                ["ParameterName"] = DomainReader.ParameterName(field.Name).TrimStart('@'),
                ["Description"] = Xml(Regex.Replace(field.Description, @"\s+", " ").Trim())
            })).ToList();
            if (entity)
            {
                var idDescription = element.Fields.FirstOrDefault(f => f.Name == "Id")?.Description ?? "业务实体唯一标识";
                if (keyType == "Guid") idDescription += "；不能为 Guid.Empty。";
                parameterComments.Insert(0, renderer.Render("ParameterComment", new()
                {
                    ["ParameterName"] = "id",
                    ["Description"] = Xml(Regex.Replace(idDescription, @"\s+", " ").Trim())
                }));
            }
            values["ParameterComments"] = string.Join("", parameterComments);
            values["UpdateComments"] = string.Join("", entity ? parameterComments.Skip(1) : parameterComments);
            values["UpdateMethod"] = aggregate ? renderer.Render("DomainUpdate", values) : "";
            foreach (var field in fields)
            {
                if (field.DataType.TrimEnd('?') == "DateTimeOffset")
                    warnings.Add($"{element.Name}.{field.Name}：{field.DataType} 按项目规范生成 DateTime{(field.DataType.EndsWith('?') ? "?" : "")}（北京时间墙钟），仅映射类型，不转换运行时数据。");
                var type = DomainReader.Resolve(field.DataType, document.Model.Elements, field.Name);
                if (type.Collection)
                    warnings.Add(entity ? $"{element.Name}.{field.Name}：一对多，外键 {element.Name}Id，Restrict 删除并自动加载。" : $"{element.Name}.{field.Name}：生成数组并复制集合；元素仍为实体引用，业务快照与幂等处理需另行实现。");
            }
            var coreDirectory = Path.Combine("src", "DedsiNative.Core", module);
            Add(Path.Combine(coreDirectory, element.Name + ".cs"), renderer.Render(aggregate ? "Aggregate" : entity ? "Entity" : element.Kind == "value_object" ? "ValueObject" : "DomainEvent", values));
            if (entity)
            {
                Add(Path.Combine("src", "DedsiNative.Infrastructure", "EntityFrameworkCore", "Configurations", element.Name + "Configuration.cs"), renderer.Render("Configuration", values));
                if (!aggregate && !mapping.HasOwner(element)) warnings.Add($"实体 {element.Name} 没有归属集合，生成独立表配置。");
            }
            else if (element.Kind == "value_object")
                warnings.Add($"值对象 {element.Name} 生成 record；引用处采用 OwnsOne 映射，不生成独立表。");
            if (aggregate)
            {
                Add(Path.Combine(coreDirectory, "I" + element.Name + "Repository.cs"), renderer.Render("RepositoryInterface", values));
                Add(Path.Combine("src", "DedsiNative.Infrastructure", "EntityFrameworkCore", "Repositories", element.Name + "Repository.cs"), renderer.Render("Repository", values));
            }
        }
        endpoints.Add(document.Model.Elements.Single(e => e.Kind == "aggregate_root"), Add);
        var plan = new GenerationPlan(root, files.ToArray(), warnings.ToArray());
        Preflight(plan);
        return plan;

        void Add(string relative, string content)
        {
            // 同一模块中的设计项不能与生成的仓储接口等类型重名。
            var key = Path.GetDirectoryName(relative) + "/" + Path.GetFileNameWithoutExtension(relative);
            if (!generatedNames.Add(key)) throw new InvalidDataException($"生成类型冲突：{relative}");
            files.Add(new GeneratedFile(Path.GetFullPath(Path.Combine(root, relative)), content));
        }
    }

    /// <summary>
    /// 复查计划后使用 CreateNew 写入；失败时仅清理本次创建的文件。
    /// </summary>
    public static void Write(GenerationPlan plan)
    {
        Preflight(plan);
        var created = new List<string>();
        try
        {
            foreach (var file in plan.Files)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(file.Path)!);
                using var stream = new FileStream(file.Path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                created.Add(file.Path);
                using var writer = new StreamWriter(stream, new UTF8Encoding(false));
                writer.Write(file.Content);
            }
        }
        catch
        {
            foreach (var path in created) File.Delete(path);
            throw;
        }
    }

    private static void Preflight(GenerationPlan plan)
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in plan.Files)
        {
            var fullPath = Path.GetFullPath(file.Path);
            var allowed = new[] { "DedsiNative.Core", "DedsiNative.Infrastructure", "DedsiNative.Endpoints" }.Any(project =>
                fullPath.StartsWith(Path.GetFullPath(Path.Combine(plan.Root, "src", project)) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));
            if (!allowed || !paths.Add(fullPath)) throw new InvalidDataException($"输出路径越界或重复：{file.Path}");
            if (File.Exists(fullPath) || Directory.Exists(fullPath)) throw new IOException($"目标已存在，禁止覆盖：{file.Path}");
            for (var directory = new DirectoryInfo(Path.GetDirectoryName(fullPath)!); directory is not null; directory = directory.Parent)
            {
                if (File.Exists(directory.FullName)) throw new IOException($"目录路径被文件占用：{directory.FullName}");
                if (directory.Exists && (directory.Attributes & FileAttributes.ReparsePoint) != 0)
                    throw new IOException($"输出路径不能经过符号链接或目录联接：{directory.FullName}");
            }
        }
    }

    private static Dictionary<string, string> FieldValues(DomainField field, bool entity, DomainElement[] elements, string module, TemplateRenderer renderer)
    {
        var type = DomainReader.Resolve(field.DataType, elements, field.Name);
        var parameter = DomainReader.ParameterName(field.Name);
        var value = !type.Nullable && (field.DataType == "string" || type.Target is not null)
            ? renderer.Render("NullGuard", new() { ["Value"] = parameter, ["Parameter"] = parameter }).Trim() : parameter;
        if (type.Collection)
            value = renderer.Render(entity ? "CollectionCopy" : "ArrayCopy", new() { ["Type"] = type.Target!.Name, ["Value"] = value }).Trim();
        return new()
        {
        ["Name"] = field.Name,
        ["Type"] = type.CSharp(module, entity),
        ["Description"] = Xml(field.Description),
        ["Setter"] = entity ? "private set;" : "init;",
        ["Required"] = field.DataType.EndsWith('?') ? "false" : "true",
        ["Value"] = value,
        ["Initializer"] = type.Collection ? "[]" : "default!"
        };
    }

    private static string Xml(string text) => (SecurityElement.Escape(text) ?? "").Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", "\n/// ");
}
