#!/usr/bin/env node

import {
  closeSync, existsSync, mkdtempSync, openSync, readFileSync,
  rmdirSync, unlinkSync, writeFileSync,
} from "node:fs";
import { tmpdir } from "node:os";
import { dirname, isAbsolute, join, relative, resolve } from "node:path";
import { spawn, spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const contentRoot = dirname(fileURLToPath(import.meta.url));
const configPath = resolve(contentRoot, "agent-loop.config.json");
const schemaRoot = resolve(contentRoot, ".agents/skills/work-item-loop/references");
const claimSchemaPath = resolve(schemaRoot, "claim-result.schema.json");
const workerSchemaPath = resolve(schemaRoot, "worker-result.schema.json");
const integrationSchemaPath = resolve(schemaRoot, "integration-result.schema.json");
const CONCURRENT_SLOTS = 2;

function positiveInteger(value, name, maximum) {
  const parsed = Number(value);
  if (!Number.isInteger(parsed) || parsed < 1 || parsed > maximum) {
    throw new Error(`${name} must be an integer from 1 to ${maximum}.`);
  }
  return parsed;
}

function loadConfig() {
  if (!existsSync(configPath)) throw new Error(`Loop configuration was not found: ${configPath}`);
  const config = JSON.parse(readFileSync(configPath, "utf8"));
  return {
    maxItems: positiveInteger(process.env.AGENT_LOOP_MAX_ITEMS ?? config.maxItems, "maxItems", 100),
    maxRetries: positiveInteger(process.env.AGENT_LOOP_MAX_RETRIES ?? config.maxRetries, "maxRetries", 20),
    codexCommand: process.env.AGENT_LOOP_CODEX_COMMAND ?? config.codexCommand ?? "codex",
    remote: process.env.AGENT_LOOP_GIT_REMOTE ?? config.git?.remote ?? "origin",
    baseBranch: process.env.AGENT_LOOP_BASE_BRANCH ?? config.git?.baseBranch ?? "main",
    branchPrefix: process.env.AGENT_LOOP_BRANCH_PREFIX ?? config.git?.branchPrefix ?? "codex/wi-",
    keepFailedWorktrees: config.git?.keepFailedWorktrees !== false,
    adoProject: process.env.ADO_PROJECT ?? config.azureDevOps?.project,
    autoCompletePullRequests: config.azureDevOps?.autoCompletePullRequests !== false,
    dryRun: false,
  };
}

function parseArgs(argv) {
  const options = loadConfig();
  for (let index = 0; index < argv.length; index += 1) {
    const argument = argv[index];
    if (argument === "--dry-run") options.dryRun = true;
    else if (argument === "--max-items") options.maxItems = positiveInteger(argv[++index], "--max-items", 100);
    else if (argument === "--max-retries") options.maxRetries = positiveInteger(argv[++index], "--max-retries", 20);
    else if (argument === "--codex-command") options.codexCommand = argv[++index];
    else if (argument === "--remote") options.remote = argv[++index];
    else if (argument === "--base-branch") options.baseBranch = argv[++index];
    else if (argument === "--ado-project") options.adoProject = argv[++index];
    else if (argument === "--keep-failed-worktrees") options.keepFailedWorktrees = true;
    else if (argument === "--remove-failed-worktrees") options.keepFailedWorktrees = false;
    else if (argument === "--no-auto-complete") options.autoCompletePullRequests = false;
    else if (argument === "--help" || argument === "-h") {
      console.log(`Usage: node agent-loop.mjs [options]

Options:
  --max-items N                Maximum successfully merged work items (1-100)
  --max-retries N              Maximum attempts per work item (1-20)
  --ado-project NAME           Azure DevOps project (or ADO_PROJECT)
  --remote NAME                Git remote, default: origin
  --base-branch NAME           Pull request target branch, default: main
  --codex-command PATH         Codex executable, default: codex
  --no-auto-complete           Create PRs without enabling autocomplete
  --keep-failed-worktrees      Preserve failed/blocked worktrees (default)
  --remove-failed-worktrees    Remove failed/blocked worktrees
  --dry-run                    Print resolved orchestration without MCP/Git writes`);
      process.exit(0);
    } else throw new Error(`Unknown argument: ${argument}`);
  }
  if (!options.codexCommand) throw new Error("codexCommand cannot be empty.");
  return options;
}

function unresolved(value) {
  return !value || /^\{\{[^}]+\}\}$/.test(value) || value === "CHANGE_ME";
}

function validateRuntimeConfig(options) {
  const missing = [];
  if (unresolved(options.adoProject)) missing.push("ADO_PROJECT / --ado-project");
  if (missing.length) throw new Error(`Missing Azure DevOps Loop configuration: ${missing.join(", ")}.`);
}

function run(command, args, options = {}) {
  const result = spawnSync(command, args, {
    cwd: options.cwd, encoding: "utf8", stdio: options.inherit ? "inherit" : "pipe",
  });
  if (result.error) throw result.error;
  if (result.status !== 0) {
    throw new Error((result.stderr || result.stdout || `${command} exited with code ${result.status}`).trim());
  }
  return result.stdout?.trim() ?? "";
}

function git(args, options = {}) {
  return run("git", args, options);
}

function repositoryNameFromRemote(remoteUrl) {
  const normalized = remoteUrl.trim().replace(/[?#].*$/, "").replace(/\/+$/, "").replace(/\.git$/i, "");
  const encodedName = normalized.split(/[/:]/).filter(Boolean).at(-1);
  if (!encodedName) throw new Error(`Cannot identify an Azure Repos repository from Git remote: ${remoteUrl}`);
  try { return decodeURIComponent(encodedName); }
  catch { return encodedName; }
}

function acquireLock(repositoryRoot) {
  const commonDirValue = git(["rev-parse", "--git-common-dir"], { cwd: repositoryRoot });
  const commonDir = isAbsolute(commonDirValue) ? commonDirValue : resolve(repositoryRoot, commonDirValue);
  const lockPath = resolve(commonDir, "dedsi-agent-loop.lock");
  if (existsSync(lockPath)) {
    let stale = true;
    try {
      const previous = JSON.parse(readFileSync(lockPath, "utf8"));
      if (previous.pid && previous.hostname === (process.env.HOSTNAME ?? "local")) {
        try { process.kill(previous.pid, 0); stale = false; } catch { stale = true; }
      }
    } catch { stale = false; }
    if (!stale) throw new Error(`Another Agent Loop owns ${lockPath}.`);
    unlinkSync(lockPath);
  }
  const descriptor = openSync(lockPath, "wx");
  writeFileSync(descriptor, JSON.stringify({ pid: process.pid, hostname: process.env.HOSTNAME ?? "local", startedAt: new Date().toISOString() }));
  closeSync(descriptor);
  return () => { if (existsSync(lockPath)) unlinkSync(lockPath); };
}

function spawnProcess(command, args, options = {}) {
  return new Promise((resolvePromise, rejectPromise) => {
    const child = spawn(command, args, { cwd: options.cwd, stdio: "inherit", env: process.env });
    child.on("error", rejectPromise);
    child.on("exit", (code, signal) => {
      if (code === 0) resolvePromise();
      else rejectPromise(new Error(`${command} exited with ${signal ? `signal ${signal}` : `code ${code}`}.`));
    });
  });
}

async function runCodexStructured(options, cwd, prompt, schemaPath) {
  const outputDirectory = mkdtempSync(join(tmpdir(), "dedsi-codex-result-"));
  const outputPath = resolve(outputDirectory, "result.json");
  try {
    await spawnProcess(options.codexCommand, [
      "exec", "--cd", cwd, "--approve-for-me",
      "--output-schema", schemaPath, "--output-last-message", outputPath, prompt,
    ], { cwd });
    if (!existsSync(outputPath)) throw new Error("Codex did not produce a structured final result.");
    return JSON.parse(readFileSync(outputPath, "utf8"));
  } finally {
    if (existsSync(outputPath)) unlinkSync(outputPath);
    if (existsSync(outputDirectory)) rmdirSync(outputDirectory);
  }
}

function claimPrompt(options, runId, capacity, runningIds) {
  return `使用 $work-item-loop 的 dispatcher 模式，通过 ado MCP 从 Azure DevOps 领取工作项；不要修改本地文件。

- Project: ${options.adoProject}
- 本次最多领取: ${capacity}
- 最大尝试次数: ${options.maxRetries}
- Run ID: ${runId}
- 当前已运行、不得重复领取的 ID: ${runningIds.length ? runningIds.join(", ") : "无"}

只允许访问 Project ${options.adoProject}。所有支持 Project 参数的 MCP 工具都必须显式传入该值；不得枚举、查询或修改其他 Project。任何返回资源所属 Project 不一致时返回 blocked。

严格按 work-item-protocol 执行：使用 wit_query 的 wiql 动作直接查询整个 Project，不得使用或查找保存查询。WIQL 必须限定 [System.TeamProject] = @project、[System.Tags] CONTAINS 'codex-loop'，并且标签包含 codex-ready、codex-in-progress 或 codex-failed 之一。排除当前运行 ID与并发批次冲突项；仅当当前运行 ID 为空且本次拥有完整空闲容量时，才可单独领取一个 codex-exclusive 项，否则排除它。优先恢复可恢复的 codex-in-progress，再选择未达重试上限的 codex-failed，最后选择 codex-ready。领取时保留无关标签，移除旧的 codex 状态/阶段/attempt/run 标签，添加 codex-in-progress、codex-stage-domain、codex-attempt-N、codex-run-${runId}，并追加 Markdown 评论。更新后重新读取确认。没有候选项返回 empty；MCP 不可用或写入未确认返回 error；业务或并发条件阻塞返回 blocked。只返回 schema 指定的 JSON。`;
}

async function claimItems(options, runId, capacity, runningIds) {
  return runCodexStructured(options, contentRoot, claimPrompt(options, runId, capacity, runningIds), claimSchemaPath);
}

function slug(value) {
  return value.normalize("NFKD").toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "").slice(0, 48) || "work-item";
}

function branchFor(options, item) {
  return `${options.branchPrefix}${item.id}-${slug(item.title)}`;
}

function parseWorktrees(repositoryRoot) {
  return git(["worktree", "list", "--porcelain"], { cwd: repositoryRoot }).split(/\n\n+/).filter(Boolean).map((record) => {
    const lines = record.split("\n");
    return {
      path: lines.find((line) => line.startsWith("worktree "))?.slice(9),
      branch: lines.find((line) => line.startsWith("branch refs/heads/"))?.slice(18),
    };
  });
}

function refExists(repositoryRoot, ref) {
  return spawnSync("git", ["show-ref", "--verify", "--quiet", ref], { cwd: repositoryRoot }).status === 0;
}

function prepareWorktree(options, repositoryRoot, worktreeParent, item) {
  const branch = branchFor(options, item);
  const existing = parseWorktrees(repositoryRoot).find((entry) => entry.branch === branch && entry.path && existsSync(entry.path));
  if (existing) return { branch, path: existing.path, reused: true };
  git(["worktree", "prune"], { cwd: repositoryRoot });
  const path = resolve(worktreeParent, `wi-${item.id}`);
  if (refExists(repositoryRoot, `refs/heads/${branch}`)) {
    git(["worktree", "add", path, branch], { cwd: repositoryRoot, inherit: true });
  } else if (refExists(repositoryRoot, `refs/remotes/${options.remote}/${branch}`)) {
    git(["worktree", "add", "-b", branch, path, `${options.remote}/${branch}`], { cwd: repositoryRoot, inherit: true });
  } else {
    git(["worktree", "add", "-b", branch, path, `${options.remote}/${options.baseBranch}`], { cwd: repositoryRoot, inherit: true });
  }
  return { branch, path, reused: false };
}

function workerPrompt(options, item, branch) {
  return `使用 $work-item-loop 的 worker 模式处理且只处理 Azure DevOps 工作项 #${item.id}。

- Project: ${options.adoProject}
- Repository: ${options.repository}（从当前 Git remote ${options.remote} 自动识别）
- Branch: ${branch}
- Attempt: ${item.attempt}

只允许访问 Project ${options.adoProject}。所有支持 Project 参数的 MCP 工具都必须显式传入该值；不得枚举、查询或修改其他 Project。工作项所属 Project 不一致时立即返回 blocked。

通过 ado MCP 完整读取工作项、验收标准和评论，确认它仍为 codex-in-progress 且 attempt 匹配。按照项目 AGENTS.md、work-item-loop、协议及适用模块 Skill 完成领域、后端、前端和验证闭环。阶段变化通过 MCP 更新 codex-stage-* 标签并追加 Markdown 评论；不得创建本地需求或工作项文档。

本 Worker 只修改代码并运行验证，不得 commit、push、创建 PR 或合并；这些由外层 Dispatcher 处理。验证全部通过返回 verified，并给出简洁 commitMessage。可重试实现/验证失败时先通过 MCP 写为 codex-failed；需要业务决定、权限或危险操作时写为 codex-blocked。只返回 schema 指定的 JSON。`;
}

async function runWorker(options, worktreeContentRoot, item, branch) {
  const result = await runCodexStructured(options, worktreeContentRoot, workerPrompt(options, item, branch), workerSchemaPath);
  if (result.workItemId !== item.id) throw new Error(`Worker returned work item ${result.workItemId}; expected ${item.id}.`);
  return result;
}

function commitAndPush(options, worktree, item, workerResult) {
  const dirty = git(["status", "--porcelain"], { cwd: worktree.path });
  if (dirty) {
    git(["add", "-A"], { cwd: worktree.path });
    const requested = workerResult.commitMessage?.split(/\r?\n/, 1)[0]?.trim();
    git(["commit", "-m", (requested || `feat(WI-${item.id}): ${item.title}`).slice(0, 120)], { cwd: worktree.path, inherit: true });
  } else {
    const ahead = Number(git(["rev-list", "--count", `${options.remote}/${options.baseBranch}..HEAD`], { cwd: worktree.path }));
    if (!Number.isInteger(ahead) || ahead < 1) {
      throw new Error(`Work item ${item.id} was verified but its branch has no changes or commits to integrate.`);
    }
  }
  git(["push", "-u", options.remote, worktree.branch], { cwd: worktree.path, inherit: true });
  return git(["rev-parse", "HEAD"], { cwd: worktree.path });
}

function integrationPrompt(options, item, worktree, commit, workerResult) {
  return `通过 ado MCP 完成 Azure DevOps 工作项 #${item.id} 的集成与终态回写；不要修改本地文件。

- Project: ${options.adoProject}
- Repository: ${options.repository}（从当前 Git remote ${options.remote} 自动识别）
- Source branch: ${worktree.branch}
- Target branch: ${options.baseBranch}
- Commit: ${commit}
- Local verification: ${workerResult.validations.join("; ") || "已由 Worker 验证"}
- Auto-complete: ${options.autoCompletePullRequests ? "启用" : "禁用"}

只允许访问 Project ${options.adoProject}。所有支持 Project 参数的 MCP 工具都必须显式传入该值；不得枚举、查询或修改其他 Project。PR、Repository 或工作项所属 Project 不一致时返回 blocked。

查找此 source/target 已存在的活动 PR；没有则创建，标题包含 #${item.id} 与工作项标题，描述包含实现摘要和验证证据。将 PR 与工作项关联，添加 codex-pr 标签和评论。${options.autoCompletePullRequests ? "设置 PR autocomplete，并持续通过 MCP 检查 PR 与 Pipeline，直到合并成功或出现明确失败/冲突；不要绕过分支策略。" : "不设置 autocomplete；返回 blocked 并说明需要人工合并。"}

只有确认 PR 已合并到目标分支后，才移除运行/阶段/失败/blocked 标签，添加 codex-completed、codex-stage-done，并写入包含 PR、commit、验证证据的评论。Pipeline 失败写 codex-failed；需要人工解决的合并冲突或审批写 codex-blocked。只返回 schema 指定的 JSON。`;
}

async function integrate(options, item, worktree, commit, workerResult) {
  const result = await runCodexStructured(options, contentRoot, integrationPrompt(options, item, worktree, commit, workerResult), integrationSchemaPath);
  if (result.workItemId !== item.id) throw new Error(`Integration returned work item ${result.workItemId}; expected ${item.id}.`);
  return result;
}

async function reportInfrastructureFailure(options, item, message) {
  const prompt = `只允许访问 Azure DevOps Project ${options.adoProject}，不得枚举、查询或修改其他 Project。通过 ado MCP 将该 Project 的工作项 #${item.id} 从 codex-in-progress 写为 codex-failed，保留无关标签，追加 Markdown 评论说明本地编排失败：${message.slice(0, 1000)}。然后只返回 integration-result schema：workItemId=${item.id}，status=failed，pullRequestId=null，pullRequestUrl=null。不要修改本地文件。`;
  try { await runCodexStructured(options, contentRoot, prompt, integrationSchemaPath); }
  catch (error) { console.error(`无法回写工作项 ${item.id}：${error instanceof Error ? error.message : String(error)}`); }
}

function removeWorktree(repositoryRoot, worktree) {
  if (existsSync(worktree.path)) git(["worktree", "remove", worktree.path], { cwd: repositoryRoot, inherit: true });
}

async function processItem(options, repositoryRoot, contentRelativePath, worktreeParent, item) {
  let worktree;
  try {
    worktree = prepareWorktree(options, repositoryRoot, worktreeParent, item);
    const workerRoot = resolve(worktree.path, contentRelativePath);
    if (!existsSync(workerRoot)) throw new Error(`Worker content root does not exist: ${workerRoot}`);
    console.log(`工作项 ${item.id} 已进入 ${worktree.branch}（${workerRoot}）。`);
    const workerResult = await runWorker(options, workerRoot, item, worktree.branch);
    if (workerResult.status !== "verified") return { item, status: workerResult.status, message: workerResult.summary, worktree };
    const commit = commitAndPush(options, worktree, item, workerResult);
    const integrationResult = await integrate(options, item, worktree, commit, workerResult);
    return { item, status: integrationResult.status, message: integrationResult.message, worktree };
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error);
    await reportInfrastructureFailure(options, item, message);
    return { item, status: "failed", message, worktree };
  }
}

async function main() {
  const options = parseArgs(process.argv.slice(2));
  validateRuntimeConfig(options);
  const repositoryRoot = git(["rev-parse", "--show-toplevel"], { cwd: contentRoot });
  const remoteUrl = git(["remote", "get-url", options.remote], { cwd: repositoryRoot });
  options.repository = repositoryNameFromRemote(remoteUrl);
  if (options.dryRun) {
    console.log(JSON.stringify({
      mode: "dry-run", concurrency: CONCURRENT_SLOTS, maxItems: options.maxItems, maxRetries: options.maxRetries,
      azureDevOps: { project: options.adoProject, repository: options.repository, queue: "project WIQL" },
      git: { remote: options.remote, baseBranch: options.baseBranch, branchPrefix: options.branchPrefix },
      note: "Dry run does not call MCP, create worktrees, commit, push, or create pull requests.",
    }, null, 2));
    return;
  }

  const contentRelativePath = relative(repositoryRoot, contentRoot);
  if (git(["status", "--porcelain"], { cwd: repositoryRoot })) {
    throw new Error("Agent Loop requires a clean primary Git worktree. Commit or stash existing changes first.");
  }

  const releaseLock = acquireLock(repositoryRoot);
  const worktreeParent = mkdtempSync(join(tmpdir(), "dedsi-agent-worktrees-"));
  const runId = `${Date.now()}-${process.pid}`;
  const running = new Map();
  let completed = 0;
  let invocations = 0;
  let stopClaiming = false;
  let queueEmpty = false;
  const maxInvocations = options.maxItems * (options.maxRetries + 1);
  const cleanup = () => releaseLock();
  process.once("SIGINT", () => { cleanup(); process.exit(130); });
  process.once("SIGTERM", () => { cleanup(); process.exit(143); });

  try {
    git(["fetch", options.remote, options.baseBranch], { cwd: repositoryRoot, inherit: true });
    while (completed < options.maxItems && (invocations < maxInvocations || running.size > 0) && (!queueEmpty || running.size > 0)) {
      while (!stopClaiming && !queueEmpty && running.size < CONCURRENT_SLOTS && completed + running.size < options.maxItems && invocations < maxInvocations) {
        const capacity = Math.min(CONCURRENT_SLOTS - running.size, options.maxItems - completed - running.size, maxInvocations - invocations);
        const claim = await claimItems(options, runId, capacity, [...running.keys()]);
        if (claim.status === "empty") { queueEmpty = true; break; }
        if (claim.status === "blocked") { console.warn(`Dispatcher 已阻塞：${claim.message}`); stopClaiming = true; break; }
        if (claim.status === "error") throw new Error(`Dispatcher failed: ${claim.message}`);
        if (claim.status !== "claimed" || claim.items.length === 0) throw new Error("Dispatcher returned claimed without items.");
        if (claim.items.length > capacity) throw new Error(`Dispatcher claimed ${claim.items.length} items for only ${capacity} available slots.`);
        const ids = new Set();
        for (const item of claim.items) {
          if (ids.has(item.id) || running.has(item.id)) throw new Error(`Dispatcher returned duplicate/running work item ${item.id}.`);
          ids.add(item.id);
          invocations += 1;
          running.set(item.id, processItem(options, repositoryRoot, contentRelativePath, worktreeParent, item));
        }
      }

      if (running.size === 0) break;
      const settled = await Promise.race([...running.entries()].map(([id, promise]) => promise.then((result) => ({ id, result }))));
      running.delete(settled.id);
      const { result } = settled;
      console.log(`工作项 ${result.item.id} 结束：${result.status}。${result.message}`);
      if (result.status === "merged") {
        completed += 1;
        queueEmpty = false;
        if (result.worktree) removeWorktree(repositoryRoot, result.worktree);
      } else {
        if (result.status === "blocked") stopClaiming = true;
        else queueEmpty = false;
        if (result.worktree && !options.keepFailedWorktrees) removeWorktree(repositoryRoot, result.worktree);
      }
    }
  } finally {
    releaseLock();
    try { rmdirSync(worktreeParent); } catch { /* Failed worktrees may intentionally remain. */ }
  }
  console.log(`Loop 结束：已合并 ${completed} 个工作项，启动 Worker ${invocations} 次。`);
}

main().catch((error) => {
  console.error(error instanceof Error ? error.message : String(error));
  process.exitCode = 1;
});
