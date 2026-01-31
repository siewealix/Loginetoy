using System.Text.RegularExpressions;

namespace PcCleaner.Core.Security;

public sealed class ExclusionPolicy
{
    private readonly List<string> _paths;
    private readonly List<Regex> _patterns;

    public ExclusionPolicy(IEnumerable<string> exclusions)
    {
        _paths = new List<string>();
        _patterns = new List<Regex>();

        foreach (var exclusion in exclusions)
        {
            if (string.IsNullOrWhiteSpace(exclusion))
            {
                continue;
            }

            if (exclusion.Contains('*') || exclusion.Contains('?'))
            {
                var regex = new Regex("^" + Regex.Escape(exclusion.Trim()).Replace("\\*", ".*").Replace("\\?", ".") + "$",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled);
                _patterns.Add(regex);
            }
            else
            {
                _paths.Add(NormalizePath(exclusion));
            }
        }
    }

    public bool IsExcluded(string path)
    {
        var normalized = NormalizePath(path);
        if (_paths.Any(p => normalized.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        var fileName = Path.GetFileName(path);
        return _patterns.Any(pattern => pattern.IsMatch(path) || pattern.IsMatch(fileName));
    }

    private static string NormalizePath(string path)
    {
        var normalized = Path.GetFullPath(path.Trim());
        return normalized.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
    }
}
