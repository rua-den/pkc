using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace Pkc.Core.Discovery;

internal static class DiscoveryPaths
{
    public const string Root = ".";

    public static string Normalize(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').Trim('/');
        while (normalized.StartsWith("./", StringComparison.Ordinal))
        {
            normalized = normalized[2..];
        }

        return normalized.Length == 0 ? Root : normalized;
    }

    public static string Join(string directory, string name) =>
        directory == Root ? name : directory + "/" + name;

    public static string Parent(string path)
    {
        var index = path.LastIndexOf('/');
        return index < 0 ? Root : path[..index];
    }

    public static bool IsUnder(string path, string area) =>
        area == Root ||
        string.Equals(path, area, StringComparison.Ordinal) ||
        path.StartsWith(area + "/", StringComparison.Ordinal);

    /// <summary>Returns <paramref name="path"/>, then each ancestor directory, ending with the root.</summary>
    public static IEnumerable<string> SelfAndAncestors(string path)
    {
        var current = path;
        while (current != Root)
        {
            yield return current;
            current = Parent(current);
        }

        yield return Root;
    }

    public static string RelativeTo(string area, string path) =>
        area == Root ? path : path.Length == area.Length ? string.Empty : path[(area.Length + 1)..];

    /// <summary>
    /// Resolves <paramref name="relative"/> against repository-relative <paramref name="baseDirectory"/>.
    /// Returns null when the result is absolute, rooted elsewhere, or escapes the repository.
    /// </summary>
    public static string? Resolve(string baseDirectory, string relative)
    {
        var value = relative.Replace('\\', '/').Trim();
        if (value.Length == 0 || value.StartsWith('/') || Path.IsPathRooted(value) || value.Contains('$'))
        {
            return null;
        }

        var segments = baseDirectory == Root
            ? new List<string>()
            : baseDirectory.Split('/').ToList();
        foreach (var segment in value.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment == ".")
            {
                continue;
            }

            if (segment == "..")
            {
                if (segments.Count == 0)
                {
                    return null;
                }

                segments.RemoveAt(segments.Count - 1);
                continue;
            }

            segments.Add(segment);
        }

        return segments.Count == 0 ? Root : string.Join('/', segments);
    }
}

/// <summary>Minimal glob matcher: <c>**</c> spans directories, <c>*</c> and <c>?</c> stay within one segment.</summary>
internal static class DiscoveryGlob
{
    private static readonly ConcurrentDictionary<string, Regex> Cache = new(StringComparer.Ordinal);

    public static bool IsMatch(string pattern, string relativePath) =>
        Cache.GetOrAdd(pattern, Compile).IsMatch(relativePath);

    private static Regex Compile(string pattern)
    {
        var builder = new StringBuilder("^");
        for (var index = 0; index < pattern.Length; index++)
        {
            var character = pattern[index];
            if (character == '*' && index + 1 < pattern.Length && pattern[index + 1] == '*')
            {
                var followedBySeparator = index + 2 < pattern.Length && pattern[index + 2] == '/';
                builder.Append(followedBySeparator ? "(?:.*/)?" : ".*");
                index += followedBySeparator ? 2 : 1;
            }
            else if (character == '*')
            {
                builder.Append("[^/]*");
            }
            else if (character == '?')
            {
                builder.Append("[^/]");
            }
            else
            {
                builder.Append(Regex.Escape(character.ToString()));
            }
        }

        builder.Append('$');
        return new Regex(builder.ToString(), RegexOptions.CultureInvariant);
    }
}
