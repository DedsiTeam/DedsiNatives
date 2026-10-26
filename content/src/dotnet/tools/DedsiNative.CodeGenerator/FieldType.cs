namespace DedsiNative.CodeGenerator;

/// <summary>
/// 已解析的类型信息；Target 非空表示明确的领域引用，而不是普通数据库列。
/// </summary>
public sealed record FieldType(string Name, bool Nullable, bool Collection, DomainElement? Target)
{
    /// <summary>
    /// 持久化实体用 ICollection 导航，事件和其他数据契约用数组。
    /// </summary>
    public string CSharp(string module, bool persistent)
    {
        if (Target is null) return Name;
        var type = Target.Name;
        return Collection ? persistent ? $"ICollection<{type}>" : type + "[]" : type + (Nullable ? "?" : "");
    }
}
