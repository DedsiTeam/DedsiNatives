using System.Text.Json;
using System.Text.RegularExpressions;

namespace DedsiNative.CodeGenerator;

/// <summary>
/// 将导出文档验证为可确定生成的模型，拒绝未知类型与无法表达的关系。
/// </summary>
public static class DomainReader
{
    private static readonly HashSet<string> Keywords = new(("abstract as base bool break byte case catch char checked class const continue decimal default delegate do double else enum event explicit extern false finally fixed float for foreach goto if implicit in int interface internal is lock long namespace new null object operator out override params private protected public readonly ref return sbyte sealed short sizeof stackalloc static string struct switch this throw true try typeof uint ulong unchecked unsafe ushort using virtual void volatile while record required file scoped global var dynamic nint nuint").Split(' '));
    private static readonly HashSet<string> Reserved = new(StringComparer.OrdinalIgnoreCase)
    { "Update", "CreationTime", "CreatorId", "CreatorName", "ConcurrencyStamp", "ExtraProperties", "GetKeys", "GetHashCode", "Equals", "ToString", "GetType", "Finalize", "MemberwiseClone", "ValidateId", "EqualityContract", "PrintMembers", "Deconstruct" };
    private static readonly Dictionary<string, string> Types = new(StringComparer.Ordinal)
    {
        ["string"] = "string", ["bool"] = "bool", ["byte"] = "byte", ["short"] = "short",
        ["int"] = "int", ["long"] = "long", ["float"] = "float", ["double"] = "double", ["decimal"] = "decimal",
        ["Guid"] = "Guid", ["DateTime"] = "DateTime", ["DateTimeOffset"] = "DateTime", ["DateOnly"] = "DateOnly", ["TimeOnly"] = "TimeOnly"
    };

    /// <summary>
    /// 读取并验证完整导出文件，错误信息包含 JSON 字段路径。
    /// </summary>
    public static DomainDocument Read(string path)
    {
        var document = JsonSerializer.Deserialize<DomainDocument>(File.ReadAllText(path), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            RespectRequiredConstructorParameters = true
        }) ?? throw new InvalidDataException("文档不能为空。");
        Validate(document);
        return document;
    }

    /// <summary>
    /// 校验模型、类型引用和归属关系，不根据自然语言说明生成业务规则。
    /// </summary>
    public static void Validate(DomainDocument document)
    {
        Require(document.Format == "dedsi-domain", "format 必须为 dedsi-domain。");
        Require(document.FormatVersion == "1.0", "formatVersion 仅支持 1.0。");
        var model = document.Model;
        Require(model is not null, "model 不能为空。");
        Uuid(model!.Id, "model.id");
        Require(model.Version > 0, "model.version 必须为正整数。");
        Require(!string.IsNullOrWhiteSpace(model.Name) && model.Description is not null, "model.name / description 无效。");
        Require(model.Elements is { Length: > 0 and <= 100 }, "model.elements 必须包含 1 到 100 个设计项。");
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < model.Elements!.Length; i++)
        {
            var element = model.Elements[i];
            var location = $"model.elements[{i}]";
            Require(element is not null, $"{location} 不能为空。");
            Uuid(element!.Id, location + ".id");
            Require(ids.Add(element.Id), $"{location}.id 重复。");
            Identifier(element.Name, location + ".name");
            Require(!Types.ContainsKey(element.Name), $"{location}.name 不能与基础类型重名。");
            Require(names.Add(element.Name), $"{location}.name 跨设计项名称冲突。");
            Require(element.Description is not null, $"{location}.description 不能为空值。");
            Require(element.Kind is "aggregate_root" or "entity" or "value_object" or "domain_event" or "event_handler", $"{location}.kind 不支持。");
            Require(element.Fields is { Length: <= 100 }, $"{location}.fields 必须是数组，最多 100 项。");
            var fieldNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var fieldIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var f = 0; f < element.Fields!.Length; f++)
            {
                var field = element.Fields[f];
                var fieldPath = $"{location}.fields[{f}]";
                Require(field is not null, $"{fieldPath} 不能为空。");
                Uuid(field!.Id, fieldPath + ".id");
                Require(fieldIds.Add(field.Id), $"{fieldPath}.id 重复。");
                Identifier(field.Name, fieldPath + ".name");
                Require(fieldNames.Add(field.Name) && !Reserved.Contains(field.Name) && !field.Name.Equals(element.Name, StringComparison.OrdinalIgnoreCase), $"{fieldPath}.name 与字段、生成成员或基类成员冲突。");
                Require(!string.IsNullOrWhiteSpace(field.Description), $"{fieldPath}.description 不能为空。");
                var type = Resolve(field.DataType, model.Elements, fieldPath + ".dataType");
                Require(!type.Collection || element.Kind != "value_object", $"{fieldPath} 值对象暂不支持实体集合。");
                if (element.Kind is "aggregate_root" or "entity" && field.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                    Require(field.Name == "Id" && field.DataType is "string" or "Guid", $"{fieldPath} 主键必须为 Id:string 或 Id:Guid，且不可空。");
            }
            var parameters = element.Fields.Select(f => ParameterName(f.Name).TrimStart('@')).ToArray();
            Require(parameters.Distinct(StringComparer.Ordinal).Count() == parameters.Length &&
                !parameters.Any(p => element.Fields.Any(f => f.Name == p)), $"{location}.fields 转换为构造参数后发生名称冲突。");
            if (element.Kind != "event_handler") Require(element.EventId is null, $"{location}.eventId 仅适用于事件处理器。");
        }
        Require(model.Elements.Count(e => e.Kind == "aggregate_root") == 1, "model.elements 必须且只能包含一个聚合根。");
        var events = model.Elements.Where(e => e.Kind == "domain_event").Select(e => e.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var assigned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var handler in model.Elements.Where(e => e.Kind == "event_handler"))
            Require(handler.EventId is not null && events.Contains(handler.EventId) && assigned.Add(handler.EventId), $"处理器 {handler.Name} 的 eventId 必须引用当前模型中的事件，且不能重复关联。");
        Require(events.SetEquals(assigned), "每个领域事件必须关联一个事件处理器（与 DedsiWork 校验一致）。");
        ValidateRelations(model.Elements);
    }

    /// <summary>
    /// 将标量类型转换为 C# 类型；导出中的 offset 时间按项目规范生成 DateTime。
    /// </summary>
    public static string TypeName(string text, string location)
    {
        Require(!string.IsNullOrWhiteSpace(text), $"{location} 不能为空。");
        var nullable = text.EndsWith('?');
        var name = nullable ? text[..^1] : text;
        if (!Types.TryGetValue(name, out var result))
            throw new InvalidDataException($"{location} 不支持类型“{text}”。请使用标量、当前模型值对象或 List<实体>。");
        return result + (nullable ? "?" : "");
    }

    /// <summary>
    /// 解析当前文档中的值对象与实体集合引用，拒绝任意 C# 类型文本。
    /// </summary>
    public static FieldType Resolve(string text, DomainElement[] elements, string location)
    {
        Require(!string.IsNullOrWhiteSpace(text), $"{location} 不能为空。");
        var nullable = text.EndsWith('?');
        var name = nullable ? text[..^1] : text;
        if (Types.ContainsKey(name)) return new(TypeName(text, location), nullable, false, null);
        var collection = Regex.Match(name, @"^List<([A-Za-z_][A-Za-z0-9_]*)>$");
        var reference = collection.Success ? collection.Groups[1].Value : name;
        var target = elements.FirstOrDefault(e => e?.Name == reference);
        Require(target is not null, $"{location} 无法解析类型“{text}”。");
        Require(collection.Success ? target!.Kind == "entity" && !nullable : target!.Kind == "value_object", $"{location} 仅支持值对象引用（可空）或非空 List<实体>。");
        return new(reference, nullable, collection.Success, target);
    }

    /// <summary>
    /// 读取业务主键类型；未声明时沿用字符串 ULID。
    /// </summary>
    public static string KeyType(DomainElement element) => element.Fields.FirstOrDefault(f => f.Name == "Id")?.DataType ?? "string";

    /// <summary>
    /// 构造参数使用小驼峰；关键字加 @，已是小写的属性加 Value 避免无 this 赋值时遮蔽成员。
    /// </summary>
    public static string ParameterName(string name)
    {
        var parameter = JsonNamingPolicy.CamelCase.ConvertName(name);
        if (parameter == name) parameter += "Value";
        return Keywords.Contains(parameter) ? "@" + parameter : parameter;
    }

    private static void ValidateRelations(DomainElement[] elements)
    {
        var owners = new HashSet<string>();
        foreach (var owner in elements.Where(e => e.Kind is "aggregate_root" or "entity"))
        foreach (var field in owner.Fields)
        {
            var type = Resolve(field.DataType, elements, owner.Name + "." + field.Name);
            if (!type.Collection) continue;
            var target = type.Target!;
            Require(owners.Add(target.Name), $"实体 {target.Name} 被多个集合引用，无法确定唯一归属。");
            var foreignKey = owner.Name + "Id";
            var declared = target.Fields.FirstOrDefault(f => f.Name.Equals(foreignKey, StringComparison.OrdinalIgnoreCase));
            Require(declared is null || declared.Name == foreignKey && declared.DataType == KeyType(owner), $"{target.Name}.{foreignKey} 外键类型必须与 {owner.Name}.Id 一致且不可空。");
        }
        var visiting = new HashSet<string>();
        var visited = new HashSet<string>();
        foreach (var element in elements) Visit(element);
        void Visit(DomainElement element)
        {
            if (visited.Contains(element.Name)) return;
            Require(visiting.Add(element.Name), $"模型引用形成循环：{element.Name}。");
            foreach (var field in element.Fields)
            {
                var target = Resolve(field.DataType, elements, element.Name + "." + field.Name).Target;
                if (target is not null) Visit(target);
            }
            visiting.Remove(element.Name);
            visited.Add(element.Name);
        }
    }

    /// <summary>
    /// 校验可用于 C# 标识符与 Windows 文件名的名称，不静默改名。
    /// </summary>
    public static void Identifier(string text, string location)
    {
        Require(text is not null && Regex.IsMatch(text, "^[A-Za-z_][A-Za-z0-9_]{0,99}$") && !Keywords.Contains(text) &&
            !Regex.IsMatch(text, "^(CON|PRN|AUX|NUL|COM[0-9]|LPT[0-9])$", RegexOptions.IgnoreCase), $"{location} 必须是合法的非保留英文 C# 标识符。");
    }

    private static void Uuid(string text, string location) => Require(Guid.TryParseExact(text, "D", out _), $"{location} 必须为 UUID。");
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidDataException(message);
    }
}
