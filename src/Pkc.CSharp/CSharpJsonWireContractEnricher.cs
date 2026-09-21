using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Pkc.Core;

namespace Pkc.CSharp;

internal sealed class CSharpJsonWireContractEnricher
{
    private const string JsonPropertyNameAttribute =
        "global::System.Text.Json.Serialization.JsonPropertyNameAttribute";

    private static readonly object RegistrationGate = new();

    public async Task<FactDocument> EnrichAsync(
        string repositoryPath,
        FactDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);
        ArgumentNullException.ThrowIfNull(document);

        var projections = document.Facts
            .Where(fact => fact.Kind == "value-transfer")
            .Where(fact => string.Equals(
                fact.Metadata.GetValueOrDefault("mechanism"),
                "api-projection",
                StringComparison.Ordinal))
            .Where(fact => !string.IsNullOrWhiteSpace(fact.Metadata.GetValueOrDefault("semanticProject")))
            .Where(fact => !string.IsNullOrWhiteSpace(fact.Metadata.GetValueOrDefault("targetMemberIdentity")))
            .Where(fact => !string.IsNullOrWhiteSpace(fact.Metadata.GetValueOrDefault("targetMemberLocation")))
            .ToArray();
        if (projections.Length == 0)
        {
            return document;
        }

        EnsureMsBuildRegistered();
        var rootPath = Path.GetFullPath(repositoryPath);
        var facts = document.Facts.ToDictionary(fact => fact.Id, StringComparer.Ordinal);

        foreach (var projectGroup in projections.GroupBy(
                     projection => projection.Metadata["semanticProject"],
                     StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var projectPath = projectGroup.Key;
            var fullProjectPath = Path.GetFullPath(Path.Combine(
                rootPath,
                projectPath.Replace('/', Path.DirectorySeparatorChar)));
            if (!File.Exists(fullProjectPath))
            {
                continue;
            }

            try
            {
                using var workspace = MSBuildWorkspace.Create();
                var project = await workspace.OpenProjectAsync(fullProjectPath, cancellationToken: cancellationToken);
                var compilation = await project.GetCompilationAsync(cancellationToken);
                if (compilation is null)
                {
                    continue;
                }

                foreach (var projection in projectGroup)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!TryParseSourceLine(
                            projection.Metadata["targetMemberLocation"],
                            out var relativePath,
                            out var startLine))
                    {
                        continue;
                    }

                    var fullSourcePath = Path.GetFullPath(Path.Combine(
                        rootPath,
                        relativePath.Replace('/', Path.DirectorySeparatorChar)));
                    var tree = compilation.SyntaxTrees.FirstOrDefault(candidate =>
                        !string.IsNullOrWhiteSpace(candidate.FilePath) &&
                        string.Equals(
                            Path.GetFullPath(candidate.FilePath),
                            fullSourcePath,
                            StringComparison.OrdinalIgnoreCase));
                    if (tree is null)
                    {
                        continue;
                    }

                    var syntaxRoot = await tree.GetRootAsync(cancellationToken);
                    var model = compilation.GetSemanticModel(tree, ignoreAccessibility: true);
                    var matchingProperties = syntaxRoot.DescendantNodes()
                        .OfType<PropertyDeclarationSyntax>()
                        .Where(property => StartLine(property) == startLine)
                        .Select(property => new
                        {
                            Syntax = property,
                            Symbol = model.GetDeclaredSymbol(property, cancellationToken)
                        })
                        .Where(candidate => candidate.Symbol is not null)
                        .Where(candidate => string.Equals(
                            BuildMemberIdentity(candidate.Symbol!),
                            projection.Metadata["targetMemberIdentity"],
                            StringComparison.Ordinal))
                        .ToArray();
                    if (matchingProperties.Length != 1 ||
                        !TryGetExplicitJsonWireName(
                            matchingProperties[0].Symbol!,
                            out var wireName,
                            out var attributeLocation))
                    {
                        continue;
                    }

                    var projectionMetadata = new Dictionary<string, string>(
                        projection.Metadata,
                        StringComparer.Ordinal)
                    {
                        ["wireName"] = wireName,
                        ["wireContract"] = "System.Text.Json.Serialization.JsonPropertyNameAttribute",
                        ["wireContractLocation"] = DescribeLocation(
                            attributeLocation,
                            rootPath,
                            relativePath)
                    };
                    facts[projection.Id] = projection with { Metadata = projectionMetadata };

                    foreach (var terminal in document.Facts.Where(fact =>
                                 fact.Kind == "value-terminal-source" &&
                                 string.Equals(
                                     fact.Metadata.GetValueOrDefault("boundary"),
                                     "API response field",
                                     StringComparison.Ordinal) &&
                                 string.Equals(
                                     fact.Metadata.GetValueOrDefault("sourceFactId"),
                                     projection.Id,
                                     StringComparison.Ordinal)))
                    {
                        var terminalMetadata = new Dictionary<string, string>(
                            terminal.Metadata,
                            StringComparer.Ordinal)
                        {
                            ["wireName"] = wireName,
                            ["wireContract"] = "System.Text.Json.Serialization.JsonPropertyNameAttribute",
                            ["wireContractLocation"] = projectionMetadata["wireContractLocation"]
                        };
                        facts[terminal.Id] = terminal with { Metadata = terminalMetadata };
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                // R7.9 wire proof is optional and fail-closed. C projection lineage remains
                // authoritative at its accepted boundary when exact JsonPropertyName identity
                // cannot be re-established from target-project semantics.
            }
        }

        return document with
        {
            Facts = facts.Values.OrderBy(fact => fact.Id, StringComparer.Ordinal).ToArray()
        };
    }

    private static bool TryGetExplicitJsonWireName(
        IPropertySymbol property,
        out string wireName,
        out Location attributeLocation)
    {
        var attributes = property.GetAttributes()
            .Where(attribute => string.Equals(
                attribute.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                JsonPropertyNameAttribute,
                StringComparison.Ordinal))
            .ToArray();
        if (attributes.Length != 1 ||
            attributes[0].ConstructorArguments.Length != 1 ||
            attributes[0].ConstructorArguments[0].Value is not string value ||
            string.IsNullOrEmpty(value))
        {
            wireName = string.Empty;
            attributeLocation = Location.None;
            return false;
        }

        var syntax = attributes[0].ApplicationSyntaxReference?.GetSyntax();
        if (syntax is null)
        {
            wireName = string.Empty;
            attributeLocation = Location.None;
            return false;
        }

        wireName = value;
        attributeLocation = syntax.GetLocation();
        return true;
    }

    private static bool TryParseSourceLine(
        string value,
        out string relativePath,
        out int startLine)
    {
        var marker = value.LastIndexOf(":L", StringComparison.Ordinal);
        if (marker <= 0 ||
            !int.TryParse(value[(marker + 2)..], out startLine) ||
            startLine <= 0)
        {
            relativePath = string.Empty;
            startLine = 0;
            return false;
        }

        relativePath = value[..marker];
        return !string.IsNullOrWhiteSpace(relativePath);
    }

    private static string BuildMemberIdentity(IPropertySymbol property)
    {
        var display = $"{property.ContainingType.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)}.{property.Name}";
        return $"{property.ContainingAssembly?.Identity.ToString() ?? "unknown-assembly"}|{display}";
    }

    private static string DescribeLocation(
        Location location,
        string rootPath,
        string fallbackPath)
    {
        if (!location.IsInSource)
        {
            return $"{fallbackPath}:L0";
        }

        var span = location.GetLineSpan();
        var relativePath = string.IsNullOrWhiteSpace(span.Path)
            ? fallbackPath
            : Path.GetRelativePath(rootPath, Path.GetFullPath(span.Path))
                .Replace(Path.DirectorySeparatorChar, '/');
        return $"{relativePath}:L{span.StartLinePosition.Line + 1}";
    }

    private static int StartLine(SyntaxNode node) =>
        node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

    private static void EnsureMsBuildRegistered()
    {
        lock (RegistrationGate)
        {
            if (!MSBuildLocator.IsRegistered)
            {
                MSBuildLocator.RegisterDefaults();
            }
        }
    }
}
