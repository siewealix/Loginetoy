namespace PcCleaner.Core.Models;

public sealed record CleanResult(
    string Category,
    bool IsDryRun,
    int ItemsDeleted,
    long BytesFreed,
    IReadOnlyList<CleanError> Errors);

public sealed record CleanError(string Path, string Message);
