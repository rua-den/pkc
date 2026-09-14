using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Frontend;

internal sealed class AngularListBehaviorScanner
{
    private static readonly Regex ComponentRegex = new(
        @"export\s+class\s+(?<name>[A-Z][A-Za-z0-9_]*Component)\b",
        RegexOptions.Compiled);

    private static readonly Regex ListRegex = new(
        @"@for\s*\(\s*(?<item>[A-Za-z_$][A-Za-z0-9_$]*)\s+of\s+(?<collection>[A-Za-z_$][A-Za-z0-9_$.]*)\s*;",
        RegexOptions.Compiled);

    private static readonly Regex ResultBindingRegex = new(
        @"this\.(?<service>[A-Za-z_$][A-Za-z0-9_$]*)\.(?<method>[A-Za-z_$][A-Za-z0-9_$]*)\s*\([^)]*\)\s*\.subscribe\s*\(\s*(?<result>[A-Za-z_$][A-Za-z0-9_$]*)\s*=>\s*this\.(?<target>[A-Za-z_$][A-Za-z0-9_$]*)\s*=\s*\k<result>",
        RegexOptions.Compiled);

    private static readonly Regex InjectServiceRegex = new(
        @"(?<service>[A-Za-z_$][A-Za-z0-9_$]*)\s*=\s*inject\s*\(\s*(?<type>[A-Z][A-Za-z0-9_$]*)\s*\)",
        RegexOptions.Compiled);

    private static readonly Regex NewServiceRegex = new(
        @"(?<service>[A-Za-z_$][A-Za-z0-9_$]*)\s*=\s*new\s+(?<type>[A-Z][A-Za-z0-9_$]*)\s*\(",
        RegexOptions.Compiled);

    private static readonly Regex ConstructorServiceRegex = new(
        @"(?:private|public|protected)\s+(?:readonly\s+)?(?<service>[A-Za-z_$][A-Za-z0-9_$]*)\s*:\s*(?<type>[A-Z][A-Za-z0-9_$]*)\b",
        RegexOptions.Compiled);

    public async Task<FactDocument> ScanAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);
        var root = Path.GetFullPath(repositoryPath);
        var facts = new List<EvidenceFact>();
        var relations = new List<EvidenceRelation>();

        var files = Directory.EnumerateFiles(root, "*.ts", SearchOption.AllDirectories)
            .Where(path => FrontendSourceScope.IsProductSource(Normalize(Path.GetRelativePath(root, path))))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relativePath = Normalize(Path.GetRelativePath(root, file));
            var text = await File.ReadAllTextAsync(file, cancellationToken);
            var component = ComponentRegex.Match(text).Groups["name"].Value;
            if (string.IsNullOrWhiteSpace(component))
            {
                continue;
            }

            var serviceTypes = FindServiceTypes(text);
            var fileFacts = new List<EvidenceFact>();

            foreach (Match match in ListRegex.Matches(text))
            {
                var item = match.Groups["item"].Value;
                var collection = NormalizeExpression(match.Groups["collection"].Value);
                var fact = CreateFact(
                    relativePath,
                    text,
                    match,
                    "ui-list-render",
                    $"{item} of {collection}",
                    component,
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["item"] = item,
                        ["collection"] = collection,
                        ["component"] = component,
                        ["analysisMode"] = "angular-template-structural-regex",
                        ["analysisConfidence"] = "medium"
                    });
                facts.Add(fact);
                fileFacts.Add(fact);
            }

            foreach (Match match in ResultBindingRegex.Matches(text))
            {
                var target = match.Groups["target"].Value;
                var apiMethod = match.Groups["method"].Value;
                var service = match.Groups["service"].Value;
                var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["service"] = service,
                    ["apiMethod"] = apiMethod,
                    ["resultParameter"] = match.Groups["result"].Value,
                    ["target"] = target,
                    ["component"] = component,
                    ["analysisMode"] = "typescript-syntactic-binding",
                    ["analysisConfidence"] = "medium"
                };

                if (serviceTypes.TryGetValue(service, out var serviceType))
                {
                    metadata["serviceType"] = serviceType;
                    metadata["serviceResolution"] = "typescript-syntactic-declaration";
                }

                var fact = CreateFact(
                    relativePath,
                    text,
                    match,
                    "ui-result-binding",
                    $"{apiMethod} -> {target}",
                    component,
                    metadata);
                facts.Add(fact);
                fileFacts.Add(fact);

                foreach (var render in fileFacts.Where(candidate =>
                             candidate.Kind == "ui-list-render" &&
                             candidate.Metadata.TryGetValue("collection", out var collection) &&
                             string.Equals(collection, target, StringComparison.Ordinal)))
                {
                    relations.Add(new EvidenceRelation(fact.Id, "feeds-list", render.Id, fact.Source));
                }
            }
        }

        return new FactDocument(
            "0.4.6-angular-list-behavior",
            facts.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            relations.OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray());
    }

    private static IReadOnlyDictionary<string, string> FindServiceTypes(string text)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        AddServiceTypes(InjectServiceRegex, text, result);
        AddServiceTypes(NewServiceRegex, text, result);
        AddServiceTypes(ConstructorServiceRegex, text, result);
        return result;
    }

    private static void AddServiceTypes(
        Regex regex,
        string text,
        IDictionary<string, string> result)
    {
        foreach (Match match in regex.Matches(text))
        {
            var service = match.Groups["service"].Value;
            var type = match.Groups["type"].Value;
            if (!string.IsNullOrWhiteSpace(service) && !string.IsNullOrWhiteSpace(type))
            {
                result[service] = type;
            }
        }
    }

    private static EvidenceFact CreateFact(
        string path,
        string text,
        Match match,
        string kind,
        string name,
        string container,
        IReadOnlyDictionary<string, string> metadata)
    {
        var startLine = 1 + text.AsSpan(0, match.Index).Count('\n');
        var endLine = startLine + match.Value.AsSpan().Count('\n');
        var source = new SourceLocation(path, startLine, endLine);
        return new EvidenceFact(
            $"ui:{path}:{startLine}:{kind}:{name}",
            kind,
            name,
            container,
            source,
            [],
            metadata);
    }

    private static string NormalizeExpression(string value) =>
        value.StartsWith("this.", StringComparison.Ordinal) ? value[5..] : value;

    private static string Normalize(string path) => path.Replace('\\', '/');
}
