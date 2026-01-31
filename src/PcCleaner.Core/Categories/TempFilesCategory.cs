using PcCleaner.Core.Abstractions;
using PcCleaner.Core.Models;
using PcCleaner.Core.Services;

namespace PcCleaner.Core.Categories;

public sealed class TempFilesCategory : ICleanerCategory
{
    private readonly FileScanner _scanner;
    private readonly CleanExecutor _executor;

    public TempFilesCategory(FileScanner scanner, CleanExecutor executor)
    {
        _scanner = scanner;
        _executor = executor;
    }

    public string Name => "Temporaire utilisateur";
    public string Description => "Fichiers temporaires de l'utilisateur (TEMP, AppData\\Local\\Temp).";
    public bool IsEnabledByDefault => true;
    public bool RequiresAdmin => false;

    public Task<IReadOnlyList<ScanItem>> ScanAsync(CancellationToken cancellationToken)
    {
        var items = new List<ScanItem>();
        var tempPath = Path.GetTempPath();
        items.AddRange(_scanner.ScanFolder(tempPath, Name, "Fichier temporaire utilisateur", SafeLevel.Safe));

        var localTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");
        items.AddRange(_scanner.ScanFolder(localTemp, Name, "Fichier temporaire local", SafeLevel.Safe));

        return Task.FromResult<IReadOnlyList<ScanItem>>(items);
    }

    public Task<CleanResult> CleanAsync(IReadOnlyList<ScanItem> items, bool dryRun, CancellationToken cancellationToken)
    {
        return Task.FromResult(_executor.DeleteFiles(Name, items, dryRun));
    }
}
