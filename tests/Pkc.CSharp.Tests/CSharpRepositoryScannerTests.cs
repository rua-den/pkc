using Pkc.CSharp;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class CSharpRepositoryScannerTests
{
    [Fact]
    public async Task ScanAsync_extracts_behavior_routes_permissions_and_semantic_calls()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "WorkPlay.cs"), Source);

            var scanner = new CSharpRepositoryScanner();
            var result = await scanner.ScanAsync(root);

            Assert.Equal("0.1.1", result.SchemaVersion);
            Assert.Contains(result.Facts, fact => fact.Kind == "class" && fact.Name == "WorkPlayController");
            Assert.Contains(result.Facts, fact =>
                fact.Kind == "endpoint" &&
                fact.Name == "Complete" &&
                fact.Metadata.TryGetValue("httpMethod", out var method) && method == "POST" &&
                fact.Metadata.TryGetValue("fullRoute", out var route) && route == "api/workplays/{id}/complete" &&
                fact.Metadata.TryGetValue("authorizationPolicies", out var policy) && policy == "ManageWorkPlay");

            Assert.Contains(result.Facts, fact =>
                fact.Kind == "condition" &&
                fact.Metadata.TryGetValue("expression", out var expression) &&
                expression.Contains("WorkPlayStatus.Completed", StringComparison.Ordinal));

            Assert.Contains(result.Facts, fact =>
                fact.Kind == "throw" &&
                fact.Metadata.TryGetValue("exceptionType", out var exceptionType) &&
                exceptionType == "InvalidOperationException");

            Assert.Contains(result.Facts, fact =>
                fact.Kind == "mutation" &&
                fact.Metadata.TryGetValue("target", out var target) && target == "Status" &&
                fact.Metadata.TryGetValue("stateMutationCandidate", out var candidate) && candidate == "true");

            Assert.Contains(result.Relations, relation =>
                relation.Kind == "invokes" &&
                relation.Target.EndsWith("WorkPlayService.ChangeStatus", StringComparison.Ordinal));

            Assert.Contains(result.Relations, relation =>
                relation.Kind == "message-publication-candidate" &&
                relation.Target.EndsWith("WorkPlayService.PublishStatusChanged", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private const string Source = """
        namespace Demo.WorkPlay;

        public enum WorkPlayStatus
        {
            Draft,
            Active,
            Completed
        }

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
                {
                    throw new InvalidOperationException("Completed WorkPlay cannot change status.");
                }

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
