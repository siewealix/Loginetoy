namespace PcCleaner.Core.Models;

public enum SafeLevel
{
    Safe,
    Caution,
    RequiresAdmin
}

public sealed record ScanItem(
    string Path,
    long SizeBytes,
    string Category,
    string Reason,
    SafeLevel SafeLevel);
