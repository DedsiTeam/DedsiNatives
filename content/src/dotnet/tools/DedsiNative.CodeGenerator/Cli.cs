namespace DedsiNative.CodeGenerator;

/// <summary>
/// 命令行入口：校验、预览或新增模型文件，错误时返回非零退出码。
/// </summary>
public static class Cli
{
    /// <summary>
    /// 执行命令；默认从当前目录或工具目录向上定位后端根目录。
    /// </summary>
    public static int Run(string[] args)
    {
        try
        {
            if (args.Length == 0 || args is ["--help"] or ["-h"])
            {
                Console.WriteLine("""
                    DedsiNative 后端代码生成器
                    validate|plan|generate <模型.domain.json> [--root <dotnet目录>] [--module <模块名>] [--templates <模板目录>]
                    validate 校验文档；plan 校验并预览文件；generate 新增文件（禁止覆盖）。
                    """);
                return 0;
            }
            if (args.Length < 2 || args[0] is not ("validate" or "plan" or "generate"))
                throw new InvalidDataException("请使用 validate、plan 或 generate，后接 .domain.json 文件路径。");
            var options = new Dictionary<string, string>(StringComparer.Ordinal);
            for (var i = 2; i < args.Length; i += 2)
            {
                if (i + 1 >= args.Length || args[i] is not ("--root" or "--module" or "--templates") ||
                    !options.TryAdd(args[i], args[i + 1]))
                    throw new InvalidDataException($"未知、重复或缺少值的选项：{args[i]}");
            }
            var document = DomainReader.Read(args[1]);
            var module = options.GetValueOrDefault("--module") ?? document.Model.Elements.Single(e => e.Kind == "aggregate_root").Name + "s";
            DomainReader.Identifier(module, "--module");
            if (args[0] == "validate")
            {
                Console.WriteLine("领域文档校验通过。");
                return 0;
            }
            var root = options.GetValueOrDefault("--root") ?? FindRoot();
            var templates = options.GetValueOrDefault("--templates") ?? Path.Combine(AppContext.BaseDirectory, "Templates");
            var plan = Generator.Plan(document, root, module, templates);
            foreach (var warning in plan.Warnings) Console.WriteLine($"提示：{warning}");
            foreach (var file in plan.Files) Console.WriteLine($"新增：{Path.GetRelativePath(root, file.Path)}");
            if (args[0] == "generate")
            {
                Generator.Write(plan);
                Console.WriteLine($"已生成 {plan.Files.Length} 个文件。EF 配置由现有 DbContext 自动扫描；可通过 Set<T>() 访问实体。");
                Console.WriteLine("请补充业务规则、关系配置并构建项目；未修改公共文件或执行数据库迁移。");
            }
            return 0;
        }
        catch (Exception error) when (error is InvalidDataException or IOException or UnauthorizedAccessException or System.Text.Json.JsonException or ArgumentException)
        {
            Console.Error.WriteLine($"生成失败：{error.Message}");
            return 1;
        }
    }

    private static string FindRoot()
    {
        foreach (var start in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
            for (var current = new DirectoryInfo(start); current is not null; current = current.Parent)
                if (File.Exists(Path.Combine(current.FullName, "src", "DedsiNative.Core", "DedsiNative.Core.csproj")))
                    return current.FullName;
        throw new InvalidDataException("找不到后端目录，请传入 --root <content/src/dotnet>。");
    }
}
