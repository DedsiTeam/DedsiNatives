using System.Text.RegularExpressions;

namespace DedsiNative.CodeGenerator;

/// <summary>
/// 从独立文本模板替换命名占位符；不执行脚本，也不递归解释字段说明。
/// </summary>
public sealed class TemplateRenderer(string directory)
{
    /// <summary>
    /// 渲染一个模板；未定义的占位符立即报错，避免输出半成品代码。
    /// </summary>
    public string Render(string name, Dictionary<string, string> values)
    {
        var path = Path.Combine(directory, name + ".tpl");
        var template = File.ReadAllText(path).Replace("\r\n", "\n");
        return Regex.Replace(template, @"\{\{\s*([A-Za-z][A-Za-z0-9]*)\s*\}\}", match =>
            values.TryGetValue(match.Groups[1].Value, out var value)
                ? value
                : throw new InvalidDataException($"模板 {path} 中存在未知占位符：{match.Value}"));
    }
}
