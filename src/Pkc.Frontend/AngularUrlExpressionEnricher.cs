using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Pkc.Core;
using Pkc.Core.Discovery;

namespace Pkc.Frontend;

internal sealed class AngularUrlExpressionEnricher
{
    private static readonly HashSet<string> ExcludedDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", ".pkc", "bin", "obj", "node_modules", "dist", "build", "coverage", "knowledge"
    };

    public async Task<FactDocument> EnrichAsync(
        string repositoryPath,
        FactDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);
        ArgumentNullException.ThrowIfNull(document);

        var rootPath = Path.GetFullPath(repositoryPath);
        var typeScriptPath = FindTypeScriptModule(rootPath);
        if (typeScriptPath is null)
        {
            return document;
        }

        var analyzerPath = Path.Combine(Path.GetTempPath(), $"pkc-angular-url-{Guid.NewGuid():N}.cjs");
        var resolverPath = Path.Combine(Path.GetTempPath(), $"pkc-angular-url-resolver-{Guid.NewGuid():N}.cjs");
        string? scopePath = null;
        try
        {
            await File.WriteAllTextAsync(analyzerPath, NodeScript, cancellationToken);
            await File.WriteAllTextAsync(resolverPath, await ReadResolverScriptAsync(cancellationToken), cancellationToken);

            var startInfo = new ProcessStartInfo
            {
                FileName = "node",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add(analyzerPath);
            startInfo.ArgumentList.Add(rootPath);
            startInfo.ArgumentList.Add(typeScriptPath);
            startInfo.ArgumentList.Add(resolverPath);
            scopePath = NodeSemanticScope.Apply(startInfo, rootPath);

            using var process = new Process { StartInfo = startInfo };
            try
            {
                if (!process.Start())
                {
                    return document;
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                return document;
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
            _ = await stderrTask;
            if (process.ExitCode != 0)
            {
                return document;
            }

            var output = JsonSerializer.Deserialize<AstOutput>(stdout, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (output is null || output.Facts.Count == 0)
            {
                return document;
            }

            var facts = document.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
            foreach (var fact in output.Facts)
            {
                if (facts.ContainsKey(fact.Id) || HasEquivalentApiCall(document, fact))
                {
                    continue;
                }

                facts[fact.Id] = new EvidenceFact(
                    fact.Id,
                    fact.Kind,
                    fact.Name,
                    fact.Container,
                    new SourceLocation(fact.Source.Path, fact.Source.StartLine, fact.Source.EndLine),
                    [],
                    fact.Metadata);
            }

            return document with
            {
                Facts = facts.Values.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray()
            };
        }
        finally
        {
            TryDelete(analyzerPath);
            TryDelete(resolverPath);
            NodeSemanticScope.TryDelete(scopePath);
        }
    }

    private static bool HasEquivalentApiCall(FactDocument document, AstFact candidate) =>
        document.Facts.Any(existing =>
            existing.Kind == "ui-api-call" &&
            string.Equals(existing.Source.Path, candidate.Source.Path, StringComparison.Ordinal) &&
            existing.Source.StartLine == candidate.Source.StartLine &&
            string.Equals(existing.Container, candidate.Container, StringComparison.Ordinal) &&
            existing.Metadata.TryGetValue("httpMethod", out var existingMethod) &&
            candidate.Metadata.TryGetValue("httpMethod", out var candidateMethod) &&
            string.Equals(existingMethod, candidateMethod, StringComparison.OrdinalIgnoreCase) &&
            existing.Metadata.TryGetValue("routeKey", out var existingRoute) &&
            candidate.Metadata.TryGetValue("routeKey", out var candidateRoute) &&
            string.Equals(existingRoute, candidateRoute, StringComparison.Ordinal));

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
            .Any(segment => ExcludedDirectoryNames.Contains(segment)) ||
            SemanticSourceScope.Excludes(rootPath, path);
    }

    private static bool IsUnderRoot(string rootPath, string path)
    {
        var relative = Path.GetRelativePath(rootPath, path);
        return relative != ".." &&
               !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
               !Path.IsPathRooted(relative);
    }

    private static async Task<string> ReadResolverScriptAsync(CancellationToken cancellationToken)
    {
        const string resourceName = "Pkc.Frontend.AngularUrlExpressionResolver.cjs";
        await using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource: {resourceName}");
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // Best-effort cleanup of temporary analyzer files.
        }
    }

    private sealed record AstOutput(IReadOnlyList<AstFact> Facts);
    private sealed record AstFact(
        string Id,
        string Kind,
        string Name,
        string? Container,
        AstSource Source,
        Dictionary<string, string> Metadata);
    private sealed record AstSource(string Path, int StartLine, int EndLine);

    private const string NodeScript = """
const fs = require('fs');
const path = require('path');

const root = path.resolve(process.argv[2]);
const ts = require(process.argv[3]);
const { createResolver } = require(process.argv[4]);
const resolver = createResolver(ts, root);
const excluded = new Set(['.git', '.pkc', 'bin', 'obj', 'node_modules', 'dist', 'build', 'coverage', 'knowledge']);
const semanticScope = process.env.PKC_SEMANTIC_SCOPE ? JSON.parse(fs.readFileSync(process.env.PKC_SEMANTIC_SCOPE, 'utf8')) : null;
const excludedAreas = new Set(semanticScope ? semanticScope.excludedAreas : []);
const withheldFiles = new Set(semanticScope ? semanticScope.withheldFiles : []);
const httpMethods = new Set(['get', 'post', 'put', 'patch', 'delete']);
const facts = [];

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
function nearestMethod(node) {
  for (let current = node.parent; current; current = current.parent) {
    if (ts.isMethodDeclaration(current) && current.name) return current.name.getText(current.getSourceFile());
  }
  return null;
}
function nearestClass(node) {
  for (let current = node.parent; current; current = current.parent) {
    if (ts.isClassDeclaration(current) && current.name) return current.name.text;
  }
  return null;
}

for (const file of walk(root).sort()) {
  const text = fs.readFileSync(file, 'utf8');
  if (!/\.(?:get|post|put|patch|delete)\s*(?:<[^>]+>)?\s*\(/i.test(text)) continue;

  const sf = resolver.loadSourceFile(file);
  if (!sf) continue;

  function visit(node) {
    if (ts.isCallExpression(node) && ts.isPropertyAccessExpression(node.expression) && node.arguments.length > 0) {
      const method = node.expression.name.text.toLowerCase();
      if (httpMethods.has(method)) {
        const url = resolver.resolveStringExpression(node.arguments[0]);
        if (url && (url.startsWith('/') || /^https?:\/\//i.test(url))) {
          const source = location(sf, node);
          const metadata = {
            framework: 'angular-static',
            analysisMode: 'typescript-ast-syntactic',
            analysisConfidence: 'medium',
            typescriptSemanticContext: 'syntax-only-no-type-checker',
            analysisCaveat: 'http-method-name-and-bounded-url-expression-detected-without-receiver-type-checking',
            httpReceiverResolution: 'syntactic-unverified',
            urlResolution: 'bounded-typescript-expression',
            httpMethod: method.toUpperCase(),
            url,
            routeKey: resolver.normalizeRouteKey(url),
            client: node.expression.expression.getText(sf)
          };
          const ownerClass = nearestClass(node);
          if (ownerClass) {
            metadata.ownerClass = ownerClass;
            metadata.ownerResolution = 'typescript-ast-class-parent';
          }
          const name = `${method.toUpperCase()} ${url}`;
          facts.push({
            id: `tsast:${source.path}:${source.startLine}:ui-api-call:${name}`,
            kind: 'ui-api-call',
            name,
            container: nearestMethod(node),
            source,
            metadata
          });
        }
      }
    }
    ts.forEachChild(node, visit);
  }

  visit(sf);
}

process.stdout.write(JSON.stringify({ facts }));
""";
}
