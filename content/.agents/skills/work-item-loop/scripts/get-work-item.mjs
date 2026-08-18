#!/usr/bin/env node

import { readFileSync, readdirSync, statSync } from "node:fs";
import { resolve, basename } from "node:path";

const allowedStatuses = new Set(["draft", "ready", "in-progress", "failed", "blocked", "completed", "cancelled"]);
const allowedStages = new Set(["backlog", "domain", "backend", "frontend", "verifying", "done"]);
const allowedPriorities = new Set(["critical", "high", "medium", "low"]);
const requiredMetadata = [
  "work-item-id", "work-item-title", "work-item-status", "work-item-stage",
  "work-item-priority", "work-item-domain", "work-item-scope",
  "work-item-attempt", "work-item-updated-at",
];

function parseArgs(argv) {
  const options = { workItemsPath: "docs/workItems", mode: "next", json: false, maxRetries: undefined };
  for (let index = 0; index < argv.length; index += 1) {
    const argument = argv[index];
    if (argument === "--json") options.json = true;
    else if (argument === "--work-items-path") options.workItemsPath = argv[++index];
    else if (argument === "--mode") options.mode = argv[++index]?.toLowerCase();
    else if (argument === "--max-retries") options.maxRetries = Number(argv[++index]);
    else if (argument === "--help" || argument === "-h") {
      console.log("Usage: get-work-item.mjs [--work-items-path PATH] [--mode next|list|validate] [--max-retries N] [--json]");
      process.exit(0);
    } else throw new Error(`Unknown argument: ${argument}`);
  }
  if (!new Set(["next", "list", "validate"]).has(options.mode)) throw new Error(`Invalid mode: ${options.mode}`);
  if (options.maxRetries !== undefined && (!Number.isInteger(options.maxRetries) || options.maxRetries < 1)) {
    throw new Error("--max-retries must be a positive integer.");
  }
  return options;
}

function markdownFiles(directory) {
  const result = [];
  for (const entry of readdirSync(directory, { withFileTypes: true })) {
    const path = resolve(directory, entry.name);
    if (entry.isDirectory()) result.push(...markdownFiles(path));
    else if (entry.isFile() && entry.name.endsWith(".md") && !entry.name.startsWith("_")) result.push(path);
  }
  return result.sort((left, right) => basename(left).localeCompare(basename(right)) || left.localeCompare(right));
}

function frontMatter(markdown) {
  const match = markdown.match(/^---\s*\r?\n([\s\S]*?)\r?\n---(?:\r?\n|$)/);
  if (!match) return {};
  const metadata = {};
  for (const line of match[1].split(/\r?\n/)) {
    const field = line.match(/^\s*([A-Za-z0-9-]+)\s*:\s*(.*?)\s*$/);
    if (!field) continue;
    let value = field[2].trim();
    if ((value.startsWith('"') && value.endsWith('"')) || (value.startsWith("'") && value.endsWith("'"))) value = value.slice(1, -1);
    metadata[field[1]] = value.trim();
  }
  return metadata;
}

function loadQueue(directory) {
  const errors = [];
  const items = markdownFiles(directory).flatMap((path) => {
    const markdown = readFileSync(path, "utf8");
    const metadata = frontMatter(markdown);
    for (const name of requiredMetadata) if (!metadata[name]) errors.push(`${basename(path)}: missing metadata '${name}'.`);
    if (!metadata["work-item-id"]) return [];

    const status = metadata["work-item-status"];
    const stage = metadata["work-item-stage"];
    const priority = metadata["work-item-priority"];
    const attempt = Number(metadata["work-item-attempt"]);
    if (!allowedStatuses.has(status)) errors.push(`${basename(path)}: invalid status '${status}'.`);
    if (!allowedStages.has(stage)) errors.push(`${basename(path)}: invalid stage '${stage}'.`);
    if (!allowedPriorities.has(priority)) errors.push(`${basename(path)}: invalid priority '${priority}'.`);
    if (!Number.isInteger(attempt) || attempt < 0) errors.push(`${basename(path)}: work-item-attempt must be a non-negative integer.`);
    if (metadata["work-item-updated-at"] && Number.isNaN(Date.parse(metadata["work-item-updated-at"]))) {
      errors.push(`${basename(path)}: work-item-updated-at must be an ISO 8601 timestamp.`);
    }
    if (!markdown.includes("<!-- LOOP_LOG_START -->") || !markdown.includes("<!-- LOOP_LOG_END -->")) {
      errors.push(`${basename(path)}: missing LOOP_LOG_START or LOOP_LOG_END marker.`);
    }
    return [{
      Id: metadata["work-item-id"], Title: metadata["work-item-title"], Status: status,
      Stage: stage, Priority: priority, Domain: metadata["work-item-domain"],
      Scope: metadata["work-item-scope"], Attempt: attempt,
      UpdatedAt: metadata["work-item-updated-at"], FileName: basename(path), Path: path,
    }];
  });

  const counts = new Map();
  for (const item of items) counts.set(item.Id, (counts.get(item.Id) ?? 0) + 1);
  for (const [id, count] of counts) if (count > 1) errors.push(`duplicate work-item-id '${id}'.`);
  const inProgress = items.filter((item) => item.Status === "in-progress");
  if (inProgress.length > 1) errors.push(`multiple in-progress work items found: ${inProgress.map((item) => item.Id).join(", ")}.`);
  if (errors.length) throw new Error(`Work item queue is invalid:\n - ${errors.join("\n - ")}`);
  return items;
}

function selectNext(items, maxRetries) {
  const statusOrder = { "in-progress": 0, failed: 1, ready: 2 };
  const priorityOrder = { critical: 0, high: 1, medium: 2, low: 3 };
  const eligible = items.filter((item) =>
    ["in-progress", "failed", "ready"].includes(item.Status)
    && !(item.Status === "failed" && maxRetries !== undefined && item.Attempt >= maxRetries));
  eligible.sort((left, right) => statusOrder[left.Status] - statusOrder[right.Status]
    || priorityOrder[left.Priority] - priorityOrder[right.Priority]
    || left.Id.localeCompare(right.Id));
  return eligible[0] ?? null;
}

try {
  const options = parseArgs(process.argv.slice(2));
  const directory = resolve(options.workItemsPath);
  if (!statSync(directory).isDirectory()) throw new Error(`Work items path is not a directory: ${directory}`);
  const items = loadQueue(directory);
  const result = options.mode === "validate"
    ? { Valid: true, Count: items.length }
    : options.mode === "list"
      ? items.sort((left, right) => left.Id.localeCompare(right.Id))
      : selectNext(items, options.maxRetries);
  console.log(options.json ? JSON.stringify(result, null, 2) : result === null ? "null" : JSON.stringify(result, null, 2));
} catch (error) {
  console.error(error instanceof Error ? error.message : String(error));
  process.exit(1);
}
