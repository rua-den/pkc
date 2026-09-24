using System.Xml.Linq;

namespace Pkc.Core.Discovery;

internal sealed record ProjectReferenceRecord(string Include, bool Conditional);

internal sealed record DotnetProjectRecord(
    string Manifest,
    string AreaPath,
    ComponentKind Kind,
    DiscoveryConfidence Confidence,
    IReadOnlyList<DiscoveryEvidence> Evidence,
    IReadOnlyList<ProjectReferenceRecord> References);

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
        return new DotnetProjectRecord(manifest, areaPath, kind, confidence, evidence, references);
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
        IReadOnlyList<SolutionRecord> solutions)
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

        var edges = new Dictionary<(string From, string To), ComponentEdge>();
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
                if (!edges.TryGetValue((from, to), out var existing) ||
                    (existing.Confidence != DiscoveryConfidence.High && confidence == DiscoveryConfidence.High))
                {
                    edges[(from, to)] = new ComponentEdge(from, to, "project-reference", confidence, [evidence]);
                }
            }
        }

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
                .ToArray(),
            unresolved
                .Distinct()
                .OrderBy(reference => reference.From, StringComparer.Ordinal)
                .ThenBy(reference => reference.Reference, StringComparer.Ordinal)
                .ThenBy(reference => reference.Reason, StringComparer.Ordinal)
                .ToArray());
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
