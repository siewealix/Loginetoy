namespace PcCleaner.Core.Abstractions;

public interface IRecycleBinService
{
    long GetRecycleBinSize();
    void EmptyRecycleBin();
}
