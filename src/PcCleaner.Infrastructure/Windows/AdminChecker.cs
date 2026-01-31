using System.Security.Principal;
using PcCleaner.Core.Abstractions;

namespace PcCleaner.Infrastructure.Windows;

public sealed class AdminChecker : IAdminChecker
{
    public bool IsRunningAsAdmin()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
