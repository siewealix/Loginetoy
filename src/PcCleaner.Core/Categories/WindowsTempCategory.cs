using PcCleaner.Core.Abstractions;
using PcCleaner.Core.Models;
using PcCleaner.Core.Services;

namespace PcCleaner.Core.Categories;

public sealed class WindowsTempCategory : ICleanerCategory
{
    private readonly FileScanner _scanner;
    private readonly CleanExecutor _executor;

    public WindowsTempCategory(FileScanner scanner, CleanExecutor executor)
    {
        _scanner = scanner;
        _executor = executor;
    }

    public string Name => "Temporaire Windows";
    public string Description => "Fichiers temporaires Windows (C:\\Windows\\Temp) - nécessite admin.";
    public bool IsEnabledByDefault => false;
    public bool RequiresAdmin => true;

    public Task<IReadOnlyList<ScanItem>> ScanAsync(CancellationToken cancellationToken)
    {
        var windowsTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
        var items = _scanner.ScanFolder(windowsTemp, Name, "Fichier temporaire Windows", SafeLevel.RequiresAdmin);
        return Task.FromResult<IReadOnlyList<ScanItem>>(items);
    }

    public Task<CleanResult> CleanAsync(IReadOnlyList<ScanItem> items, bool dryRun, CancellationToken cancellationToken)
    {
        return Task.FromResult(_executor.DeleteFiles(Name, items, dryRun));
    }
}
