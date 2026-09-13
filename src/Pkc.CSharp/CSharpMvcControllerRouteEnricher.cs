using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Pkc.Core;

namespace Pkc.CSharp;

internal sealed class CSharpMvcControllerRouteEnricher
{
    public FactDocument Enrich(FactDocument document)
    {
        var types = document.Facts
            .Where(fact => string.Equals(fact.Kind, "class", StringComparison.Ordinal))
            .Select(CreateTypeInfo)
            .ToArray();

        var byQualifiedName = types
            .Where(type => !string.IsNullOrWhiteSpace(type.QualifiedName))
            .GroupBy(type => type.QualifiedName, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        var bySimpleName = types
            .GroupBy(type => type.Fact.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        var facts = document.Facts
            .Select(fact => EnrichEndpoint(fact, byQualifiedName, bySimpleName))
            .ToArray();

        return document with { Facts = facts };
    }

    private static EvidenceFact EnrichEndpoint(
        EvidenceFact fact,
        IReadOnlyDictionary<string, TypeInfo> byQualifiedName,
        IReadOnlyDictionary<string, TypeInfo[]> bySimpleName)
    {
        if (!string.Equals(fact.Kind, "endpoint", StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(fact.Container) ||
            !byQualifiedName.TryGetValue(fact.Container, out var controller))
        {
            return fact;
        }

        var route = ResolveEffectiveControllerRoute(controller, byQualifiedName, bySimpleName);
        if (!route.Found)
        {
            return fact;
        }

        var metadata = new Dictionary<string, string>(fact.Metadata, StringComparer.Ordinal);
        if (route.Template is null)
        {
            // A route attribute exists, so framework inheritance stops here, but PKC cannot
            // prove its template from a literal. Keeping an action-only full route would be a
            // false claim because the effective controller prefix is unknown.
            metadata.Remove("controllerRouteTemplate");
            metadata.Remove("fullRoute");
            return fact with { Metadata = metadata };
        }

        if (string.IsNullOrWhiteSpace(route.Template))
        {
            metadata.Remove("controllerRouteTemplate");
        }
        else
        {
            metadata["controllerRouteTemplate"] = route.Template;
        }

        metadata.TryGetValue("routeTemplate", out var actionRoute);
        var fullRoute = CombineRoute(route.Template, actionRoute, controller.Fact.Name, fact.Name);
        if (string.IsNullOrWhiteSpace(fullRoute))
        {
            metadata.Remove("fullRoute");
        }
        else
        {
            metadata["fullRoute"] = fullRoute;
        }

        return fact with { Metadata = metadata };
    }

    private static RouteResolution ResolveEffectiveControllerRoute(
        TypeInfo controller,
        IReadOnlyDictionary<string, TypeInfo> byQualifiedName,
        IReadOnlyDictionary<string, TypeInfo[]> bySimpleName)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        TypeInfo? current = controller;

        while (current is not null && visited.Add(current.QualifiedName))
        {
            if (current.DeclaredRoute.Found)
            {
                return current.DeclaredRoute;
            }

            current = ResolveBaseClass(current, byQualifiedName, bySimpleName);
        }

        return default;
    }

    private static TypeInfo? ResolveBaseClass(
        TypeInfo current,
        IReadOnlyDictionary<string, TypeInfo> byQualifiedName,
        IReadOnlyDictionary<string, TypeInfo[]> bySimpleName)
    {
        if (!current.Fact.Metadata.TryGetValue("baseTypes", out var baseTypes) ||
            string.IsNullOrWhiteSpace(baseTypes))
        {
            return null;
        }

        foreach (var candidate in SplitTopLevel(baseTypes))
        {
            var normalized = NormalizeTypeReference(candidate);
            if (normalized.Length == 0)
            {
                continue;
            }

            if (byQualifiedName.TryGetValue(normalized, out var exact))
            {
                return exact;
            }

            if (!normalized.Contains('.', StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(current.NamespaceName) &&
                byQualifiedName.TryGetValue($"{current.NamespaceName}.{normalized}", out var sameNamespace))
            {
                return sameNamespace;
            }

            var simpleName = normalized.Split('.').Last();
            if (bySimpleName.TryGetValue(simpleName, out var matches) && matches.Length == 1)
            {
                return matches[0];
            }
        }

        // Ambiguous or external base classes are intentionally left unresolved rather than
        // guessing a route prefix from a same-named type.
        return null;
    }

    private static TypeInfo CreateTypeInfo(EvidenceFact fact)
    {
        fact.Metadata.TryGetValue("namespace", out var namespaceName);
        var qualifiedName = string.IsNullOrWhiteSpace(namespaceName)
            ? fact.Name
            : $"{namespaceName}.{fact.Name}";

        return new TypeInfo(
            fact,
            qualifiedName,
            namespaceName ?? string.Empty,
            GetDeclaredRoute(fact.Attributes));
    }

    private static RouteResolution GetDeclaredRoute(IReadOnlyList<string> attributes)
    {
        foreach (var attributeText in attributes)
        {
            var tree = CSharpSyntaxTree.ParseText($"[{attributeText}] class __PkcRouteProbe {{ }}");
            var attribute = tree.GetRoot().DescendantNodes().OfType<AttributeSyntax>().FirstOrDefault();
            if (attribute is null ||
                !string.Equals(NormalizeAttributeName(attribute.Name.ToString()), "Route", StringComparison.Ordinal))
            {
                continue;
            }

            var argument = attribute.ArgumentList?.Arguments
                .FirstOrDefault(item => item.NameEquals is null && item.NameColon is null);
            var template = argument?.Expression is LiteralExpressionSyntax literal &&
                           literal.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression)
                ? literal.Token.ValueText
                : null;

            return new RouteResolution(true, template);
        }

        return default;
    }

    private static string NormalizeAttributeName(string value)
    {
        var simpleName = value.Split('.').Last();
        return simpleName.EndsWith("Attribute", StringComparison.Ordinal)
            ? simpleName[..^"Attribute".Length]
            : simpleName;
    }

    private static IReadOnlyList<string> SplitTopLevel(string value)
    {
        var result = new List<string>();
        var start = 0;
        var depth = 0;

        for (var index = 0; index < value.Length; index++)
        {
            depth += value[index] switch
            {
                '<' => 1,
                '>' => -1,
                _ => 0
            };

            if (value[index] == ',' && depth == 0)
            {
                result.Add(value[start..index].Trim());
                start = index + 1;
            }
        }

        result.Add(value[start..].Trim());
        return result;
    }

    private static string NormalizeTypeReference(string value)
    {
        var normalized = value.Trim().Replace("global::", string.Empty, StringComparison.Ordinal);
        var genericStart = normalized.IndexOf('<');
        return genericStart >= 0 ? normalized[..genericStart].Trim() : normalized;
    }

    private static string? CombineRoute(
        string? controllerRoute,
        string? actionRoute,
        string controllerName,
        string actionName)
    {
        var controller = NormalizeRoutePart(controllerRoute);
        var action = NormalizeRoutePart(actionRoute);
        var combined = string.Join("/", new[] { controller, action }.Where(part => !string.IsNullOrWhiteSpace(part)));
        if (combined.Length == 0)
        {
            return null;
        }

        var controllerToken = controllerName.EndsWith("Controller", StringComparison.Ordinal)
            ? controllerName[..^"Controller".Length]
            : controllerName;

        return combined
            .Replace("[controller]", controllerToken, StringComparison.OrdinalIgnoreCase)
            .Replace("[action]", actionName, StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeRoutePart(string? route) =>
        string.IsNullOrWhiteSpace(route) ? null : route.Trim().Trim('/');

    private sealed record TypeInfo(
        EvidenceFact Fact,
        string QualifiedName,
        string NamespaceName,
        RouteResolution DeclaredRoute);

    private readonly record struct RouteResolution(bool Found, string? Template);
}
