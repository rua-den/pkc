using System.Text;

namespace Pkc.Knowledge;

public sealed class PortableKnowledgePackRenderer
{
    public const string InstructionsRelativePath = "knowledge/AI_INSTRUCTIONS.md";
    public const string BundleFileName = "PKC_KNOWLEDGE.md";
    public const string ArchiveFileName = "PKC_KNOWLEDGE.zip";

    public string RenderInstructions(string? sourceRepositoryLabel = null)
    {
        var builder = new StringBuilder();
        builder.AppendLine("---");
        builder.AppendLine("id: \"pkc:ai-instructions\"");
        builder.AppendLine("title: \"How to use this PKC knowledge pack\"");
        builder.AppendLine("type: \"ai-instructions\"");
        if (!string.IsNullOrWhiteSpace(sourceRepositoryLabel))
        {
            builder.AppendLine($"source_repository_label: {Yaml(sourceRepositoryLabel)}");
        }
        builder.AppendLine("generated: true");
        builder.AppendLine("---");
        builder.AppendLine();
        builder.AppendLine("# How to use this PKC knowledge pack");
        builder.AppendLine();
        builder.AppendLine("This pack contains portable product/system knowledge compiled from grounded evidence.");
        if (!string.IsNullOrWhiteSpace(sourceRepositoryLabel))
        {
            builder.AppendLine($"The source repository label is `{sourceRepositoryLabel}`. Treat this as a source identifier, not as proof of an approved product name.");
        }
        builder.AppendLine();
        builder.AppendLine("## Reading order");
        builder.AppendLine();
        builder.AppendLine("1. Start with `index.md` to understand the observed capability surface and current knowledge boundary.");
        builder.AppendLine("2. Read the relevant file under `features/` for product/system capability questions.");
        builder.AppendLine("3. Read linked files under `workflows/` when the question needs operation-level validations, failures, state changes, side effects, integrations, backend flow or source evidence.");
        builder.AppendLine();
        builder.AppendLine("## Authority and uncertainty rules");
        builder.AppendLine();
        builder.AppendLine("- Treat `authority: code-observed` as observed implementation behavior, not approved business intent.");
        builder.AppendLine("- Respect `Important unknowns`; do not invent missing UI, runtime, delivery-history or product-intent evidence.");
        builder.AppendLine("- Distinguish production behavior from conditional or development-only behavior when the knowledge says a flow is conditional.");
        builder.AppendLine("- Prefer an explicit unknown over an unsupported inference.");
        builder.AppendLine("- Use source/evidence sections for traceability, but ordinary product questions should be answerable without the original source repository.");
        builder.AppendLine("- When practical, name the relevant knowledge file when explaining an important claim.");
        builder.AppendLine();
        builder.AppendLine("## Important boundary");
        builder.AppendLine();
        builder.AppendLine("This pack may combine multiple evidence qualities. Do not silently upgrade fallback or incomplete evidence into a stronger claim than the generated knowledge supports.");
        return builder.ToString();
    }

    public string RenderBundle(
        IReadOnlyDictionary<string, string> canonicalFiles,
        string? sourceRepositoryLabel = null)
    {
        ArgumentNullException.ThrowIfNull(canonicalFiles);

        var requiredInstructions = canonicalFiles.TryGetValue(InstructionsRelativePath, out var instructions)
            ? instructions
            : RenderInstructions(sourceRepositoryLabel);

        var builder = new StringBuilder();
        builder.AppendLine("---");
        builder.AppendLine("id: \"pkc:portable-knowledge-bundle\"");
        builder.AppendLine("title: \"PKC Portable Knowledge Bundle\"");
        builder.AppendLine("type: \"knowledge-bundle\"");
        if (!string.IsNullOrWhiteSpace(sourceRepositoryLabel))
        {
            builder.AppendLine($"source_repository_label: {Yaml(sourceRepositoryLabel)}");
        }
        builder.AppendLine("generated: true");
        builder.AppendLine("---");
        builder.AppendLine();
        builder.AppendLine("# PKC Portable Knowledge Bundle");
        builder.AppendLine();
        builder.AppendLine("This single-file transport contains the canonical `knowledge/` pack. File boundaries are preserved below so an AI can reason from the same hierarchy as the multi-file form.");
        if (!string.IsNullOrWhiteSpace(sourceRepositoryLabel))
        {
            builder.AppendLine($"Source repository label: `{sourceRepositoryLabel}` (identifier only; not proof of an approved product name).");
        }
        builder.AppendLine();

        AppendEmbeddedFile(builder, InstructionsRelativePath, requiredInstructions);

        foreach (var path in OrderedPaths(canonicalFiles.Keys))
        {
            if (string.Equals(path, InstructionsRelativePath, StringComparison.Ordinal))
            {
                continue;
            }

            AppendEmbeddedFile(builder, path, canonicalFiles[path]);
        }

        return builder.ToString();
    }

    private static IEnumerable<string> OrderedPaths(IEnumerable<string> paths) =>
        paths
            .Where(path => path.StartsWith("knowledge/", StringComparison.Ordinal))
            .OrderBy(PathRank)
            .ThenBy(path => path, StringComparer.Ordinal);

    private static int PathRank(string path)
    {
        if (string.Equals(path, "knowledge/index.md", StringComparison.Ordinal)) return 0;
        if (path.StartsWith("knowledge/features/", StringComparison.Ordinal)) return 1;
        if (path.StartsWith("knowledge/workflows/", StringComparison.Ordinal)) return 2;
        return 3;
    }

    private static void AppendEmbeddedFile(StringBuilder builder, string path, string content)
    {
        builder.AppendLine("---");
        builder.AppendLine();
        builder.AppendLine($"<!-- PKC_FILE: {path} -->");
        builder.AppendLine($"## Embedded file: `{path}`");
        builder.AppendLine();
        builder.AppendLine(content.TrimEnd());
        builder.AppendLine();
    }

    private static string Yaml(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}
