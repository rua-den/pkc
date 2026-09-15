using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Frontend;

internal sealed class AngularServiceIdentityEnricher
{
    private static readonly Regex NamedImportRegex = new(
        "^[\\t ]*\\bimport\\s*\\{(?<bindings>[^}]+)\\}\\s*from\\s*['\\\"](?<module>[^'\\\"]+)['\\\"]",
        RegexOptions.Compiled | RegexOptions.Multiline);

    public async Task<FactDocument> EnrichAsync(
        string repositoryPath,
        FactDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);
        ArgumentNullException.ThrowIfNull(document);

        var root = Path.GetFullPath(repositoryPath);
        var sourcePaths = Directory.EnumerateFiles(root, "*.ts", SearchOption.AllDirectories)
            .Select(path => Normalize(Path.GetRelativePath(root, path)))
            .Where(FrontendSourceScope.IsProductSource)
            .ToHashSet(StringComparer.Ordinal);
        var importCache = new Dictionary<string, IReadOnlyDictionary<string, ImportedType>>(StringComparer.Ordinal);

        var facts = new List<EvidenceFact>(document.Facts.Count);
        foreach (var fact in document.Facts)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (fact.Kind == "ui-api-call" &&
                fact.Metadata.TryGetValue("ownerClass", out var ownerClass) &&
                !string.IsNullOrWhiteSpace(ownerClass))
            {
                var metadata = new Dictionary<string, string>(fact.Metadata, StringComparer.Ordinal)
                {
                    ["ownerClassName"] = ownerClass,
                    ["ownerModule"] = fact.Source.Path,
                    ["ownerClass"] = TypeIdentity(fact.Source.Path, ownerClass),
                    ["ownerIdentityResolution"] = "typescript-source-module+class"
                };
                facts.Add(fact with { Metadata = metadata });
                continue;
            }

            if (fact.Kind == "ui-result-binding" &&
                fact.Metadata.TryGetValue("serviceType", out var serviceType) &&
                !string.IsNullOrWhiteSpace(serviceType))
            {
                var imports = await GetImportsAsync(
                    root,
                    fact.Source.Path,
                    sourcePaths,
                    importCache,
                    cancellationToken);

                if (imports.TryGetValue(serviceType, out var imported))
                {
                    var metadata = new Dictionary<string, string>(fact.Metadata, StringComparer.Ordinal)
                    {
                        ["serviceTypeName"] = serviceType,
                        ["serviceModule"] = imported.ModulePath,
                        ["serviceExportName"] = imported.ExportedName,
                        ["serviceType"] = TypeIdentity(imported.ModulePath, imported.ExportedName),
                        ["serviceIdentityResolution"] = "typescript-active-relative-import+declaration"
                    };
                    facts.Add(fact with { Metadata = metadata });
                    continue;
                }
            }

            facts.Add(fact);
        }

        return document with { Facts = facts };
    }

    private static async Task<IReadOnlyDictionary<string, ImportedType>> GetImportsAsync(
        string root,
        string sourcePath,
        IReadOnlySet<string> sourcePaths,
        IDictionary<string, IReadOnlyDictionary<string, ImportedType>> cache,
        CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(sourcePath, out var cached))
        {
            return cached;
        }

        var result = new Dictionary<string, ImportedType>(StringComparer.Ordinal);
        var ambiguousLocalNames = new HashSet<string>(StringComparer.Ordinal);
        var fullPath = Path.Combine(root, sourcePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
        {
            cache[sourcePath] = result;
            return result;
        }

        var text = await File.ReadAllTextAsync(fullPath, cancellationToken);
        foreach (Match match in NamedImportRegex.Matches(text))
        {
            if (!IsActiveCodePosition(text, match.Index))
            {
                continue;
            }

            var moduleSpecifier = match.Groups["module"].Value;
            var resolvedModule = ResolveRelativeModule(sourcePath, moduleSpecifier, sourcePaths);
            if (resolvedModule is null)
            {
                continue;
            }

            foreach (var rawBinding in match.Groups["bindings"].Value.Split(','))
            {
                var binding = rawBinding.Trim();
                if (binding.StartsWith("type ", StringComparison.Ordinal) || binding.Length == 0)
                {
                    continue;
                }

                var aliasParts = Regex.Split(binding, @"\s+as\s+", RegexOptions.IgnoreCase);
                var exportedName = aliasParts[0].Trim();
                var localName = aliasParts.Length == 2 ? aliasParts[1].Trim() : exportedName;
                if (!IsIdentifier(exportedName) || !IsIdentifier(localName) || ambiguousLocalNames.Contains(localName))
                {
                    continue;
                }

                var imported = new ImportedType(exportedName, resolvedModule);
                if (result.TryGetValue(localName, out var existing) && existing != imported)
                {
                    result.Remove(localName);
                    ambiguousLocalNames.Add(localName);
                    continue;
                }

                result[localName] = imported;
            }
        }

        cache[sourcePath] = result;
        return result;
    }

    private static bool IsActiveCodePosition(string text, int position)
    {
        var state = TypeScriptLexicalState.Code;
        var interpolatedTemplate = false;

        for (var index = 0; index < position; index++)
        {
            var current = text[index];
            var next = index + 1 < text.Length ? text[index + 1] : '\0';

            switch (state)
            {
                case TypeScriptLexicalState.Code:
                    if (current == '/' && next == '/')
                    {
                        state = TypeScriptLexicalState.LineComment;
                        index++;
                    }
                    else if (current == '/' && next == '*')
                    {
                        state = TypeScriptLexicalState.BlockComment;
                        index++;
                    }
                    else if (current == '\'')
                    {
                        state = TypeScriptLexicalState.SingleQuotedString;
                    }
                    else if (current == '"')
                    {
                        state = TypeScriptLexicalState.DoubleQuotedString;
                    }
                    else if (current == '`')
                    {
                        state = TypeScriptLexicalState.TemplateLiteral;
                        interpolatedTemplate = false;
                    }
                    else if (current == '/')
                    {
                        // Conservative fallback for JavaScript/TypeScript regex literals. Treating a
                        // division operator as a regex can only suppress ownership proof; it cannot
                        // create false ownership authority.
                        state = TypeScriptLexicalState.RegexLiteral;
                    }
                    break;

                case TypeScriptLexicalState.LineComment:
                    if (current is '\r' or '\n')
                    {
                        state = TypeScriptLexicalState.Code;
                    }
                    break;

                case TypeScriptLexicalState.BlockComment:
                    if (current == '*' && next == '/')
                    {
                        state = TypeScriptLexicalState.Code;
                        index++;
                    }
                    break;

                case TypeScriptLexicalState.SingleQuotedString:
                    if (current == '\\')
                    {
                        index++;
                    }
                    else if (current == '\'')
                    {
                        state = TypeScriptLexicalState.Code;
                    }
                    break;

                case TypeScriptLexicalState.DoubleQuotedString:
                    if (current == '\\')
                    {
                        index++;
                    }
                    else if (current == '"')
                    {
                        state = TypeScriptLexicalState.Code;
                    }
                    break;

                case TypeScriptLexicalState.TemplateLiteral:
                    if (current == '\\')
                    {
                        index++;
                    }
                    else if (current == '$' && next == '{')
                    {
                        // A template expression can contain nested templates and arbitrary code.
                        // Rather than guess where the outer template resumes, remain conservative
                        // for the rest of this prefix and suppress import authority.
                        interpolatedTemplate = true;
                        index++;
                    }
                    else if (current == '`' && !interpolatedTemplate)
                    {
                        state = TypeScriptLexicalState.Code;
                    }
                    break;

                case TypeScriptLexicalState.RegexLiteral:
                    if (current == '\\')
                    {
                        index++;
                    }
                    else if (current == '/')
                    {
                        state = TypeScriptLexicalState.Code;
                    }
                    else if (current is '\r' or '\n')
                    {
                        state = TypeScriptLexicalState.Code;
                    }
                    break;
            }
        }

        return state == TypeScriptLexicalState.Code;
    }

    private static string? ResolveRelativeModule(
        string sourcePath,
        string moduleSpecifier,
        IReadOnlySet<string> sourcePaths)
    {
        if (!moduleSpecifier.StartsWith(".", StringComparison.Ordinal))
        {
            return null;
        }

        var slash = sourcePath.LastIndexOf('/');
        var directory = slash >= 0 ? sourcePath[..slash] : string.Empty;
        var combined = string.IsNullOrEmpty(directory)
            ? moduleSpecifier
            : $"{directory}/{moduleSpecifier}";
        var normalizedBase = CollapseSegments(combined);

        var candidates = new[]
        {
            normalizedBase,
            normalizedBase.EndsWith(".ts", StringComparison.OrdinalIgnoreCase) ? normalizedBase : $"{normalizedBase}.ts",
            $"{normalizedBase.TrimEnd('/')}/index.ts"
        };

        return candidates.FirstOrDefault(sourcePaths.Contains);
    }

    private static string CollapseSegments(string value)
    {
        var segments = new List<string>();
        foreach (var segment in Normalize(value).Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment == ".")
            {
                continue;
            }

            if (segment == "..")
            {
                if (segments.Count == 0)
                {
                    return string.Empty;
                }

                segments.RemoveAt(segments.Count - 1);
                continue;
            }

            segments.Add(segment);
        }

        return string.Join('/', segments);
    }

    private static bool IsIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !(char.IsLetter(value[0]) || value[0] is '_' or '$'))
        {
            return false;
        }

        return value.Skip(1).All(character => char.IsLetterOrDigit(character) || character is '_' or '$');
    }

    private static string TypeIdentity(string modulePath, string typeName) =>
        $"{Normalize(modulePath)}#{typeName}";

    private static string Normalize(string path) => path.Replace('\\', '/');

    private enum TypeScriptLexicalState
    {
        Code,
        LineComment,
        BlockComment,
        SingleQuotedString,
        DoubleQuotedString,
        TemplateLiteral,
        RegexLiteral
    }

    private sealed record ImportedType(string ExportedName, string ModulePath);
}
