#!/usr/bin/env node

import { chmodSync, cpSync, mkdtempSync, readFileSync, readdirSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { dirname, resolve } from "node:path";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

function run(command, args, cwd, env = process.env) {
  const result = spawnSync(command, args, { cwd, env, encoding: "utf8" });
  if (result.error) throw result.error;
  if (result.status !== 0) throw new Error(result.stderr || result.stdout || `${command} failed`);
  return result.stdout.trim();
}

const scriptRoot = dirname(fileURLToPath(import.meta.url));
const sourceRoot = resolve(scriptRoot, "../../../..");
const fixtureRoot = mkdtempSync(resolve(tmpdir(), "dedsi-agent-loop-test-"));
const projectRoot = resolve(fixtureRoot, "project");
const remoteRoot = resolve(fixtureRoot, "remote.git");
const stateRoot = resolve(fixtureRoot, "state");
const fakeCodex = resolve(fixtureRoot, "fake-codex.mjs");

try {
  cpSync(sourceRoot, projectRoot, { recursive: true });
  run("git", ["init", "--bare", remoteRoot], fixtureRoot);
  run("git", ["init", "-b", "main"], projectRoot);
  run("git", ["config", "user.name", "Agent Loop Test"], projectRoot);
  run("git", ["config", "user.email", "agent-loop@example.invalid"], projectRoot);
  run("git", ["add", "-A"], projectRoot);
  run("git", ["commit", "-m", "test fixture"], projectRoot);
  run("git", ["remote", "add", "origin", remoteRoot], projectRoot);
  run("git", ["push", "-u", "origin", "main"], projectRoot);

  writeFileSync(fakeCodex, `#!/usr/bin/env node
import { existsSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { basename, resolve } from "node:path";
const args = process.argv.slice(2);
const schema = args[args.indexOf("--output-schema") + 1];
const output = args[args.indexOf("--output-last-message") + 1];
const cwd = args[args.indexOf("--cd") + 1];
const prompt = args.at(-1);
const stateRoot = process.env.AGENT_LOOP_TEST_STATE;
mkdirSync(stateRoot, { recursive: true });
const statePath = resolve(stateRoot, "state.json");
const state = existsSync(statePath) ? JSON.parse(readFileSync(statePath, "utf8")) : { next: 101, claimSizes: [] };
let result;
if (basename(schema) === "claim-result.schema.json") {
  const capacity = Number(prompt.match(/本次最多领取: (\\d+)/)?.[1] ?? 1);
  const retryMarker = resolve(stateRoot, "retry-101");
  const items = [];
  if (existsSync(retryMarker) && !state.retryClaimed && capacity > 0) {
    state.retryClaimed = true;
    items.push({ id: 101, title: "Test Item 101", attempt: 2 });
  }
  while (items.length < capacity && state.next < 104) {
    items.push({ id: state.next, title: "Test Item " + state.next++, attempt: 1 });
  }
  state.claimSizes.push(items.length);
  writeFileSync(statePath, JSON.stringify(state));
  result = items.length ? { status: "claimed", items, message: "claimed" } : { status: "empty", items: [], message: "empty" };
} else if (basename(schema) === "worker-result.schema.json") {
  const id = Number(prompt.match(/工作项 #(\\d+)/)?.[1]);
  writeFileSync(resolve(cwd, "worker-" + id + ".txt"), "implemented " + id + "\\n");
  await new Promise((resolvePromise) => setTimeout(resolvePromise, 150));
  result = { workItemId: id, status: "verified", summary: "verified", commitMessage: "feat: implement " + id, validations: ["fake test passed"] };
} else {
  const id = Number(prompt.match(/工作项 #(\\d+)/)?.[1]);
  const retryMarker = resolve(stateRoot, "retry-101");
  if (id === 101 && !existsSync(retryMarker)) {
    writeFileSync(retryMarker, "retry");
    result = { workItemId: id, status: "failed", pullRequestId: id + 1000, pullRequestUrl: "https://example.invalid/pr/" + id, message: "retry integration" };
  } else {
    result = { workItemId: id, status: "merged", pullRequestId: id + 1000, pullRequestUrl: "https://example.invalid/pr/" + id, message: "merged" };
  }
}
writeFileSync(output, JSON.stringify(result));
`);
  chmodSync(fakeCodex, 0o755);

  const env = { ...process.env, AGENT_LOOP_TEST_STATE: stateRoot, AGENT_LOOP_CONCURRENCY: "7" };
  const output = run(process.execPath, [
    "agent-loop.mjs",
    "--codex-command", fakeCodex,
    "--ado-project", "TestProject",
    "--max-items", "3",
  ], projectRoot, env);

  const state = JSON.parse(readFileSync(resolve(stateRoot, "state.json"), "utf8"));
  if (JSON.stringify(state.claimSizes) !== JSON.stringify([2, 1, 1])) {
    throw new Error(`Expected claim batches [2,1,1], got ${JSON.stringify(state.claimSizes)}.`);
  }
  const remoteBranches = run("git", ["for-each-ref", "--format=%(refname:short)", "refs/heads/codex/"], remoteRoot).split("\n").filter(Boolean);
  if (remoteBranches.length !== 3) throw new Error(`Expected 3 pushed work-item branches, got ${remoteBranches.length}.`);
  if (!output.includes("已合并 3 个工作项")) throw new Error(`Unexpected Loop summary: ${output}`);
  console.log("agent-loop integration test passed");
} finally {
  rmSync(fixtureRoot, { recursive: true, force: true });
}
