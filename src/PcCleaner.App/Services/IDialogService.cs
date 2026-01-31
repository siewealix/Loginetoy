namespace PcCleaner.App.Services;

public interface IDialogService
{
    bool Confirm(string message, string title);
    void ShowInfo(string message, string title);
}
