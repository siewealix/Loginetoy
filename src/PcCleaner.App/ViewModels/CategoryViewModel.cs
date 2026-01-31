using PcCleaner.Core.Abstractions;

namespace PcCleaner.App.ViewModels;

public sealed class CategoryViewModel : ViewModelBase
{
    private bool _isSelected;

    public CategoryViewModel(ICleanerCategory category)
    {
        Category = category;
        _isSelected = category.IsEnabledByDefault;
    }

    public ICleanerCategory Category { get; }

    public string Name => Category.Name;
    public string Description => Category.Description;
    public bool RequiresAdmin => Category.RequiresAdmin;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}
