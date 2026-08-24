#!/usr/bin/env node

import { existsSync, readFileSync, realpathSync } from "node:fs";
import { spawn, spawnSync } from "node:child_process";
import { delimiter, dirname, relative, resolve } from "node:path";
import { fileURLToPath } from "node:url";

function parseEnvFile(text) {
  const values = {};
  for (const sourceLine of text.split(/\r?\n/)) {
    const line = sourceLine.trim();
    if (!line || line.startsWith("#")) continue;
    const separator = line.indexOf("=");
    if (separator < 1) continue;
    const key = line.slice(0, separator).trim();
    let value = line.slice(separator + 1).trim();
    if (value.startsWith('"') && value.endsWith('"')) {
      try { value = JSON.parse(value); }
      catch { throw new Error(`Invalid quoted value for ${key} in .env.local.`); }
    } else if (value.startsWith("'") && value.endsWith("'")) {
      value = value.slice(1, -1);
    }
    values[key] = value;
  }
  return values;
}

function projectRootsForWorktrees(projectRoot) {
  const canonicalProjectRoot = realpathSync(projectRoot);
  const topLevelResult = spawnSync("git", ["-C", canonicalProjectRoot, "rev-parse", "--show-toplevel"], { encoding: "utf8" });
  if (topLevelResult.status !== 0) return [canonicalProjectRoot];

  const currentTopLevel = topLevelResult.stdout.trim();
  const projectRelativePath = relative(currentTopLevel, canonicalProjectRoot);
  const worktreesResult = spawnSync("git", ["-C", currentTopLevel, "worktree", "list", "--porcelain"], { encoding: "utf8" });
  if (worktreesResult.status !== 0) return [canonicalProjectRoot];

  const roots = [canonicalProjectRoot];
  for (const line of worktreesResult.stdout.split(/\r?\n/)) {
    if (!line.startsWith("worktree ")) continue;
    roots.push(resolve(line.slice("worktree ".length), projectRelativePath));
  }
  return [...new Set(roots)];
}

function resolvePatEnvironment(projectRoot, inheritedEnvironment = process.env) {
  if (inheritedEnvironment.PERSONAL_ACCESS_TOKEN) {
    return { encodedPat: inheritedEnvironment.PERSONAL_ACCESS_TOKEN, source: "environment" };
  }

  for (const root of projectRootsForWorktrees(projectRoot)) {
    const envPath = resolve(root, ".env.local");
    if (!existsSync(envPath)) continue;
    const values = parseEnvFile(readFileSync(envPath, "utf8"));
    if (values.PERSONAL_ACCESS_TOKEN) {
      return { encodedPat: values.PERSONAL_ACCESS_TOKEN, source: envPath };
    }
    if (values.ADO_PAT) {
      return {
        encodedPat: Buffer.from(`codex:${values.ADO_PAT}`, "utf8").toString("base64"),
        source: envPath,
      };
    }
  }

  throw new Error("Azure DevOps PAT is not configured. Add ADO_PAT=<your raw PAT> to the project root .env.local file.");
}

function run() {
  const projectRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..");
  let encodedPat;
  try {
    ({ encodedPat } = resolvePatEnvironment(projectRoot));
  } catch (error) {
    console.error(error instanceof Error ? error.message : String(error));
    process.exitCode = 1;
    return;
  }

  const nodeBinDirectory = dirname(process.execPath);
  const npxFileName = process.platform === "win32" ? "npx.cmd" : "npx";
  const siblingNpx = resolve(nodeBinDirectory, npxFileName);
  const npxCandidates = process.platform === "darwin"
    ? [siblingNpx, "/opt/homebrew/bin/npx", "/usr/local/bin/npx"]
    : [siblingNpx];
  const npxCommand = npxCandidates.find((candidate) => existsSync(candidate)) ?? npxFileName;
  const child = spawn(npxCommand, ["--yes", "--prefer-offline", "@azure-devops/mcp", ...process.argv.slice(2)], {
    stdio: "inherit",
    env: {
      ...process.env,
      PATH: [nodeBinDirectory, process.env.PATH].filter(Boolean).join(delimiter),
      NPM_CONFIG_AUDIT: "false",
      NPM_CONFIG_FUND: "false",
      NPM_CONFIG_UPDATE_NOTIFIER: "false",
      PERSONAL_ACCESS_TOKEN: encodedPat,
    },
  });
  child.on("error", (error) => {
    console.error(`Failed to start Azure DevOps MCP: ${error.message}`);
    process.exitCode = 1;
  });
  child.on("exit", (code, signal) => {
    process.exitCode = Number.isInteger(code) ? code : signal ? 1 : 0;
  });
  for (const signal of ["SIGINT", "SIGTERM"]) {
    process.once(signal, () => child.kill(signal));
  }
}

if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) run();

export { parseEnvFile, projectRootsForWorktrees, resolvePatEnvironment };
