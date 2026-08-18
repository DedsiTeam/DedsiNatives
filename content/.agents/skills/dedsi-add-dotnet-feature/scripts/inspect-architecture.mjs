#!/usr/bin/env node

import { existsSync, readFileSync, readdirSync } from "node:fs";
import { dirname, basename, resolve } from "node:path";
import { fileURLToPath } from "node:url";

function files(root, predicate) {
  if (!existsSync(root)) return [];
  return readdirSync(root, { withFileTypes: true }).flatMap((entry) => {
    const path = resolve(root, entry.name);
    if (["bin", "obj"].includes(entry.name)) return [];
    return entry.isDirectory() ? files(path, predicate) : entry.isFile() && predicate(path) ? [path] : [];
  });
}

function option(argv, name, fallback) {
  const index = argv.indexOf(name);
  if (index >= 0 && !argv[index + 1]) throw new Error(`${name} requires a path.`);
  return resolve(index >= 0 ? argv[index + 1] : fallback);
}

function csharp(path) {
  return readFileSync(path, "utf8").replace(/@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|\/\/[^\r\n]*|\/\*[\s\S]*?\*\//g,
    (match) => match.replace(/[^\r\n]/g, " "));
}

try {
  const scriptRoot = dirname(fileURLToPath(import.meta.url));
  const dotnetRoot = option(process.argv.slice(2), "--dotnet-root", resolve(scriptRoot, "../../../../src/dotnet"));
  const source = resolve(dotnetRoot, "src");
  const core = resolve(source, "DedsiNative.Core");
  const endpoints = resolve(source, "DedsiNative.Endpoints");
  const issues = [];

  for (const path of files(core, (file) => /EventHandler\.cs$/.test(file))) {
    issues.push(`Core 只能定义领域事件，EventHandler 必须放在 Endpoints/Applications：${path}`);
  }

  const coreProject = resolve(core, "DedsiNative.Core.csproj");
  if (!existsSync(coreProject)) issues.push(`未找到 Core 项目文件：${coreProject}`);
  else {
    const project = readFileSync(coreProject, "utf8");
    for (const forbidden of ["EntityFrameworkCore", "FastEndpoints", "DedsiNative.Infrastructure", "DedsiNative.Endpoints", "DedsiNative.Host"]) {
      if (project.includes(forbidden)) issues.push(`Core 项目禁止依赖 ${forbidden}：${coreProject}`);
    }
  }

  for (const path of files(endpoints, (file) => file.endsWith(".cs"))) {
    const code = csharp(path);
    if (/\b(?:IDedsiNativeDbContext|DedsiNativeDbContext)\b/.test(code)) issues.push(`Endpoints 禁止直接依赖 DbContext：${path}`);
  }

  if (issues.length) {
    console.error(`架构静态检查发现 ${issues.length} 个问题：\n${issues.map((issue) => `- ${issue}`).join("\n")}`);
    process.exit(1);
  }
  console.log(`架构静态检查通过：Core 无 EventHandler 和基础设施依赖，Endpoints 无 DbContext 直连。`);
} catch (error) {
  console.error(error instanceof Error ? error.message : String(error));
  process.exit(1);
}
