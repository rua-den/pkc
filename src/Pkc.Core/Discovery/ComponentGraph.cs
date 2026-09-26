using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace Pkc.Core.Discovery;

internal sealed record ProjectReferenceRecord(string Include, bool Conditional);

internal sealed record CopyTaskRecord(string SourceFiles, string DestinationFolder, bool Conditional = false, bool SameTargetOwnOutput = false);

internal sealed record DotnetProjectRecord(
    string Manifest,
    string AreaPath,
    ComponentKind Kind,
    DiscoveryConfidence Confidence,
    IReadOnlyList<DiscoveryEvidence> Evidence,
    IReadOnlyList<ProjectReferenceRecord> References)
{
    /// <summary>Literal assembly name (declared or file-name default); null when it depends on unevaluated properties.</summary>
    public string? AssemblyName { get; init; }

    public IReadOnlyList<string> OutputPaths { get; init; } = [];

    public IReadOnlyList<CopyTaskRecord> Copies { get; init; } = [];
}

internal sealed record RuntimeLoaderFinding(
    string ComponentManifest,
    string LoaderFile,
    string? Identity,
    string? ScanDirectory = null,
    string? ScanConvention = null);

internal sealed record AngularProjectRecord(
    string Id,
    string Manifest,
    string AreaPath,
    ComponentKind Kind,
    DiscoveryConfidence Confidence,
    IReadOnlyList<DiscoveryEvidence> Evidence);

internal sealed record SolutionRecord(string Path, IReadOnlyList<string> ProjectPaths);

/// <summary>Classifies a .NET project from its own manifest. Anything not stated by the manifest stays UNKNOWN.</summary>
internal static class DotnetProjectIdentity
{
    private static readonly string[] OwnOutputItemTokens =
        ["$(TargetPath)", "$(TargetDir)", "$(TargetFileName)", "$(OutDir)", "$(OutputPath)", "@(IntermediateAssembly)"];

    private static readonly string[] WebApplicationProjectTypeGuids =
    [
        "349C5851-65DF-11DA-9384-00065B846F21", // ASP.NET web application
        "3D9AD99F-2412-4246-B90B-4EAA41C64699"  // WCF service application
    ];

    public static DotnetProjectRecord Describe(
        XDocument document,
        string manifest,
        string areaPath,
        IReadOnlyCollection<string> testEvidenceKinds,
        string? outputTypeDeclaredBy)
    {
        var root = document.Root!;
        var references = new List<ProjectReferenceRecord>();
        var assemblyNames = new SortedSet<string>(StringComparer.Ordinal);
        var outputPaths = new SortedSet<string>(StringComparer.Ordinal);
        var copies = new List<CopyTaskRecord>();
        var outputTypes = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        var projectTypeGuids = string.Empty;
        var sdks = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        AddSdks(sdks, root.Attribute("Sdk")?.Value);

        foreach (var element in root.Descendants())
        {
            switch (element.Name.LocalName)
            {
                case "Sdk":
                    AddSdks(sdks, element.Attribute("Name")?.Value);
                    break;
                case "Import":
                    AddSdks(sdks, element.Attribute("Sdk")?.Value);
                    break;
                case "OutputType":
                    outputTypes.Add(element.Value.Trim());
                    break;
                case "AssemblyName":
                    assemblyNames.Add(element.Value.Trim());
                    break;
                case "OutputPath" or "OutDir" or "BaseOutputPath":
                    outputPaths.Add(element.Value.Trim());
                    break;
                case "Copy" when element.Attribute("DestinationFolder") is { } destination:
                    var target = element.Ancestors().FirstOrDefault(ancestor => ancestor.Name.LocalName == "Target");
                    var copyConditional = element.AncestorsAndSelf()
                        .TakeWhile(ancestor => ancestor != root)
                        .Any(ancestor => ancestor.Attribute("Condition") is not null || ancestor.Name.LocalName is "When" or "Otherwise");
                    var localOwnOutputItems = target?.Descendants()
                        .Where(group => group.Name.LocalName == "ItemGroup")
                        .SelectMany(group => group.Elements())
                        .GroupBy(item => "@(" + item.Name.LocalName + ")", StringComparer.OrdinalIgnoreCase)
                        .Where(group => group.Count() == 1)
                        .Where(group =>
                        {
                            var item = group.Single();
                            var conditional = item.AncestorsAndSelf()
                                .TakeWhile(ancestor => ancestor != root)
                                .Any(ancestor => ancestor.Attribute("Condition") is not null || ancestor.Name.LocalName is "When" or "Otherwise");
                            var include = item.Attribute("Include")?.Value;
                            return !conditional && include is not null && item.Attribute("Exclude") is null && item.Attribute("Remove") is null &&
                                   OwnOutputItemTokens.Any(token => include.Contains(token, StringComparison.OrdinalIgnoreCase));
                        })
                        .Select(group => group.Key)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
                    var source = element.Attribute("SourceFiles")?.Value ?? string.Empty;
                    copies.Add(new CopyTaskRecord(source, destination.Value, copyConditional, localOwnOutputItems.Contains(source.Trim())));
                    break;
                case "ProjectTypeGuids":
                    projectTypeGuids += element.Value;
                    break;
                case "ProjectReference":
                    var conditional = element.AncestorsAndSelf()
                        .TakeWhile(ancestor => ancestor != root)
                        .Any(ancestor => ancestor.Attribute("Condition") is not null ||
                                         ancestor.Name.LocalName is "When" or "Otherwise");
                    foreach (var include in (element.Attribute("Include")?.Value ?? string.Empty)
                                 .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        references.Add(new ProjectReferenceRecord(include, conditional));
                    }

                    break;
            }
        }

        var (kind, confidence, evidence) = Classify(
            manifest, sdks, outputTypes, projectTypeGuids, testEvidenceKinds, outputTypeDeclaredBy);
        var assemblyName = assemblyNames.Count switch
        {
            0 => Path.GetFileNameWithoutExtension(manifest),
            1 when !assemblyNames.Single().Contains('$') => assemblyNames.Single(),
            _ => null
        };
        return new DotnetProjectRecord(manifest, areaPath, kind, confidence, evidence, references)
        {
            AssemblyName = assemblyName,
            OutputPaths = outputPaths.ToArray(),
            Copies = copies
        };
    }

    public static DotnetProjectRecord Unreadable(string manifest, string areaPath) =>
        new(manifest, areaPath, ComponentKind.Unknown, DiscoveryConfidence.Unknown,
            [new DiscoveryEvidence("manifest-unreadable", manifest)], []);

    private static (ComponentKind Kind, DiscoveryConfidence Confidence, IReadOnlyList<DiscoveryEvidence> Evidence) Classify(
        string manifest,
        IReadOnlySet<string> sdks,
        IReadOnlySet<string> outputTypes,
        string projectTypeGuids,
        IReadOnlyCollection<string> testEvidenceKinds,
        string? outputTypeDeclaredBy)
    {
        if (testEvidenceKinds.Count > 0)
        {
            return (ComponentKind.TestProject, DiscoveryConfidence.High,
                testEvidenceKinds.Select(kind => new DiscoveryEvidence(kind, manifest)).ToArray());
        }

        if (outputTypes.Count > 1 || outputTypes.Any(value => value.Contains("$(", StringComparison.Ordinal)))
        {
            return Unknown(new DiscoveryEvidence("msbuild-output-type-not-evaluated", manifest));
        }

        if (outputTypes.Count == 0 && outputTypeDeclaredBy is not null)
        {
            return Unknown(new DiscoveryEvidence("msbuild-output-type-not-evaluated", outputTypeDeclaredBy));
        }

        var webSdk = sdks.Contains("Microsoft.NET.Sdk.Web");
        var workerSdk = sdks.Contains("Microsoft.NET.Sdk.Worker");
        var declaredOutputType = outputTypes.SingleOrDefault();
        var outputType = declaredOutputType ?? (webSdk || workerSdk ? "Exe" : "Library");
        var executable = outputType.Equals("Exe", StringComparison.OrdinalIgnoreCase) ||
                         outputType.Equals("WinExe", StringComparison.OrdinalIgnoreCase);

        if (webSdk && executable)
        {
            return High(ComponentKind.WebHost, "msbuild-web-sdk", manifest);
        }

        if (WebApplicationProjectTypeGuids.Any(guid => projectTypeGuids.Contains(guid, StringComparison.OrdinalIgnoreCase)))
        {
            return High(ComponentKind.WebHost, "msbuild-web-application-project-type", manifest);
        }

        if (workerSdk && executable)
        {
            return High(ComponentKind.WorkerHost, "msbuild-worker-sdk", manifest);
        }

        if (executable)
        {
            return High(ComponentKind.ExecutableHost, "msbuild-output-type-exe", manifest);
        }

        if (outputType.Equals("Library", StringComparison.OrdinalIgnoreCase))
        {
            return High(
                ComponentKind.Library,
                declaredOutputType is null ? "msbuild-default-output-type-library" : "msbuild-output-type-library",
                manifest);
        }

        return Unknown(new DiscoveryEvidence("msbuild-output-type-unsupported", manifest));
    }

    private static (ComponentKind, DiscoveryConfidence, IReadOnlyList<DiscoveryEvidence>) High(
        ComponentKind kind, string evidenceKind, string manifest) =>
        (kind, DiscoveryConfidence.High, [new DiscoveryEvidence(evidenceKind, manifest)]);

    private static (ComponentKind, DiscoveryConfidence, IReadOnlyList<DiscoveryEvidence>) Unknown(DiscoveryEvidence evidence) =>
        (ComponentKind.Unknown, DiscoveryConfidence.Unknown, [evidence]);

    private static void AddSdks(ISet<string> sdks, string? value)
    {
        foreach (var sdk in (value ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            sdks.Add(sdk.Split('/')[0].Trim());
        }
    }
}

/// <summary>
/// Builds components, project-reference edges and ownership from manifest records.
/// Ownership only flows from production hosts through unconditional, resolved references to non-test components.
/// </summary>
internal static class ComponentGraph
{
    private static readonly HashSet<ComponentKind> HostKinds =
    [
        ComponentKind.WebHost,
        ComponentKind.WorkerHost,
        ComponentKind.ExecutableHost,
        ComponentKind.AngularApplication
    ];

    public static (IReadOnlyList<RepositoryComponent> Components, IReadOnlyList<ComponentEdge> Edges, IReadOnlyList<UnresolvedReference> Unresolved) Build(
        IReadOnlyList<DotnetProjectRecord> projects,
        IReadOnlyList<AngularProjectRecord> angularProjects,
        IReadOnlyList<SolutionRecord> solutions,
        IReadOnlyList<RuntimeLoaderFinding> loaders)
    {
        var projectIndex = new ManifestIndex(projects.Select(project => project.Manifest));

        var nodes = new Dictionary<string, Node>(StringComparer.Ordinal);
        foreach (var project in projects)
        {
            nodes[DotnetId(project.Manifest)] = new Node(
                DotnetId(project.Manifest), project.Manifest, project.AreaPath, project.Kind, project.Confidence, project.Evidence);
        }

        foreach (var project in angularProjects)
        {
            nodes[project.Id] = new Node(project.Id, project.Manifest, project.AreaPath, project.Kind, project.Confidence, project.Evidence);
        }

        foreach (var solution in solutions)
        {
            foreach (var path in solution.ProjectPaths)
            {
                if (projectIndex.Find(path) is { } manifest)
                {
                    nodes[DotnetId(manifest)].Solutions.Add(solution.Path);
                }
            }
        }

        var edges = new Dictionary<(string From, string To, string Kind), ComponentEdge>();
        var unresolved = new List<UnresolvedReference>();
        foreach (var project in projects)
        {
            var from = DotnetId(project.Manifest);
            var projectDirectory = DiscoveryPaths.Parent(project.Manifest);
            foreach (var reference in project.References)
            {
                var include = reference.Include.Replace('\\', '/');
                var evidence = new DiscoveryEvidence(
                    reference.Conditional ? "msbuild-conditional-project-reference" : "msbuild-project-reference",
                    project.Manifest);

                if (include.Contains("$(", StringComparison.Ordinal) ||
                    include.Contains("@(", StringComparison.Ordinal) ||
                    include.Contains("%(", StringComparison.Ordinal))
                {
                    unresolved.Add(new UnresolvedReference(from, include, "msbuild-property-not-evaluated", [evidence]));
                    continue;
                }

                if (include.Contains('*') || include.Contains('?'))
                {
                    unresolved.Add(new UnresolvedReference(from, include, "msbuild-wildcard-not-evaluated", [evidence]));
                    continue;
                }

                var resolved = DiscoveryPaths.Resolve(projectDirectory, include);
                if (resolved is null)
                {
                    unresolved.Add(new UnresolvedReference(from, include, "outside-repository", [evidence]));
                    continue;
                }

                if (projectIndex.Find(resolved) is not { } target)
                {
                    unresolved.Add(new UnresolvedReference(from, resolved, "project-file-not-found", [evidence]));
                    continue;
                }

                var to = DotnetId(target);
                var confidence = reference.Conditional ? DiscoveryConfidence.Unknown : DiscoveryConfidence.High;
                if (!edges.TryGetValue((from, to, "project-reference"), out var existing) ||
                    (existing.Confidence != DiscoveryConfidence.High && confidence == DiscoveryConfidence.High))
                {
                    edges[(from, to, "project-reference")] = new ComponentEdge(from, to, "project-reference", confidence, [evidence]);
                }
            }
        }

        AddRuntimePluginEdges(projects, loaders, nodes, edges, unresolved);

        var productionAdjacency = edges.Values
            .Where(edge => edge.Confidence == DiscoveryConfidence.High)
            .Where(edge => nodes[edge.From].Kind != ComponentKind.TestProject && nodes[edge.To].Kind != ComponentKind.TestProject)
            .GroupBy(edge => edge.From, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(edge => edge.To).ToArray(), StringComparer.Ordinal);

        foreach (var host in nodes.Values.Where(node => HostKinds.Contains(node.Kind)))
        {
            var pending = new Stack<string>([host.Id]);
            var visited = new HashSet<string>(StringComparer.Ordinal);
            while (pending.Count > 0)
            {
                var current = pending.Pop();
                if (!visited.Add(current))
                {
                    continue;
                }

                nodes[current].Owners.Add(host.Id);
                foreach (var next in productionAdjacency.GetValueOrDefault(current) ?? [])
                {
                    pending.Push(next);
                }
            }
        }

        foreach (var edge in edges.Values.Where(edge => nodes[edge.From].Kind == ComponentKind.TestProject))
        {
            nodes[edge.To].TestReferences.Add(edge.From);
        }

        var components = nodes.Values
            .OrderBy(node => node.Id, StringComparer.Ordinal)
            .Select(node =>
            {
                var test = node.Kind == ComponentKind.TestProject;
                var ownership = test
                    ? OwnershipStatus.TestOnly
                    : HostKinds.Contains(node.Kind)
                        ? OwnershipStatus.Host
                        : node.Owners.Count > 0 ? OwnershipStatus.Owned : OwnershipStatus.Unknown;
                return new RepositoryComponent(
                    node.Id,
                    node.Manifest,
                    node.AreaPath,
                    node.Kind,
                    test ? SourceRole.TestEvidence : SourceRole.Unknown,
                    node.Confidence,
                    node.Evidence
                        .Distinct()
                        .OrderBy(evidence => evidence.Kind, StringComparer.Ordinal)
                        .ThenBy(evidence => evidence.Path, StringComparer.Ordinal)
                        .ToArray(),
                    node.Solutions.ToArray(),
                    ownership,
                    test ? [] : node.Owners.ToArray(),
                    node.TestReferences.ToArray());
            })
            .ToArray();

        return (
            components,
            edges.Values
                .OrderBy(edge => edge.From, StringComparer.Ordinal)
                .ThenBy(edge => edge.To, StringComparer.Ordinal)
                .ThenBy(edge => edge.Kind, StringComparer.Ordinal)
                .ToArray(),
            unresolved
                .Distinct()
                .OrderBy(reference => reference.From, StringComparer.Ordinal)
                .ThenBy(reference => reference.Reference, StringComparer.Ordinal)
                .ThenBy(reference => reference.Reason, StringComparer.Ordinal)
                .ToArray());
    }

    private static readonly string[] OwnOutputTokens =
        ["$(TargetPath)", "$(TargetDir)", "$(TargetFileName)", "$(OutDir)", "$(OutputPath)", "@(IntermediateAssembly)"];

    private static readonly string[] ProjectDirectoryTokens =
        ["$(MSBuildProjectDirectory)", "$(MSBuildThisFileDirectory)", "$(ProjectDir)"];

    /// <summary>
    /// A runtime plugin edge needs a production host loader, a unique literal assembly identity, and build/copy
    /// provenance delivering the plugin output into the host tree. Anything less is reported, never promoted.
    /// </summary>
    private static void AddRuntimePluginEdges(
        IReadOnlyList<DotnetProjectRecord> projects,
        IReadOnlyList<RuntimeLoaderFinding> loaders,
        IReadOnlyDictionary<string, Node> nodes,
        IDictionary<(string From, string To, string Kind), ComponentEdge> edges,
        ICollection<UnresolvedReference> unresolved)
    {
        var byManifest = projects.ToDictionary(project => project.Manifest, StringComparer.Ordinal);
        var byAssembly = projects
            .Where(project => project.AssemblyName is not null)
            .GroupBy(project => project.AssemblyName!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.OrdinalIgnoreCase);

        foreach (var finding in loaders
                     .OrderBy(finding => finding.ComponentManifest, StringComparer.Ordinal)
                     .ThenBy(finding => finding.LoaderFile, StringComparer.Ordinal)
                     .ThenBy(finding => finding.Identity, StringComparer.Ordinal))
        {
            var from = DotnetId(finding.ComponentManifest);
            var test = nodes[from].Kind == ComponentKind.TestProject;
            var loaderEvidence = new DiscoveryEvidence("runtime-loader", finding.LoaderFile);

            if (finding.ScanDirectory is not null && finding.ScanConvention == "subdirectory-named-assembly")
            {
                if (test)
                {
                    continue;
                }

                var scanHost = byManifest[finding.ComponentManifest];
                foreach (var scanPlugin in projects
                             .Where(project => project.Manifest != scanHost.Manifest)
                             .Where(project => nodes[DotnetId(project.Manifest)].Kind != ComponentKind.TestProject))
                {
                    // Only plugins whose own output is delivered into this host are candidates; others are unrelated.
                    if (OutputDelivery(scanPlugin, scanHost) is null)
                    {
                        continue;
                    }

                    if (scanPlugin.AssemblyName is null ||
                        !scanPlugin.AssemblyName.Equals(Path.GetFileNameWithoutExtension(scanPlugin.Manifest), StringComparison.OrdinalIgnoreCase) ||
                        byAssembly.GetValueOrDefault(scanPlugin.AssemblyName) is not { Length: 1 })
                    {
                        var ambiguous = scanPlugin.AssemblyName is not null &&
                                        byAssembly.GetValueOrDefault(scanPlugin.AssemblyName) is { Length: > 1 };
                        unresolved.Add(new UnresolvedReference(
                            from,
                            scanPlugin.Manifest,
                            ambiguous ? "runtime-plugin-identity-ambiguous" : "runtime-plugin-scan-directory-mismatch",
                            [loaderEvidence]));
                        continue;
                    }

                    var scanCopy = ScanOutputDelivery(scanPlugin, scanHost, finding.ScanDirectory);
                    if (scanCopy is null)
                    {
                        unresolved.Add(new UnresolvedReference(
                            from,
                            scanPlugin.Manifest,
                            "runtime-plugin-scan-directory-mismatch",
                            [loaderEvidence]));
                        continue;
                    }

                    var scanTo = DotnetId(scanPlugin.Manifest);
                    var scanEvidence = new[] { loaderEvidence, new DiscoveryEvidence("runtime-plugin-identity", scanPlugin.Manifest), scanCopy };
                    var scanKey = (from, scanTo, "runtime-plugin-load");
                    edges[scanKey] = edges.TryGetValue(scanKey, out var scanExisting)
                        ? scanExisting with { Evidence = scanExisting.Evidence.Concat(scanEvidence).Distinct().ToArray() }
                        : new ComponentEdge(from, scanTo, "runtime-plugin-load", DiscoveryConfidence.High, scanEvidence);
                }

                continue;
            }

            if (finding.Identity is null)
            {
                if (!test)
                {
                    unresolved.Add(new UnresolvedReference(from, finding.LoaderFile, "runtime-loader-identity-unresolved", [loaderEvidence]));
                }

                continue;
            }

            var matches = byAssembly.GetValueOrDefault(finding.Identity) ?? [];
            if (matches.Length != 1)
            {
                if (!test)
                {
                    unresolved.Add(new UnresolvedReference(
                        from,
                        finding.Identity,
                        matches.Length == 0 ? "runtime-plugin-identity-not-found" : "runtime-plugin-identity-ambiguous",
                        [loaderEvidence]));
                }

                continue;
            }

            var plugin = matches[0];
            var to = DotnetId(plugin.Manifest);
            if (to == from)
            {
                continue;
            }

            if (test)
            {
                nodes[to].TestReferences.Add(from);
                continue;
            }

            var copy = CopyProvenance(plugin, byManifest[finding.ComponentManifest], edges);
            if (copy is null)
            {
                unresolved.Add(new UnresolvedReference(from, finding.Identity, "runtime-plugin-copy-unproven", [loaderEvidence]));
                continue;
            }

            var evidence = new[] { loaderEvidence, new DiscoveryEvidence("runtime-plugin-identity", plugin.Manifest), copy };
            var key = (from, to, "runtime-plugin-load");
            edges[key] = edges.TryGetValue(key, out var existing)
                ? existing with { Evidence = existing.Evidence.Concat(evidence).Distinct().ToArray() }
                : new ComponentEdge(from, to, "runtime-plugin-load", DiscoveryConfidence.High, evidence);
        }

        foreach (var plugin in projects.Where(project => nodes[DotnetId(project.Manifest)].Kind != ComponentKind.TestProject))
        {
            foreach (var host in projects.Where(project => HostKinds.Contains(nodes[DotnetId(project.Manifest)].Kind) && project != plugin))
            {
                var delivered = OutputDelivery(plugin, host);
                if (delivered is not null && !edges.ContainsKey((DotnetId(host.Manifest), DotnetId(plugin.Manifest), "runtime-plugin-load")))
                {
                    unresolved.Add(new UnresolvedReference(
                        DotnetId(plugin.Manifest), DotnetId(host.Manifest), "plugin-copy-without-identified-loader", [delivered]));
                }
            }
        }

        foreach (var edge in edges.Values.Where(edge => edge.Kind == "runtime-plugin-load").ToArray())
        {
            // Normalize evidence order for byte-stable output.
            edges[(edge.From, edge.To, edge.Kind)] = edge with
            {
                Evidence = edge.Evidence
                    .OrderBy(evidence => evidence.Kind, StringComparer.Ordinal)
                    .ThenBy(evidence => evidence.Path, StringComparer.Ordinal)
                    .ToArray()
            };
        }
    }

    private static DiscoveryEvidence? CopyProvenance(
        DotnetProjectRecord plugin,
        DotnetProjectRecord host,
        IDictionary<(string From, string To, string Kind), ComponentEdge> edges) =>
        OutputDelivery(plugin, host) ??
        (edges.TryGetValue((DotnetId(host.Manifest), DotnetId(plugin.Manifest), "project-reference"), out var reference) &&
         reference.Confidence == DiscoveryConfidence.High
            ? new DiscoveryEvidence("project-reference-output-copy", host.Manifest)
            : null);

    /// <summary>Explicit build metadata that places the plugin's output inside the host's tree.</summary>
    private static DiscoveryEvidence? OutputDelivery(DotnetProjectRecord plugin, DotnetProjectRecord host)
    {
        var pluginDirectory = DiscoveryPaths.Parent(plugin.Manifest);
        var hostDirectory = DiscoveryPaths.Parent(host.Manifest);
        bool IntoHost(string? destination) =>
            destination is not null &&
            DiscoveryPaths.IsUnder(destination, host.AreaPath) &&
            !DiscoveryPaths.IsUnder(destination, plugin.AreaPath);

        if (plugin.OutputPaths.Any(output => IntoHost(ResolveLiteralPrefix(pluginDirectory, output))))
        {
            return new DiscoveryEvidence("msbuild-output-path-into-host", plugin.Manifest);
        }

        if (plugin.Copies.Any(copy =>
                !copy.Conditional &&
                (copy.SameTargetOwnOutput || OwnOutputTokens.Any(token => copy.SourceFiles.Contains(token, StringComparison.OrdinalIgnoreCase))) &&
                IntoHost(ResolveLiteralPrefix(pluginDirectory, copy.DestinationFolder))))
        {
            return new DiscoveryEvidence("msbuild-copy-into-host", plugin.Manifest);
        }

        if (host.Copies.Any(copy =>
                !copy.Conditional &&
                ResolveLiteralPrefix(hostDirectory, copy.SourceFiles) is { } source &&
                DiscoveryPaths.IsUnder(source, plugin.AreaPath) &&
                IntoHost(ResolveLiteralPrefix(hostDirectory, copy.DestinationFolder))))
        {
            return new DiscoveryEvidence("msbuild-copy-into-host", host.Manifest);
        }

        return null;
    }

    private static DiscoveryEvidence? ScanOutputDelivery(DotnetProjectRecord plugin, DotnetProjectRecord host, string scanDirectory)
    {
        if (plugin.OutputPaths.Count > 0 || host.OutputPaths.Count > 0 || plugin.AssemblyName is null ||
            !plugin.AssemblyName.Equals(Path.GetFileNameWithoutExtension(plugin.Manifest), StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var pluginDirectory = DiscoveryPaths.Parent(plugin.Manifest);
        foreach (var copy in plugin.Copies.Where(copy => !copy.Conditional &&
                     (copy.SameTargetOwnOutput || OwnOutputTokens.Any(token => copy.SourceFiles.Contains(token, StringComparison.OrdinalIgnoreCase)))))
        {
            if (ExactScanDestination(pluginDirectory, host, copy.DestinationFolder, scanDirectory))
            {
                return new DiscoveryEvidence("msbuild-copy-into-host-scan-directory", plugin.Manifest);
            }
        }

        return null;
    }

    private static bool ExactScanDestination(string pluginDirectory, DotnetProjectRecord host, string destination, string scanDirectory)
    {
        var text = destination.Trim().Replace('\\', '/');
        foreach (var token in ProjectDirectoryTokens)
        {
            if (text.StartsWith(token, StringComparison.OrdinalIgnoreCase))
            {
                text = text[token.Length..].TrimStart('/');
                break;
            }
        }

        var outIndex = text.IndexOf("$(OutDir)", StringComparison.OrdinalIgnoreCase);
        if (outIndex < 0 || (outIndex > 0 && text[outIndex - 1] != '/'))
        {
            return false;
        }

        var prefix = text[..outIndex].TrimEnd('/');
        var suffix = text[(outIndex + "$(OutDir)".Length)..].Trim('/');
        var normalizedScanDirectory = scanDirectory.Trim('/').Replace('\\', '/');
        var expected = Regex.Escape(normalizedScanDirectory) + @"/\$\(ProjectName\)(?:/\%\(RecursiveDir\))?";
        if (!Regex.IsMatch(suffix, "^" + expected + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            return false;
        }

        var resolvedPrefix = DiscoveryPaths.Resolve(pluginDirectory, prefix);
        return resolvedPrefix is not null && resolvedPrefix.Equals(host.AreaPath, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Resolves the literal leading part of an MSBuild path (project-directory properties allowed as a prefix);
    /// stops at the first segment that needs evaluation. Null when nothing literal remains.
    /// </summary>
    private static string? ResolveLiteralPrefix(string baseDirectory, string value)
    {
        var text = value.Trim().Replace('\\', '/');
        foreach (var token in ProjectDirectoryTokens)
        {
            if (text.StartsWith(token, StringComparison.OrdinalIgnoreCase))
            {
                text = text[token.Length..].TrimStart('/');
                break;
            }
        }

        var literal = text
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .TakeWhile(segment => !segment.Contains("$(") && !segment.Contains("@(") && !segment.Contains("%(") && !segment.Contains(';'))
            .ToArray();
        return literal.Length == 0 || text.StartsWith("$(", StringComparison.Ordinal)
            ? null
            : DiscoveryPaths.Resolve(baseDirectory, string.Join('/', literal));
    }

    private static string DotnetId(string manifest) => "dotnet:" + manifest;

    private sealed class Node(
        string id,
        string manifest,
        string areaPath,
        ComponentKind kind,
        DiscoveryConfidence confidence,
        IReadOnlyList<DiscoveryEvidence> evidence)
    {
        public string Id { get; } = id;
        public string Manifest { get; } = manifest;
        public string AreaPath { get; } = areaPath;
        public ComponentKind Kind { get; } = kind;
        public DiscoveryConfidence Confidence { get; } = confidence;
        public IReadOnlyList<DiscoveryEvidence> Evidence { get; } = evidence;
        public SortedSet<string> Solutions { get; } = new(StringComparer.Ordinal);
        public SortedSet<string> Owners { get; } = new(StringComparer.Ordinal);
        public SortedSet<string> TestReferences { get; } = new(StringComparer.Ordinal);
    }

    /// <summary>
    /// Exact repository-relative path match first; a unique case-insensitive match is accepted because
    /// Windows-authored manifests routinely differ in casing from the checked-in path.
    /// </summary>
    private sealed class ManifestIndex(IEnumerable<string> manifests)
    {
        private readonly HashSet<string> _exact = new(manifests, StringComparer.Ordinal);

        public string? Find(string path)
        {
            if (_exact.Contains(path))
            {
                return path;
            }

            var matches = _exact.Where(candidate => candidate.Equals(path, StringComparison.OrdinalIgnoreCase)).Take(2).ToArray();
            return matches.Length == 1 ? matches[0] : null;
        }
    }
}
