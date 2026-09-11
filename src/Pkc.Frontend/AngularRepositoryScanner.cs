using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Frontend;

public sealed class AngularRepositoryScanner : IFrontendAdapter
{
    private static readonly HashSet<string> ExcludedDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", ".pkc", "bin", "obj", "node_modules", "dist", "build", "coverage", "knowledge"
    };

    private static readonly HashSet<string> ReservedMethodNames = new(StringComparer.Ordinal)
    {
        "if", "for", "foreach", "while", "switch", "catch", "subscribe", "return"
    };

    private static readonly Regex ScreenRegex = new(
        @"export\s+class\s+(?<name>[A-Z][A-Za-z0-9_]*Component)\b",
        RegexOptions.Compiled);

    private static readonly Regex RouteRegex = new(
        @"\{\s*path\s*:\s*[""'](?<path>[^""']*)[""'][^}]*?component\s*:\s*(?<component>[A-Z][A-Za-z0-9_]*)\s*\}",
        RegexOptions.Compiled);

    private static readonly Regex ButtonRegex = new(
        @"<button\b(?<attrs>[^>]*)>(?<body>[\s\S]*?)</button>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ClickRegex = new(
        @"\(click\)\s*=\s*[""']\s*(?<handler>[A-Za-z_$][A-Za-z0-9_$]*)",
        RegexOptions.Compiled);

    private static readonly Regex PermissionRegex = new(
        @"hasPermission\s*\(\s*[""'](?<permission>[^""']+)[""']\s*\)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex HttpCallRegex = new(
        @"(?<client>[A-Za-z_$][A-Za-z0-9_$.]*)\.(?<method>get|post|put|patch|delete)(?:<[^>]+>)?\s*\(\s*(?<quote>[`""'])(?<url>[\s\S]*?)\k<quote>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex MethodRegex = new(
        @"(?m)^\s*(?<name>[A-Za-z_$][A-Za-z0-9_$]*)\s*\([^\r\n)]*\)\s*(?::\s*[^{\r\n]+)?\{",
        RegexOptions.Compiled);

    private static readonly Regex TemplateParameterRegex = new(@"\$\{[^}]+\}", RegexOptions.Compiled);
    private static readonly Regex RouteParameterRegex = new(@"\{[^}/]+\}|:[A-Za-z0-9_]+", RegexOptions.Compiled);
    private static readonly Regex HtmlTagRegex = new(@"<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex AngularInterpolationRegex = new(@"\{\{[^}]+\}\}", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    public string Id => "angular-static";

    public bool CanHandle(string repositoryPath)
    {
        if (Directory.EnumerateFiles(repositoryPath, "angular.json", SearchOption.AllDirectories)
            .Any(path => !IsExcluded(repositoryPath, path)))
        {
            return true;
        }

        foreach (var packageJson in Directory.EnumerateFiles(repositoryPath, "package.json", SearchOption.AllDirectories)
                     .Where(path => !IsExcluded(repositoryPath, path)))
        {
            if (File.ReadAllText(packageJson).Contains("\"@angular/core\"", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return Directory.EnumerateFiles(repositoryPath, "*.ts", SearchOption.AllDirectories)
            .Where(path => !IsExcluded(repositoryPath, path))
            .Any(path => File.ReadAllText(path).Contains("@Component", StringComparison.Ordinal));
    }

    public async Task<FactDocument> ScanAsync(string repositoryPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);
        var rootPath = Path.GetFullPath(repositoryPath);
        if (!Directory.Exists(rootPath)) throw new DirectoryNotFoundException($"Repository path does not exist: {rootPath}");

        var facts = new List<EvidenceFact>();
        var relations = new List<EvidenceRelation>();
        var files = Directory.EnumerateFiles(rootPath, "*.ts", SearchOption.AllDirectories)
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
            "0.4.1-angular",
            facts.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            relations.OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray());
    }

    private static void ExtractFile(string relativePath, string text, ICollection<EvidenceFact> facts, ICollection<EvidenceRelation> relations)
    {
        var screenFacts = new List<EvidenceFact>();
        foreach (Match match in ScreenRegex.Matches(text))
        {
            var name = match.Groups["name"].Value;
            var fact = CreateFact(relativePath, text, match, "ui-screen", name, null,
                new Dictionary<string, string>(StringComparer.Ordinal) { ["framework"] = "angular-static" });
            facts.Add(fact);
            screenFacts.Add(fact);
        }

        foreach (Match match in RouteRegex.Matches(text))
        {
            var path = "/" + match.Groups["path"].Value.Trim('/');
            var component = match.Groups["component"].Value;
            var fact = CreateFact(relativePath, text, match, "ui-route", path, component,
                new Dictionary<string, string>(StringComparer.Ordinal) { ["path"] = path, ["component"] = component, ["framework"] = "angular-static" });
            facts.Add(fact);
            relations.Add(new EvidenceRelation(fact.Id, "renders", component, fact.Source));
        }

        foreach (Match match in ButtonRegex.Matches(text))
        {
            var attrs = match.Groups["attrs"].Value;
            var label = CleanLabel(match.Groups["body"].Value);
            if (string.IsNullOrWhiteSpace(label)) label = "button";

            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["label"] = label,
                ["framework"] = "angular-static"
            };
            var click = ClickRegex.Match(attrs);
            if (click.Success) metadata["handler"] = click.Groups["handler"].Value;
            var permission = FindNearbyPermission(text, match.Index);
            if (!string.IsNullOrWhiteSpace(permission)) metadata["permission"] = permission;

            var screen = screenFacts.FirstOrDefault()?.Name;
            var fact = CreateFact(relativePath, text, match, "ui-action", label, screen, metadata);
            facts.Add(fact);
            if (metadata.TryGetValue("handler", out var handler))
                relations.Add(new EvidenceRelation(fact.Id, "triggers-handler", handler, fact.Source));
        }

        foreach (Match match in HttpCallRegex.Matches(text))
        {
            var method = match.Groups["method"].Value.ToUpperInvariant();
            var url = match.Groups["url"].Value;
            var container = FindNearestMethodName(text, match.Index);
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["httpMethod"] = method,
                ["url"] = url,
                ["routeKey"] = NormalizeRouteKey(url),
                ["client"] = match.Groups["client"].Value,
                ["framework"] = "angular-static"
            };
            facts.Add(CreateFact(relativePath, text, match, "ui-api-call", $"{method} {url}", container, metadata));
        }
    }

    private static string? FindNearestMethodName(string text, int beforeIndex)
    {
        string? name = null;
        foreach (Match match in MethodRegex.Matches(text[..Math.Min(beforeIndex, text.Length)]))
        {
            var candidate = match.Groups["name"].Value;
            if (!ReservedMethodNames.Contains(candidate)) name = candidate;
        }
        return name;
    }

    private static string? FindNearbyPermission(string text, int beforeIndex)
    {
        var start = Math.Max(0, beforeIndex - 500);
        var matches = PermissionRegex.Matches(text[start..beforeIndex]);
        return matches.Count == 0 ? null : matches[^1].Groups["permission"].Value;
    }

    private static string CleanLabel(string value)
    {
        var cleaned = AngularInterpolationRegex.Replace(value, " ");
        cleaned = HtmlTagRegex.Replace(cleaned, " ");
        return WhitespaceRegex.Replace(cleaned, " ").Trim();
    }

    public static string NormalizeRouteKey(string value)
    {
        var route = value.Split('?', '#')[0].Trim();
        route = TemplateParameterRegex.Replace(route, "{param}");
        route = RouteParameterRegex.Replace(route, "{param}");
        while (route.Contains("//", StringComparison.Ordinal)) route = route.Replace("//", "/", StringComparison.Ordinal);
        if (!route.StartsWith('/')) route = "/" + route;
        return route.TrimEnd('/').ToLowerInvariant();
    }

    private static EvidenceFact CreateFact(string relativePath, string text, Match match, string kind, string name, string? container, IReadOnlyDictionary<string, string> metadata)
    {
        var source = GetLocation(relativePath, text, match.Index, match.Length);
        return new EvidenceFact($"ng:{relativePath}:{source.StartLine}:{kind}:{name}", kind, name, container, source, [], metadata);
    }

    private static SourceLocation GetLocation(string path, string text, int index, int length)
    {
        var start = 1;
        for (var i = 0; i < index && i < text.Length; i++) if (text[i] == '\n') start++;
        var end = start;
        for (var i = index; i < Math.Min(text.Length, index + length); i++) if (text[i] == '\n') end++;
        return new SourceLocation(path, start, end);
    }

    private static bool IsExcluded(string rootPath, string path)
    {
        var relative = Path.GetRelativePath(rootPath, path);
        return relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => ExcludedDirectoryNames.Contains(segment));
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/');
}
