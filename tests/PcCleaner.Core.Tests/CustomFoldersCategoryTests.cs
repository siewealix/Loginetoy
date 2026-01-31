using PcCleaner.Core.Abstractions;
using PcCleaner.Core.Categories;
using PcCleaner.Core.Models;
using PcCleaner.Core.Security;
using PcCleaner.Core.Services;
using Xunit;

namespace PcCleaner.Core.Tests;

public class CustomFoldersCategoryTests
{
    [Fact]
    public async Task Scans_Custom_Folder_Files()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        var filePath = Path.Combine(tempRoot, "sample.tmp");
        await File.WriteAllTextAsync(filePath, "data");

        var settings = new CleanerSettings
        {
            CustomFolders = new List<string> { tempRoot }
        };

        var fileSystem = new TestFileSystem();
        var safePolicy = SafePathPolicy.CreateDefault(settings.CustomFolders);
        var exclusions = new ExclusionPolicy(Array.Empty<string>());
        var scanner = new FileScanner(fileSystem, safePolicy, exclusions);
        var executor = new CleanExecutor(fileSystem);
        var category = new CustomFoldersCategory(settings, scanner, executor);

        var items = await category.ScanAsync(CancellationToken.None);

        Assert.Single(items);
        Assert.Equal(filePath, items[0].Path);

        Directory.Delete(tempRoot, true);
    }

    private sealed class TestFileSystem : IFileSystem
    {
        public bool DirectoryExists(string path) => Directory.Exists(path);
        public bool FileExists(string path) => File.Exists(path);
        public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
            => Directory.EnumerateFiles(path, searchPattern, searchOption);
        public IEnumerable<string> EnumerateDirectories(string path) => Directory.EnumerateDirectories(path);
        public long GetFileSize(string path) => new FileInfo(path).Length;
        public bool IsReparsePoint(string path)
        {
            var attributes = File.GetAttributes(path);
            return attributes.HasFlag(FileAttributes.ReparsePoint);
        }
        public void DeleteFile(string path) => File.Delete(path);
        public void DeleteDirectory(string path, bool recursive) => Directory.Delete(path, recursive);
    }
}
