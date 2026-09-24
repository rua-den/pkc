using System.Diagnostics;
using System.Text.Json;
using Pkc.Core;
using Pkc.Core.Discovery;

namespace Pkc.Frontend;

internal sealed class AngularTypeScriptAstScanner
{
    public async Task<AngularAstScanAttempt> TryScanAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default)
    {
        var rootPath = Path.GetFullPath(repositoryPath);
        var typeScriptPath = FindTypeScriptModule(rootPath);
        if (typeScriptPath is null)
        {
            return new AngularAstScanAttempt(null, "local-typescript-runtime-not-found");
        }

        var scriptPath = Path.Combine(Path.GetTempPath(), $"pkc-angular-ast-{Guid.NewGuid():N}.cjs");
        string? scopePath = null;
        try
        {
            await File.WriteAllTextAsync(scriptPath, NodeScript, cancellationToken);

            var startInfo = new ProcessStartInfo
            {
                FileName = "node",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add(scriptPath);
            startInfo.ArgumentList.Add(rootPath);
            startInfo.ArgumentList.Add(typeScriptPath);
            scopePath = NodeSemanticScope.Apply(startInfo, rootPath);

            using var process = new Process { StartInfo = startInfo };
            try
            {
                if (!process.Start())
                {
                    return new AngularAstScanAttempt(null, "node-process-did-not-start");
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                return new AngularAstScanAttempt(null, $"node-unavailable: {exception.Message}");
            }

            using var registration = cancellationToken.Register(() =>
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                    }
                }
                catch
                {
                    // Cancellation cleanup only.
                }
            });

            var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            if (process.ExitCode != 0)
            {
                var reason = string.IsNullOrWhiteSpace(stderr)
                    ? $"typescript-ast-process-exit-{process.ExitCode}"
                    : $"typescript-ast-process-failed: {FirstLine(stderr)}";
                return new AngularAstScanAttempt(null, reason);
            }

            var output = JsonSerializer.Deserialize<AstOutput>(stdout, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (output is null)
            {
                return new AngularAstScanAttempt(null, "typescript-ast-output-invalid");
            }

            var facts = output.Facts.Select(fact => new EvidenceFact(
                    fact.Id,
                    fact.Kind,
                    fact.Name,
                    fact.Container,
                    new SourceLocation(fact.Source.Path, fact.Source.StartLine, fact.Source.EndLine),
                    [],
                    fact.Metadata))
                .OrderBy(fact => fact.Id, StringComparer.Ordinal)
                .ToArray();

            var relations = output.Relations.Select(relation => new EvidenceRelation(
                    relation.FromFactId,
                    relation.Kind,
                    relation.Target,
                    new SourceLocation(relation.Source.Path, relation.Source.StartLine, relation.Source.EndLine)))
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray();

            return new AngularAstScanAttempt(
                new FactDocument("0.4.3-angular-typescript-ast", facts, relations),
                null);
        }
        finally
        {
            try
            {
                File.Delete(scriptPath);
            }
            catch
            {
                // Best-effort cleanup of a temporary analyzer script.
            }

            NodeSemanticScope.TryDelete(scopePath);
        }
    }

    private static string? FindTypeScriptModule(string rootPath)
    {
        var candidateRoots = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { rootPath };

        foreach (var angularJson in Directory.EnumerateFiles(rootPath, "angular.json", SearchOption.AllDirectories)
                     .Where(path => !IsExcluded(rootPath, path)))
        {
            var current = Path.GetDirectoryName(angularJson);
            while (!string.IsNullOrWhiteSpace(current) && IsUnderRoot(rootPath, current))
            {
                candidateRoots.Add(current);
                if (string.Equals(current, rootPath, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                current = Path.GetDirectoryName(current);
            }
        }

        foreach (var candidateRoot in candidateRoots.OrderBy(path => path.Length))
        {
            var candidate = Path.Combine(candidateRoot, "node_modules", "typescript", "lib", "typescript.js");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static bool IsExcluded(string rootPath, string path)
    {
        var relative = Path.GetRelativePath(rootPath, path);
        return relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => segment is ".git" or ".pkc" or "bin" or "obj" or "dist" or "build" or "coverage" or "knowledge") ||
            SemanticSourceScope.Excludes(rootPath, path);
    }

    private static bool IsUnderRoot(string rootPath, string path)
    {
        var relative = Path.GetRelativePath(rootPath, path);
        return relative != ".." &&
               !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
               !Path.IsPathRooted(relative);
    }

    private static string FirstLine(string value) =>
        value.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim() ?? value.Trim();

    internal sealed record AngularAstScanAttempt(FactDocument? Document, string? FailureReason);

    private sealed record AstOutput(IReadOnlyList<AstFact> Facts, IReadOnlyList<AstRelation> Relations);
    private sealed record AstFact(
        string Id,
        string Kind,
        string Name,
        string? Container,
        AstSource Source,
        Dictionary<string, string> Metadata);
    private sealed record AstRelation(string FromFactId, string Kind, string Target, AstSource Source);
    private sealed record AstSource(string Path, int StartLine, int EndLine);

    private const string NodeScript = """
const fs = require('fs');
const path = require('path');

const root = path.resolve(process.argv[2]);
const tsPath = process.argv[3];
const ts = require(tsPath);
const excluded = new Set(['.git', '.pkc', 'bin', 'obj', 'node_modules', 'dist', 'build', 'coverage', 'knowledge']);
const semanticScope = process.env.PKC_SEMANTIC_SCOPE ? JSON.parse(fs.readFileSync(process.env.PKC_SEMANTIC_SCOPE, 'utf8')) : null;
const excludedAreas = new Set(semanticScope ? semanticScope.excludedAreas : []);
const withheldFiles = new Set(semanticScope ? semanticScope.withheldFiles : []);
const httpMethods = new Set(['get', 'post', 'put', 'patch', 'delete']);
const facts = [];
const relations = [];

function walk(dir, result = []) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    if (entry.isDirectory() && excluded.has(entry.name)) continue;
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) { if (!excludedAreas.has(relative(full))) walk(full, result); }
    else if (entry.isFile() && entry.name.endsWith('.ts') && !entry.name.endsWith('.d.ts') && !withheldFiles.has(relative(full))) result.push(full);
  }
  return result;
}

function normalizePath(value) { return value.split(path.sep).join('/'); }
function relative(file) { return normalizePath(path.relative(root, file)); }
function location(sf, node) {
  const start = sf.getLineAndCharacterOfPosition(node.getStart(sf));
  const end = sf.getLineAndCharacterOfPosition(node.getEnd());
  return { path: relative(sf.fileName), startLine: start.line + 1, endLine: end.line + 1 };
}
function baseMetadata(kind) {
  const metadata = {
    framework: 'angular-static',
    analysisMode: 'typescript-ast-syntactic',
    analysisConfidence: kind === 'ui-api-call' ? 'medium' : 'high',
    typescriptSemanticContext: 'syntax-only-no-type-checker'
  };
  if (kind === 'ui-api-call') {
    metadata.analysisCaveat = 'http-method-name-and-url-shape-detected-without-receiver-type-checking';
    metadata.httpReceiverResolution = 'syntactic-unverified';
  }
  return metadata;
}
function addFact(sf, node, kind, name, container, metadata = {}) {
  const source = location(sf, node);
  const fact = {
    id: `tsast:${source.path}:${source.startLine}:${kind}:${name}`,
    kind, name, container: container || null, source,
    metadata: Object.assign(baseMetadata(kind), metadata)
  };
  facts.push(fact);
  return fact;
}
function addRelation(from, kind, target, source) { relations.push({ fromFactId: from, kind, target, source }); }
function propName(node) {
  if (!node) return null;
  if (ts.isIdentifier(node) || ts.isStringLiteral(node) || ts.isNumericLiteral(node)) return node.text;
  return node.getText();
}
function stringValue(node, sf) {
  if (!node) return null;
  if (ts.isStringLiteral(node) || ts.isNoSubstitutionTemplateLiteral(node)) return node.text;
  if (ts.isTemplateExpression(node)) {
    const text = node.getText(sf);
    return text.length >= 2 ? text.slice(1, -1) : text;
  }
  return null;
}
function getDecorators(node) {
  if (ts.canHaveDecorators && ts.getDecorators) return ts.getDecorators(node) || [];
  return node.decorators || [];
}
function hasComponentDecorator(node) {
  return getDecorators(node).some(decorator => {
    const expression = decorator.expression;
    return ts.isCallExpression(expression) && propName(expression.expression) === 'Component';
  });
}
function nearestMethod(node) {
  for (let current = node.parent; current; current = current.parent) {
    if (ts.isMethodDeclaration(current) && current.name) return propName(current.name);
  }
  return null;
}
function nearestClass(node) {
  for (let current = node.parent; current; current = current.parent) {
    if (ts.isClassDeclaration(current) && current.name) return current.name.text;
  }
  return null;
}
function callName(call) {
  const expression = call.expression;
  if (ts.isIdentifier(expression)) return expression.text;
  if (ts.isPropertyAccessExpression(expression)) return expression.name.text;
  return null;
}
function collectCalls(node) {
  const calls = [];
  function visit(child) {
    if (ts.isCallExpression(child)) {
      const name = callName(child);
      if (name) calls.push(name);
    }
    ts.forEachChild(child, visit);
  }
  if (node) ts.forEachChild(node, visit);
  return [...new Set(calls)];
}
function transitiveCalls(methodMap, rootName, maxDepth = 3) {
  const result = [];
  const seen = new Set([rootName]);
  const queue = [{ name: rootName, depth: 0 }];
  while (queue.length) {
    const current = queue.shift();
    const method = methodMap.get(current.name);
    if (!method || !method.body) continue;
    for (const called of collectCalls(method.body)) {
      if (seen.has(called)) continue;
      seen.add(called);
      result.push(called);
      if (current.depth + 1 < maxDepth && methodMap.has(called)) queue.push({ name: called, depth: current.depth + 1 });
    }
  }
  return result;
}
function normalizeRouteKey(value) {
  let route = value.split('?')[0].split('#')[0].trim();
  route = route.replace(/\$\{[^}]+\}/g, '{param}').replace(/\{[^}/]+\}|:[A-Za-z0-9_]+/g, '{param}');
  while (route.includes('//')) route = route.replace(/\/\//g, '/');
  if (!route.startsWith('/')) route = '/' + route;
  return route.replace(/\/$/, '').toLowerCase();
}

for (const file of walk(root).sort()) {
  const text = fs.readFileSync(file, 'utf8');
  const sf = ts.createSourceFile(file, text, ts.ScriptTarget.Latest, true, ts.ScriptKind.TS);

  function visit(node) {
    if (ts.isClassDeclaration(node) && node.name && node.name.text.endsWith('Component') && hasComponentDecorator(node)) {
      const methodMap = new Map();
      for (const member of node.members) {
        if (ts.isMethodDeclaration(member) && member.name) methodMap.set(propName(member.name), member);
      }
      const loadMethods = transitiveCalls(methodMap, 'ngOnInit');
      const metadata = {};
      if (loadMethods.length) metadata.loadMethods = loadMethods.join(', ');
      addFact(sf, node, 'ui-screen', node.name.text, null, metadata);
    }

    if (ts.isObjectLiteralExpression(node)) {
      let routePath = null;
      let component = null;
      for (const property of node.properties) {
        if (!ts.isPropertyAssignment(property)) continue;
        const name = propName(property.name);
        if (name === 'path') routePath = stringValue(property.initializer, sf);
        if (name === 'component' && ts.isIdentifier(property.initializer)) component = property.initializer.text;
      }
      if (routePath !== null && component) {
        const displayPath = '/' + routePath.replace(/^\/+|\/+$/g, '');
        const fact = addFact(sf, node, 'ui-route', displayPath, component, { path: displayPath, component });
        addRelation(fact.id, 'renders', component, fact.source);
      }
    }

    if (ts.isCallExpression(node) && ts.isPropertyAccessExpression(node.expression)) {
      const method = node.expression.name.text.toLowerCase();
      if (httpMethods.has(method) && node.arguments.length > 0) {
        const url = stringValue(node.arguments[0], sf);
        if (url && (url.startsWith('/') || url.startsWith('http'))) {
          const container = nearestMethod(node);
          const metadata = {
            httpMethod: method.toUpperCase(),
            url,
            routeKey: normalizeRouteKey(url),
            client: node.expression.expression.getText(sf)
          };
          const ownerClass = nearestClass(node);
          if (ownerClass) {
            metadata.ownerClass = ownerClass;
            metadata.ownerResolution = 'typescript-ast-class-parent';
          }
          addFact(sf, node, 'ui-api-call', `${method.toUpperCase()} ${url}`, container, metadata);
        }
      }
    }

    ts.forEachChild(node, visit);
  }

  visit(sf);
}

process.stdout.write(JSON.stringify({ facts, relations }));
""";
}
