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

    private static readonly Regex FormGroupAttributeRegex = new(
        @"\[formGroup\]\s*=\s*[""'](?<name>[A-Za-z_$][A-Za-z0-9_$]*)[""']",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex FormOpenRegex = new(
        @"<form\b(?<attrs>[^>]*)>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex FormCloseRegex = new(
        @"</form\s*>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ClassDeclarationRegex = new(
        @"\bclass\s+(?<name>[A-Za-z_$][A-Za-z0-9_$]*)\b[^\{]*\{",
        RegexOptions.Compiled);

    private static readonly Regex NamedImportRegex = new(
        @"(?m)^\s*import\s*\{(?<bindings>[^}]+)\}\s*from\s*[""'](?<module>[^""']+)[""']",
        RegexOptions.Compiled);

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

    private static readonly Regex DialogDataInjectionRegex = new(
        @"(?m)^\s*(?:private|public|protected)?\s*(?<name>[A-Za-z_$][A-Za-z0-9_$]*)\s*:\s*[^=;\r\n]+\s*=\s*inject\(\s*MAT_DIALOG_DATA\s*\)",
        RegexOptions.Compiled);

    private static readonly Regex FormBuilderInjectionRegex = new(
        @"(?m)^\s*(?:private|public|protected)?\s*(?<name>[A-Za-z_$][A-Za-z0-9_$]*)\s*=\s*inject\(\s*FormBuilder\s*\)",
        RegexOptions.Compiled);

    private static readonly Regex FormGroupInitializerRegex = new(
        @"(?m)^\s*(?<form>[A-Za-z_$][A-Za-z0-9_$]*)\s*=\s*this\.(?<builder>[A-Za-z_$][A-Za-z0-9_$]*)(?:\.nonNullable)?\.group\s*\(\s*\{(?<body>[\s\S]*?)\}\s*\)",
        RegexOptions.Compiled);

    private static readonly Regex FormControlInitializerRegex = new(
        @"(?m)^\s*(?<field>[A-Za-z_$][A-Za-z0-9_$]*)\s*:\s*\[\s*this\.(?<data>[A-Za-z_$][A-Za-z0-9_$]*)\.(?<property>[A-Za-z_$][A-Za-z0-9_$]*)\b(?=\s*(?:,|\]))",
        RegexOptions.Compiled);

    private static readonly Regex FormGroupNameRegex = new(
        @"\bformGroupName\s*=\s*[""'][^""']+[""']",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
            var formGroup = FindNearestFormGroup(template, match.Index);
            if (!string.IsNullOrWhiteSpace(formGroup))
            {
                field = field with
                {
                    Metadata = new Dictionary<string, string>(field.Metadata, StringComparer.Ordinal)
                    {
                        ["formGroup"] = formGroup
                    }
                };
            }
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

        ExtractDisplayedValueLineage(relativePath, text, component, facts);
    }

    private static void ExtractDisplayedValueLineage(
        string relativePath,
        string text,
        string component,
        ICollection<EvidenceFact> facts)
    {
        if (!HasExactNamedImport(text, "MAT_DIALOG_DATA", "@angular/material/dialog") ||
            !HasExactNamedImport(text, "FormBuilder", "@angular/forms") ||
            !HasExactNamedImport(text, "inject", "@angular/core"))
        {
            return;
        }

        if (!TryGetComponentBody(text, component, out var componentBody, out var componentOffset))
        {
            return;
        }

        var dataBindings = DialogDataInjectionRegex.Matches(componentBody)
            .Cast<Match>()
            .Where(match => IsActiveCodePosition(componentBody, match.Index))
            .ToArray();
        var formBuilders = FormBuilderInjectionRegex.Matches(componentBody)
            .Cast<Match>()
            .Where(match => IsActiveCodePosition(componentBody, match.Index))
            .ToArray();
        var formInitializers = FormGroupInitializerRegex.Matches(componentBody)
            .Cast<Match>()
            .Where(match => IsActiveCodePosition(componentBody, match.Index))
            .ToArray();
        if (dataBindings.Length != 1 || formBuilders.Length != 1 || formInitializers.Length != 1)
        {
            return;
        }

        var dataBinding = dataBindings[0];
        var formBuilder = formBuilders[0].Groups["name"].Value;
        var formInitializer = formInitializers[0];
        if (!string.Equals(formInitializer.Groups["builder"].Value, formBuilder, StringComparison.Ordinal))
        {
            return;
        }

        var dataName = dataBinding.Groups["name"].Value;
        var formName = formInitializer.Groups["form"].Value;
        if (formInitializer.Groups["body"].Value.Contains(".group(", StringComparison.Ordinal) ||
            FormGroupNameRegex.IsMatch(text))
        {
            return;
        }
        var displayedFields = facts
            .Where(fact => fact.Kind == "ui-field" &&
                          string.Equals(fact.Container, component, StringComparison.Ordinal) &&
                          fact.Metadata.TryGetValue("field", out var name) &&
                          !string.IsNullOrWhiteSpace(name) &&
                          fact.Metadata.TryGetValue("formGroup", out var formGroup) &&
                          string.Equals(formGroup, formName, StringComparison.Ordinal))
            .ToArray();
        var componentIdentity = $"{relativePath}#{component}";
        var controls = FormControlInitializerRegex.Matches(formInitializer.Groups["body"].Value)
            .Cast<Match>()
            .Where(match => IsActiveCodePosition(
                formInitializer.Groups["body"].Value,
                match.Index))
            .ToArray();

        foreach (var displayedField in displayedFields)
        {
            var field = displayedField.Metadata["field"];
            var ownershipCount = displayedFields.Count(candidate =>
                string.Equals(candidate.Metadata.GetValueOrDefault("field"), field, StringComparison.Ordinal));
            if (ownershipCount != 1)
            {
                continue;
            }

            var matchingControls = controls
                .Where(match =>
                    string.Equals(match.Groups["field"].Value, field, StringComparison.Ordinal) &&
                    string.Equals(match.Groups["data"].Value, dataName, StringComparison.Ordinal) &&
                    string.Equals(match.Groups["property"].Value, field, StringComparison.Ordinal))
                .ToArray();
            if (matchingControls.Length != 1)
            {
                continue;
            }

            var control = matchingControls[0];
            var controlIdentity = $"{component}.{formName}.{field}";
            var sourceIdentity = $"{component}.{dataName}.{field}";
            var controlIndex = componentOffset + formInitializer.Index + formInitializer.Groups["body"].Index + control.Index;
            var sourceToControl = Tag(CreateFact(
                relativePath,
                text,
                controlIndex,
                Math.Max(dataBinding.Length, control.Length),
                "value-transfer",
                $"{sourceIdentity}->{controlIdentity}",
                component,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                ["knowledgeClass"] = "value-lineage",
                ["sourceOccurrence"] = $"MAT_DIALOG_DATA.{dataName}.{field}",
                ["targetOccurrence"] = $"{component}.{formName}.{field}",
                ["sourceToken"] = "MAT_DIALOG_DATA",
                ["sourceBinding"] = dataName,
                ["sourceProperty"] = field,
                ["targetForm"] = formName,
                ["targetControl"] = field,
                ["componentIdentity"] = componentIdentity,
                ["controlIdentity"] = controlIdentity,
                ["sourceIdentity"] = sourceIdentity,
                ["lineageDomain"] = "angular-ui",
                ["mechanism"] = "copy",
                ["temporalSemantics"] = "snapshot",
                ["expression"] = $"this.{dataName}.{field}",
                ["proof"] = "exact-MAT_DIALOG_DATA-binding+exact-FormBuilder-control-initializer"
                }));
            facts.Add(sourceToControl);

            facts.Add(Tag(new EvidenceFact(
                $"nglineage:{displayedField.Id}:form-control-binding",
                "value-transfer",
                $"{controlIdentity}->{component}.displayed.{field}",
                component,
                displayedField.Source,
                [],
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                ["knowledgeClass"] = "value-lineage",
                ["sourceOccurrence"] = $"{component}.{formName}.{field}",
                ["targetOccurrence"] = $"{component}.displayed.{field}",
                ["sourceForm"] = formName,
                ["sourceControl"] = field,
                ["targetField"] = field,
                ["componentIdentity"] = componentIdentity,
                ["controlIdentity"] = controlIdentity,
                ["lineageDomain"] = "angular-ui",
                ["predecessorTransferFactId"] = sourceToControl.Id,
                ["mechanism"] = "form-control-binding",
                ["temporalSemantics"] = "dynamic",
                ["proof"] = "exact-formControlName-to-form-control-identity"
                })));
        }
    }

    private static bool HasExactNamedImport(string text, string name, string module)
    {
        foreach (Match match in NamedImportRegex.Matches(text))
        {
            if (!IsActiveCodePosition(text, match.Index))
            {
                continue;
            }

            if (!string.Equals(match.Groups["module"].Value, module, StringComparison.Ordinal))
            {
                continue;
            }

            if (match.Groups["bindings"].Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(binding => string.Equals(binding, name, StringComparison.Ordinal)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsActiveCodePosition(string text, int position)
    {
        var state = TypeScriptLexicalState.Code;
        for (var index = 0; index < position && index < text.Length; index++)
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
            }
        }

        return state == TypeScriptLexicalState.Code;
    }

    private static string? FindNearestFormGroup(string template, int beforeIndex)
    {
        var openings = FormOpenRegex.Matches(template)
            .Cast<Match>()
            .Where(match => match.Index < beforeIndex)
            .OrderByDescending(match => match.Index)
            .ToArray();
        foreach (var opening in openings)
        {
            var closing = FormCloseRegex.Match(template, opening.Index + opening.Length);
            if (!closing.Success || closing.Index <= beforeIndex)
            {
                continue;
            }

            var group = FormGroupAttributeRegex.Match(opening.Groups["attrs"].Value);
            return group.Success ? group.Groups["name"].Value : null;
        }

        return null;
    }

    private static bool TryGetComponentBody(string text, string component, out string body, out int bodyOffset)
    {
        var declarations = ClassDeclarationRegex.Matches(text)
            .Cast<Match>()
            .Where(match => string.Equals(match.Groups["name"].Value, component, StringComparison.Ordinal))
            .ToArray();
        if (declarations.Length != 1)
        {
            body = string.Empty;
            bodyOffset = 0;
            return false;
        }

        var openBrace = declarations[0].Index + declarations[0].Value.LastIndexOf('{');
        var closeBrace = FindMatchingBrace(text, openBrace);
        if (closeBrace <= openBrace)
        {
            body = string.Empty;
            bodyOffset = 0;
            return false;
        }

        bodyOffset = openBrace + 1;
        body = text.Substring(bodyOffset, closeBrace - bodyOffset);
        return true;
    }

    private static int FindMatchingBrace(string text, int openBrace)
    {
        var depth = 0;
        var quote = '\0';
        for (var index = openBrace; index < text.Length; index++)
        {
            var current = text[index];
            if (quote != '\0')
            {
                if (current == '\\')
                {
                    index++;
                }
                else if (current == quote)
                {
                    quote = '\0';
                }

                continue;
            }

            if (current is '\'' or '"' or '`')
            {
                quote = current;
            }
            else if (current == '{')
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

    private enum TypeScriptLexicalState
    {
        Code,
        LineComment,
        BlockComment,
        SingleQuotedString,
        DoubleQuotedString,
        TemplateLiteral,
    }
}
