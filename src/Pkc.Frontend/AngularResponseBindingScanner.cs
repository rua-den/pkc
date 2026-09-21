using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Frontend;

internal sealed class AngularResponseBindingScanner
{
    private static readonly Regex NamedImportRegex = new(
        "^[\\t ]*\\bimport\\s*(?:type\\s+)?\\{(?<bindings>[^}]+)\\}\\s*from\\s*['\\\"](?<module>[^'\\\"]+)['\\\"]",
        RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly Regex InterfaceRegex = new(
        @"(?:export\s+)?interface\s+(?<name>[A-Z][A-Za-z0-9_$]*)\b[^{]*\{",
        RegexOptions.Compiled);

    private static readonly Regex InterfacePropertyRegex = new(
        @"(?m)^\s*(?:readonly\s+)?(?<name>[A-Za-z_$][A-Za-z0-9_$]*)\??\s*:\s*[^;\r\n]+;?",
        RegexOptions.Compiled);

    private static readonly Regex ClassRegex = new(
        @"(?:export\s+)?(?:abstract\s+)?class\s+(?<name>[A-Z][A-Za-z0-9_$]*)\b[^{]*\{",
        RegexOptions.Compiled);

    private static readonly Regex MethodRegex = new(
        @"(?m)^\s*(?<name>[A-Za-z_$][A-Za-z0-9_$]*)\s*\([^\r\n)]*\)\s*(?::\s*[^{\r\n]+)?\{",
        RegexOptions.Compiled);

    private static readonly Regex TypedHttpCallRegex = new(
        @"(?<client>[A-Za-z_$][A-Za-z0-9_$.]*)\.(?<method>get|post|put|patch|delete)\s*<\s*(?<type>[A-Za-z_$][A-Za-z0-9_$]*)\s*>\s*\(\s*(?<quote>[`""'])(?<url>[\s\S]*?)\k<quote>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ResultMemberBindingRegex = new(
        @"this\.(?<service>[A-Za-z_$][A-Za-z0-9_$]*)\.(?<method>[A-Za-z_$][A-Za-z0-9_$]*)\s*\([^)]*\)\s*\.subscribe\s*\(\s*(?<result>[A-Za-z_$][A-Za-z0-9_$]*)\s*=>\s*this\.(?<target>[A-Za-z_$][A-Za-z0-9_$]*)\s*=\s*\k<result>\.(?<member>[A-Za-z_$][A-Za-z0-9_$]*)\s*\)",
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

    private static readonly Regex TemplateRegex = new(
        @"\btemplate\s*:\s*`(?<body>[\s\S]*?)`",
        RegexOptions.Compiled);

    private static readonly Regex InterpolationRegex = new(
        @"\{\{\s*(?<member>[A-Za-z_$][A-Za-z0-9_$]*)\s*\}\}",
        RegexOptions.Compiled);

    private static readonly Regex TemplateParameterRegex = new(@"\$\{[^}]+\}", RegexOptions.Compiled);
    private static readonly Regex RouteParameterRegex = new(@"\{[^}/]+\}|:[A-Za-z0-9_]+", RegexOptions.Compiled);

    public async Task<FactDocument> ScanAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);
        var root = Path.GetFullPath(repositoryPath);
        var files = Directory.EnumerateFiles(root, "*.ts", SearchOption.AllDirectories)
            .Select(path => new SourceFile(
                path,
                Normalize(Path.GetRelativePath(root, path))))
            .Where(file => FrontendSourceScope.IsProductSource(file.RelativePath))
            .OrderBy(file => file.RelativePath, StringComparer.Ordinal)
            .ToArray();
        var sourcePaths = files
            .Select(file => file.RelativePath)
            .ToHashSet(StringComparer.Ordinal);

        var texts = new Dictionary<string, string>(StringComparer.Ordinal);
        var imports = new Dictionary<string, IReadOnlyDictionary<string, ImportedType>>(StringComparer.Ordinal);
        var contracts = new Dictionary<string, TypeContract>(StringComparer.Ordinal);
        var ambiguousContracts = new HashSet<string>(StringComparer.Ordinal);

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var text = await File.ReadAllTextAsync(file.FullPath, cancellationToken);
            texts[file.RelativePath] = text;
            imports[file.RelativePath] = FindImports(file.RelativePath, text, sourcePaths);

            foreach (var declaration in FindInterfaces(file.RelativePath, text))
            {
                if (!contracts.TryAdd(declaration.Identity, declaration))
                {
                    contracts.Remove(declaration.Identity);
                    ambiguousContracts.Add(declaration.Identity);
                }
            }
        }

        var facts = new List<EvidenceFact>();
        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var text = texts[file.RelativePath];
            var fileImports = imports[file.RelativePath];

            foreach (Match match in TypedHttpCallRegex.Matches(text))
            {
                if (!IsActiveCodePosition(text, match.Index))
                {
                    continue;
                }

                var ownerClass = FindContainingClass(text, match.Index);
                var apiMethod = FindContainingMethod(text, match.Index);
                var typeName = match.Groups["type"].Value;
                var responseIdentity = ResolveTypeIdentity(
                    file.RelativePath,
                    typeName,
                    fileImports,
                    contracts,
                    ambiguousContracts);
                if (ownerClass is null ||
                    apiMethod is null ||
                    responseIdentity is null ||
                    !contracts.TryGetValue(responseIdentity, out var responseContract))
                {
                    continue;
                }

                var httpMethod = match.Groups["method"].Value.ToUpperInvariant();
                var url = match.Groups["url"].Value;
                var serviceIdentity = TypeIdentity(file.RelativePath, ownerClass);
                foreach (var member in responseContract.Members)
                {
                    var source = GetLocation(file.RelativePath, text, match.Index, match.Length);
                    facts.Add(new EvidenceFact(
                        $"ui-wire:{file.RelativePath}:{source.StartLine}:api-response-member:{apiMethod}:{member.Name}",
                        "ui-api-response-member",
                        $"{apiMethod} result.{member.Name}",
                        ownerClass,
                        source,
                        [],
                        new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["serviceType"] = serviceIdentity,
                            ["serviceTypeName"] = ownerClass,
                            ["serviceModule"] = file.RelativePath,
                            ["apiMethod"] = apiMethod,
                            ["httpMethod"] = httpMethod,
                            ["url"] = url,
                            ["routeKey"] = NormalizeRouteKey(url),
                            ["responseType"] = responseContract.Identity,
                            ["responseTypeName"] = responseContract.Name,
                            ["resultMember"] = member.Name,
                            ["resultMemberLocation"] = $"{member.Source.Path}:L{member.Source.StartLine}",
                            ["analysisMode"] = "typescript-bounded-response-contract",
                            ["analysisConfidence"] = "high",
                            ["proof"] = "typed-http-generic+exact-local-interface-member"
                        }));
                }
            }

            var componentClasses = FindClasses(text)
                .Where(classRange => classRange.Name.EndsWith("Component", StringComparison.Ordinal))
                .ToArray();
            var serviceTypes = FindServiceTypes(text);
            foreach (Match match in ResultMemberBindingRegex.Matches(text))
            {
                if (!IsActiveCodePosition(text, match.Index))
                {
                    continue;
                }

                var component = componentClasses
                    .Where(range => range.OpenBrace < match.Index && range.CloseBrace >= match.Index)
                    .OrderByDescending(range => range.OpenBrace)
                    .FirstOrDefault();
                var service = match.Groups["service"].Value;
                if (component is null ||
                    !serviceTypes.TryGetValue(service, out var serviceTypeName))
                {
                    continue;
                }

                var serviceIdentity = ResolveTypeIdentity(
                    file.RelativePath,
                    serviceTypeName,
                    fileImports,
                    contracts: null,
                    ambiguousContracts: null);
                if (serviceIdentity is null)
                {
                    continue;
                }

                var source = GetLocation(file.RelativePath, text, match.Index, match.Length);
                facts.Add(new EvidenceFact(
                    $"ui-wire:{file.RelativePath}:{source.StartLine}:result-member-binding:{match.Groups["method"].Value}:{match.Groups["target"].Value}",
                    "ui-result-member-binding",
                    $"{match.Groups["method"].Value} result.{match.Groups["member"].Value} -> {match.Groups["target"].Value}",
                    component.Name,
                    source,
                    [],
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["service"] = service,
                        ["serviceType"] = serviceIdentity,
                        ["apiMethod"] = match.Groups["method"].Value,
                        ["resultParameter"] = match.Groups["result"].Value,
                        ["resultMember"] = match.Groups["member"].Value,
                        ["target"] = match.Groups["target"].Value,
                        ["component"] = component.Name,
                        ["componentIdentity"] = TypeIdentity(file.RelativePath, component.Name),
                        ["analysisMode"] = "typescript-bounded-result-member-binding",
                        ["analysisConfidence"] = "high",
                        ["proof"] = "exact-subscribe-result-receiver-member-assignment"
                    }));
            }

            if (componentClasses.Length == 1)
            {
                var component = componentClasses[0];
                foreach (Match template in TemplateRegex.Matches(text))
                {
                    if (!IsActiveCodePosition(text, template.Index) ||
                        template.Index > component.OpenBrace)
                    {
                        continue;
                    }

                    var body = template.Groups["body"];
                    foreach (Match interpolation in InterpolationRegex.Matches(body.Value))
                    {
                        var absoluteIndex = body.Index + interpolation.Index;
                        var source = GetLocation(
                            file.RelativePath,
                            text,
                            absoluteIndex,
                            interpolation.Length);
                        var member = interpolation.Groups["member"].Value;
                        facts.Add(new EvidenceFact(
                            $"ui-wire:{file.RelativePath}:{source.StartLine}:member-render:{component.Name}:{member}",
                            "ui-member-render",
                            $"render {member}",
                            component.Name,
                            source,
                            [],
                            new Dictionary<string, string>(StringComparer.Ordinal)
                            {
                                ["member"] = member,
                                ["component"] = component.Name,
                                ["componentIdentity"] = TypeIdentity(file.RelativePath, component.Name),
                                ["analysisMode"] = "angular-bounded-interpolation",
                                ["analysisConfidence"] = "high",
                                ["proof"] = "exact-simple-template-interpolation"
                            }));
                    }
                }
            }
        }

        return new FactDocument(
            "0.4.7-angular-response-binding",
            facts.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray(),
            []);
    }

    private static IReadOnlyList<TypeContract> FindInterfaces(string path, string text)
    {
        var result = new List<TypeContract>();
        foreach (Match match in InterfaceRegex.Matches(text))
        {
            if (!IsActiveCodePosition(text, match.Index))
            {
                continue;
            }

            var openBrace = match.Index + match.Value.LastIndexOf('{');
            var closeBrace = FindMatchingBrace(text, openBrace);
            if (closeBrace < 0)
            {
                continue;
            }

            var bodyStart = openBrace + 1;
            var body = text[bodyStart..closeBrace];
            var members = new List<TypeMember>();
            foreach (Match property in InterfacePropertyRegex.Matches(body))
            {
                var absoluteIndex = bodyStart + property.Index;
                if (!IsActiveCodePosition(text, absoluteIndex))
                {
                    continue;
                }

                members.Add(new TypeMember(
                    property.Groups["name"].Value,
                    GetLocation(path, text, absoluteIndex, property.Length)));
            }

            var name = match.Groups["name"].Value;
            result.Add(new TypeContract(TypeIdentity(path, name), name, members));
        }

        return result;
    }

    private static IReadOnlyList<ClassRange> FindClasses(string text)
    {
        var result = new List<ClassRange>();
        foreach (Match match in ClassRegex.Matches(text))
        {
            if (!IsActiveCodePosition(text, match.Index))
            {
                continue;
            }

            var openBrace = match.Index + match.Value.LastIndexOf('{');
            var closeBrace = FindMatchingBrace(text, openBrace);
            if (closeBrace >= 0)
            {
                result.Add(new ClassRange(match.Groups["name"].Value, openBrace, closeBrace));
            }
        }

        return result;
    }

    private static string? FindContainingClass(string text, int index) =>
        FindClasses(text)
            .Where(range => range.OpenBrace < index && range.CloseBrace >= index)
            .OrderByDescending(range => range.OpenBrace)
            .Select(range => range.Name)
            .FirstOrDefault();

    private static string? FindContainingMethod(string text, int index)
    {
        string? result = null;
        var bestStart = -1;
        foreach (Match match in MethodRegex.Matches(text))
        {
            if (match.Index > index || !IsActiveCodePosition(text, match.Index))
            {
                continue;
            }

            var openBrace = match.Index + match.Value.LastIndexOf('{');
            var closeBrace = FindMatchingBrace(text, openBrace);
            if (openBrace < index && closeBrace >= index && openBrace > bestStart)
            {
                bestStart = openBrace;
                result = match.Groups["name"].Value;
            }
        }

        return result;
    }

    private static IReadOnlyDictionary<string, string> FindServiceTypes(string text)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var ambiguous = new HashSet<string>(StringComparer.Ordinal);
        AddServiceTypes(InjectServiceRegex, text, result, ambiguous);
        AddServiceTypes(NewServiceRegex, text, result, ambiguous);
        AddServiceTypes(ConstructorServiceRegex, text, result, ambiguous);
        return result;
    }

    private static void AddServiceTypes(
        Regex regex,
        string text,
        IDictionary<string, string> result,
        ISet<string> ambiguous)
    {
        foreach (Match match in regex.Matches(text))
        {
            if (!IsActiveCodePosition(text, match.Index))
            {
                continue;
            }

            var service = match.Groups["service"].Value;
            var type = match.Groups["type"].Value;
            if (ambiguous.Contains(service))
            {
                continue;
            }

            if (result.TryGetValue(service, out var existing) &&
                !string.Equals(existing, type, StringComparison.Ordinal))
            {
                result.Remove(service);
                ambiguous.Add(service);
                continue;
            }

            result[service] = type;
        }
    }

    private static IReadOnlyDictionary<string, ImportedType> FindImports(
        string sourcePath,
        string text,
        IReadOnlySet<string> sourcePaths)
    {
        var result = new Dictionary<string, ImportedType>(StringComparer.Ordinal);
        var ambiguous = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in NamedImportRegex.Matches(text))
        {
            if (!IsActiveCodePosition(text, match.Index))
            {
                continue;
            }

            var resolvedModule = ResolveRelativeModule(
                sourcePath,
                match.Groups["module"].Value,
                sourcePaths);
            if (resolvedModule is null)
            {
                continue;
            }

            foreach (var rawBinding in match.Groups["bindings"].Value.Split(','))
            {
                var binding = rawBinding.Trim();
                if (binding.StartsWith("type ", StringComparison.Ordinal))
                {
                    binding = binding[5..].Trim();
                }
                if (binding.Length == 0)
                {
                    continue;
                }

                var aliasParts = Regex.Split(binding, @"\s+as\s+", RegexOptions.IgnoreCase);
                var exportedName = aliasParts[0].Trim();
                var localName = aliasParts.Length == 2 ? aliasParts[1].Trim() : exportedName;
                if (!IsIdentifier(exportedName) ||
                    !IsIdentifier(localName) ||
                    ambiguous.Contains(localName))
                {
                    continue;
                }

                var imported = new ImportedType(exportedName, resolvedModule);
                if (result.TryGetValue(localName, out var existing) && existing != imported)
                {
                    result.Remove(localName);
                    ambiguous.Add(localName);
                    continue;
                }

                result[localName] = imported;
            }
        }

        return result;
    }

    private static string? ResolveTypeIdentity(
        string sourcePath,
        string typeName,
        IReadOnlyDictionary<string, ImportedType> imports,
        IReadOnlyDictionary<string, TypeContract>? contracts,
        IReadOnlySet<string>? ambiguousContracts)
    {
        var localIdentity = TypeIdentity(sourcePath, typeName);
        if (ambiguousContracts?.Contains(localIdentity) == true)
        {
            return null;
        }
        if (contracts?.ContainsKey(localIdentity) == true)
        {
            return localIdentity;
        }

        return imports.TryGetValue(typeName, out var imported)
            ? TypeIdentity(imported.ModulePath, imported.ExportedName)
            : null;
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
            normalizedBase.EndsWith(".ts", StringComparison.OrdinalIgnoreCase)
                ? normalizedBase
                : $"{normalizedBase}.ts",
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

    private static int FindMatchingBrace(string text, int openBrace)
    {
        if (openBrace < 0 || openBrace >= text.Length || text[openBrace] != '{')
        {
            return -1;
        }

        var depth = 0;
        var state = TypeScriptLexicalState.Code;
        for (var index = openBrace; index < text.Length; index++)
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
                    else if (current == '\'') state = TypeScriptLexicalState.SingleQuotedString;
                    else if (current == '"') state = TypeScriptLexicalState.DoubleQuotedString;
                    else if (current == '`') state = TypeScriptLexicalState.TemplateLiteral;
                    else if (current == '{') depth++;
                    else if (current == '}' && --depth == 0) return index;
                    break;
                case TypeScriptLexicalState.LineComment:
                    if (current is '\r' or '\n') state = TypeScriptLexicalState.Code;
                    break;
                case TypeScriptLexicalState.BlockComment:
                    if (current == '*' && next == '/')
                    {
                        state = TypeScriptLexicalState.Code;
                        index++;
                    }
                    break;
                case TypeScriptLexicalState.SingleQuotedString:
                    if (current == '\\') index++;
                    else if (current == '\'') state = TypeScriptLexicalState.Code;
                    break;
                case TypeScriptLexicalState.DoubleQuotedString:
                    if (current == '\\') index++;
                    else if (current == '"') state = TypeScriptLexicalState.Code;
                    break;
                case TypeScriptLexicalState.TemplateLiteral:
                    if (current == '\\') index++;
                    else if (current == '`') state = TypeScriptLexicalState.Code;
                    break;
                case TypeScriptLexicalState.RegexLiteral:
                    break;
            }
        }
        return -1;
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
                    else if (current == '\'') state = TypeScriptLexicalState.SingleQuotedString;
                    else if (current == '"') state = TypeScriptLexicalState.DoubleQuotedString;
                    else if (current == '`')
                    {
                        state = TypeScriptLexicalState.TemplateLiteral;
                        interpolatedTemplate = false;
                    }
                    else if (current == '/') state = TypeScriptLexicalState.RegexLiteral;
                    break;
                case TypeScriptLexicalState.LineComment:
                    if (current is '\r' or '\n') state = TypeScriptLexicalState.Code;
                    break;
                case TypeScriptLexicalState.BlockComment:
                    if (current == '*' && next == '/')
                    {
                        state = TypeScriptLexicalState.Code;
                        index++;
                    }
                    break;
                case TypeScriptLexicalState.SingleQuotedString:
                    if (current == '\\') index++;
                    else if (current == '\'') state = TypeScriptLexicalState.Code;
                    break;
                case TypeScriptLexicalState.DoubleQuotedString:
                    if (current == '\\') index++;
                    else if (current == '"') state = TypeScriptLexicalState.Code;
                    break;
                case TypeScriptLexicalState.TemplateLiteral:
                    if (current == '\\') index++;
                    else if (current == '$' && next == '{')
                    {
                        interpolatedTemplate = true;
                        index++;
                    }
                    else if (current == '`' && !interpolatedTemplate)
                    {
                        state = TypeScriptLexicalState.Code;
                    }
                    break;
                case TypeScriptLexicalState.RegexLiteral:
                    if (current == '\\') index++;
                    else if (current == '/' || current is '\r' or '\n') state = TypeScriptLexicalState.Code;
                    break;
            }
        }
        return state == TypeScriptLexicalState.Code;
    }

    private static SourceLocation GetLocation(
        string path,
        string text,
        int index,
        int length)
    {
        var startLine = 1 + text.AsSpan(0, index).Count('\n');
        var endLine = startLine + text.AsSpan(index, length).Count('\n');
        return new SourceLocation(path, startLine, endLine);
    }

    private static string NormalizeRouteKey(string value)
    {
        var route = value.Split('?', '#')[0].Trim();
        route = TemplateParameterRegex.Replace(route, "{param}");
        route = RouteParameterRegex.Replace(route, "{param}");
        while (route.Contains("//", StringComparison.Ordinal))
        {
            route = route.Replace("//", "/", StringComparison.Ordinal);
        }
        if (!route.StartsWith('/')) route = "/" + route;
        return route.TrimEnd('/').ToLowerInvariant();
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

    private sealed record SourceFile(string FullPath, string RelativePath);
    private sealed record ImportedType(string ExportedName, string ModulePath);
    private sealed record TypeContract(string Identity, string Name, IReadOnlyList<TypeMember> Members);
    private sealed record TypeMember(string Name, SourceLocation Source);
    private sealed record ClassRange(string Name, int OpenBrace, int CloseBrace);

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
}
