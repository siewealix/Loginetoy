using PcCleaner.Core.Abstractions;
using PcCleaner.Core.Models;

namespace PcCleaner.Core.Services;

public sealed class CleanExecutor
{
    private readonly IFileSystem _fileSystem;

    public CleanExecutor(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public CleanResult DeleteFiles(string category, IReadOnlyList<ScanItem> items, bool dryRun)
    {
        var errors = new List<CleanError>();
        var deleted = 0;
        long bytesFreed = 0;

        foreach (var item in items)
        {
            if (dryRun)
            {
                bytesFreed += item.SizeBytes;
                continue;
            }

            try
            {
                if (_fileSystem.FileExists(item.Path))
                {
                    _fileSystem.DeleteFile(item.Path);
                    deleted++;
                    bytesFreed += item.SizeBytes;
                }
            }
            catch (Exception ex)
            {
                errors.Add(new CleanError(item.Path, ex.Message));
            }
        }

        return new CleanResult(category, dryRun, deleted, bytesFreed, errors);
    }
}
