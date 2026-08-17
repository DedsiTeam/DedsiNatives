param(
    [Parameter(Mandatory = $false)]
    [string]$DotnetRoot
)

if ([string]::IsNullOrWhiteSpace($DotnetRoot)) {
    $DotnetRoot = Join-Path $PSScriptRoot "../../../.."
}

$resolvedRoot = Resolve-Path -LiteralPath $DotnetRoot -ErrorAction Stop
$sourceRoot = Join-Path $resolvedRoot "src"
$endpointsRoot = Join-Path $sourceRoot "DedsiNative.Endpoints"
$infrastructureRoot = Join-Path $sourceRoot "DedsiNative.Infrastructure"
$entityFrameworkCoreRoot = Join-Path $infrastructureRoot "EntityFrameworkCore"
$queriesRoot = Join-Path $entityFrameworkCoreRoot "Queries"
$repositoriesRoot = Join-Path $entityFrameworkCoreRoot "Repositories"
$issues = [System.Collections.Generic.List[string]]::new()

function Get-CSharpCode {
    param(
        [Parameter(Mandatory = $true)]
        [string]$LiteralPath
    )

    $content = Get-Content -LiteralPath $LiteralPath -Raw
    # 屏蔽字符串、字符和注释，同时保留换行，以免 XML 文档或示例代码产生误报，
    # 并让后续 Endpoint 命中位置仍能对应原文件行号。
    $nonCodePattern = @'
(?s)@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|//[^\r\n]*|/\*.*?\*/
'@
    $maskMatch = [System.Text.RegularExpressions.MatchEvaluator] {
        param($match)
        return [System.Text.RegularExpressions.Regex]::Replace($match.Value, "[^`r`n]", " ")
    }

    return [System.Text.RegularExpressions.Regex]::Replace($content, $nonCodePattern.Trim(), $maskMatch)
}

# Endpoint 只能依赖 Query 或 Repository 契约，不得直接依赖接口或具体 DbContext。
if (Test-Path -LiteralPath $endpointsRoot) {
    foreach ($endpointFile in (Get-ChildItem -LiteralPath $endpointsRoot -Recurse -File -Filter "*.cs")) {
        $code = Get-CSharpCode -LiteralPath $endpointFile.FullName
        $matches = $code -split '\r?\n' |
            Select-String -Pattern "\b(?:IDedsiNativeDbContext|DedsiNativeDbContext)\b"
        if ($matches) {
            $lineNumbers = ($matches.LineNumber | Sort-Object -Unique) -join ", "
            $issues.Add("Endpoint 禁止直接依赖 IDedsiNativeDbContext 或 DedsiNativeDbContext：$($endpointFile.FullName)，命中行：$lineNumbers")
        }
    }
}
else {
    $issues.Add("未找到 Endpoints 项目目录：$endpointsRoot")
}

# Query 实现必须在主构造函数中注入 DbContext 接口，不能注入具体实现。
if (Test-Path -LiteralPath $queriesRoot) {
    foreach ($queryFile in (Get-ChildItem -LiteralPath $queriesRoot -Recurse -File -Filter "*Query.cs")) {
        $code = Get-CSharpCode -LiteralPath $queryFile.FullName
        $className = [System.IO.Path]::GetFileNameWithoutExtension($queryFile.Name)
        $escapedClassName = [System.Text.RegularExpressions.Regex]::Escape($className)
        $constructorMatch = [System.Text.RegularExpressions.Regex]::Match(
            $code,
            "\bclass\s+$escapedClassName\s*\((?<parameters>[\s\S]*?)\)\s*:")

        if (-not $constructorMatch.Success) {
            $issues.Add("Query 实现必须使用主构造函数：$($queryFile.FullName)")
            continue
        }

        $parameters = $constructorMatch.Groups["parameters"].Value
        if ($parameters -match "\bDedsiNativeDbContext\b") {
            $issues.Add("Query 实现禁止注入具体 DedsiNativeDbContext：$($queryFile.FullName)")
        }

        if ($parameters -notmatch "\bI[A-Za-z_]\w*DbContext\s+[A-Za-z_]\w*\b") {
            $issues.Add("Query 实现的主构造函数必须注入 I...DbContext 接口：$($queryFile.FullName)")
        }
    }
}
else {
    $issues.Add("未找到 Query 实现目录：$queriesRoot")
}

# Repository 实现必须通过 ABP DbContextProvider 获取具体 DbContext。
if (Test-Path -LiteralPath $repositoriesRoot) {
    foreach ($repositoryFile in (Get-ChildItem -LiteralPath $repositoriesRoot -Recurse -File -Filter "*Repository.cs")) {
        $code = Get-CSharpCode -LiteralPath $repositoryFile.FullName
        $className = [System.IO.Path]::GetFileNameWithoutExtension($repositoryFile.Name)
        $escapedClassName = [System.Text.RegularExpressions.Regex]::Escape($className)
        $constructorMatch = [System.Text.RegularExpressions.Regex]::Match(
            $code,
            "\bclass\s+$escapedClassName\s*\((?<parameters>[\s\S]*?)\)\s*:")

        if (-not $constructorMatch.Success) {
            $issues.Add("Repository 实现必须使用主构造函数：$($repositoryFile.FullName)")
            continue
        }

        $parameters = $constructorMatch.Groups["parameters"].Value
        if ($parameters -notmatch "\bIDbContextProvider\s*<\s*DedsiNativeDbContext\s*>\s+dbContextProvider\b") {
            $issues.Add("Repository 主构造函数必须注入 IDbContextProvider<DedsiNativeDbContext> dbContextProvider：$($repositoryFile.FullName)")
        }
    }
}
else {
    $issues.Add("未找到 Repository 实现目录：$repositoriesRoot")
}

# 检查每个实体映射是否会被当前程序集扫描机制发现。
$configurationRoot = Join-Path $entityFrameworkCoreRoot "Configurations"
if (-not (Test-Path -LiteralPath $configurationRoot)) {
    $issues.Add("缺少实体映射目录：$configurationRoot")
}

# 检查 DbContext 是否继续使用程序集扫描加载 IEntityTypeConfiguration。
$dbContextFile = Join-Path $entityFrameworkCoreRoot "DedsiNativeDbContext.cs"
if (Test-Path -LiteralPath $dbContextFile) {
    $dbContextCode = Get-CSharpCode -LiteralPath $dbContextFile
    if ($dbContextCode -notmatch "\bApplyConfigurationsFromAssembly\s*\(") {
        $issues.Add("DbContext 未调用 ApplyConfigurationsFromAssembly，实体映射可能不会生效：$dbContextFile")
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

Write-Output "持久化静态检查通过：Endpoint 未直接依赖 DbContext；Query 与 Repository 的主构造函数注入符合规范；实体映射目录及程序集扫描配置存在。"
exit 0
