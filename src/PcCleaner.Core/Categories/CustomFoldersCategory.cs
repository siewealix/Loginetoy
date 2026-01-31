using PcCleaner.Core.Abstractions;
using PcCleaner.Core.Models;
using PcCleaner.Core.Services;

namespace PcCleaner.Core.Categories;

public sealed class CustomFoldersCategory : ICleanerCategory
{
    private readonly CleanerSettings _settings;
    private readonly FileScanner _scanner;
    private readonly CleanExecutor _executor;

    public CustomFoldersCategory(CleanerSettings settings, FileScanner scanner, CleanExecutor executor)
    {
        _settings = settings;
        _scanner = scanner;
        _executor = executor;
    }

    public string Name => "Dossiers personnalisés";
    public string Description => "Dossiers configurés par l'utilisateur (liste blanche).";
    public bool IsEnabledByDefault => false;
    public bool RequiresAdmin => false;

    public Task<IReadOnlyList<ScanItem>> ScanAsync(CancellationToken cancellationToken)
    {
        var items = new List<ScanItem>();
        foreach (var folder in _settings.CustomFolders)
        {
            items.AddRange(_scanner.ScanFolder(folder, Name, "Dossier personnalisé", SafeLevel.Caution));
        }

        return Task.FromResult<IReadOnlyList<ScanItem>>(items);
    }

    public Task<CleanResult> CleanAsync(IReadOnlyList<ScanItem> items, bool dryRun, CancellationToken cancellationToken)
    {
        return Task.FromResult(_executor.DeleteFiles(Name, items, dryRun));
    }
}
