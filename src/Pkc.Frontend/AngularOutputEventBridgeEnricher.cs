using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Pkc.Core;

namespace Pkc.Frontend;

internal sealed class AngularOutputEventBridgeEnricher
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

        var apiCalls = document.Facts
            .Where(fact =>
                fact.Kind == "ui-api-call" &&
                !string.IsNullOrWhiteSpace(fact.Container) &&
                fact.Metadata.TryGetValue("ownerClass", out var ownerClass) &&
                !string.IsNullOrWhiteSpace(ownerClass))
            .Select(fact => new ApiCallInput(fact.Id, fact.Metadata["ownerClass"], fact.Container!))
            .ToArray();
        if (apiCalls.Length == 0)
        {
            return document;
        }

        var analyzerPath = Path.Combine(Path.GetTempPath(), $"pkc-angular-output-bridge-{Guid.NewGuid():N}.cjs");
        var apiFactsPath = Path.Combine(Path.GetTempPath(), $"pkc-angular-output-bridge-api-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(
                analyzerPath,
                await ReadBridgeScriptAsync(cancellationToken),
                cancellationToken);
            await File.WriteAllTextAsync(
                apiFactsPath,
                JsonSerializer.Serialize(apiCalls, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }),
                cancellationToken);

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
            startInfo.ArgumentList.Add(apiFactsPath);

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

            var output = JsonSerializer.Deserialize<BridgeOutput>(stdout, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (output is null || output.Facts.Count == 0)
            {
                return document;
            }

            var facts = document.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);
            var idMap = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var fact in output.Facts)
            {
                var existingMatches = document.Facts.Where(candidate =>
                        candidate.Kind == fact.Kind &&
                        string.Equals(candidate.Name, fact.Name, StringComparison.Ordinal) &&
                        string.Equals(candidate.Container, fact.Container, StringComparison.Ordinal) &&
                        string.Equals(candidate.Source.Path, fact.Source.Path, StringComparison.Ordinal) &&
                        candidate.Source.StartLine == fact.Source.StartLine)
                    .Take(2)
                    .ToArray();

                if (existingMatches.Length > 1)
                {
                    continue;
                }

                if (existingMatches.Length == 1)
                {
                    var existing = existingMatches[0];
                    var metadata = new Dictionary<string, string>(existing.Metadata, StringComparer.Ordinal);
                    foreach (var item in fact.Metadata)
                    {
                        metadata[item.Key] = item.Value;
                    }
                    facts[existing.Id] = existing with { Metadata = metadata };
                    idMap[fact.Id] = existing.Id;
                    continue;
                }

                var evidence = new EvidenceFact(
                    fact.Id,
                    fact.Kind,
                    fact.Name,
                    fact.Container,
                    new SourceLocation(fact.Source.Path, fact.Source.StartLine, fact.Source.EndLine),
                    [],
                    fact.Metadata);
                facts[evidence.Id] = evidence;
                idMap[fact.Id] = evidence.Id;
            }

            var relations = document.Relations.ToList();
            var relationKeys = relations.Select(RelationKey).ToHashSet(StringComparer.Ordinal);
            foreach (var relation in output.Relations)
            {
                if (!idMap.TryGetValue(relation.FromFactId, out var fromFactId) ||
                    !facts.ContainsKey(relation.Target))
                {
                    continue;
                }

                var evidence = new EvidenceRelation(
                    fromFactId,
                    relation.Kind,
                    relation.Target,
                    new SourceLocation(relation.Source.Path, relation.Source.StartLine, relation.Source.EndLine));
                if (relationKeys.Add(RelationKey(evidence)))
                {
                    relations.Add(evidence);
                }
            }

            return document with
            {
                Facts = facts.Values.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
                Relations = relations
                    .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                    .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                    .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                    .ToArray()
            };
        }
        finally
        {
            TryDelete(analyzerPath);
            TryDelete(apiFactsPath);
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
            .Any(segment => ExcludedDirectoryNames.Contains(segment));
    }

    private static bool IsUnderRoot(string rootPath, string path)
    {
        var relative = Path.GetRelativePath(rootPath, path);
        return relative != ".." &&
               !relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
               !Path.IsPathRooted(relative);
    }

    private static async Task<string> ReadBridgeScriptAsync(CancellationToken cancellationToken)
    {
        const string resourceName = "Pkc.Frontend.AngularOutputEventBridge.cjs";
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
            // Best-effort cleanup only.
        }
    }

    private static string RelationKey(EvidenceRelation relation) =>
        $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}";

    private sealed record ApiCallInput(string Id, string OwnerClass, string Container);
    private sealed record BridgeOutput(IReadOnlyList<BridgeFact> Facts, IReadOnlyList<BridgeRelation> Relations);
    private sealed record BridgeFact(
        string Id,
        string Kind,
        string Name,
        string? Container,
        BridgeSource Source,
        Dictionary<string, string> Metadata);
    private sealed record BridgeRelation(string FromFactId, string Kind, string Target, BridgeSource Source);
    private sealed record BridgeSource(string Path, int StartLine, int EndLine);
}
