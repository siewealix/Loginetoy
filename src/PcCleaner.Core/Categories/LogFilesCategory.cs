using PcCleaner.Core.Abstractions;
using PcCleaner.Core.Models;
using PcCleaner.Core.Services;

namespace PcCleaner.Core.Categories;

public sealed class LogFilesCategory : ICleanerCategory
{
    private readonly CleanerSettings _settings;
    private readonly IFileSystem _fileSystem;
    private readonly FileScanner _scanner;
    private readonly CleanExecutor _executor;

    public LogFilesCategory(CleanerSettings settings, IFileSystem fileSystem, FileScanner scanner, CleanExecutor executor)
    {
        _settings = settings;
        _fileSystem = fileSystem;
        _scanner = scanner;
        _executor = executor;
    }

    public string Name => "Logs applicatifs";
    public string Description => "Fichiers .log dans les dossiers configurés.";
    public bool IsEnabledByDefault => true;
    public bool RequiresAdmin => false;

    public Task<IReadOnlyList<ScanItem>> ScanAsync(CancellationToken cancellationToken)
    {
        var items = new List<ScanItem>();
        foreach (var folder in _settings.LogFolders)
        {
            if (!_fileSystem.DirectoryExists(folder))
            {
                continue;
            }

            var files = _fileSystem.EnumerateFiles(folder, "*.log", SearchOption.AllDirectories);
            items.AddRange(_scanner.ScanFiles(files, Name, "Fichiers log configurés", SafeLevel.Safe));
        }

        return Task.FromResult<IReadOnlyList<ScanItem>>(items);
    }

    public Task<CleanResult> CleanAsync(IReadOnlyList<ScanItem> items, bool dryRun, CancellationToken cancellationToken)
    {
        return Task.FromResult(_executor.DeleteFiles(Name, items, dryRun));
    }
}
