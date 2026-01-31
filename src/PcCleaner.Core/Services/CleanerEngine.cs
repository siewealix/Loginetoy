using Microsoft.Extensions.Logging;
using PcCleaner.Core.Abstractions;
using PcCleaner.Core.Models;

namespace PcCleaner.Core.Services;

public sealed class CleanerEngine
{
    private readonly IReadOnlyList<ICleanerCategory> _categories;
    private readonly IAdminChecker _adminChecker;
    private readonly ILogger<CleanerEngine> _logger;

    public CleanerEngine(IEnumerable<ICleanerCategory> categories, IAdminChecker adminChecker, ILogger<CleanerEngine> logger)
    {
        _categories = categories.ToList();
        _adminChecker = adminChecker;
        _logger = logger;
    }

    public IReadOnlyList<ICleanerCategory> Categories => _categories;

    public async Task<IReadOnlyList<ScanItem>> ScanAsync(IEnumerable<ICleanerCategory> selectedCategories, CancellationToken cancellationToken)
    {
        var results = new List<ScanItem>();
        var isAdmin = _adminChecker.IsRunningAsAdmin();

        foreach (var category in selectedCategories)
        {
            if (category.RequiresAdmin && !isAdmin)
            {
                _logger.LogWarning("Skipping {Category} because admin rights are required.", category.Name);
                continue;
            }

            _logger.LogInformation("Scanning category {Category}.", category.Name);
            var items = await category.ScanAsync(cancellationToken).ConfigureAwait(false);
            results.AddRange(items);
        }

        return results;
    }

    public async Task<IReadOnlyList<CleanResult>> CleanAsync(
        IEnumerable<ICleanerCategory> selectedCategories,
        IReadOnlyList<ScanItem> items,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var results = new List<CleanResult>();
        var isAdmin = _adminChecker.IsRunningAsAdmin();

        foreach (var category in selectedCategories)
        {
            if (category.RequiresAdmin && !isAdmin)
            {
                _logger.LogWarning("Skipping clean for {Category} because admin rights are required.", category.Name);
                continue;
            }

            var categoryItems = items.Where(item => item.Category == category.Name).ToList();
            var result = await category.CleanAsync(categoryItems, dryRun, cancellationToken).ConfigureAwait(false);
            results.Add(result);
        }

        return results;
    }
}
