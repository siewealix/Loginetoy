namespace PcCleaner.Core.Models;

public sealed class CleanerSettings
{
    public List<string> CustomFolders { get; init; } = new();
    public List<string> LogFolders { get; init; } = new();
    public List<string> Exclusions { get; init; } = new();
}
