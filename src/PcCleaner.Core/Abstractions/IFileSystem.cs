namespace PcCleaner.Core.Abstractions;

public interface IFileSystem
{
    bool DirectoryExists(string path);
    bool FileExists(string path);
    IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);
    IEnumerable<string> EnumerateDirectories(string path);
    long GetFileSize(string path);
    bool IsReparsePoint(string path);
    void DeleteFile(string path);
    void DeleteDirectory(string path, bool recursive);
}
