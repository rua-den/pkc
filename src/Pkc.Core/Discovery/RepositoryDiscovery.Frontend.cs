using System.Security.Cryptography;
using System.Text.Json;

namespace Pkc.Core.Discovery;

/// <summary>
/// RD3 vendor/custom frontend classification. Vendor authority comes only from restore manifests whose
/// declared file sets can be checked (NuGet package content, LibMan file lists). Generated authority comes only
/// from bundle configuration and resolvable source maps. Names, banners and look-alike content never classify.
/// </summary>
public sealed partial class RepositoryDiscovery
{
    private sealed partial class DiscoveryWalk
    {
        private const long MaxComparedFileBytes = 16 * 1024 * 1024;

        private const int GeneratedPriority = 1;
        private const int VendorPriority = 2;
        private const int CarveOutPriority = 3;

        private static readonly string[] PackageTransformSuffixes = [".pp", ".transform", ".install.xdt", ".uninstall.xdt"];

        private sealed record FileDecision(
            int Priority,
            SourceRole Role,
            ScanMode ScanMode,
            DiscoveryConfidence Confidence,
            IReadOnlyList<DiscoveryEvidence> Evidence);

        private sealed class VendorDestination
        {
            public bool FileSetDeclared { get; set; } = true;
            public SortedSet<string> DeclaredFiles { get; } = new(StringComparer.Ordinal);
            public SortedSet<string> Manifests { get; } = new(StringComparer.Ordinal);
        }

        private (IReadOnlyList<GeneratedArtifact> Artifacts, IReadOnlyList<string> ByteComparisons, IReadOnlyList<UnresolvedReference> Unresolved)
            ClassifyFrontendDependencies(IReadOnlyList<string> files)
        {
            var inventory = new HashSet<string>(files, StringComparer.Ordinal);
            var decisions = new Dictionary<string, FileDecision>(StringComparer.Ordinal);
            var artifacts = new List<GeneratedArtifact>();
            var comparisons = new SortedSet<string>(StringComparer.Ordinal);
            var unresolved = new List<UnresolvedReference>();

            var manifests = _manifests.ToArray();
            foreach (var (path, _) in manifests.Where(pair => pair.Value == "nuget-packages-config"))
            {
                ClassifyNuGetContent(path, inventory, decisions, comparisons, unresolved);
            }

            var destinations = new SortedDictionary<string, VendorDestination>(StringComparer.Ordinal);
            foreach (var (path, _) in manifests.Where(pair => pair.Value == "libman-manifest"))
            {
                CollectLibManDestinations(path, destinations, unresolved);
            }

            ApplyVendorDestinations(destinations, files, decisions);

            foreach (var (path, _) in manifests.Where(pair => pair.Value == "legacy-bundle-config"))
            {
                ClassifyBundles(path, files, inventory, decisions, artifacts);
            }

            ClassifySourceMaps(files, inventory, decisions, artifacts);

            foreach (var (path, decision) in decisions.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                var owner = NearestDirectoryArea(path);
                owner.AddOverride(
                    DiscoveryPaths.RelativeTo(owner.Path, path),
                    decision.Role,
                    decision.ScanMode,
                    decision.Confidence,
                    decision.Evidence);
            }

            return (
                artifacts
                    .OrderBy(artifact => artifact.Path, StringComparer.Ordinal)
                    .ThenBy(artifact => artifact.Kind, StringComparer.Ordinal)
                    .ToArray(),
                comparisons.ToArray(),
                unresolved);
        }

        private void ClassifyNuGetContent(
            string packagesConfig,
            IReadOnlySet<string> inventory,
            IDictionary<string, FileDecision> decisions,
            ISet<string> comparisons,
            ICollection<UnresolvedReference> unresolved)
        {
            var document = ReadXmlManifest(RepositoryFile(packagesConfig), packagesConfig);
            if (document?.Root is null)
            {
                return;
            }

            var restored = RestoredPackageFolders();
            var projectDirectory = DiscoveryPaths.Parent(packagesConfig);
            var configEvidence = new DiscoveryEvidence("nuget-packages-config", packagesConfig);
            foreach (var package in document.Root.Elements().Where(element => element.Name.LocalName == "package"))
            {
                var id = package.Attribute("id")?.Value.Trim();
                var version = package.Attribute("version")?.Value.Trim();
                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(version))
                {
                    continue;
                }

                var candidates = restored.GetValueOrDefault($"{id}.{version}".ToLowerInvariant()) ?? [];
                if (candidates.Count != 1)
                {
                    unresolved.Add(new UnresolvedReference(
                        packagesConfig,
                        $"{id} {version}",
                        candidates.Count == 0 ? "nuget-package-not-restored" : "nuget-package-restore-ambiguous",
                        [configEvidence]));
                    continue;
                }

                var packageFolder = candidates.Single();
                var packageDirectory = new DirectoryInfo(FullPath(packageFolder));
                foreach (var contentDirectory in packageDirectory.EnumerateDirectories()
                             .Where(directory => directory.Name.Equals("content", StringComparison.OrdinalIgnoreCase))
                             .OrderBy(directory => directory.Name, StringComparer.Ordinal))
                {
                    foreach (var contentFile in contentDirectory.EnumerateFiles("*", SearchOption.AllDirectories)
                                 .Select(file => (File: file, Relative: DiscoveryPaths.Normalize(Path.GetRelativePath(contentDirectory.FullName, file.FullName))))
                                 .OrderBy(entry => entry.Relative, StringComparer.Ordinal))
                    {
                        _cancellationToken.ThrowIfCancellationRequested();
                        if (PackageTransformSuffixes.Any(suffix => contentFile.Relative.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }

                        var projectFile = DiscoveryPaths.Join(projectDirectory, contentFile.Relative);
                        if (!inventory.Contains(projectFile))
                        {
                            continue;
                        }

                        var packageFile = $"{packageFolder}/{contentDirectory.Name}/{contentFile.Relative}";
                        var identical = FilesIdentical(RepositoryFile(projectFile), contentFile.File);
                        if (identical is null)
                        {
                            continue;
                        }

                        comparisons.Add(projectFile);
                        comparisons.Add(packageFile);
                        var contentEvidence = new DiscoveryEvidence("nuget-package-content", packageFile);
                        Decide(decisions, projectFile, identical.Value
                            ? new FileDecision(VendorPriority, SourceRole.ThirdPartyRuntime, ScanMode.RuntimeDependencyIndex,
                                DiscoveryConfidence.High, [configEvidence, contentEvidence])
                            : new FileDecision(CarveOutPriority, SourceRole.ThirdPartyModified, ScanMode.DeepScan,
                                DiscoveryConfidence.High,
                                [configEvidence, contentEvidence, new DiscoveryEvidence("content-differs-from-package", packageFile)]));
                    }
                }
            }
        }

        /// <summary>Restored package folders proven by RD1, keyed by lower-case `id.version`.</summary>
        private Dictionary<string, List<string>> RestoredPackageFolders()
        {
            var folders = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (var area in _areas.Values.Where(area => area.Kinds.Contains("nuget-package-restore")))
            {
                var name = Path.GetFileName(area.Path);
                var parent = DiscoveryPaths.Parent(area.Path);
                var keys = new[] { name, parent == DiscoveryPaths.Root ? null : Path.GetFileName(parent) + "." + name };
                foreach (var key in keys.OfType<string>().Select(key => key.ToLowerInvariant()).Distinct())
                {
                    if (!folders.TryGetValue(key, out var list))
                    {
                        folders[key] = list = [];
                    }

                    list.Add(area.Path);
                }
            }

            return folders;
        }

        private void CollectLibManDestinations(
            string libman,
            IDictionary<string, VendorDestination> destinations,
            ICollection<UnresolvedReference> unresolved)
        {
            using var document = ReadJsonManifest(RepositoryFile(libman), libman);
            if (document is null ||
                document.RootElement.ValueKind != JsonValueKind.Object ||
                !document.RootElement.TryGetProperty("libraries", out var libraries) ||
                libraries.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            var manifestDirectory = DiscoveryPaths.Parent(libman);
            var evidence = new DiscoveryEvidence("libman-manifest", libman);
            var defaultDestination = GetString(document.RootElement, "defaultDestination");
            foreach (var library in libraries.EnumerateArray().Where(entry => entry.ValueKind == JsonValueKind.Object))
            {
                var name = GetString(library, "library") ?? "(unnamed)";
                var declared = GetString(library, "destination") ?? defaultDestination;
                if (string.IsNullOrWhiteSpace(declared))
                {
                    unresolved.Add(new UnresolvedReference(libman, name, "libman-destination-undeclared", [evidence]));
                    continue;
                }

                var destination = DiscoveryPaths.Resolve(manifestDirectory, declared);
                if (destination is null || DiscoveryPaths.IsUnder(manifestDirectory, destination))
                {
                    unresolved.Add(new UnresolvedReference(libman, name, "libman-destination-unsupported", [evidence]));
                    continue;
                }

                if (!destinations.TryGetValue(destination, out var entry))
                {
                    destinations[destination] = entry = new VendorDestination();
                }

                entry.Manifests.Add(libman);
                if (!library.TryGetProperty("files", out var declaredFiles) ||
                    declaredFiles.ValueKind != JsonValueKind.Array ||
                    declaredFiles.GetArrayLength() == 0)
                {
                    entry.FileSetDeclared = false;
                    continue;
                }

                foreach (var file in declaredFiles.EnumerateArray().Where(value => value.ValueKind == JsonValueKind.String))
                {
                    if (DiscoveryPaths.Resolve(destination, file.GetString()!) is { } resolved)
                    {
                        entry.DeclaredFiles.Add(resolved);
                    }
                }
            }
        }

        private void ApplyVendorDestinations(
            IReadOnlyDictionary<string, VendorDestination> destinations,
            IReadOnlyList<string> files,
            IDictionary<string, FileDecision> decisions)
        {
            foreach (var (destination, entry) in destinations)
            {
                if (!Directory.Exists(FullPath(destination)) || IsInsideExcludedArea(destination))
                {
                    continue;
                }

                var area = Area(destination);
                area.Kinds.Add("libman-destination");
                area.Evidence.AddRange(entry.Manifests.Select(manifest => new DiscoveryEvidence("libman-library-destination", manifest)));
                if (!entry.FileSetDeclared)
                {
                    // All library files are restored, but which ones is not declared: locally added files cannot be told apart.
                    area.Classify(SourceRole.ThirdPartyUnknown, ScanMode.DeepScan, DiscoveryConfidence.Unknown);
                    continue;
                }

                area.Classify(SourceRole.ThirdPartyRuntime, ScanMode.RuntimeDependencyIndex, DiscoveryConfidence.High);
                foreach (var file in files.Where(file => DiscoveryPaths.IsUnder(file, destination) && !entry.DeclaredFiles.Contains(file)))
                {
                    Decide(decisions, file, new FileDecision(
                        CarveOutPriority,
                        SourceRole.Unknown,
                        ScanMode.DeepScan,
                        DiscoveryConfidence.Unknown,
                        entry.Manifests.Select(manifest => new DiscoveryEvidence("vendor-destination-undeclared-file", manifest)).ToArray()));
                }
            }
        }

        private void ClassifyBundles(
            string bundleConfig,
            IReadOnlyList<string> files,
            IReadOnlySet<string> inventory,
            IDictionary<string, FileDecision> decisions,
            ICollection<GeneratedArtifact> artifacts)
        {
            using var document = ReadJsonManifest(RepositoryFile(bundleConfig), bundleConfig);
            if (document is null || document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            var baseDirectory = DiscoveryPaths.Parent(bundleConfig);
            var candidates = files
                .Where(file => DiscoveryPaths.IsUnder(file, baseDirectory))
                .Select(file => (File: file, Relative: DiscoveryPaths.RelativeTo(baseDirectory, file)))
                .ToArray();
            var evidence = new DiscoveryEvidence("bundleconfig-output", bundleConfig);

            foreach (var bundle in document.RootElement.EnumerateArray().Where(entry => entry.ValueKind == JsonValueKind.Object))
            {
                var declaredOutput = GetString(bundle, "outputFileName");
                var output = declaredOutput is null ? null : DiscoveryPaths.Resolve(baseDirectory, declaredOutput);
                if (output is null || !bundle.TryGetProperty("inputFiles", out var inputFiles) || inputFiles.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                var inputs = new SortedSet<string>(StringComparer.Ordinal);
                var unresolvedInputs = new SortedSet<string>(StringComparer.Ordinal);
                var exclusions = new List<string>();
                foreach (var value in inputFiles.EnumerateArray().Where(value => value.ValueKind == JsonValueKind.String))
                {
                    var pattern = value.GetString()!.Trim();
                    if (pattern.StartsWith('!'))
                    {
                        exclusions.Add(DiscoveryPaths.Normalize(pattern[1..]));
                        continue;
                    }

                    var normalized = DiscoveryPaths.Normalize(pattern);
                    if (normalized.Contains('*') || normalized.Contains('?'))
                    {
                        var matches = candidates.Where(entry => DiscoveryGlob.IsMatch(normalized, entry.Relative)).Select(entry => entry.File).ToArray();
                        if (matches.Length == 0)
                        {
                            unresolvedInputs.Add(DiscoveryPaths.Join(baseDirectory, normalized));
                        }

                        inputs.UnionWith(matches);
                        continue;
                    }

                    var resolved = DiscoveryPaths.Resolve(baseDirectory, normalized);
                    if (resolved is not null && inventory.Contains(resolved))
                    {
                        inputs.Add(resolved);
                    }
                    else
                    {
                        unresolvedInputs.Add(resolved ?? normalized);
                    }
                }

                inputs.RemoveWhere(input =>
                    input == output ||
                    exclusions.Any(exclusion => DiscoveryGlob.IsMatch(exclusion, DiscoveryPaths.RelativeTo(baseDirectory, input))));

                artifacts.Add(new GeneratedArtifact(output, "bundle-output", inputs.ToArray(), unresolvedInputs.ToArray(), [evidence]));
                if (inventory.Contains(output))
                {
                    Decide(decisions, output, new FileDecision(
                        GeneratedPriority, SourceRole.GeneratedOrRestorable, ScanMode.LightIndex, DiscoveryConfidence.High, [evidence]));
                }
            }
        }

        private void ClassifySourceMaps(
            IReadOnlyList<string> files,
            IReadOnlySet<string> inventory,
            IDictionary<string, FileDecision> decisions,
            ICollection<GeneratedArtifact> artifacts)
        {
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file);
                if (!extension.Equals(".js", StringComparison.OrdinalIgnoreCase) &&
                    !extension.Equals(".css", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var map = new[] { file + ".map", file[..^extension.Length] + ".map" }.FirstOrDefault(inventory.Contains);
                if (map is null)
                {
                    continue;
                }

                using var document = ReadJsonManifest(RepositoryFile(map), map);
                if (document is null ||
                    document.RootElement.ValueKind != JsonValueKind.Object ||
                    !document.RootElement.TryGetProperty("sources", out var sources) ||
                    sources.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                if (GetString(document.RootElement, "file") is { } declaredFile &&
                    !Path.GetFileName(declaredFile).Equals(Path.GetFileName(file), StringComparison.Ordinal))
                {
                    continue;
                }

                var sourceRoot = GetString(document.RootElement, "sourceRoot");
                var baseDirectory = string.IsNullOrEmpty(sourceRoot)
                    ? DiscoveryPaths.Parent(map)
                    : sourceRoot.Contains(':') ? null : DiscoveryPaths.Resolve(DiscoveryPaths.Parent(map), sourceRoot);
                var inputs = new SortedSet<string>(StringComparer.Ordinal);
                var unresolvedInputs = new SortedSet<string>(StringComparer.Ordinal);
                foreach (var source in sources.EnumerateArray().Where(value => value.ValueKind == JsonValueKind.String).Select(value => value.GetString()!))
                {
                    var resolved = baseDirectory is null || source.Contains(':') ? null : DiscoveryPaths.Resolve(baseDirectory, source);
                    if (resolved is not null && resolved != file && inventory.Contains(resolved))
                    {
                        inputs.Add(resolved);
                    }
                    else
                    {
                        unresolvedInputs.Add(resolved ?? source);
                    }
                }

                if (inputs.Count == 0)
                {
                    continue;
                }

                var evidence = new DiscoveryEvidence("source-map", map);
                artifacts.Add(new GeneratedArtifact(file, "source-map-output", inputs.ToArray(), unresolvedInputs.ToArray(), [evidence]));
                Decide(decisions, file, new FileDecision(
                    GeneratedPriority, SourceRole.GeneratedOrRestorable, ScanMode.LightIndex, DiscoveryConfidence.High, [evidence]));
            }
        }

        private static void Decide(IDictionary<string, FileDecision> decisions, string path, FileDecision decision)
        {
            if (!decisions.TryGetValue(path, out var existing) || decision.Priority > existing.Priority)
            {
                decisions[path] = decision;
            }
        }

        private AreaBuilder NearestDirectoryArea(string file) =>
            DiscoveryPaths.SelfAndAncestors(DiscoveryPaths.Parent(file))
                .Select(candidate => _areas.GetValueOrDefault(candidate))
                .OfType<AreaBuilder>()
                .First();

        private bool IsInsideExcludedArea(string path) =>
            DiscoveryPaths.SelfAndAncestors(path)
                .Select(candidate => _areas.GetValueOrDefault(candidate))
                .Any(area => area is { ScanMode: ScanMode.SafeAutoExclude });

        /// <summary>Byte equality; null when either file is too large to compare within discovery bounds.</summary>
        private bool? FilesIdentical(FileInfo left, FileInfo right)
        {
            if (left.Length > MaxComparedFileBytes || right.Length > MaxComparedFileBytes)
            {
                return null;
            }

            if (left.Length != right.Length)
            {
                return false;
            }

            using var leftStream = left.OpenRead();
            using var rightStream = right.OpenRead();
            return SHA256.HashData(leftStream).AsSpan().SequenceEqual(SHA256.HashData(rightStream));
        }

        private string FullPath(string relativePath) =>
            relativePath == DiscoveryPaths.Root
                ? _rootPath
                : Path.Combine(_rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

        private FileInfo RepositoryFile(string relativePath) => new(FullPath(relativePath));
    }
}
