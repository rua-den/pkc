using Pkc.CSharp;
using Pkc.Knowledge;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class FeatureCandidateBuilderTests
{
    [Fact]
    public async Task Build_groups_endpoint_and_transitive_behavior_evidence()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-candidate-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "WorkPlay.cs"), Source);

            var facts = await new CSharpRepositoryScanner().ScanAsync(root);
            var result = new FeatureCandidateBuilder().Build(facts);

            var candidate = Assert.Single(result.Candidates);
            Assert.Equal("WorkPlay", candidate.Area);
            Assert.Equal("WorkPlay Complete", candidate.Name);
            Assert.Contains(candidate.Facts, fact => fact.Kind == "endpoint" && fact.Name == "Complete");
            Assert.Contains(candidate.Facts, fact => fact.Kind == "method" && fact.Name == "ChangeStatus");
            Assert.Contains(candidate.Facts, fact => fact.Kind == "condition");
            Assert.Contains(candidate.Facts, fact => fact.Kind == "throw");
            Assert.Contains(candidate.Facts, fact => fact.Kind == "mutation" && fact.Metadata["target"] == "Status");
            Assert.Contains(candidate.Relations, relation => relation.Kind == "message-publication-candidate");
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
