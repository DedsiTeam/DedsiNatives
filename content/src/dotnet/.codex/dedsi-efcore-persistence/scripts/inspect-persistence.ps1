param(
    [Parameter(Mandatory = $false)]
    [string]$DotnetRoot = "content/src/dotnet"
)

$resolvedRoot = Resolve-Path -LiteralPath $DotnetRoot -ErrorAction Stop
$sourceRoot = Join-Path $resolvedRoot "src"
$hostEndpoints = Join-Path $sourceRoot "DedsiNative.Host/Endpoints"
$infrastructureRoot = Join-Path $sourceRoot "DedsiNative.Infrastructure"
$issues = [System.Collections.Generic.List[string]]::new()

# 检查 Host Endpoint 是否绕过查询/仓储契约直接依赖数据库上下文。
if (Test-Path -LiteralPath $hostEndpoints) {
    $dbContextReferences = Get-ChildItem -LiteralPath $hostEndpoints -Recurse -File -Filter "*.cs" |
        Select-String -Pattern "\bI?DedsiNativeDbContext\b"

    foreach ($fileGroup in ($dbContextReferences | Group-Object -Property Path)) {
        $lineNumbers = ($fileGroup.Group.LineNumber | Sort-Object -Unique) -join ", "
        $issues.Add("禁止 Endpoint 直接依赖 DbContext：$($fileGroup.Name)，命中行：$lineNumbers")
    }
}

# 检查每个实体映射是否会被当前程序集扫描机制发现。
$configurationRoot = Join-Path $infrastructureRoot "EntityFrameworkCore/Configurations"
if (-not (Test-Path -LiteralPath $configurationRoot)) {
    $issues.Add("缺少实体映射目录：$configurationRoot")
}

# 检查 DbContext 是否继续使用程序集扫描加载 IEntityTypeConfiguration。
$dbContextFile = Join-Path $infrastructureRoot "EntityFrameworkCore/DedsiNativeDbContext.cs"
if (Test-Path -LiteralPath $dbContextFile) {
    $assemblyScan = Select-String -LiteralPath $dbContextFile -Pattern "ApplyConfigurationsFromAssembly" -Quiet
    if (-not $assemblyScan) {
        $issues.Add("DbContext 未调用 ApplyConfigurationsFromAssembly，实体映射可能不会生效。")
    }
}
else {
    $issues.Add("未找到 DbContext 文件：$dbContextFile")
}

if ($issues.Count -gt 0) {
    Write-Output "持久化静态检查发现 $($issues.Count) 个问题："
    $issues | ForEach-Object { Write-Output "- $_" }
    exit 1
}

Write-Output "持久化静态检查通过：未发现 Endpoint 直接依赖 DbContext，且映射扫描配置存在。"
exit 0
