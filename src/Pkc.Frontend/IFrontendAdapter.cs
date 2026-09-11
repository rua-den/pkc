using Pkc.Core;

namespace Pkc.Frontend;

public interface IFrontendAdapter
{
    string Id { get; }

    bool CanHandle(string repositoryPath);

    Task<FactDocument> ScanAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default);
}
