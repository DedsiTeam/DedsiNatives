[CmdletBinding()]
param(
    [Parameter()]
    [string]$WorkItemsPath = "docs/workItems",

    [Parameter()]
    [ValidateSet("Next", "List", "Validate")]
    [string]$Mode = "Next",

    [Parameter()]
    [switch]$Json
)

$ErrorActionPreference = "Stop"

$allowedStatuses = @("draft", "ready", "in-progress", "failed", "blocked", "completed", "cancelled")
$allowedStages = @("backlog", "domain", "backend", "frontend", "verifying", "done")
$allowedPriorities = @("critical", "high", "medium", "low")
$requiredMetadata = @(
    "work-item-id",
    "work-item-title",
    "work-item-status",
    "work-item-stage",
    "work-item-priority",
    "work-item-domain",
    "work-item-scope",
    "work-item-attempt",
    "work-item-updated-at"
)

function Get-FrontMatterContent {
    param(
        [Parameter(Mandatory)]
        [string]$Markdown,

        [Parameter(Mandatory)]
        [string]$Name
    )

    $frontMatter = [regex]::Match($Markdown, '\A---\s*\r?\n(?<value>[\s\S]*?)\r?\n---(?:\r?\n|\z)')
    if (-not $frontMatter.Success) {
        return $null
    }

    $escapedName = [regex]::Escape($Name)
    $pattern = '(?m)^\s*' + $escapedName + '\s*:\s*(?<value>.*?)\s*$'
    $match = [regex]::Match(
        $frontMatter.Groups["value"].Value,
        $pattern,
        [System.Text.RegularExpressions.RegexOptions]::None
    )

    if (-not $match.Success) {
        return $null
    }

    $value = $match.Groups["value"].Value.Trim()
    if (
        ($value.StartsWith('"') -and $value.EndsWith('"')) -or
        ($value.StartsWith("'") -and $value.EndsWith("'"))
    ) {
        $value = $value.Substring(1, $value.Length - 2)
    }

    return $value.Trim()
}

$resolvedWorkItemsPath = Resolve-Path -LiteralPath $WorkItemsPath -ErrorAction Stop
$files = Get-ChildItem -LiteralPath $resolvedWorkItemsPath.Path -Recurse -File -Filter "*.md" |
    Where-Object { -not $_.Name.StartsWith("_", [System.StringComparison]::Ordinal) } |
    Sort-Object Name

$items = @()
$errors = [System.Collections.Generic.List[string]]::new()

foreach ($file in $files) {
    $markdown = Get-Content -LiteralPath $file.FullName -Raw
    $metadata = @{}

    foreach ($name in $requiredMetadata) {
        $metadata[$name] = Get-FrontMatterContent -Markdown $markdown -Name $name
        if ([string]::IsNullOrWhiteSpace($metadata[$name])) {
            $errors.Add("$($file.Name): missing metadata '$name'.")
        }
    }

    if ([string]::IsNullOrWhiteSpace($metadata["work-item-id"])) {
        continue
    }

    $status = $metadata["work-item-status"]
    $stage = $metadata["work-item-stage"]
    $priority = $metadata["work-item-priority"]
    $attempt = 0

    if ($status -notin $allowedStatuses) {
        $errors.Add("$($file.Name): invalid status '$status'.")
    }
    if ($stage -notin $allowedStages) {
        $errors.Add("$($file.Name): invalid stage '$stage'.")
    }
    if ($priority -notin $allowedPriorities) {
        $errors.Add("$($file.Name): invalid priority '$priority'.")
    }
    if (-not [int]::TryParse($metadata["work-item-attempt"], [ref]$attempt) -or $attempt -lt 0) {
        $errors.Add("$($file.Name): work-item-attempt must be a non-negative integer.")
    }

    $items += [pscustomobject]@{
        Id        = $metadata["work-item-id"]
        Title     = $metadata["work-item-title"]
        Status    = $status
        Stage     = $stage
        Priority  = $priority
        Domain    = $metadata["work-item-domain"]
        Scope     = $metadata["work-item-scope"]
        Attempt   = $attempt
        UpdatedAt = $metadata["work-item-updated-at"]
        FileName  = $file.Name
        Path      = $file.FullName
    }
}

$duplicateIds = $items | Group-Object Id | Where-Object Count -gt 1
foreach ($duplicate in $duplicateIds) {
    $errors.Add("duplicate work-item-id '$($duplicate.Name)'.")
}

$inProgress = @($items | Where-Object Status -eq "in-progress")
if ($inProgress.Count -gt 1) {
    $errors.Add("multiple in-progress work items found: $($inProgress.Id -join ', ').")
}

if ($errors.Count -gt 0) {
    throw "Work item queue is invalid:`n - $($errors -join "`n - ")"
}

if ($Mode -eq "Validate") {
    $result = [pscustomobject]@{ Valid = $true; Count = $items.Count }
}
elseif ($Mode -eq "List") {
    $result = @($items | Sort-Object Id)
}
else {
    $statusOrder = @{ "in-progress" = 0; "failed" = 1; "ready" = 2 }
    $priorityOrder = @{ "critical" = 0; "high" = 1; "medium" = 2; "low" = 3 }
    $eligible = @(
        $items |
            Where-Object { $_.Status -in @("in-progress", "failed", "ready") } |
            Sort-Object `
                @{ Expression = { $statusOrder[$_.Status] } }, `
                @{ Expression = { $priorityOrder[$_.Priority] } }, `
                @{ Expression = { $_.Id } }
    )
    $result = if ($eligible.Count -gt 0) { $eligible[0] } else { $null }
}

if ($Json) {
    if ($null -eq $result) {
        Write-Output "null"
    }
    else {
        ConvertTo-Json -InputObject $result -Depth 4
    }
}
else {
    Write-Output $result
}
