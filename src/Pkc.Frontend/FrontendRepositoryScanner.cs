using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Frontend;

public sealed class FrontendRepositoryScanner
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".ts", ".tsx", ".js", ".jsx"
    };

    private static readonly HashSet<string> ExcludedDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", ".pkc", "bin", "obj", "node_modules", "dist", "build", "coverage", "knowledge"
    };

    private static readonly Regex ComponentRegex = new(
        "(?:export\\s+)?(?:default\\s+)?function\\s+(?<name>[A-Z][A-Za-z0-9_]*)\\s*\\(",
        RegexOptions.Compiled);

    private static readonly Regex FunctionRegex = new(
        "(?:(?:export\\s+)?(?:async\\s+)?function\\s+(?<fn>[A-Za-z_$][A-Za-z0-9_$]*)\\s*\\()|(?:const\\s+(?<const>[A-Za-z_$][A-Za-z0-9_$]*)\\s*=\\s*(?:async\\s*)?\\([^)]*\\)\\s*=>)",
        RegexOptions.Compiled);

    private static readonly Regex RouteRegex = new(
        "<Route\\b[\\s\\S]*?path\\s*=\\s*[\"'](?<path>[^\"']+)[\"'][\\s\\S]*?element\\s*=\\s*\\{\\s*<(?<component>[A-Z][A-Za-z0-9_]*)[\\s\\S]*?\\}\\s*/?>",
        RegexOptions.Compiled);

    private static readonly Regex ButtonRegex = new(
        "<(?:button|Button)\\b(?<attrs>[^>]*)>(?<body>[\\s\\S]*?)</(?:button|Button)>",
        RegexOptions.Compiled);

    private static readonly Regex OnClickRegex = new(
        "onClick\\s*=\\s*\\{\\s*(?<handler>[A-Za-z_$][A-Za-z0-9_$]*)\\s*\\}",
        RegexOptions.Compiled);

    private static readonly Regex PermissionRegex = new(
        "(?:hasPermission|can|canAccess)\\s*\\(\\s*[\"'](?<permission>[^\"']+)[\"']\\s*\\)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex FetchRegex = new(
        "fetch\\s*\\(\\s*(?<quote>[`\"'])(?<url>[\\s\\S]*?)\\k<quote>\\s*,\\s*\\{(?<options>[\\s\\S]*?)\\}\\s*\\)",
        RegexOptions.Compiled);

    private static readonly Regex FetchMethodRegex = new(
        "method\\s*:\\s*[\"'](?<method>GET|POST|PUT|PATCH|DELETE)[\"']",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ClientCallRegex = new(
        "(?<client>[A-Za-z_$][A-Za-z0-9_$.]*)\\.(?<method>get|post|put|patch|delete)\\s*\\(\\s*(?<quote>[`\"'])(?<url>[\\s\\S]*?)\\k<quote>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex TemplateParameterRegex = new("\\$\\{[^}]+\\}", RegexOptions.Compiled);
    private static readonly Regex RouteParameterRegex = new("\\{[^}/]+\\}|:[A-Za-z0-9_]+", RegexOptions.Compiled);
    private static readonly Regex JsxNoiseRegex = new("<[^>]+>|\\{[^}]*\\}", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new("\\s+", RegexOptions.Compiled);

    public async Task<FactDocument> ScanAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);

        var rootPath = Path.GetFullPath(repositoryPath);
        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException($"Repository path does not exist: {rootPath}");
        }

        var facts = new List<EvidenceFact>();
        var relations = new List<EvidenceRelation>();

        var files = Directory.EnumerateFiles(rootPath, "*.*", SearchOption.AllDirectories)
            .Where(path => Extensions.Contains(Path.GetExtension(path)))
            .Where(path => !IsExcluded(rootPath, path))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relativePath = NormalizePath(Path.GetRelativePath(rootPath, file));
            var text = await File.ReadAllTextAsync(file, cancellationToken);
            ExtractFile(relativePath, text, facts, relations);
        }

        return new FactDocument(
            "0.3.0-frontend",
            facts.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            relations
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray());
    }

    private static void ExtractFile(
        string relativePath,
        string text,
        ICollection<EvidenceFact> facts,
        ICollection<EvidenceRelation> relations)
    {
        var screenFacts = new List<EvidenceFact>();

        foreach (Match match in ComponentRegex.Matches(text))
        {
            var name = match.Groups["name"].Value;
            var fact = CreateFact(relativePath, text, match, "ui-screen", name, null,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["framework"] = "react-static"
                });
            facts.Add(fact);
            screenFacts.Add(fact);
        }

        foreach (Match match in RouteRegex.Matches(text))
        {
            var path = match.Groups["path"].Value;
            var component = match.Groups["component"].Value;
            var fact = CreateFact(relativePath, text, match, "ui-route", path, component,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["path"] = path,
                    ["component"] = component
                });
            facts.Add(fact);
            relations.Add(new EvidenceRelation(fact.Id, "renders", component, fact.Source));
        }

        foreach (Match match in ButtonRegex.Matches(text))
        {
            var attrs = match.Groups["attrs"].Value;
            var label = CleanLabel(match.Groups["body"].Value);
            if (string.IsNullOrWhiteSpace(label))
            {
                label = "button";
            }

            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["label"] = label
            };

            var onClick = OnClickRegex.Match(attrs);
            if (onClick.Success)
            {
                metadata["handler"] = onClick.Groups["handler"].Value;
            }

            var permission = FindNearbyPermission(text, match.Index);
            if (!string.IsNullOrWhiteSpace(permission))
            {
                metadata["permission"] = permission;
            }

            var screen = FindNearestFunctionName(text, match.Index, requirePascalCase: true)
                ?? screenFacts.FirstOrDefault()?.Name;

            var fact = CreateFact(relativePath, text, match, "ui-action", label, screen, metadata);
            facts.Add(fact);

            if (metadata.TryGetValue("handler", out var handler))
            {
                relations.Add(new EvidenceRelation(fact.Id, "triggers-handler", handler, fact.Source));
            }
        }

        foreach (Match match in FetchRegex.Matches(text))
        {
            var options = match.Groups["options"].Value;
            var methodMatch = FetchMethodRegex.Match(options);
            var method = methodMatch.Success ? methodMatch.Groups["method"].Value.ToUpperInvariant() : "GET";
            AddApiCall(relativePath, text, match, method, match.Groups["url"].Value, "fetch", facts);
        }

        foreach (Match match in ClientCallRegex.Matches(text))
        {
            AddApiCall(
                relativePath,
                text,
                match,
                match.Groups["method"].Value.ToUpperInvariant(),
                match.Groups["url"].Value,
                match.Groups["client"].Value,
                facts);
        }
    }

    private static void AddApiCall(
        string relativePath,
        string text,
        Match match,
        string method,
        string url,
        string client,
        ICollection<EvidenceFact> facts)
    {
        var handler = FindNearestFunctionName(text, match.Index, requirePascalCase: false);
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["httpMethod"] = method,
            ["url"] = url,
            ["routeKey"] = NormalizeRouteKey(url),
            ["client"] = client
        };

        facts.Add(CreateFact(relativePath, text, match, "ui-api-call", $"{method} {url}", handler, metadata));
    }

    public static string NormalizeRouteKey(string value)
    {
        var route = value.Split('?', '#')[0].Trim();
        route = TemplateParameterRegex.Replace(route, "{param}");
        route = RouteParameterRegex.Replace(route, "{param}");
        while (route.Contains("//", StringComparison.Ordinal))
        {
            route = route.Replace("//", "/", StringComparison.Ordinal);
        }

        if (!route.StartsWith('/'))
        {
            route = "/" + route;
        }

        return route.TrimEnd('/').ToLowerInvariant();
    }

    private static EvidenceFact CreateFact(
        string relativePath,
        string text,
        Match match,
        string kind,
        string name,
        string? container,
        IReadOnlyDictionary<string, string> metadata)
    {
        var source = GetLocation(relativePath, text, match.Index, match.Length);
        var id = $"web:{relativePath}:{source.StartLine}:{kind}:{name}";
        return new EvidenceFact(id, kind, name, container, source, [], metadata);
    }

    private static SourceLocation GetLocation(string path, string text, int index, int length)
    {
        var start = 1;
        for (var i = 0; i < index && i < text.Length; i++)
        {
            if (text[i] == '\n') start++;
        }

        var end = start;
        var stop = Math.Min(text.Length, index + length);
        for (var i = index; i < stop; i++)
        {
            if (text[i] == '\n') end++;
        }

        return new SourceLocation(path, start, end);
    }

    private static string? FindNearestFunctionName(string text, int beforeIndex, bool requirePascalCase)
    {
        string? name = null;
        foreach (Match match in FunctionRegex.Matches(text[..Math.Min(beforeIndex, text.Length)]))
        {
            var candidate = match.Groups["fn"].Success ? match.Groups["fn"].Value : match.Groups["const"].Value;
            if (requirePascalCase && (candidate.Length == 0 || !char.IsUpper(candidate[0])))
            {
                continue;
            }

            name = candidate;
        }

        return name;
    }

    private static string? FindNearbyPermission(string text, int beforeIndex)
    {
        var start = Math.Max(0, beforeIndex - 350);
        var segment = text[start..beforeIndex];
        var matches = PermissionRegex.Matches(segment);
        return matches.Count == 0 ? null : matches[^1].Groups["permission"].Value;
    }

    private static string CleanLabel(string value)
    {
        var noTags = JsxNoiseRegex.Replace(value, " ");
        return WhitespaceRegex.Replace(noTags, " ").Trim();
    }

    private static bool IsExcluded(string rootPath, string path)
    {
        var relative = Path.GetRelativePath(rootPath, path);
        return relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => ExcludedDirectoryNames.Contains(segment));
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/');
}
