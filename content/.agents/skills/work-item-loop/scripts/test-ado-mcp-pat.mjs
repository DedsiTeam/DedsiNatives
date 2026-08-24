#!/usr/bin/env node

import { mkdirSync, mkdtempSync, realpathSync, rmSync, writeFileSync } from "node:fs";
import { spawnSync } from "node:child_process";
import { tmpdir } from "node:os";
import { resolve } from "node:path";
import {
  parseEnvFile,
  projectRootsForWorktrees,
  resolvePatEnvironment,
} from "../../../../.codex/ado-mcp-pat.mjs";

const fixtureRoot = mkdtempSync(resolve(tmpdir(), "dedsi-ado-pat-test-"));

function git(args, cwd) {
  const result = spawnSync("git", args, { cwd, encoding: "utf8" });
  if (result.status !== 0) throw new Error(result.stderr || result.stdout || "git failed");
}

try {
  mkdirSync(resolve(fixtureRoot, ".codex"));
  writeFileSync(resolve(fixtureRoot, ".env.local"), 'ADO_PAT="test-pat"\n');

  const parsed = parseEnvFile("# comment\nADO_PAT='abc'\n");
  if (parsed.ADO_PAT !== "abc") throw new Error("Failed to parse quoted PAT.");

  const result = resolvePatEnvironment(fixtureRoot, {});
  const decoded = Buffer.from(result.encodedPat, "base64").toString("utf8");
  if (decoded !== "codex:test-pat") throw new Error("PAT was not encoded in the required format.");

  const inherited = resolvePatEnvironment(fixtureRoot, { PERSONAL_ACCESS_TOKEN: "already-encoded" });
  if (inherited.encodedPat !== "already-encoded") throw new Error("Host environment must take precedence.");

  const repositoryRoot = resolve(fixtureRoot, "repository");
  const primaryProjectRoot = resolve(repositoryRoot, "project");
  const worktreeRoot = resolve(fixtureRoot, "worktree");
  mkdirSync(primaryProjectRoot, { recursive: true });
  writeFileSync(resolve(primaryProjectRoot, "tracked.txt"), "fixture\n");
  git(["init", "-b", "main"], repositoryRoot);
  git(["config", "user.name", "PAT Launcher Test"], repositoryRoot);
  git(["config", "user.email", "pat-launcher@example.invalid"], repositoryRoot);
  git(["add", "project/tracked.txt"], repositoryRoot);
  git(["commit", "-m", "fixture"], repositoryRoot);
  git(["worktree", "add", "-b", "test-worktree", worktreeRoot], repositoryRoot);
  writeFileSync(resolve(primaryProjectRoot, ".env.local"), "ADO_PAT=shared-pat\n");

  const linkedProjectRoot = resolve(worktreeRoot, "project");
  const worktreeCandidates = projectRootsForWorktrees(linkedProjectRoot);
  if (!worktreeCandidates.includes(realpathSync(primaryProjectRoot))) {
    throw new Error(`Primary project root was not discovered: ${JSON.stringify(worktreeCandidates)}`);
  }
  const worktreeResult = resolvePatEnvironment(linkedProjectRoot, {});
  const decodedWorktreePat = Buffer.from(worktreeResult.encodedPat, "base64").toString("utf8");
  if (decodedWorktreePat !== "codex:shared-pat") throw new Error("Worktree did not reuse the primary PAT.");

  console.log("ado MCP PAT launcher tests passed");
} finally {
  rmSync(fixtureRoot, { recursive: true, force: true });
}
