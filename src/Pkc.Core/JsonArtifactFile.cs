using System.Text.Json;

namespace Pkc.Core;

/// <summary>
/// Writes JSON artifacts by streaming to a temporary sibling file and atomically replacing the target once the write
/// has completed. Large repositories produce fact documents whose JSON text exceeds what a single in-memory string
/// can hold; streaming yields the same UTF-8 bytes (no BOM) as serializing to a string first. A failed or interrupted
/// write never leaves a truncated artifact behind and never destroys the previous one.
/// </summary>
public static class JsonArtifactFile
{
    public static async Task WriteAsync<T>(
        string path,
        T value,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporaryPath = fullPath + $".tmp-{Guid.NewGuid():N}";
        try
        {
            await using (var stream = new FileStream(
                             temporaryPath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             bufferSize: 1 << 16,
                             FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await JsonSerializer.SerializeAsync(stream, value, options, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            File.Move(temporaryPath, fullPath, overwrite: true);
        }
        catch
        {
            TryDelete(temporaryPath);
            throw;
        }
    }

    public static async Task<T?> ReadAsync<T>(
        string path,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 1 << 16,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        return await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken);
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // Best-effort cleanup of an incomplete temporary artifact.
        }
    }
}

/// <summary>
/// Records artifact writes that are useful but not required for the run's product output. A failure is reported
/// instead of aborting the run, so the expensive analysis that preceded it is not lost.
/// </summary>
public sealed class ArtifactWriteLog
{
    private readonly List<ArtifactWriteResult> _results = [];

    public IReadOnlyList<ArtifactWriteResult> Results => _results;

    public IEnumerable<ArtifactWriteResult> Failures => _results.Where(result => !result.Succeeded);

    public async Task<bool> TryWriteJsonAsync<T>(
        string name,
        string path,
        T value,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await JsonArtifactFile.WriteAsync(path, value, options, cancellationToken);
            _results.Add(new ArtifactWriteResult(name, path, true, null));
            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _results.Add(new ArtifactWriteResult(name, path, false, exception.Message));
            return false;
        }
    }

    public void Record(string name, string path, bool succeeded, string? error = null) =>
        _results.Add(new ArtifactWriteResult(name, path, succeeded, error));
}

public sealed record ArtifactWriteResult(string Name, string Path, bool Succeeded, string? Error);
