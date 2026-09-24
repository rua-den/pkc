using System.Text.RegularExpressions;
using Pkc.Core;
using Pkc.Core.Discovery;

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

    private static readonly Regex ClassRegex = new(
        @"(?:export\s+)?(?:abstract\s+)?class\s+(?<name>[A-Z][A-Za-z0-9_$]*)\b[^{]*\{",
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

    private static readonly Regex VisibilityConditionRegex = new(
        @"@if\s*\((?<condition>[^\r\n]*)\)\s*\{",
        RegexOptions.Compiled);

    private static readonly Regex HttpCallRegex = new(
        @"(?<client>[A-Za-z_$][A-Za-z0-9_$.]*)\.(?<method>get|post|put|patch|delete)(?:<[^>]+>)?\s*\(\s*(?<quote>[`""'])(?<url>[\s\S]*?)\k<quote>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex MethodRegex = new(
        @"(?m)^\s*(?<name>[A-Za-z_$][A-Za-z0-9_$]*)\s*\([^\r\n)]*\)\s*(?::\s*[^{\r\n]+)?\{",
        RegexOptions.Compiled);

    private static readonly Regex CalledMethodRegex = new(
        @"(?:this\.)?(?:[A-Za-z_$][A-Za-z0-9_$]*\.)*(?<name>[A-Za-z_$][A-Za-z0-9_$]*)\s*\(",
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
        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException($"Repository path does not exist: {rootPath}");
        }

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
            "0.4.2-angular",
            facts.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            relations.OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
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
        var loadMethods = FindTransitiveCalledMethods(text, "ngOnInit", maxDepth: 3);

        foreach (Match match in ScreenRegex.Matches(text))
        {
            var name = match.Groups["name"].Value;
            var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["framework"] = "angular-static"
            };

            if (loadMethods.Count > 0)
            {
                metadata["loadMethods"] = string.Join(", ", loadMethods);
            }

            var fact = CreateFact(
                relativePath,
                text,
                match,
                "ui-screen",
                name,
                null,
                metadata);

            facts.Add(fact);
            screenFacts.Add(fact);
        }

        foreach (Match match in RouteRegex.Matches(text))
        {
            var path = "/" + match.Groups["path"].Value.Trim('/');
            var component = match.Groups["component"].Value;

            var fact = CreateFact(
                relativePath,
                text,
                match,
                "ui-route",
                path,
                component,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["path"] = path,
                    ["component"] = component,
                    ["framework"] = "angular-static"
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
                ["label"] = label,
                ["framework"] = "angular-static"
            };

            var click = ClickRegex.Match(attrs);
            if (click.Success)
            {
                var handler = click.Groups["handler"].Value;
                metadata["handler"] = handler;

                var calledMethods = FindTransitiveCalledMethods(text, handler, maxDepth: 3);
                if (calledMethods.Count > 0)
                {
                    metadata["calledMethods"] = string.Join(", ", calledMethods);
                }
            }

            var permission = FindNearbyPermission(text, match.Index);
            if (!string.IsNullOrWhiteSpace(permission))
            {
                metadata["permission"] = permission;
            }

            var visibilityCondition = FindNearbyVisibilityCondition(text, match.Index);
            if (!string.IsNullOrWhiteSpace(visibilityCondition))
            {
                metadata["visibilityCondition"] = visibilityCondition;
            }

            var screen = screenFacts.FirstOrDefault()?.Name;
            var fact = CreateFact(
                relativePath,
                text,
                match,
                "ui-action",
                label,
                screen,
                metadata);

            facts.Add(fact);

            if (metadata.TryGetValue("handler", out var handlerName))
            {
                relations.Add(new EvidenceRelation(
                    fact.Id,
                    "triggers-handler",
                    handlerName,
                    fact.Source));
            }
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

            var ownerClass = FindContainingClassName(text, match.Index);
            if (!string.IsNullOrWhiteSpace(ownerClass))
            {
                metadata["ownerClass"] = ownerClass;
                metadata["ownerResolution"] = "typescript-syntactic-class-range";
            }

            facts.Add(CreateFact(
                relativePath,
                text,
                match,
                "ui-api-call",
                $"{method} {url}",
                container,
                metadata));
        }
    }

    private static string? FindContainingClassName(string text, int index)
    {
        string? owner = null;
        var ownerStart = -1;

        foreach (Match match in ClassRegex.Matches(text))
        {
            if (match.Index > index)
            {
                break;
            }

            var openBrace = match.Index + match.Value.LastIndexOf('{');
            var closeBrace = FindMatchingBrace(text, openBrace);
            if (openBrace >= 0 && closeBrace >= index && index > openBrace && openBrace > ownerStart)
            {
                owner = match.Groups["name"].Value;
                ownerStart = openBrace;
            }
        }

        return owner;
    }

    private static int FindMatchingBrace(string text, int openBrace)
    {
        if (openBrace < 0 || openBrace >= text.Length || text[openBrace] != '{')
        {
            return -1;
        }

        var depth = 0;
        var mode = LexicalMode.Normal;
        var escaped = false;

        for (var index = openBrace; index < text.Length; index++)
        {
            var current = text[index];
            var next = index + 1 < text.Length ? text[index + 1] : '\0';

            if (mode == LexicalMode.LineComment)
            {
                if (current == '\n') mode = LexicalMode.Normal;
                continue;
            }

            if (mode == LexicalMode.BlockComment)
            {
                if (current == '*' && next == '/')
                {
                    mode = LexicalMode.Normal;
                    index++;
                }
                continue;
            }

            if (mode is LexicalMode.SingleQuote or LexicalMode.DoubleQuote or LexicalMode.Template)
            {
                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (current == '\\')
                {
                    escaped = true;
                    continue;
                }

                if ((mode == LexicalMode.SingleQuote && current == '\'') ||
                    (mode == LexicalMode.DoubleQuote && current == '"') ||
                    (mode == LexicalMode.Template && current == '`'))
                {
                    mode = LexicalMode.Normal;
                }
                continue;
            }

            if (current == '/' && next == '/')
            {
                mode = LexicalMode.LineComment;
                index++;
                continue;
            }

            if (current == '/' && next == '*')
            {
                mode = LexicalMode.BlockComment;
                index++;
                continue;
            }

            if (current == '\'')
            {
                mode = LexicalMode.SingleQuote;
                continue;
            }

            if (current == '"')
            {
                mode = LexicalMode.DoubleQuote;
                continue;
            }

            if (current == '`')
            {
                mode = LexicalMode.Template;
                continue;
            }

            if (current == '{')
            {
                depth++;
            }
            else if (current == '}' && --depth == 0)
            {
                return index;
            }
        }

        return -1;
    }

    private enum LexicalMode
    {
        Normal,
        SingleQuote,
        DoubleQuote,
        Template,
        LineComment,
        BlockComment
    }

    private static IReadOnlyList<string> FindTransitiveCalledMethods(
        string text,
        string rootMethod,
        int maxDepth)
    {
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal) { rootMethod };
        var queue = new Queue<(string Name, int Depth)>();
        queue.Enqueue((rootMethod, 0));

        while (queue.Count > 0)
        {
            var (methodName, depth) = queue.Dequeue();
            var body = FindMethodBody(text, methodName);
            if (string.IsNullOrWhiteSpace(body))
            {
                continue;
            }

            foreach (var called in CalledMethodRegex.Matches(body)
                         .Select(match => match.Groups["name"].Value)
                         .Where(name => !ReservedMethodNames.Contains(name)))
            {
                if (!seen.Add(called))
                {
                    continue;
                }

                result.Add(called);
                if (depth + 1 < maxDepth && FindMethodBody(text, called) is not null)
                {
                    queue.Enqueue((called, depth + 1));
                }
            }
        }

        return result;
    }

    private static string? FindMethodBody(string text, string methodName)
    {
        var method = MethodRegex.Matches(text)
            .Cast<Match>()
            .FirstOrDefault(match =>
                string.Equals(match.Groups["name"].Value, methodName, StringComparison.Ordinal));

        if (method is null)
        {
            return null;
        }

        var openBrace = method.Index + method.Length - 1;
        if (openBrace < 0 || openBrace >= text.Length || text[openBrace] != '{')
        {
            return null;
        }

        var depth = 0;
        for (var index = openBrace; index < text.Length; index++)
        {
            if (text[index] == '{')
            {
                depth++;
            }
            else if (text[index] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return text[(openBrace + 1)..index];
                }
            }
        }

        return null;
    }

    private static string? FindNearestMethodName(string text, int beforeIndex)
    {
        string? name = null;

        foreach (Match match in MethodRegex.Matches(text[..Math.Min(beforeIndex, text.Length)]))
        {
            var candidate = match.Groups["name"].Value;
            if (!ReservedMethodNames.Contains(candidate))
            {
                name = candidate;
            }
        }

        return name;
    }

    private static string? FindNearbyPermission(string text, int beforeIndex)
    {
        var start = Math.Max(0, beforeIndex - 500);
        var matches = PermissionRegex.Matches(text[start..beforeIndex]);
        return matches.Count == 0
            ? null
            : matches[^1].Groups["permission"].Value;
    }

    private static string? FindNearbyVisibilityCondition(string text, int beforeIndex)
    {
        var start = Math.Max(0, beforeIndex - 700);
        var matches = VisibilityConditionRegex.Matches(text[start..beforeIndex]);
        return matches.Count == 0
            ? null
            : WhitespaceRegex.Replace(matches[^1].Groups["condition"].Value, " ").Trim();
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
        return new EvidenceFact(
            $"ng:{relativePath}:{source.StartLine}:{kind}:{name}",
            kind,
            name,
            container,
            source,
            [],
            metadata);
    }

    private static SourceLocation GetLocation(
        string path,
        string text,
        int index,
        int length)
    {
        var start = 1;
        for (var i = 0; i < index && i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                start++;
            }
        }

        var end = start;
        for (var i = index; i < Math.Min(text.Length, index + length); i++)
        {
            if (text[i] == '\n')
            {
                end++;
            }
        }

        return new SourceLocation(path, start, end);
    }

    private static bool IsExcluded(string rootPath, string path)
    {
        var relative = Path.GetRelativePath(rootPath, path);
        return relative
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => ExcludedDirectoryNames.Contains(segment)) ||
            SemanticSourceScope.Excludes(rootPath, path);
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/');
}
