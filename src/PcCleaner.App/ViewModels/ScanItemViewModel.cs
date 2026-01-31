using PcCleaner.Core.Models;

namespace PcCleaner.App.ViewModels;

public sealed class ScanItemViewModel : ViewModelBase
{
    private bool _isSelected;

    public ScanItemViewModel(ScanItem item)
    {
        Item = item;
        _isSelected = true;
    }

    public ScanItem Item { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public string Path => Item.Path;
    public string Category => Item.Category;
    public string Reason => Item.Reason;
    public long SizeBytes => Item.SizeBytes;
    public string SafeLevel => Item.SafeLevel.ToString();
}
