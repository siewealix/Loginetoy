using PcCleaner.Core.Models;

namespace PcCleaner.Core.Abstractions;

public interface ICleanerCategory
{
    string Name { get; }
    string Description { get; }
    bool IsEnabledByDefault { get; }
    bool RequiresAdmin { get; }

    Task<IReadOnlyList<ScanItem>> ScanAsync(CancellationToken cancellationToken);
    Task<CleanResult> CleanAsync(IReadOnlyList<ScanItem> items, bool dryRun, CancellationToken cancellationToken);
}
