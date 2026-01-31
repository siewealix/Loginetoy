using PcCleaner.Core.Models;

namespace PcCleaner.App.ViewModels;

public sealed class CleanErrorViewModel
{
    public CleanErrorViewModel(CleanError error)
    {
        Path = error.Path;
        Message = error.Message;
    }

    public string Path { get; }
    public string Message { get; }
}
