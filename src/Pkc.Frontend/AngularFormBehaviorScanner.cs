using System.Text.RegularExpressions;
using Pkc.Core;

namespace Pkc.Frontend;

internal sealed class AngularFormBehaviorScanner
{
    private static readonly HashSet<string> ExcludedDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", ".pkc", "bin", "obj", "node_modules", "dist", "build", "coverage", "knowledge"
    };

    private static readonly Regex ComponentRegex = new(
        @"@Component\s*\(\s*\{(?<metadata>[\s\S]*?)\}\s*\)\s*(?:export\s+)?class\s+(?<name>[A-Z][A-Za-z0-9_]*Component)\b",
        RegexOptions.Compiled);

    private static readonly Regex TemplateUrlRegex = new(
        @"templateUrl\s*:\s*[""'](?<url>[^""']+)[""']",
        RegexOptions.Compiled);

    private static readonly Regex InlineTemplateRegex = new(
        @"template\s*:\s*(?<quote>`|[""'])(?<template>[\s\S]*?)\k<quote>",
        RegexOptions.Compiled);

    private static readonly Regex FieldRegex = new(
        @"<(?<tag>input|select|textarea|mat-select)\b(?<attrs>[^>]*)>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex SelectRegex = new(
        @"<(?<tag>select|mat-select)\b(?<attrs>[^>]*)>(?<body>[\s\S]*?)</(?:select|mat-select)>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex OptionRegex = new(
        @"<(?:option|mat-option)\b(?<attrs>[^>]*)>(?<label>[\s\S]*?)</(?:option|mat-option)>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex FormControlNameRegex = new(
        @"formControlName\s*=\s*[""'](?<name>[^""']+)[""']",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ValueRegex = new(
        @"(?:\[value\]|value)\s*=\s*[""'](?<value>[^""']+)[""']",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex BoundRequiredRegex = new(
        @"\[required\]\s*=\s*[""'](?<condition>[^""']+)[""']",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex RequiredRegex = new(
        @"(?:^|\s)required(?:\s|$)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex BoundDisabledRegex = new(
        @"\[disabled\]\s*=\s*[""'](?<condition>[^""']+)[""']",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex DisabledRegex = new(
        @"(?:^|\s)disabled(?:\s|$)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex NgIfRegex = new(
        @"\*ngIf\s*=\s*[""'](?<condition>[^""']+)[""']",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex AtIfRegex = new(
        @"@if\s*\((?<condition>[^\r\n{}]+)\)\s*\{",
        RegexOptions.Compiled);

    private static readonly Regex DirectRequiredValidatorRegex = new(
        @"(?<field>[A-Za-z_$][A-Za-z0-9_$]*)\s*:\s*(?<initializer>\[[^\r\n;]{0,400}Validators\.required[^\r\n;]{0,400}\]|(?:this\.)?[A-Za-z_$][A-Za-z0-9_$]*\.control\([^\r\n;]{0,400}Validators\.required[^\r\n;]{0,400}\))",
        RegexOptions.Compiled);

    private static readonly Regex ConditionalRequiredValidatorRegex = new(
        @"(?:(?:this\.)?[A-Za-z_$][A-Za-z0-9_$]*\.controls\.(?<field>[A-Za-z_$][A-Za-z0-9_$]*)|(?:this\.)?[A-Za-z_$][A-Za-z0-9_$]*\.get\(\s*[""'](?<field2>[^""']+)[""']\s*\))\s*\??\.setValidators\(\s*(?<validators>[^)]*Validators\.required[^)]*)\)",
        RegexOptions.Compiled);

    private static readonly Regex RequestBindingRegex = new(
        @"(?<requestField>[A-Za-z_$][A-Za-z0-9_$]*)\s*:\s*(?<expression>(?:this\.)?(?<form>[A-Za-z_$][A-Za-z0-9_$]*)\.(?:value\.(?<field1>[A-Za-z_$][A-Za-z0-9_$]*)|getRawValue\(\)\.(?<field2>[A-Za-z_$][A-Za-z0-9_$]*)|controls\.(?<field3>[A-Za-z_$][A-Za-z0-9_$]*)\.value))",
        RegexOptions.Compiled);

    private static readonly Regex MethodRegex = new(
        @"(?m)^\s*(?<name>[A-Za-z_$][A-Za-z0-9_$]*)\s*\([^\r\n)]*\)\s*(?::\s*[^{\r\n]+)?\{",
        RegexOptions.Compiled);

    private static readonly Regex HtmlTagRegex = new(@"<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex AngularInterpolationRegex = new(@"\{\{[^}]+\}\}", RegexOptions.Compiled);
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    public async Task<FactDocument> ScanAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default)
    {
        var rootPath = Path.GetFullPath(repositoryPath);
        var facts = new List<EvidenceFact>();
        var relations = new List<EvidenceRelation>();

        var typeScriptFiles = Directory.EnumerateFiles(rootPath, "*.ts", SearchOption.AllDirectories)
            .Where(path => !path.EndsWith(".d.ts", StringComparison.OrdinalIgnoreCase))
            .Where(path => !IsExcluded(rootPath, path))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var file in typeScriptFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var text = await File.ReadAllTextAsync(file, cancellationToken);
            var relativePath = NormalizePath(Path.GetRelativePath(rootPath, file));

            foreach (Match componentMatch in ComponentRegex.Matches(text))
            {
                var component = componentMatch.Groups["name"].Value;
                var metadataText = componentMatch.Groups["metadata"].Value;
                var metadataOffset = componentMatch.Groups["metadata"].Index;

                var inlineTemplate = InlineTemplateRegex.Match(metadataText);
                if (inlineTemplate.Success)
                {
                    var template = inlineTemplate.Groups["template"].Value;
                    var templateOffset = metadataOffset + inlineTemplate.Groups["template"].Index;
                    ExtractTemplate(
                        relativePath,
                        text,
                        template,
                        templateOffset,
                        component,
                        facts,
                        relations);
                }

                var templateUrl = TemplateUrlRegex.Match(metadataText);
                if (templateUrl.Success)
                {
                    var fullTemplatePath = Path.GetFullPath(Path.Combine(
                        Path.GetDirectoryName(file)!,
                        templateUrl.Groups["url"].Value));

                    if (File.Exists(fullTemplatePath) && !IsExcluded(rootPath, fullTemplatePath))
                    {
                        var templateText = await File.ReadAllTextAsync(fullTemplatePath, cancellationToken);
                        var templateRelativePath = NormalizePath(Path.GetRelativePath(rootPath, fullTemplatePath));
                        ExtractTemplate(
                            templateRelativePath,
                            templateText,
                            templateText,
                            0,
                            component,
                            facts,
                            relations);
                    }
                }

                ExtractTypeScriptBehavior(
                    relativePath,
                    text,
                    component,
                    facts,
                    relations);
            }
        }

        return new FactDocument(
            "0.4.4-angular-form-behavior",
            facts
                .GroupBy(fact => fact.Id, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(fact => fact.Id, StringComparer.Ordinal)
                .ToArray(),
            relations
                .GroupBy(RelationKey, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(relation => relation.FromFactId, StringComparer.Ordinal)
                .ThenBy(relation => relation.Kind, StringComparer.Ordinal)
                .ThenBy(relation => relation.Target, StringComparer.Ordinal)
                .ToArray());
    }

    private static void ExtractTemplate(
        string relativePath,
        string fullText,
        string template,
        int templateOffset,
        string component,
        ICollection<EvidenceFact> facts,
        ICollection<EvidenceRelation> relations)
    {
        foreach (Match match in FieldRegex.Matches(template))
        {
            var attrs = match.Groups["attrs"].Value;
            var control = FormControlNameRegex.Match(attrs);
            if (!control.Success)
            {
                continue;
            }

            var fieldName = control.Groups["name"].Value;
            var absoluteIndex = templateOffset + match.Index;
            var field = CreateFact(
                relativePath,
                fullText,
                absoluteIndex,
                match.Length,
                "ui-field",
                fieldName,
                component,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["field"] = fieldName,
                    ["formControlName"] = fieldName,
                    ["element"] = match.Groups["tag"].Value.ToLowerInvariant(),
                    ["framework"] = "angular-static"
                });
            facts.Add(Tag(field));

            var boundRequired = BoundRequiredRegex.Match(attrs);
            if (boundRequired.Success)
            {
                facts.Add(Tag(CreateBehaviorFact(
                    relativePath,
                    fullText,
                    absoluteIndex,
                    match.Length,
                    "ui-field-validation",
                    fieldName,
                    component,
                    "required",
                    boundRequired.Groups["condition"].Value)));
            }
            else if (RequiredRegex.IsMatch(RemoveBoundAttribute(attrs, "required")))
            {
                facts.Add(Tag(CreateBehaviorFact(
                    relativePath,
                    fullText,
                    absoluteIndex,
                    match.Length,
                    "ui-field-validation",
                    fieldName,
                    component,
                    "required",
                    null)));
            }

            var visibilityCondition = NgIfRegex.Match(attrs).Groups["condition"].Value;
            if (string.IsNullOrWhiteSpace(visibilityCondition))
            {
                visibilityCondition = FindNearbyAtIfCondition(template, match.Index);
            }
            if (!string.IsNullOrWhiteSpace(visibilityCondition))
            {
                facts.Add(Tag(CreateBehaviorFact(
                    relativePath,
                    fullText,
                    absoluteIndex,
                    match.Length,
                    "ui-field-visibility",
                    fieldName,
                    component,
                    "visible",
                    visibilityCondition)));
            }

            var boundDisabled = BoundDisabledRegex.Match(attrs);
            if (boundDisabled.Success)
            {
                facts.Add(Tag(CreateBehaviorFact(
                    relativePath,
                    fullText,
                    absoluteIndex,
                    match.Length,
                    "ui-field-enabled-state",
                    fieldName,
                    component,
                    "disabled",
                    boundDisabled.Groups["condition"].Value)));
            }
            else if (DisabledRegex.IsMatch(RemoveBoundAttribute(attrs, "disabled")))
            {
                facts.Add(Tag(CreateBehaviorFact(
                    relativePath,
                    fullText,
                    absoluteIndex,
                    match.Length,
                    "ui-field-enabled-state",
                    fieldName,
                    component,
                    "disabled",
                    null)));
            }

            if (string.Equals(match.Groups["tag"].Value, "input", StringComparison.OrdinalIgnoreCase) &&
                attrs.Contains("radio", StringComparison.OrdinalIgnoreCase))
            {
                var value = ValueRegex.Match(attrs);
                if (value.Success)
                {
                    facts.Add(Tag(CreateOptionFact(
                        relativePath,
                        fullText,
                        absoluteIndex,
                        match.Length,
                        fieldName,
                        component,
                        value.Groups["value"].Value,
                        value.Groups["value"].Value)));
                }
            }
        }

        foreach (Match select in SelectRegex.Matches(template))
        {
            var control = FormControlNameRegex.Match(select.Groups["attrs"].Value);
            if (!control.Success)
            {
                continue;
            }

            var fieldName = control.Groups["name"].Value;
            foreach (Match option in OptionRegex.Matches(select.Groups["body"].Value))
            {
                var value = ValueRegex.Match(option.Groups["attrs"].Value);
                if (!value.Success)
                {
                    continue;
                }

                var label = CleanLabel(option.Groups["label"].Value);
                var absoluteIndex = templateOffset + select.Groups["body"].Index + option.Index;
                facts.Add(Tag(CreateOptionFact(
                    relativePath,
                    fullText,
                    absoluteIndex,
                    option.Length,
                    fieldName,
                    component,
                    value.Groups["value"].Value,
                    label)));
            }
        }
    }

    private static void ExtractTypeScriptBehavior(
        string relativePath,
        string text,
        string component,
        ICollection<EvidenceFact> facts,
        ICollection<EvidenceRelation> relations)
    {
        foreach (Match match in DirectRequiredValidatorRegex.Matches(text))
        {
            var fieldName = match.Groups["field"].Value;
            facts.Add(Tag(CreateBehaviorFact(
                relativePath,
                text,
                match.Index,
                match.Length,
                "ui-field-validation",
                fieldName,
                component,
                "required",
                null)));
        }

        foreach (Match match in ConditionalRequiredValidatorRegex.Matches(text))
        {
            var fieldName = match.Groups["field"].Success
                ? match.Groups["field"].Value
                : match.Groups["field2"].Value;
            var condition = FindNearbyTypeScriptIfCondition(text, match.Index);
            var fact = CreateBehaviorFact(
                relativePath,
                text,
                match.Index,
                match.Length,
                "ui-field-validation",
                fieldName,
                component,
                "required",
                condition);

            var metadata = new Dictionary<string, string>(fact.Metadata, StringComparer.Ordinal)
            {
                ["validatorExpression"] = match.Groups["validators"].Value.Trim()
            };
            if (string.IsNullOrWhiteSpace(condition))
            {
                metadata["analysisCaveat"] = "setValidators-required-detected-but-enclosing-condition-not-resolved";
            }
            facts.Add(Tag(fact with { Metadata = metadata }));
        }

        foreach (Match match in RequestBindingRegex.Matches(text))
        {
            var fieldName = FirstNonEmpty(
                match.Groups["field1"].Value,
                match.Groups["field2"].Value,
                match.Groups["field3"].Value);
            var requestField = match.Groups["requestField"].Value;
            var container = FindNearestMethodName(text, match.Index) ?? component;

            var fact = CreateFact(
                relativePath,
                text,
                match.Index,
                match.Length,
                "ui-field-binding",
                $"{fieldName}->{requestField}",
                container,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["field"] = fieldName,
                    ["requestField"] = requestField,
                    ["form"] = match.Groups["form"].Value,
                    ["expression"] = match.Groups["expression"].Value,
                    ["component"] = component,
                    ["framework"] = "angular-static"
                });
            facts.Add(Tag(fact));
        }
    }

    private static EvidenceFact CreateBehaviorFact(
        string path,
        string text,
        int index,
        int length,
        string kind,
        string field,
        string component,
        string behavior,
        string? condition)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["field"] = field,
            ["behavior"] = behavior,
            ["framework"] = "angular-static"
        };
        if (!string.IsNullOrWhiteSpace(condition))
        {
            metadata["condition"] = WhitespaceRegex.Replace(condition, " ").Trim();
        }

        return CreateFact(path, text, index, length, kind, $"{field}:{behavior}", component, metadata);
    }

    private static EvidenceFact CreateOptionFact(
        string path,
        string text,
        int index,
        int length,
        string field,
        string component,
        string value,
        string label) =>
        CreateFact(
            path,
            text,
            index,
            length,
            "ui-field-option",
            $"{field}:{value}",
            component,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["field"] = field,
                ["value"] = value,
                ["label"] = label,
                ["framework"] = "angular-static"
            });

    private static EvidenceFact Tag(EvidenceFact fact)
    {
        var metadata = new Dictionary<string, string>(fact.Metadata, StringComparer.Ordinal)
        {
            ["analysisMode"] = "angular-ui-behavior-regex-fallback",
            ["analysisConfidence"] = "medium",
            ["analysisFallbackReason"] = "angular-form-behavior-not-yet-compiler-ast-backed"
        };
        return fact with { Metadata = metadata };
    }

    private static string? FindNearbyAtIfCondition(string template, int beforeIndex)
    {
        var start = Math.Max(0, beforeIndex - 1000);
        var matches = AtIfRegex.Matches(template[start..beforeIndex]);
        return matches.Count == 0
            ? null
            : WhitespaceRegex.Replace(matches[^1].Groups["condition"].Value, " ").Trim();
    }

    private static string? FindNearbyTypeScriptIfCondition(string text, int beforeIndex)
    {
        var start = Math.Max(0, beforeIndex - 1200);
        var window = text[start..beforeIndex];
        var matches = Regex.Matches(
            window,
            @"if\s*\((?<condition>[^\r\n{}]+)\)\s*\{",
            RegexOptions.CultureInvariant);
        return matches.Count == 0
            ? null
            : WhitespaceRegex.Replace(matches[^1].Groups["condition"].Value, " ").Trim();
    }

    private static string? FindNearestMethodName(string text, int beforeIndex)
    {
        string? name = null;
        foreach (Match match in MethodRegex.Matches(text[..Math.Min(beforeIndex, text.Length)]))
        {
            name = match.Groups["name"].Value;
        }
        return name;
    }

    private static string RemoveBoundAttribute(string attrs, string name) =>
        Regex.Replace(
            attrs,
            $@"\[{Regex.Escape(name)}\]\s*=\s*[""'][^""']*[""']",
            " ",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static string CleanLabel(string value)
    {
        var cleaned = AngularInterpolationRegex.Replace(value, " ");
        cleaned = HtmlTagRegex.Replace(cleaned, " ");
        return WhitespaceRegex.Replace(cleaned, " ").Trim();
    }

    private static string FirstNonEmpty(params string[] values) =>
        values.First(value => !string.IsNullOrWhiteSpace(value));

    private static EvidenceFact CreateFact(
        string relativePath,
        string text,
        int index,
        int length,
        string kind,
        string name,
        string? container,
        IReadOnlyDictionary<string, string> metadata)
    {
        var source = GetLocation(relativePath, text, index, length);
        return new EvidenceFact(
            $"ngbehavior:{relativePath}:{source.StartLine}:{kind}:{name}",
            kind,
            name,
            container,
            source,
            [],
            metadata);
    }

    private static SourceLocation GetLocation(string path, string text, int index, int length)
    {
        var start = 1;
        for (var i = 0; i < index && i < text.Length; i++)
        {
            if (text[i] == '\n') start++;
        }

        var end = start;
        for (var i = index; i < Math.Min(text.Length, index + length); i++)
        {
            if (text[i] == '\n') end++;
        }

        return new SourceLocation(path, start, end);
    }

    private static bool IsExcluded(string rootPath, string path)
    {
        var relative = Path.GetRelativePath(rootPath, path);
        return relative
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(segment => ExcludedDirectoryNames.Contains(segment));
    }

    private static string NormalizePath(string path) => path.Replace('\\', '/');

    private static string RelationKey(EvidenceRelation relation) =>
        $"{relation.FromFactId}|{relation.Kind}|{relation.Target}|{relation.Source.Path}|{relation.Source.StartLine}";
}
