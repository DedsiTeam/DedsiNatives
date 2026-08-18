#!/usr/bin/env node

import { existsSync } from "node:fs";
import { dirname, resolve } from "node:path";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

function parseArgs(argv) {
  const options = { maxItems: 1, maxRetries: 3, codexCommand: "codex", dryRun: false };
  for (let index = 0; index < argv.length; index += 1) {
    const argument = argv[index];
    if (argument === "--dry-run") options.dryRun = true;
    else if (argument === "--max-items") options.maxItems = Number(argv[++index]);
    else if (argument === "--max-retries") options.maxRetries = Number(argv[++index]);
    else if (argument === "--codex-command") options.codexCommand = argv[++index];
    else if (argument === "--help" || argument === "-h") {
      console.log("Usage: node agent-loop.mjs [--max-items N] [--max-retries N] [--codex-command PATH] [--dry-run]");
      process.exit(0);
    } else throw new Error(`Unknown argument: ${argument}`);
  }
  if (!Number.isInteger(options.maxItems) || options.maxItems < 1 || options.maxItems > 100) throw new Error("--max-items must be an integer from 1 to 100.");
  if (!Number.isInteger(options.maxRetries) || options.maxRetries < 1 || options.maxRetries > 20) throw new Error("--max-retries must be an integer from 1 to 20.");
  if (!options.codexCommand) throw new Error("--codex-command cannot be empty.");
  return options;
}

function run(command, args, options = {}) {
  const result = spawnSync(command, args, { cwd: options.cwd, encoding: "utf8", stdio: options.inherit ? "inherit" : "pipe" });
  if (result.error) throw result.error;
  if (result.status !== 0) throw new Error((result.stderr || result.stdout || `${command} exited with code ${result.status}`).trim());
  return result.stdout?.trim() ?? "";
}

try {
  const options = parseArgs(process.argv.slice(2));
  const contentRoot = dirname(fileURLToPath(import.meta.url));
  const workItemsPath = resolve(contentRoot, "docs/workItems");
  const selectorPath = resolve(contentRoot, ".agents/skills/work-item-loop/scripts/get-work-item.mjs");
  if (!existsSync(selectorPath)) throw new Error(`Work item selector was not found: ${selectorPath}`);

  run(process.execPath, [selectorPath, "--work-items-path", workItemsPath, "--mode", "validate", "--json"]);
  let completed = 0;
  let invocations = 0;
  const maxInvocations = options.maxItems * (options.maxRetries + 1);

  while (completed < options.maxItems) {
    const candidate = JSON.parse(run(process.execPath, [selectorPath, "--work-items-path", workItemsPath, "--mode", "next", "--max-retries", String(options.maxRetries), "--json"]));
    if (candidate === null) {
      const allItems = JSON.parse(run(process.execPath, [selectorPath, "--work-items-path", workItemsPath, "--mode", "list", "--json"]));
      const exhausted = allItems.filter((item) => item.Status === "failed" && item.Attempt >= options.maxRetries);
      if (exhausted.length) console.warn(`没有可执行工作项；已跳过达到 ${options.maxRetries} 次上限的失败项：${exhausted.map((item) => item.Id).join(", ")}`);
      else console.log("工作项队列为空：没有 ready、可重试 failed 或 in-progress 工作项。");
      break;
    }

    invocations += 1;
    if (invocations > maxInvocations) {
      console.warn(`已达到本次执行的 Agent 调用上限 ${maxInvocations}，Loop 停止。`);
      break;
    }
    const prompt = `使用 $work-item-loop 处理且只处理以下工作项：\n\n- ID: ${candidate.Id}\n- 文件: ${candidate.Path}\n- 当前状态: ${candidate.Status}\n- 当前阶段: ${candidate.Stage}\n- 已尝试次数: ${candidate.Attempt}\n\n从 content 根目录执行完整的领域、后端、前端与验证闭环。严格遵守工作项协议：先完整读取根 AGENTS.md、.agents/skills/work-item-loop/SKILL.md、其协议引用和适用的模块 Skill。按项目 Codex 子智能体规则执行契约检查与后端/前端委派。领取、阶段、日志、终态、验证和权限规则仅以上述共享文件为准。达到终态后立即退出，由外层执行器决定是否启动下一项。`;
    console.log(`[${invocations}] 准备处理 ${candidate.Id}: ${candidate.Title}`);
    if (options.dryRun) {
      console.log(`Dry run：不会启动 Codex。\n命令预览：${options.codexCommand} exec --cd ${contentRoot} --sandbox workspace-write <prompt>\n\n${prompt}`);
      break;
    }

    run(options.codexCommand, ["exec", "--cd", contentRoot, "--sandbox", "workspace-write", prompt], { cwd: contentRoot, inherit: true });
    const allItems = JSON.parse(run(process.execPath, [selectorPath, "--work-items-path", workItemsPath, "--mode", "list", "--json"]));
    const updated = allItems.find((item) => item.Id === candidate.Id);
    if (!updated) throw new Error(`Agent 返回后找不到工作项 ${candidate.Id}。Loop 已停止。`);
    if (updated.Status === "completed") {
      completed += 1;
      console.log(`工作项 ${updated.Id} 已完成（${completed}/${options.maxItems}）。`);
    } else if (updated.Status === "failed" && updated.Attempt < options.maxRetries) {
      console.warn(`工作项 ${updated.Id} 本轮失败，将开始下一次有限重试。`);
    } else if (updated.Status === "failed") {
      console.warn(`工作项 ${updated.Id} 验证失败并达到最大尝试次数；将继续寻找其他工作项。`);
    } else if (updated.Status === "blocked") {
      console.warn(`工作项 ${updated.Id} 已阻塞，需要人工处理。`);
      break;
    } else throw new Error(`Agent 返回后工作项 ${updated.Id} 仍为 '${updated.Status}'，没有进入终态。Loop 已停止。`);
  }
  console.log(`Loop 结束：本次完成 ${completed} 个工作项，调用 Agent ${invocations} 次。`);
} catch (error) {
  console.error(error instanceof Error ? error.message : String(error));
  process.exit(1);
}
