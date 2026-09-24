using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Pkc.Core.Discovery;

/// <summary>
/// Deterministic shallow repository discovery. Walks the filesystem once, reads only a bounded set of
/// build/workspace manifests, and never parses source bodies or runtime configuration values.
///
/// Exclusion authority rule: an area becomes <see cref="ScanMode.SafeAutoExclude"/> only when a
/// structural fact other than its own name proves it is generated or restorable (a sibling manifest
/// whose tool owns that output, a declared build output path, or a restored package archive).
/// </summary>
public sealed class RepositoryDiscovery
{
    public const string SchemaVersion = "0.2.0-discovery";

    public RepositoryProfile Discover(string repositoryPath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryPath);

        var rootPath = Path.GetFullPath(repositoryPath);
        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException($"Repository path does not exist: {rootPath}");
        }

        var walk = new DiscoveryWalk(rootPath, cancellationToken);
        walk.Visit(new DirectoryInfo(rootPath), DiscoveryPaths.Root, MsBuildOutputLayout.Default, inheritedOutputTypeDeclaredBy: null);
        return walk.BuildProfile();
    }

    private sealed class DiscoveryWalk(string rootPath, CancellationToken cancellationToken)
    {
        private const long MaxManifestBytes = 4 * 1024 * 1024;
        private const string TestProjectTypeGuid = "3AC096D0-A1C2-E12C-1390-A8335801FDAB";

        private static readonly HashSet<string> MsBuildProjectExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".csproj", ".vbproj", ".fsproj" };

        private static readonly HashSet<string> TestFrameworkPackages =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Microsoft.NET.Test.Sdk",
                "xunit",
                "xunit.core",
                "xunit.v3",
                "xunit.v3.core",
                "NUnit",
                "MSTest",
                "MSTest.TestFramework",
                "MSTest.TestAdapter",
                "TUnit"
            };

        private static readonly HashSet<string> TestFrameworkAssemblies =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Microsoft.VisualStudio.QualityTools.UnitTestFramework",
                "Microsoft.VisualStudio.TestPlatform.TestFramework",
                "nunit.framework",
                "xunit",
                "xunit.core"
            };

        private static readonly Regex SolutionProjectLine = new(
            @"^\s*Project\(""\{[^}]+\}""\)\s*=\s*""[^""]*""\s*,\s*""(?<path>[^""]+)""",
            RegexOptions.CultureInvariant);

        private static readonly Regex TestFileIncludeSegment = new(
            @"^\*\.(?<token>[A-Za-z0-9_-]+)\.(ts|tsx|js|jsx|mjs|cjs)$",
            RegexOptions.CultureInvariant);

        private static readonly JsonDocumentOptions JsonManifestOptions = new()
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        private readonly List<string> _files = [];
        private readonly SortedDictionary<string, string> _manifests = new(StringComparer.Ordinal);
        private readonly Dictionary<string, AreaBuilder> _areas = new(StringComparer.Ordinal);
        private readonly Dictionary<string, List<DiscoveryEvidence>> _declaredOutputs = new(StringComparer.Ordinal);
        private readonly SortedSet<string> _contentReads = new(StringComparer.Ordinal);
        private readonly List<DotnetProjectRecord> _dotnetProjects = [];
        private readonly List<AngularProjectRecord> _angularProjects = [];
        private readonly List<SolutionRecord> _solutions = [];
        private int _directories;

        public void Visit(
            DirectoryInfo directory,
            string relativeDirectory,
            MsBuildOutputLayout inheritedLayout,
            string? inheritedOutputTypeDeclaredBy)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _directories++;
            if (relativeDirectory == DiscoveryPaths.Root)
            {
                Area(DiscoveryPaths.Root).Kinds.Add("repository-root");
            }

            FileSystemInfo[] entries;
            try
            {
                entries = directory.EnumerateFileSystemInfos()
                    .OrderBy(entry => entry.Name, StringComparer.Ordinal)
                    .ToArray();
            }
            catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
            {
                var unreadable = Area(relativeDirectory);
                unreadable.Kinds.Add("unreadable-directory");
                unreadable.Classify(SourceRole.Unknown, ScanMode.Unknown, DiscoveryConfidence.Unknown);
                unreadable.Enumerated = false;
                return;
            }

            var files = entries.OfType<FileInfo>().ToArray();
            var fileNames = files.Select(file => file.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                var relativeFile = DiscoveryPaths.Join(relativeDirectory, file.Name);
                _files.Add(relativeFile);

                var manifestKind = ManifestKind(file.Name);
                if (manifestKind is not null)
                {
                    _manifests[relativeFile] = manifestKind;
                }

                if (manifestKind == "dotnet-solution")
                {
                    InspectSolution(file, relativeFile);
                }

                var infrastructureKind = InfrastructureKind(relativeFile, file.Name);
                if (infrastructureKind is not null)
                {
                    _manifests[relativeFile] = infrastructureKind;
                    var infrastructure = Area(relativeFile);
                    infrastructure.Kinds.Add(infrastructureKind);
                    infrastructure.Classify(SourceRole.Infrastructure, ScanMode.LightIndex, DiscoveryConfidence.High);
                    infrastructure.Evidence.Add(new DiscoveryEvidence("infrastructure-file-format", relativeFile));
                }
            }

            var directoryLayout = inheritedLayout;
            var outputTypeDeclaredBy = inheritedOutputTypeDeclaredBy;
            foreach (var file in files.Where(file =>
                         file.Name.Equals("Directory.Build.props", StringComparison.OrdinalIgnoreCase) ||
                         file.Name.Equals("Directory.Build.targets", StringComparison.OrdinalIgnoreCase)))
            {
                var relativeFile = DiscoveryPaths.Join(relativeDirectory, file.Name);
                var document = ReadXmlManifest(file, relativeFile);
                directoryLayout = directoryLayout.Combine(
                    document is null ? MsBuildOutputLayout.Unproven : MsBuildOutputLayout.From(document));
                if (document is null || document.Descendants().Any(element => element.Name.LocalName == "OutputType"))
                {
                    outputTypeDeclaredBy ??= relativeFile;
                }
            }

            var projects = files.Where(file => MsBuildProjectExtensions.Contains(file.Extension)).ToArray();
            var projectLayout = directoryLayout;
            foreach (var project in projects)
            {
                projectLayout = projectLayout.Combine(InspectProject(
                    project,
                    DiscoveryPaths.Join(relativeDirectory, project.Name),
                    relativeDirectory,
                    outputTypeDeclaredBy));
            }

            if (fileNames.Contains("package.json"))
            {
                var package = Area(relativeDirectory);
                package.Kinds.Add("node-package");
                package.Evidence.Add(new DiscoveryEvidence("npm-manifest", DiscoveryPaths.Join(relativeDirectory, "package.json")));
            }

            var angularWorkspace = files.FirstOrDefault(file => file.Name.Equals("angular.json", StringComparison.OrdinalIgnoreCase));
            if (angularWorkspace is not null)
            {
                InspectAngularWorkspace(angularWorkspace, relativeDirectory);
            }

            foreach (var child in entries.OfType<DirectoryInfo>())
            {
                var relativeChild = DiscoveryPaths.Join(relativeDirectory, child.Name);
                if (child.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    var linked = Area(relativeChild);
                    linked.Kinds.Add("reparse-point");
                    linked.Classify(SourceRole.Unknown, ScanMode.Unknown, DiscoveryConfidence.Unknown);
                    linked.Evidence.Add(new DiscoveryEvidence("reparse-point-not-followed", relativeChild));
                    linked.Enumerated = false;
                    continue;
                }

                var exclusion = ProveExclusion(child, relativeChild, relativeDirectory, fileNames, projects, projectLayout);
                if (exclusion is not null)
                {
                    var excluded = Area(relativeChild);
                    excluded.Kinds.Add(exclusion.Value.Kind);
                    excluded.Classify(exclusion.Value.Role, ScanMode.SafeAutoExclude, DiscoveryConfidence.High);
                    excluded.Evidence.AddRange(exclusion.Value.Evidence);
                    excluded.Enumerated = false;
                    continue;
                }

                Visit(child, relativeChild, directoryLayout, outputTypeDeclaredBy);
            }
        }

        public RepositoryProfile BuildProfile()
        {
            var files = _files.OrderBy(path => path, StringComparer.Ordinal).ToArray();
            var provisional = new RepositoryProfile(
                RepositoryDiscovery.SchemaVersion,
                new RepositoryInventory(0, 0, [], [], []),
                [],
                _areas.Values.Select(builder => builder.Build(0, [], _ => 0)).ToArray(),
                [],
                [],
                [],
                []);

            var areaFiles = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            var overrideCounts = new Dictionary<(string Area, string Pattern), int>();
            var roleCounts = new Dictionary<SourceRole, int>();
            var scanModeCounts = new Dictionary<ScanMode, int>();
            foreach (var file in files)
            {
                var (nearest, overrideOwner, match) = provisional.Resolve(file);
                if (!areaFiles.TryGetValue(nearest.Path, out var attributed))
                {
                    areaFiles[nearest.Path] = attributed = [];
                }

                attributed.Add(file);
                if (overrideOwner is not null && match is not null)
                {
                    var key = (overrideOwner.Path, match.Pattern);
                    overrideCounts[key] = overrideCounts.GetValueOrDefault(key) + 1;
                }

                var classification = provisional.Classify(file);
                roleCounts[classification.Role] = roleCounts.GetValueOrDefault(classification.Role) + 1;
                scanModeCounts[classification.ScanMode] = scanModeCounts.GetValueOrDefault(classification.ScanMode) + 1;
            }

            var areas = _areas.Values
                .OrderBy(builder => builder.Path, StringComparer.Ordinal)
                .Select(builder =>
                {
                    var attributed = areaFiles.GetValueOrDefault(builder.Path) ?? [];
                    return builder.Build(
                        attributed.Count,
                        CountLanguages(attributed),
                        pattern => overrideCounts.GetValueOrDefault((builder.Path, pattern)));
                })
                .ToArray();

            var inventory = new RepositoryInventory(
                files.Length,
                _directories,
                CountLanguages(files),
                roleCounts.OrderBy(pair => pair.Key).Select(pair => new RoleCount(pair.Key, pair.Value)).ToArray(),
                scanModeCounts.OrderBy(pair => pair.Key).Select(pair => new ScanModeCount(pair.Key, pair.Value)).ToArray());

            var (components, edges, unresolved) = ComponentGraph.Build(_dotnetProjects, _angularProjects, _solutions);

            return new RepositoryProfile(
                RepositoryDiscovery.SchemaVersion,
                inventory,
                _manifests.Select(pair => new RepositoryManifest(pair.Key, pair.Value)).ToArray(),
                areas,
                components,
                edges,
                unresolved,
                _contentReads.ToArray())
            {
                Files = files
            };
        }

        private (string Kind, SourceRole Role, IReadOnlyList<DiscoveryEvidence> Evidence)? ProveExclusion(
            DirectoryInfo child,
            string relativeChild,
            string relativeParent,
            IReadOnlySet<string> siblingFileNames,
            IReadOnlyList<FileInfo> siblingProjects,
            MsBuildOutputLayout projectLayout)
        {
            var name = child.Name;

            if (relativeParent == DiscoveryPaths.Root && name == ".pkc")
            {
                return ("pkc-output", SourceRole.ToolState, [new DiscoveryEvidence("pkc-output-root", DiscoveryPaths.Root)]);
            }

            if (name == ".git" && File.Exists(Path.Combine(child.FullName, "HEAD")))
            {
                return ("vcs-metadata", SourceRole.ToolState, [new DiscoveryEvidence("git-metadata-head", relativeChild + "/HEAD")]);
            }

            if (_declaredOutputs.TryGetValue(relativeChild, out var declared))
            {
                return ("declared-build-output", SourceRole.GeneratedOrRestorable, declared);
            }

            if (name.Equals("node_modules", StringComparison.Ordinal) && siblingFileNames.Contains("package.json"))
            {
                return ("npm-restore", SourceRole.GeneratedOrRestorable,
                    [new DiscoveryEvidence("npm-manifest-sibling", DiscoveryPaths.Join(relativeParent, "package.json"))]);
            }

            if (siblingProjects.Count > 0 &&
                ((name.Equals("bin", StringComparison.OrdinalIgnoreCase) && !projectLayout.OutputUnproven) ||
                 (name.Equals("obj", StringComparison.OrdinalIgnoreCase) && !projectLayout.IntermediateUnproven)))
            {
                var evidenceKind = name.Equals("bin", StringComparison.OrdinalIgnoreCase)
                    ? "msbuild-default-output-path"
                    : "msbuild-default-intermediate-path";
                return ("msbuild-output", SourceRole.GeneratedOrRestorable,
                    siblingProjects
                        .Select(project => new DiscoveryEvidence(evidenceKind, DiscoveryPaths.Join(relativeParent, project.Name)))
                        .ToArray());
            }

            if (name.Equals(".angular", StringComparison.Ordinal) && siblingFileNames.Contains("angular.json"))
            {
                return ("angular-cli-cache", SourceRole.GeneratedOrRestorable,
                    [new DiscoveryEvidence("angular-workspace-sibling", DiscoveryPaths.Join(relativeParent, "angular.json"))]);
            }

            // Restored NuGet package folders: packages.config layout `<id>.<version>/<id>.<version>.nupkg`
            // and global-packages layout `<id>/<version>/<id>.<version>.nupkg`.
            var archiveNames = relativeParent == DiscoveryPaths.Root
                ? new[] { name + ".nupkg" }
                : new[] { name + ".nupkg", Path.GetFileName(relativeParent) + "." + name + ".nupkg" };
            foreach (var archiveName in archiveNames)
            {
                if (File.Exists(Path.Combine(child.FullName, archiveName)))
                {
                    return ("nuget-package-restore", SourceRole.GeneratedOrRestorable,
                        [new DiscoveryEvidence("nuget-package-archive", DiscoveryPaths.Join(relativeChild, archiveName))]);
                }
            }

            return null;
        }

        private MsBuildOutputLayout InspectProject(
            FileInfo project,
            string relativeProject,
            string relativeDirectory,
            string? outputTypeDeclaredBy)
        {
            var area = Area(relativeDirectory);
            area.Kinds.Add("dotnet-project");
            area.Evidence.Add(new DiscoveryEvidence("msbuild-project-file", relativeProject));

            var document = ReadXmlManifest(project, relativeProject);
            if (document?.Root is null)
            {
                area.Evidence.Add(new DiscoveryEvidence("manifest-unreadable", relativeProject));
                _dotnetProjects.Add(DotnetProjectIdentity.Unreadable(relativeProject, relativeDirectory));
                return MsBuildOutputLayout.Unproven;
            }

            var testEvidence = new SortedSet<string>(StringComparer.Ordinal);
            var sdk = document.Root.Attribute("Sdk")?.Value ?? string.Empty;
            if (sdk.Contains("MSTest.Sdk", StringComparison.OrdinalIgnoreCase))
            {
                testEvidence.Add("test-framework-sdk");
            }

            foreach (var element in document.Root.Descendants())
            {
                switch (element.Name.LocalName)
                {
                    case "IsTestProject" when element.Value.Trim().Equals("true", StringComparison.OrdinalIgnoreCase):
                        testEvidence.Add("msbuild-is-test-project");
                        break;
                    case "ProjectTypeGuids" when element.Value.Contains(TestProjectTypeGuid, StringComparison.OrdinalIgnoreCase):
                        testEvidence.Add("msbuild-test-project-type");
                        break;
                    case "PackageReference" when TestFrameworkPackages.Contains(element.Attribute("Include")?.Value.Trim() ?? string.Empty):
                        testEvidence.Add("test-framework-package");
                        break;
                    case "Reference" when TestFrameworkAssemblies.Contains(AssemblySimpleName(element.Attribute("Include")?.Value)):
                        testEvidence.Add("test-framework-reference");
                        break;
                }
            }

            if (testEvidence.Count > 0)
            {
                area.Classify(SourceRole.TestEvidence, ScanMode.TestEvidence, DiscoveryConfidence.High);
                area.Evidence.AddRange(testEvidence.Select(kind => new DiscoveryEvidence(kind, relativeProject)));
            }

            _dotnetProjects.Add(DotnetProjectIdentity.Describe(
                document, relativeProject, relativeDirectory, testEvidence, outputTypeDeclaredBy));
            return MsBuildOutputLayout.From(document);
        }

        private void InspectSolution(FileInfo solution, string relativeSolution)
        {
            var solutionDirectory = DiscoveryPaths.Parent(relativeSolution);
            var projectPaths = new SortedSet<string>(StringComparer.Ordinal);
            if (solution.Extension.Equals(".slnx", StringComparison.OrdinalIgnoreCase))
            {
                var document = ReadXmlManifest(solution, relativeSolution);
                foreach (var path in document?.Descendants()
                             .Where(element => element.Name.LocalName == "Project")
                             .Select(element => element.Attribute("Path")?.Value)
                             .OfType<string>() ?? [])
                {
                    AddSolutionProject(projectPaths, solutionDirectory, path);
                }
            }
            else if (CanRead(solution, relativeSolution))
            {
                try
                {
                    foreach (var line in File.ReadLines(solution.FullName))
                    {
                        var match = SolutionProjectLine.Match(line);
                        if (match.Success)
                        {
                            AddSolutionProject(projectPaths, solutionDirectory, match.Groups["path"].Value);
                        }
                    }
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
                {
                    projectPaths.Clear();
                }
            }

            _solutions.Add(new SolutionRecord(relativeSolution, projectPaths.ToArray()));
        }

        private static void AddSolutionProject(ISet<string> projectPaths, string solutionDirectory, string path)
        {
            if (MsBuildProjectExtensions.Contains(Path.GetExtension(path)) &&
                DiscoveryPaths.Resolve(solutionDirectory, path) is { } resolved)
            {
                projectPaths.Add(resolved);
            }
        }

        private void InspectAngularWorkspace(FileInfo workspaceFile, string relativeDirectory)
        {
            var relativeWorkspaceFile = DiscoveryPaths.Join(relativeDirectory, workspaceFile.Name);
            var area = Area(relativeDirectory);
            area.Kinds.Add("angular-workspace");
            area.Evidence.Add(new DiscoveryEvidence("angular-workspace-file", relativeWorkspaceFile));

            using var document = ReadJsonManifest(workspaceFile, relativeWorkspaceFile);
            if (document is null ||
                document.RootElement.ValueKind != JsonValueKind.Object ||
                !document.RootElement.TryGetProperty("projects", out var projects) ||
                projects.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            foreach (var project in projects.EnumerateObject().OrderBy(project => project.Name, StringComparer.Ordinal))
            {
                if (project.Value.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                RegisterAngularProject(project.Name, project.Value, relativeDirectory, relativeWorkspaceFile);

                var targets = GetObject(project.Value, "architect") ?? GetObject(project.Value, "targets");
                if (targets is null)
                {
                    continue;
                }

                var sourceRoot = GetString(project.Value, "sourceRoot") is { } declaredSourceRoot
                    ? DiscoveryPaths.Resolve(relativeDirectory, declaredSourceRoot)
                    : null;
                RegisterAngularOutput(targets.Value, relativeDirectory, sourceRoot, relativeWorkspaceFile);
                RegisterAngularTestFiles(targets.Value, relativeDirectory, relativeWorkspaceFile, area);
            }
        }

        private void RegisterAngularProject(string name, JsonElement project, string relativeDirectory, string relativeWorkspaceFile)
        {
            var declaredRoot = GetString(project, "root");
            // An absent or empty Angular project root means the workspace root.
            var areaPath = string.IsNullOrWhiteSpace(declaredRoot)
                ? relativeDirectory
                : DiscoveryPaths.Resolve(relativeDirectory, declaredRoot);
            var (kind, evidenceKind) = GetString(project, "projectType") switch
            {
                "application" => (ComponentKind.AngularApplication, "angular-project-type"),
                "library" => (ComponentKind.AngularLibrary, "angular-project-type"),
                null => AngularBuilderKind(project),
                _ => (ComponentKind.Unknown, "angular-project-type-unsupported")
            };
            if (areaPath is null)
            {
                (kind, evidenceKind) = (ComponentKind.Unknown, "angular-project-root-unresolved");
            }

            _angularProjects.Add(new AngularProjectRecord(
                $"angular:{relativeWorkspaceFile}#{name}",
                relativeWorkspaceFile,
                areaPath ?? relativeDirectory,
                kind,
                kind == ComponentKind.Unknown ? DiscoveryConfidence.Unknown : DiscoveryConfidence.High,
                [new DiscoveryEvidence(evidenceKind, relativeWorkspaceFile)]));
        }

        /// <summary>Only official Angular build builders identify the project type when projectType is absent.</summary>
        private static (ComponentKind Kind, string EvidenceKind) AngularBuilderKind(JsonElement project)
        {
            var targets = GetObject(project, "architect") ?? GetObject(project, "targets");
            var build = targets is null ? null : GetObject(targets.Value, "build");
            var builder = build is null ? null : GetString(build.Value, "builder");
            var official = builder is not null &&
                           (builder.StartsWith("@angular-devkit/build-angular:", StringComparison.Ordinal) ||
                            builder.StartsWith("@angular/build:", StringComparison.Ordinal));
            var target = official ? builder![(builder!.IndexOf(':') + 1)..] : null;
            return target switch
            {
                "application" or "browser" or "browser-esbuild" => (ComponentKind.AngularApplication, "angular-application-builder"),
                "ng-packagr" => (ComponentKind.AngularLibrary, "angular-library-builder"),
                _ => (ComponentKind.Unknown, "angular-project-type-undeclared")
            };
        }

        private void RegisterAngularOutput(JsonElement targets, string relativeDirectory, string? sourceRoot, string relativeWorkspaceFile)
        {
            var options = GetObject(targets, "build") is { } build ? GetObject(build, "options") : null;
            if (options is null)
            {
                return;
            }

            if (options.Value.TryGetProperty("deleteOutputPath", out var deleteOutputPath) &&
                deleteOutputPath.ValueKind == JsonValueKind.False)
            {
                // Output that is not cleaned by the build may hold hand-placed files; not proven generated.
                return;
            }

            var declared = GetString(options.Value, "outputPath") ??
                           (GetObject(options.Value, "outputPath") is { } outputObject ? GetString(outputObject, "base") : null);
            var output = declared is null ? null : DiscoveryPaths.Resolve(relativeDirectory, declared);
            if (output is null ||
                output == DiscoveryPaths.Root ||
                DiscoveryPaths.IsUnder(relativeDirectory, output) ||
                (sourceRoot is not null && DiscoveryPaths.IsUnder(sourceRoot, output)))
            {
                return;
            }

            if (!_declaredOutputs.TryGetValue(output, out var evidence))
            {
                _declaredOutputs[output] = evidence = [];
            }

            var item = new DiscoveryEvidence("angular-build-output-path", relativeWorkspaceFile);
            if (!evidence.Contains(item))
            {
                evidence.Add(item);
            }
        }

        private void RegisterAngularTestFiles(JsonElement targets, string relativeDirectory, string relativeWorkspaceFile, AreaBuilder area)
        {
            var options = GetObject(targets, "test") is { } test ? GetObject(test, "options") : null;
            var declaredConfig = options is null ? null : GetString(options.Value, "tsConfig");
            var configPath = declaredConfig is null ? null : DiscoveryPaths.Resolve(relativeDirectory, declaredConfig);
            if (configPath is null ||
                !DiscoveryPaths.IsUnder(configPath, relativeDirectory) ||
                ManifestKind(Path.GetFileName(configPath)) != "typescript-config")
            {
                return;
            }

            var configFile = new FileInfo(Path.Combine(rootPath, configPath.Replace('/', Path.DirectorySeparatorChar)));
            using var document = configFile.Exists ? ReadJsonManifest(configFile, configPath) : null;
            if (document is null ||
                document.RootElement.ValueKind != JsonValueKind.Object ||
                !document.RootElement.TryGetProperty("include", out var include) ||
                include.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            var configDirectory = DiscoveryPaths.RelativeTo(relativeDirectory, DiscoveryPaths.Parent(configPath));
            foreach (var entry in include.EnumerateArray())
            {
                var pattern = entry.ValueKind == JsonValueKind.String ? DiscoveryPaths.Normalize(entry.GetString()!) : null;
                if (pattern is null || pattern == DiscoveryPaths.Root || pattern.Split('/').Contains(".."))
                {
                    continue;
                }

                // Only a test-target include that selects a dedicated test-file shape is test evidence.
                // Declaration files (`*.d.ts`) and broad globs are shared compilation inputs, not tests.
                var match = TestFileIncludeSegment.Match(pattern.Split('/').Last());
                if (!match.Success || match.Groups["token"].Value.Equals("d", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var areaPattern = configDirectory.Length == 0 || configDirectory == DiscoveryPaths.Root
                    ? pattern
                    : configDirectory + "/" + pattern;
                area.AddOverride(
                    areaPattern,
                    SourceRole.TestEvidence,
                    ScanMode.TestEvidence,
                    DiscoveryConfidence.High,
                    [
                        new DiscoveryEvidence("angular-test-target", relativeWorkspaceFile),
                        new DiscoveryEvidence("typescript-config-include", configPath)
                    ]);
            }
        }

        private XDocument? ReadXmlManifest(FileInfo file, string relativePath)
        {
            if (!CanRead(file, relativePath))
            {
                return null;
            }

            try
            {
                using var stream = file.OpenRead();
                using var reader = XmlReader.Create(stream, new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null
                });
                return XDocument.Load(reader);
            }
            catch (Exception exception) when (exception is XmlException or IOException or UnauthorizedAccessException)
            {
                return null;
            }
        }

        private JsonDocument? ReadJsonManifest(FileInfo file, string relativePath)
        {
            if (!CanRead(file, relativePath))
            {
                return null;
            }

            try
            {
                using var stream = file.OpenRead();
                return JsonDocument.Parse(stream, JsonManifestOptions);
            }
            catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
            {
                return null;
            }
        }

        private bool CanRead(FileInfo file, string relativePath)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (file.Length > MaxManifestBytes)
            {
                return false;
            }

            _contentReads.Add(relativePath);
            return true;
        }

        private AreaBuilder Area(string relativePath)
        {
            if (!_areas.TryGetValue(relativePath, out var area))
            {
                _areas[relativePath] = area = new AreaBuilder(relativePath);
            }

            return area;
        }

        private static IReadOnlyList<LanguageCount> CountLanguages(IEnumerable<string> files) =>
            files
                .GroupBy(Language, StringComparer.Ordinal)
                .OrderBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => new LanguageCount(group.Key, group.Count()))
                .ToArray();

        private static string Language(string relativePath)
        {
            var name = Path.GetFileName(relativePath);
            if (name.Equals("Dockerfile", StringComparison.OrdinalIgnoreCase))
            {
                return "dockerfile";
            }

            return Path.GetExtension(name).ToLowerInvariant() switch
            {
                ".cs" => "csharp",
                ".vb" => "visual-basic",
                ".fs" => "fsharp",
                ".ts" or ".tsx" or ".mts" or ".cts" => "typescript",
                ".js" or ".jsx" or ".mjs" or ".cjs" => "javascript",
                ".html" or ".htm" => "html",
                ".cshtml" or ".vbhtml" or ".razor" => "razor",
                ".aspx" or ".ascx" or ".asax" or ".master" or ".ashx" or ".asmx" => "aspnet-webforms",
                ".css" or ".scss" or ".sass" or ".less" => "stylesheet",
                ".json" => "json",
                ".xml" or ".config" or ".props" or ".targets" or ".csproj" or ".vbproj" or ".fsproj" or ".resx" => "xml",
                ".yml" or ".yaml" => "yaml",
                ".sql" => "sql",
                ".ps1" or ".psm1" or ".psd1" => "powershell",
                ".sh" or ".bash" => "shell",
                ".cmd" or ".bat" => "batch",
                ".py" => "python",
                ".java" => "java",
                ".tf" => "terraform",
                ".bicep" => "bicep",
                ".md" => "markdown",
                ".sln" or ".slnx" => "solution",
                ".dll" or ".exe" or ".pdb" or ".nupkg" or ".zip" => "binary",
                ".png" or ".jpg" or ".jpeg" or ".gif" or ".svg" or ".ico" or ".webp" or ".bmp" => "image",
                ".woff" or ".woff2" or ".ttf" or ".eot" or ".otf" => "font",
                _ => "other"
            };
        }

        private static string? ManifestKind(string name)
        {
            var extension = Path.GetExtension(name);
            if (extension.Equals(".sln", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".slnx", StringComparison.OrdinalIgnoreCase))
            {
                return "dotnet-solution";
            }

            if (MsBuildProjectExtensions.Contains(extension))
            {
                return "dotnet-project";
            }

            if (name.StartsWith("tsconfig", StringComparison.OrdinalIgnoreCase) &&
                name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return "typescript-config";
            }

            if ((name.StartsWith("appsettings", StringComparison.OrdinalIgnoreCase) &&
                 name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) ||
                name.Equals("app.config", StringComparison.OrdinalIgnoreCase) ||
                (name.StartsWith("web.", StringComparison.OrdinalIgnoreCase) &&
                 name.EndsWith(".config", StringComparison.OrdinalIgnoreCase)))
            {
                return "runtime-configuration";
            }

            if (name.StartsWith("webpack.config.", StringComparison.OrdinalIgnoreCase))
            {
                return "js-build-script";
            }

            return name.ToLowerInvariant() switch
            {
                "directory.build.props" => "msbuild-directory-props",
                "directory.build.targets" => "msbuild-directory-targets",
                "directory.packages.props" => "msbuild-central-packages",
                "global.json" => "dotnet-global-json",
                "nuget.config" => "nuget-config",
                "packages.config" => "nuget-packages-config",
                "packages.lock.json" => "nuget-lockfile",
                "package.json" => "node-package",
                "package-lock.json" or "npm-shrinkwrap.json" or "yarn.lock" or "pnpm-lock.yaml" => "node-lockfile",
                "angular.json" or ".angular-cli.json" => "angular-workspace",
                "bower.json" => "bower-manifest",
                "libman.json" => "libman-manifest",
                "bundleconfig.json" => "legacy-bundle-config",
                "gulpfile.js" or "gruntfile.js" => "js-build-script",
                _ => null
            };
        }

        private static string? InfrastructureKind(string relativePath, string name)
        {
            var lower = name.ToLowerInvariant();
            var yaml = lower.EndsWith(".yml", StringComparison.Ordinal) || lower.EndsWith(".yaml", StringComparison.Ordinal);

            if (lower == "dockerfile" || lower.StartsWith("dockerfile.", StringComparison.Ordinal) ||
                lower.EndsWith(".dockerfile", StringComparison.Ordinal))
            {
                return "container-build";
            }

            if (yaml && (lower.StartsWith("docker-compose", StringComparison.Ordinal) ||
                         lower.StartsWith("compose.", StringComparison.Ordinal)))
            {
                return "container-compose";
            }

            if ((yaml && lower.StartsWith("azure-pipelines", StringComparison.Ordinal)) ||
                lower == ".gitlab-ci.yml" ||
                lower == "jenkinsfile" ||
                (yaml && DiscoveryPaths.Parent(relativePath).Equals(".github/workflows", StringComparison.Ordinal)))
            {
                return "ci-pipeline";
            }

            if (lower == "chart.yaml")
            {
                return "helm-chart";
            }

            return Path.GetExtension(lower) switch
            {
                ".tf" => "terraform",
                ".bicep" => "bicep",
                _ => null
            };
        }

        private static string AssemblySimpleName(string? include) =>
            include is null ? string.Empty : include.Split(',')[0].Trim();

        private static JsonElement? GetObject(JsonElement element, string name) =>
            element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(name, out var value) &&
            value.ValueKind == JsonValueKind.Object
                ? value
                : null;

        private static string? GetString(JsonElement element, string name) =>
            element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(name, out var value) &&
            value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
    }

    private sealed class AreaBuilder(string path)
    {
        private readonly List<(string Pattern, SourceRole Role, ScanMode ScanMode, DiscoveryConfidence Confidence, IReadOnlyList<DiscoveryEvidence> Evidence)> _overrides = [];

        public string Path { get; } = path;
        public SortedSet<string> Kinds { get; } = new(StringComparer.Ordinal);
        public List<DiscoveryEvidence> Evidence { get; } = [];
        public SourceRole Role { get; private set; } = SourceRole.Unknown;
        public ScanMode ScanMode { get; private set; } = ScanMode.DeepScan;
        public DiscoveryConfidence Confidence { get; private set; } = DiscoveryConfidence.Unknown;
        public bool Enumerated { get; set; } = true;

        public void Classify(SourceRole role, ScanMode scanMode, DiscoveryConfidence confidence)
        {
            Role = role;
            ScanMode = scanMode;
            Confidence = confidence;
        }

        public void AddOverride(
            string pattern,
            SourceRole role,
            ScanMode scanMode,
            DiscoveryConfidence confidence,
            IReadOnlyList<DiscoveryEvidence> evidence)
        {
            if (_overrides.All(existing => existing.Pattern != pattern))
            {
                _overrides.Add((pattern, role, scanMode, confidence, evidence));
            }
        }

        public SourceArea Build(int fileCount, IReadOnlyList<LanguageCount> languages, Func<string, int> overrideFileCount) =>
            new(
                Path,
                Kinds.ToArray(),
                Role,
                ScanMode,
                Confidence,
                Evidence
                    .Distinct()
                    .OrderBy(evidence => evidence.Kind, StringComparer.Ordinal)
                    .ThenBy(evidence => evidence.Path, StringComparer.Ordinal)
                    .ToArray(),
                Enumerated,
                fileCount,
                languages,
                _overrides
                    .OrderBy(entry => entry.Pattern, StringComparer.Ordinal)
                    .Select(entry => new SourceAreaOverride(
                        entry.Pattern,
                        entry.Role,
                        entry.ScanMode,
                        entry.Confidence,
                        entry.Evidence,
                        overrideFileCount(entry.Pattern)))
                    .ToArray());
    }

    /// <summary>
    /// Whether MSBuild's default <c>bin/</c> and <c>obj/</c> locations are still proven for projects in a directory.
    /// Any redirect that does not literally stay under the default folder invalidates the proof.
    /// </summary>
    internal readonly record struct MsBuildOutputLayout(bool OutputUnproven, bool IntermediateUnproven)
    {
        private static readonly string[] OutputProperties = ["BaseOutputPath", "OutputPath", "OutDir"];
        private static readonly string[] IntermediateProperties = ["BaseIntermediateOutputPath", "IntermediateOutputPath", "MSBuildProjectExtensionsPath"];

        public static MsBuildOutputLayout Default => new(false, false);

        public static MsBuildOutputLayout Unproven => new(true, true);

        public MsBuildOutputLayout Combine(MsBuildOutputLayout other) =>
            new(OutputUnproven || other.OutputUnproven, IntermediateUnproven || other.IntermediateUnproven);

        public static MsBuildOutputLayout From(XDocument document)
        {
            var outputUnproven = false;
            var intermediateUnproven = false;
            foreach (var element in document.Descendants())
            {
                var name = element.Name.LocalName;
                var value = element.Value;
                if (OutputProperties.Contains(name, StringComparer.Ordinal))
                {
                    outputUnproven |= !StaysUnder(value, "bin");
                }
                else if (IntermediateProperties.Contains(name, StringComparer.Ordinal))
                {
                    intermediateUnproven |= !StaysUnder(value, "obj");
                }
                else if ((name == "UseArtifactsOutput" && !value.Trim().Equals("false", StringComparison.OrdinalIgnoreCase)) ||
                         name == "ArtifactsPath")
                {
                    outputUnproven = true;
                    intermediateUnproven = true;
                }
            }

            return new MsBuildOutputLayout(outputUnproven, intermediateUnproven);
        }

        private static bool StaysUnder(string value, string folder)
        {
            var normalized = value.Trim().Replace('\\', '/');
            while (normalized.StartsWith("./", StringComparison.Ordinal))
            {
                normalized = normalized[2..];
            }

            return normalized.Equals(folder, StringComparison.OrdinalIgnoreCase) ||
                   normalized.StartsWith(folder + "/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
