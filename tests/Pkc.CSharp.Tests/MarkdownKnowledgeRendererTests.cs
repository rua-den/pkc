using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class MarkdownKnowledgeRendererTests
{
    [Fact]
    public async Task Render_produces_grounded_portable_workflow_markdown()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-markdown-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "WorkPlay.cs"), Source);

            var facts = await new CSharpRepositoryScanner().ScanAsync(root);
            var candidate = Assert.Single(new FeatureCandidateBuilder().Build(facts).Candidates);
            var knowledge = await new GroundedKnowledgeSynthesizer().SynthesizeAsync(candidate);
            var renderer = new MarkdownKnowledgeRenderer();
            var markdown = renderer.Render(knowledge);

            Assert.Equal("knowledge/workflows/workplay/complete.md", renderer.GetRelativePath(knowledge));
            Assert.Contains("type: \"workflow\"", markdown, StringComparison.Ordinal);
            Assert.Contains("ManageWorkPlay", markdown, StringComparison.Ordinal);
            Assert.Contains("workPlay.Status == WorkPlayStatus.Completed", markdown, StringComparison.Ordinal);
            Assert.Contains("Completed WorkPlay cannot change status", markdown, StringComparison.Ordinal);
            Assert.Contains("Sets `Status` to `status`", markdown, StringComparison.Ordinal);
            Assert.Contains("PublishStatusChanged", markdown, StringComparison.Ordinal);
            Assert.Contains("Frontend/UI entry point", markdown, StringComparison.Ordinal);
            Assert.Contains("authority: \"code-observed\"", markdown, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private const string Source = """
        namespace Demo;

        public enum WorkPlayStatus { Draft, Active, Completed }

        public sealed class WorkPlay
        {
            public WorkPlayStatus Status { get; private set; }
            public void SetStatus(WorkPlayStatus status) => Status = status;
        }

        public sealed class WorkPlayService
        {
            public void ChangeStatus(WorkPlay workPlay, WorkPlayStatus status)
            {
                if (workPlay.Status == WorkPlayStatus.Completed)
                    throw new InvalidOperationException("Completed WorkPlay cannot change status.");

                workPlay.SetStatus(status);
                PublishStatusChanged(workPlay);
            }

            private static void PublishStatusChanged(WorkPlay workPlay) { }
        }

        [Route("api/workplays")]
        public sealed class WorkPlayController
        {
            private readonly WorkPlayService _service = new();

            [HttpPost("{id}/complete")]
            [Authorize(Policy = "ManageWorkPlay")]
            public void Complete(int id)
            {
                var workPlay = new WorkPlay();
                _service.ChangeStatus(workPlay, WorkPlayStatus.Completed);
            }
        }
        """;
}
