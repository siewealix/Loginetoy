using PcCleaner.Core.Abstractions;
using PcCleaner.Core.Models;
using PcCleaner.Core.Services;

namespace PcCleaner.Core.Categories;

public sealed class ThumbnailCacheCategory : ICleanerCategory
{
    private readonly IFileSystem _fileSystem;
    private readonly FileScanner _scanner;
    private readonly CleanExecutor _executor;

    public ThumbnailCacheCategory(IFileSystem fileSystem, FileScanner scanner, CleanExecutor executor)
    {
        _fileSystem = fileSystem;
        _scanner = scanner;
        _executor = executor;
    }

    public string Name => "Cache des miniatures";
    public string Description => "Cache des miniatures Windows (thumbcache*.db).";
    public bool IsEnabledByDefault => true;
    public bool RequiresAdmin => false;

    public Task<IReadOnlyList<ScanItem>> ScanAsync(CancellationToken cancellationToken)
    {
        var explorerPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Microsoft",
            "Windows",
            "Explorer");

        if (!_fileSystem.DirectoryExists(explorerPath))
        {
            return Task.FromResult<IReadOnlyList<ScanItem>>(Array.Empty<ScanItem>());
        }

        var files = _fileSystem.EnumerateFiles(explorerPath, "thumbcache*.db", SearchOption.TopDirectoryOnly);
        var items = _scanner.ScanFiles(files, Name, "Cache de miniatures", SafeLevel.Caution);
        return Task.FromResult<IReadOnlyList<ScanItem>>(items);
    }

    public Task<CleanResult> CleanAsync(IReadOnlyList<ScanItem> items, bool dryRun, CancellationToken cancellationToken)
    {
        return Task.FromResult(_executor.DeleteFiles(Name, items, dryRun));
    }
}
