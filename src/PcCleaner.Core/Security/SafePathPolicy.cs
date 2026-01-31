using System.Runtime.InteropServices;

namespace PcCleaner.Core.Security;

public sealed class SafePathPolicy
{
    private readonly HashSet<string> _allowedRoots;
    private readonly HashSet<string> _blockedRoots;

    public SafePathPolicy(IEnumerable<string> allowedRoots, IEnumerable<string> blockedRoots)
    {
        _allowedRoots = new HashSet<string>(NormalizePaths(allowedRoots));
        _blockedRoots = new HashSet<string>(NormalizePaths(blockedRoots));
    }

    public bool IsAllowed(string path)
    {
        var normalized = NormalizePath(path);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return false;
        }

        var isAllowed = _allowedRoots.Any(root => normalized.StartsWith(root, StringComparison.OrdinalIgnoreCase));
        if (!isAllowed)
        {
            return false;
        }

        if (_blockedRoots.Any(root => normalized.StartsWith(root, StringComparison.OrdinalIgnoreCase)))
        {
            return _allowedRoots.Any(root => normalized.StartsWith(root, StringComparison.OrdinalIgnoreCase));
        }

        return true;
    }

    public static SafePathPolicy CreateDefault(IEnumerable<string> allowedRoots)
    {
        var blockedRoots = GetDefaultBlockedRoots();
        return new SafePathPolicy(allowedRoots, blockedRoots);
    }

    private static IEnumerable<string> NormalizePaths(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            var normalized = NormalizePath(path);
            if (!string.IsNullOrWhiteSpace(normalized))
            {
                yield return normalized;
            }
        }
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var trimmed = path.Trim();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            trimmed = trimmed.Replace('/', '\');
        }

        return Path.GetFullPath(trimmed).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
               Path.DirectorySeparatorChar;
    }

    private static IEnumerable<string> GetDefaultBlockedRoots()
    {
        var blocked = new List<string>();
        var systemRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        if (!string.IsNullOrWhiteSpace(systemRoot))
        {
            blocked.Add(systemRoot);
        }

        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        if (!string.IsNullOrWhiteSpace(programFiles))
        {
            blocked.Add(programFiles);
        }

        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        if (!string.IsNullOrWhiteSpace(programFilesX86))
        {
            blocked.Add(programFilesX86);
        }

        var system32 = Path.Combine(systemRoot, "System32");
        if (!string.IsNullOrWhiteSpace(system32))
        {
            blocked.Add(system32);
        }

        return blocked;
    }
}
