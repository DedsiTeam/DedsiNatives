using DedsiNative.CodeGenerator;
using Xunit;

namespace DedsiNative.CodeGenerator.Tests;

/// <summary>
/// 验证导出协议、拒绝覆盖与复杂模型的明确生成边界。
/// </summary>
public sealed class GeneratorTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "dedsi-codegen-" + Guid.NewGuid().ToString("N"));
    private static string Templates => Path.Combine(AppContext.BaseDirectory, "Templates");

    public GeneratorTests()
    {
        foreach (var project in new[] { "DedsiNative.Core", "DedsiNative.Infrastructure", "DedsiNative.Endpoints" })
        {
            var folder = Path.Combine(root, "src", project);
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, project + ".csproj"), "<Project />");
        }
    }

    [Fact]
    public void ExportWithoutEventIdCanBeReadAndWrittenDeterministically()
    {
        var document = Sample();
        var first = Generator.Plan(document, root, "Devices", Templates);
        var second = Generator.Plan(document, root, "Devices", Templates);
        Assert.Equal(first.Files, second.Files);
        Assert.Equal(12, first.Files.Length);
        Assert.False(Directory.Exists(Path.Combine(root, "src", "DedsiNative.Core", "Devices")));
        Generator.Write(first);
        Assert.All(first.Files, file => Assert.Equal(file.Content, File.ReadAllText(file.Path)));
        Assert.Contains("timestamp without time zone", first.Files.Single(f => f.Path.EndsWith("Configuration.cs")).Content);
        Assert.Throws<IOException>(() => Generator.Plan(document, root, "Devices", Templates));
    }

    [Fact]
    public void ConflictDiscoveredAfterPlanningDoesNotWriteOtherFiles()
    {
        var plan = Generator.Plan(Sample(), root, "Devices", Templates);
        var existing = plan.Files[^1].Path;
        Directory.CreateDirectory(Path.GetDirectoryName(existing)!);
        File.WriteAllText(existing, "用户代码");
        Assert.Throws<IOException>(() => Generator.Write(plan));
        Assert.Equal("用户代码", File.ReadAllText(existing));
        Assert.All(plan.Files[..^1], file => Assert.False(File.Exists(file.Path)));
    }

    [Theory]
    [InlineData("List<Device>")]
    [InlineData("金额")]
    [InlineData("List<MissingEntity>")]
    [InlineData("string; public class Injection {}")]
    public void UnknownTypesAreRejectedBeforeWriting(string type)
    {
        var document = Sample();
        document.Model.Elements[0].Fields[0] = document.Model.Elements[0].Fields[0] with { DataType = type };
        Assert.Contains("dataType", Assert.Throws<InvalidDataException>(() => Generator.Plan(document, root, "Devices", Templates)).Message);
    }

    [Theory]
    [InlineData("../escape")]
    [InlineData("class")]
    [InlineData("CON")]
    public void InvalidModuleCannotEscapeOutput(string module) =>
        Assert.Throws<InvalidDataException>(() => Generator.Plan(Sample(), root, module, Templates));

    [Fact]
    public void FiveKindsProduceModelsButNoHandlerOrEntityRepository()
    {
        var sample = Sample();
        var eventId = Guid.NewGuid().ToString();
        var document = sample with { Model = sample.Model with { Elements = [
            sample.Model.Elements[0],
            new(Guid.NewGuid().ToString(), "entity", "DevicePart", "部件", []),
            new(Guid.NewGuid().ToString(), "value_object", "DeviceAddress", "地址", []),
            new(eventId, "domain_event", "DeviceCreated", "已创建", []),
            new(Guid.NewGuid().ToString(), "event_handler", "DeviceCreatedHandler", "处理器", [], eventId)
        ] } };
        var plan = Generator.Plan(document, root, "Devices", Templates);
        Assert.Equal(16, plan.Files.Length);
        Assert.Equal(3, plan.Warnings.Length);
        Assert.DoesNotContain(plan.Files, f => f.Path.Contains("Handler") || f.Path.Contains("PartRepository"));
    }

    [Fact]
    public void BrokenEventLinkAndUnsupportedVersionAreRejected()
    {
        var sample = Sample();
        Assert.Throws<InvalidDataException>(() => DomainReader.Validate(sample with { FormatVersion = "2.0" }));
        var broken = sample with { Model = sample.Model with { Elements = [sample.Model.Elements[0], new(Guid.NewGuid().ToString(), "event_handler", "Handler", "", [], Guid.NewGuid().ToString())] } };
        Assert.Throws<InvalidDataException>(() => DomainReader.Validate(broken));
    }

    [Fact]
    public void DescriptionCannotInjectCodeOrTemplateTokens()
    {
        var sample = Sample();
        sample.Model.Elements[0].Fields[0] = sample.Model.Elements[0].Fields[0] with { Description = "编号</summary>\npublic class Evil {} {{Unknown}}" };
        var model = Generator.Plan(sample, root, "Devices", Templates).Files[0].Content;
        Assert.Contains("&lt;/summary&gt;", model);
        Assert.Contains("\n/// public class Evil {} {{Unknown}}", model);
    }

    [Fact]
    public void TemplateCanBeEditedAndUnknownPlaceholderFailsBeforeWriting()
    {
        var custom = Path.Combine(root, "templates");
        Directory.CreateDirectory(custom);
        foreach (var file in Directory.GetFiles(Templates)) File.Copy(file, Path.Combine(custom, Path.GetFileName(file)));
        File.AppendAllText(Path.Combine(custom, "Aggregate.tpl"), "\n// 自定义模板\n");
        Assert.Contains("自定义模板", Generator.Plan(Sample(), root, "Devices", custom).Files[0].Content);
        File.AppendAllText(Path.Combine(custom, "Repository.tpl"), "{{Missing}}");
        Assert.Throws<InvalidDataException>(() => Generator.Plan(Sample(), root, "Devices", custom));
        Assert.False(Directory.Exists(Path.Combine(root, "src", "DedsiNative.Core", "Devices")));
    }

    private static DomainDocument Sample() => DomainReader.Read(Path.Combine(AppContext.BaseDirectory, "Device.domain.json"));

    public void Dispose() => Directory.Delete(root, true);
}
