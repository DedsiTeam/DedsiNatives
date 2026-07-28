[CmdletBinding()]
param(
    [Parameter()]
    [ValidateRange(1, 100)]
    [int]$MaxItems = 1,

    [Parameter()]
    [ValidateRange(1, 20)]
    [int]$MaxRetries = 3,

    [Parameter()]
    [string]$CodexCommand = "codex",

    [Parameter()]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"
$contentRoot = $PSScriptRoot
$workItemsPath = Join-Path $contentRoot "docs/workItems"
$selectorPath = Join-Path $contentRoot ".agents/skills/work-item-loop/scripts/Get-WorkItem.ps1"

if (-not (Test-Path -LiteralPath $selectorPath -PathType Leaf)) {
    throw "Work item selector was not found: $selectorPath"
}

& $selectorPath -WorkItemsPath $workItemsPath -Mode Validate | Out-Null

$completedThisRun = 0
$invocations = 0
$maxInvocations = $MaxItems * ($MaxRetries + 1)

while ($completedThisRun -lt $MaxItems) {
    $candidateJson = & $selectorPath -WorkItemsPath $workItemsPath -Mode Next -Json
    $candidate = $candidateJson | ConvertFrom-Json

    if ($null -eq $candidate) {
        Write-Host "工作项队列为空：没有 ready、failed 或 in-progress 工作项。"
        break
    }

    if ($candidate.Status -eq "failed" -and [int]$candidate.Attempt -ge $MaxRetries) {
        Write-Warning "工作项 $($candidate.Id) 已达到最大尝试次数 $MaxRetries。请人工检查后再将其恢复为 ready。"
        break
    }

    $invocations++
    if ($invocations -gt $maxInvocations) {
        Write-Warning "已达到本次执行的 Agent 调用上限 $maxInvocations，Loop 停止。"
        break
    }

    $prompt = @"
使用 `$work-item-loop 处理且只处理以下工作项：

- ID: $($candidate.Id)
- 文件: $($candidate.Path)
- 当前状态: $($candidate.Status)
- 当前阶段: $($candidate.Stage)
- 已尝试次数: $($candidate.Attempt)

从 content 根目录执行完整的领域、后端、前端与验证闭环。严格遵守工作项协议：
- 不得选择或修改第二个工作项；
- ready/failed 领取时递增 attempt，in-progress 恢复时不递增；
- 进入后端和前端阶段时，必须按变更范围完整读取并应用对应模块 `.agents/skills`；
- 在工作项执行日志中记录实际使用的后端和前端 Skill；
- 最终必须写回 completed、failed 或 blocked；
- 不得提交、推送、重置 Git 或执行未经授权的破坏性操作；
- 达到终态后立即退出，由外层执行器决定是否启动下一项。
"@

    Write-Host "[$($invocations)] 准备处理 $($candidate.Id): $($candidate.Title)"

    if ($DryRun) {
        Write-Host "DryRun：不会启动 Codex。"
        Write-Output $prompt
        break
    }

    & $CodexCommand exec `
        --cd $contentRoot `
        --sandbox workspace-write `
        $prompt

    if ($LASTEXITCODE -ne 0) {
        throw "Codex 执行失败，退出码：$LASTEXITCODE。工作项保留当前状态以便恢复。"
    }

    $allItemsJson = & $selectorPath -WorkItemsPath $workItemsPath -Mode List -Json
    $allItems = @($allItemsJson | ConvertFrom-Json)
    $updated = $allItems | Where-Object Id -eq $candidate.Id | Select-Object -First 1

    if ($null -eq $updated) {
        throw "Agent 返回后找不到工作项 $($candidate.Id)。Loop 已停止。"
    }

    switch ($updated.Status) {
        "completed" {
            $completedThisRun++
            Write-Host "工作项 $($updated.Id) 已完成（$completedThisRun/$MaxItems）。"
        }
        "failed" {
            if ([int]$updated.Attempt -ge $MaxRetries) {
                Write-Warning "工作项 $($updated.Id) 验证失败并达到最大尝试次数。"
                break
            }
            Write-Warning "工作项 $($updated.Id) 本轮失败，将开始下一次有限重试。"
        }
        "blocked" {
            Write-Warning "工作项 $($updated.Id) 已阻塞，需要人工处理。"
            break
        }
        default {
            throw "Agent 返回后工作项 $($updated.Id) 仍为 '$($updated.Status)'，没有进入终态。Loop 已停止。"
        }
    }

    if ($updated.Status -eq "blocked") {
        break
    }
}

Write-Host "Loop 结束：本次完成 $completedThisRun 个工作项，调用 Agent $invocations 次。"
