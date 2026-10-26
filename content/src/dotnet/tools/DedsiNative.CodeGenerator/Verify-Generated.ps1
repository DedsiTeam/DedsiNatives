$ErrorActionPreference = 'Stop'
$backendRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$verificationRoot = Join-Path $PSScriptRoot ('obj/verification-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $verificationRoot -Force | Out-Null
foreach ($project in @('DedsiNative.Core', 'DedsiNative.Infrastructure', 'DedsiNative.Endpoints')) {
    $folder = Join-Path $verificationRoot "src/$project"
    New-Item -ItemType Directory -Path $folder -Force | Out-Null
    Set-Content -LiteralPath (Join-Path $folder "$project.csproj") -Value '<Project />' -Encoding UTF8
}

# 补充其余四种设计项，用真实依赖验证生成代码，而非仅对比模板字符串。
$document = Get-Content -LiteralPath (Join-Path $backendRoot 'tests/DedsiNative.CodeGenerator.Tests/Fixtures/Device.domain.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$eventId = [Guid]::NewGuid().ToString()
$scalarFields = @('bool', 'byte', 'short', 'int', 'long', 'float', 'double', 'decimal', 'Guid', 'DateTime', 'DateOnly', 'TimeOnly') | ForEach-Object {
    @{ id = [Guid]::NewGuid().ToString(); name = "Field$_"; dataType = "$_`?"; description = "验证 $_ 可空映射" }
}
$document.model.elements += @(
    @{ id = [Guid]::NewGuid().ToString(); kind = 'entity'; name = 'DevicePart'; description = '部件'; fields = @($scalarFields) },
    @{ id = [Guid]::NewGuid().ToString(); kind = 'value_object'; name = 'DeviceAddress'; description = '地址'; fields = @() },
    @{ id = $eventId; kind = 'domain_event'; name = 'DeviceCreated'; description = '事件'; fields = @() },
    @{ id = [Guid]::NewGuid().ToString(); kind = 'event_handler'; name = 'DeviceCreatedHandler'; description = '处理器'; fields = @(); eventId = $eventId }
)
$inputPath = Join-Path $verificationRoot 'sample.domain.json'
$document | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $inputPath -Encoding UTF8
dotnet run --project (Join-Path $PSScriptRoot 'DedsiNative.CodeGenerator.csproj') -p:UseSharedCompilation=false -p:BuildInParallel=false -- generate $inputPath --root $verificationRoot --module Devices
if ($LASTEXITCODE -ne 0) { throw '生成样例失败。' }

$infrastructureProject = [System.Security.SecurityElement]::Escape((Join-Path $backendRoot 'src/DedsiNative.Infrastructure/DedsiNative.Infrastructure.csproj'))
$endpointsProject = [System.Security.SecurityElement]::Escape((Join-Path $backendRoot 'src/DedsiNative.Endpoints/DedsiNative.Endpoints.csproj'))
$verificationProject = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><OutputType>Exe</OutputType><Nullable>enable</Nullable></PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="$infrastructureProject" />
    <ProjectReference Include="$endpointsProject" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Relational" />
  </ItemGroup>
</Project>
"@
Set-Content -LiteralPath (Join-Path $verificationRoot 'Verification.csproj') -Value $verificationProject -Encoding UTF8
$verificationCode = @'
using DedsiNative.Devices;
using DedsiNative.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

var options = new DbContextOptionsBuilder<DedsiNativeDbContext>()
    .UseNpgsql("Host=localhost;Database=codegen_verification")
    .Options;
using var context = new VerificationContext(options);
var entity = context.Model.FindEntityType(typeof(Device)) ?? throw new Exception("未发现生成的聚合配置。");
if (entity.FindProperty("Id")!.GetMaxLength() != 26) throw new Exception("主键长度错误。");
if (!entity.FindProperty("Description")!.IsNullable) throw new Exception("可空配置错误。");
if (entity.FindProperty("PurchasedAt")!.GetColumnType() != "timestamp without time zone") throw new Exception("时间映射错误。");
var device = new Device("device-test", "D001", null, null);
if (device.Id != "device-test") throw new Exception("不应对字符串主键执行 ULID 格式校验。");
if (device.Code != "D001") throw new Exception("构造函数未赋值。");
Console.WriteLine("生成代码编译、构造与 EF 模型验证通过，未连接数据库。");

sealed class VerificationContext(DbContextOptions<DedsiNativeDbContext> options) : DedsiNativeDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(VerificationContext).Assembly);
        base.OnModelCreating(builder);
    }
}
'@
Set-Content -LiteralPath (Join-Path $verificationRoot 'Program.cs') -Value $verificationCode -Encoding UTF8
dotnet run --project (Join-Path $verificationRoot 'Verification.csproj') -p:UseSharedCompilation=false -p:BuildInParallel=false
if ($LASTEXITCODE -ne 0) { throw "生成代码验证失败，输出目录：$verificationRoot" }
Write-Output "验证目录：$verificationRoot"
