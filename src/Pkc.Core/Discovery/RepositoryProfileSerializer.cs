using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pkc.Core.Discovery;

internal static class DiscoveryJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        NewLine = "\n",
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper) }
    };
}

public static class RepositoryProfileSerializer
{
    /// <summary>Byte-stable serialization: no timestamps, no absolute paths, '\n' line endings.</summary>
    public static string Serialize(RepositoryProfile profile) =>
        JsonSerializer.Serialize(profile, DiscoveryJson.Options) + "\n";
}

public static class ScanPlanSerializer
{
    /// <summary>Byte-stable serialization: no timestamps, no absolute paths, '\n' line endings.</summary>
    public static string Serialize(ScanPlan plan) =>
        JsonSerializer.Serialize(plan, DiscoveryJson.Options) + "\n";
}

/// <summary>Local-only discovery artifacts under <c>.pkc/discovery</c>; never part of the portable workspace.</summary>
public static class RepositoryDiscoveryArtifacts
{
    public const string ProfileRelativePath = ".pkc/discovery/repository-profile.json";
    public const string ScanPlanRelativePath = ".pkc/discovery/scan-plan.json";

    public static async Task<(string ProfilePath, string PlanPath)> WriteAsync(
        string repositoryPath,
        RepositoryProfile profile,
        ScanPlan plan,
        CancellationToken cancellationToken = default)
    {
        var profilePath = await WriteFileAsync(repositoryPath, ProfileRelativePath, RepositoryProfileSerializer.Serialize(profile), cancellationToken);
        var planPath = await WriteFileAsync(repositoryPath, ScanPlanRelativePath, ScanPlanSerializer.Serialize(plan), cancellationToken);
        return (profilePath, planPath);
    }

    private static async Task<string> WriteFileAsync(
        string repositoryPath,
        string relativePath,
        string content,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(Path.GetFullPath(repositoryPath), relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), cancellationToken);
        return path;
    }
}
