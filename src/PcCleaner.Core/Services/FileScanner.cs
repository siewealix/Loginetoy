using PcCleaner.Core.Abstractions;
using PcCleaner.Core.Models;
using PcCleaner.Core.Security;

namespace PcCleaner.Core.Services;

public sealed class FileScanner
{
    private readonly IFileSystem _fileSystem;
    private readonly SafePathPolicy _policy;
    private readonly ExclusionPolicy _exclusions;

    public FileScanner(IFileSystem fileSystem, SafePathPolicy policy, ExclusionPolicy exclusions)
    {
        _fileSystem = fileSystem;
        _policy = policy;
        _exclusions = exclusions;
    }

    public IReadOnlyList<ScanItem> ScanFolder(string folderPath, string category, string reason, SafeLevel safeLevel)
    {
        var items = new List<ScanItem>();
        if (!_fileSystem.DirectoryExists(folderPath) || !_policy.IsAllowed(folderPath))
        {
            return items;
        }

        try
        {
            foreach (var file in _fileSystem.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories))
            {
                if (_fileSystem.IsReparsePoint(file))
                {
                    continue;
                }

                if (!_policy.IsAllowed(file) || _exclusions.IsExcluded(file))
                {
                    continue;
                }

                var size = _fileSystem.GetFileSize(file);
                items.Add(new ScanItem(file, size, category, reason, safeLevel));
            }
        }
        catch (UnauthorizedAccessException)
        {
            return items;
        }
        catch (IOException)
        {
            return items;
        }

        return items;
    }

    public IReadOnlyList<ScanItem> ScanFiles(IEnumerable<string> files, string category, string reason, SafeLevel safeLevel)
    {
        var items = new List<ScanItem>();
        foreach (var file in files)
        {
            try
            {
                if (!_fileSystem.FileExists(file) || _fileSystem.IsReparsePoint(file))
                {
                    continue;
                }

                if (!_policy.IsAllowed(file) || _exclusions.IsExcluded(file))
                {
                    continue;
                }

                var size = _fileSystem.GetFileSize(file);
                items.Add(new ScanItem(file, size, category, reason, safeLevel));
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            catch (IOException)
            {
                continue;
            }
        }

        return items;
    }
}
