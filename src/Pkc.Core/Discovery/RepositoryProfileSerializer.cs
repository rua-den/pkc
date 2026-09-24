using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pkc.Core.Discovery;

public static class RepositoryProfileSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        NewLine = "\n",
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper) }
    };

    /// <summary>Byte-stable serialization: no timestamps, no absolute paths, '\n' line endings.</summary>
    public static string Serialize(RepositoryProfile profile) =>
        JsonSerializer.Serialize(profile, Options) + "\n";
}

/// <summary>Local-only discovery artifacts under <c>.pkc/discovery</c>; never part of the portable workspace.</summary>
public static class RepositoryDiscoveryArtifacts
{
    public const string ProfileRelativePath = ".pkc/discovery/repository-profile.json";

    public static async Task<string> WriteAsync(
        string repositoryPath,
        RepositoryProfile profile,
        CancellationToken cancellationToken = default)
    {
        var profilePath = Path.Combine(
            Path.GetFullPath(repositoryPath),
            ProfileRelativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(profilePath)!);
        await File.WriteAllTextAsync(
            profilePath,
            RepositoryProfileSerializer.Serialize(profile),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            cancellationToken);
        return profilePath;
    }
}
