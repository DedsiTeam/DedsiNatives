#!/usr/bin/env node

import { existsSync, readFileSync, readdirSync } from "node:fs";
import { dirname, extname, basename, resolve } from "node:path";
import { fileURLToPath } from "node:url";

function parseRoot(argv) {
  const index = argv.indexOf("--dotnet-root");
  if (index >= 0 && !argv[index + 1]) throw new Error("--dotnet-root requires a path.");
  return resolve(index >= 0 ? argv[index + 1] : dirname(fileURLToPath(import.meta.url)), index >= 0 ? "" : "../../../../src/dotnet");
}

function files(root, predicate) {
  if (!existsSync(root)) return [];
  return readdirSync(root, { withFileTypes: true }).flatMap((entry) => {
    const path = resolve(root, entry.name);
    return entry.isDirectory() ? files(path, predicate) : entry.isFile() && predicate(path) ? [path] : [];
  });
}

function csharp(path) {
  return readFileSync(path, "utf8").replace(/@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|\/\/[^\r\n]*|\/\*[\s\S]*?\*\//g,
    (match) => match.replace(/[^\r\n]/g, " "));
}

try {
  const root = parseRoot(process.argv.slice(2));
  const source = resolve(root, "src");
  const endpoints = resolve(source, "DedsiNative.Endpoints");
  const ef = resolve(source, "DedsiNative.Infrastructure/EntityFrameworkCore");
  const queries = resolve(ef, "Queries");
  const repositories = resolve(ef, "Repositories");
  const issues = [];

  if (!existsSync(endpoints)) issues.push(`未找到 Endpoints 项目目录：${endpoints}`);
  for (const path of files(endpoints, (file) => extname(file) === ".cs")) {
    const lines = csharp(path).split(/\r?\n/).flatMap((line, index) => /\b(?:IDedsiNativeDbContext|DedsiNativeDbContext)\b/.test(line) ? [index + 1] : []);
    if (lines.length) issues.push(`Endpoint 禁止直接依赖 IDedsiNativeDbContext 或 DedsiNativeDbContext：${path}，命中行：${lines.join(", ")}`);
  }

  if (!existsSync(queries)) issues.push(`未找到 Query 实现目录：${queries}`);
  for (const path of files(queries, (file) => basename(file).endsWith("Query.cs"))) {
    const code = csharp(path);
    const name = basename(path, ".cs").replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    const constructor = code.match(new RegExp(`\\bclass\\s+${name}\\s*\\((?<parameters>[\\s\\S]*?)\\)\\s*:`));
    if (!constructor) issues.push(`Query 实现必须使用主构造函数：${path}`);
    else {
      const parameters = constructor.groups.parameters;
      if (/\bDedsiNativeDbContext\b/.test(parameters)) issues.push(`Query 实现禁止注入具体 DedsiNativeDbContext：${path}`);
      if (!/\bI[A-Za-z_]\w*DbContext\s+[A-Za-z_]\w*\b/.test(parameters)) issues.push(`Query 实现的主构造函数必须注入 I...DbContext 接口：${path}`);
    }
  }

  if (!existsSync(repositories)) issues.push(`未找到 Repository 实现目录：${repositories}`);
  for (const path of files(repositories, (file) => basename(file).endsWith("Repository.cs"))) {
    const code = csharp(path);
    const name = basename(path, ".cs").replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    const constructor = code.match(new RegExp(`\\bclass\\s+${name}\\s*\\((?<parameters>[\\s\\S]*?)\\)\\s*:`));
    if (!constructor) issues.push(`Repository 实现必须使用主构造函数：${path}`);
    else if (!/\bIDbContextProvider\s*<\s*DedsiNativeDbContext\s*>\s+dbContextProvider\b/.test(constructor.groups.parameters)) {
      issues.push(`Repository 主构造函数必须注入 IDbContextProvider<DedsiNativeDbContext> dbContextProvider：${path}`);
    }
  }

  const configurations = resolve(ef, "Configurations");
  if (!existsSync(configurations)) issues.push(`缺少实体映射目录：${configurations}`);
  const dbContext = resolve(ef, "DedsiNativeDbContext.cs");
  if (!existsSync(dbContext)) issues.push(`未找到 DbContext 文件：${dbContext}`);
  else if (!/\bApplyConfigurationsFromAssembly\s*\(/.test(csharp(dbContext))) issues.push(`DbContext 未调用 ApplyConfigurationsFromAssembly，实体映射可能不会生效：${dbContext}`);

  if (issues.length) {
    console.error(`持久化静态检查发现 ${issues.length} 个问题：\n${issues.map((issue) => `- ${issue}`).join("\n")}`);
    process.exit(1);
  }
  console.log("持久化静态检查通过：Endpoint 未直接依赖 DbContext；Query 与 Repository 的主构造函数注入符合规范；实体映射目录及程序集扫描配置存在。");
} catch (error) {
  console.error(error instanceof Error ? error.message : String(error));
  process.exit(1);
}
