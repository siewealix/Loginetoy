using PcCleaner.Core.Abstractions;
using PcCleaner.Core.Models;

namespace PcCleaner.Core.Categories;

public sealed class RecycleBinCategory : ICleanerCategory
{
    private readonly IRecycleBinService _recycleBinService;

    public RecycleBinCategory(IRecycleBinService recycleBinService)
    {
        _recycleBinService = recycleBinService;
    }

    public string Name => "Corbeille";
    public string Description => "Estimation et nettoyage de la corbeille Windows.";
    public bool IsEnabledByDefault => false;
    public bool RequiresAdmin => false;

    public Task<IReadOnlyList<ScanItem>> ScanAsync(CancellationToken cancellationToken)
    {
        var size = _recycleBinService.GetRecycleBinSize();
        var item = new ScanItem("Corbeille", size, Name, "Éléments dans la corbeille", SafeLevel.Caution);
        return Task.FromResult<IReadOnlyList<ScanItem>>(new List<ScanItem> { item });
    }

    public Task<CleanResult> CleanAsync(IReadOnlyList<ScanItem> items, bool dryRun, CancellationToken cancellationToken)
    {
        if (!dryRun)
        {
            _recycleBinService.EmptyRecycleBin();
        }

        var bytes = items.Sum(item => item.SizeBytes);
        return Task.FromResult(new CleanResult(Name, dryRun, dryRun ? 0 : items.Count, bytes, Array.Empty<CleanError>()));
    }
}
