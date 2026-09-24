using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Pkc.Core.Discovery;

/// <summary>
/// Local record that a full semantic scan completed and its merged facts were written, so a later failure (knowledge
/// synthesis, rendering, workspace write) can be resumed with <c>--resume</c> without re-running the expensive
/// scanners. Local-only; never part of the portable workspace.
/// </summary>
public sealed record ScanCheckpoint(
    string SchemaVersion,
    string PlanFingerprint,
    string ScannerIdentity,
    bool Scoped,
    DateTimeOffset CreatedUtc,
    string FactsFile,
    string FactsSha256,
    long FactsBytes,
    int Facts,
    int Relations,
    double ScanSeconds,
    ScanCoverage Coverage)
{
    public const string CurrentSchemaVersion = "0.1.0-scan-checkpoint";
    public const string RelativePath = ".pkc/checkpoint/scan-checkpoint.json";
    public const string FactsRelativePath = ".pkc/facts.json";

    /// <summary>Identity of the scanner build: a checkpoint from different scanner code is never reused.</summary>
    public static string IdentityOf(IEnumerable<Assembly> scannerAssemblies) =>
        "sha256:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
            "\n",
            scannerAssemblies
                .Select(assembly => $"{assembly.GetName().Name}:{assembly.ManifestModule.ModuleVersionId:N}")
                .Order(StringComparer.Ordinal))))).ToLowerInvariant();

    public static async Task<ScanCheckpoint> CreateAsync(
        string repositoryPath,
        string planFingerprint,
        string scannerIdentity,
        FactDocument facts,
        double scanSeconds,
        ScanCoverage coverage,
        CancellationToken cancellationToken = default)
    {
        var factsPath = FullPath(repositoryPath, FactsRelativePath);
        return new ScanCheckpoint(
            CurrentSchemaVersion,
            planFingerprint,
            scannerIdentity,
            coverage.Scoped,
            DateTimeOffset.UtcNow,
            FactsRelativePath,
            await Sha256Async(factsPath, cancellationToken),
            new FileInfo(factsPath).Length,
            facts.Facts.Count,
            facts.Relations.Count,
            Math.Round(scanSeconds, 1),
            coverage);
    }

    public static Task WriteAsync(string repositoryPath, ScanCheckpoint checkpoint, CancellationToken cancellationToken = default) =>
        JsonArtifactFile.WriteAsync(FullPath(repositoryPath, RelativePath), checkpoint, DiscoveryJson.Options, cancellationToken);

    /// <summary>Removes a checkpoint so a full scan that is about to overwrite the facts cannot leave a stale one behind.</summary>
    public static void Delete(string repositoryPath)
    {
        var path = FullPath(repositoryPath, RelativePath);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Loads the checkpoint only when it is complete and still matches the current plan, scanner build and scope.
    /// Otherwise returns the reason it cannot be used.
    /// </summary>
    public static async Task<(ScanCheckpoint? Checkpoint, string? Reason)> TryLoadAsync(
        string repositoryPath,
        string planFingerprint,
        string scannerIdentity,
        bool scoped,
        CancellationToken cancellationToken = default)
    {
        var path = FullPath(repositoryPath, RelativePath);
        if (!File.Exists(path))
        {
            return (null, "no scan checkpoint from a completed scan");
        }

        ScanCheckpoint? checkpoint;
        try
        {
            checkpoint = await JsonArtifactFile.ReadAsync<ScanCheckpoint>(path, DiscoveryJson.Options, cancellationToken);
        }
        catch (JsonException)
        {
            return (null, "the scan checkpoint is unreadable");
        }

        if (checkpoint is null || checkpoint.SchemaVersion != CurrentSchemaVersion)
        {
            return (null, "the scan checkpoint was written by another PKC version");
        }

        if (checkpoint.PlanFingerprint != planFingerprint)
        {
            return (null, "repository structure or manifests changed since the checkpoint");
        }

        if (checkpoint.ScannerIdentity != scannerIdentity)
        {
            return (null, "PKC scanner code changed since the checkpoint");
        }

        if (checkpoint.Scoped != scoped)
        {
            return (null, scoped
                ? "the checkpoint was produced by a whole-root scan, not the plan-scoped `run` scan"
                : "the checkpoint was produced by the plan-scoped `run` scan, not a whole-root scan");
        }

        var factsPath = FullPath(repositoryPath, checkpoint.FactsFile);
        if (!File.Exists(factsPath) ||
            new FileInfo(factsPath).Length != checkpoint.FactsBytes ||
            await Sha256Async(factsPath, cancellationToken) != checkpoint.FactsSha256)
        {
            return (null, "facts.json is missing or no longer matches the checkpoint");
        }

        return (checkpoint, null);
    }

    public static async Task<FactDocument> LoadFactsAsync(
        string repositoryPath,
        ScanCheckpoint checkpoint,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default) =>
        await JsonArtifactFile.ReadAsync<FactDocument>(FullPath(repositoryPath, checkpoint.FactsFile), options, cancellationToken)
        ?? throw new InvalidDataException("facts.json is empty.");

    private static async Task<string> Sha256Async(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return "sha256:" + Convert.ToHexString(await SHA256.HashDataAsync(stream, cancellationToken)).ToLowerInvariant();
    }

    private static string FullPath(string repositoryPath, string relativePath) =>
        Path.Combine(Path.GetFullPath(repositoryPath), relativePath.Replace('/', Path.DirectorySeparatorChar));
}
