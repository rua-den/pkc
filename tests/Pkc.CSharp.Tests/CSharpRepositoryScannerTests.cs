using Pkc.CSharp;
using Xunit;

namespace Pkc.CSharp.Tests;

public sealed class CSharpRepositoryScannerTests
{
    [Fact]
    public async Task ScanAsync_extracts_symbols_endpoints_and_invocations()
    {
        var root = Path.Combine(Path.GetTempPath(), "pkc-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "WorkPlay.cs"), Source);

            var scanner = new CSharpRepositoryScanner();
            var result = await scanner.ScanAsync(root);

            Assert.Contains(result.Facts, fact => fact.Kind == "class" && fact.Name == "WorkPlayController");
            Assert.Contains(result.Facts, fact =>
                fact.Kind == "endpoint" &&
                fact.Name == "Complete" &&
                fact.Metadata.TryGetValue("httpMethod", out var method) &&
                method == "POST");
            Assert.Contains(result.Facts, fact => fact.Kind == "enum" && fact.Name == "WorkPlayStatus");
            Assert.Contains(result.Facts, fact => fact.Kind == "enum-member" && fact.Name == "Completed");
            Assert.Contains(result.Relations, relation =>
                relation.Kind == "invokes-syntax" &&
                relation.Target == "_service.ChangeStatus");
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

        public sealed class WorkPlayService
        {
            public void ChangeStatus(int id, WorkPlayStatus status) { }
        }

        [Route("api/workplays")]
        public sealed class WorkPlayController
        {
            private readonly WorkPlayService _service = new();

            [HttpPost("{id}/complete")]
            [Authorize("ManageWorkPlay")]
            public void Complete(int id)
            {
                _service.ChangeStatus(id, WorkPlayStatus.Completed);
            }
        }
        """;
}
