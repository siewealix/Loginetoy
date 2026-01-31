using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PcCleaner.Core.Abstractions;
using PcCleaner.Core.Categories;
using PcCleaner.Core.Models;
using PcCleaner.Core.Security;
using PcCleaner.Core.Services;
using PcCleaner.Infrastructure.FileSystem;
using PcCleaner.Infrastructure.Windows;
using Serilog;
using Serilog.Extensions.Logging;

namespace PcCleaner.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var settings = configuration.GetSection("CleanerSettings").Get<CleanerSettings>() ?? new CleanerSettings();
        settings = new CleanerSettings
        {
            CustomFolders = settings.CustomFolders.Select(ExpandPath).ToList(),
            LogFolders = settings.LogFolders.Select(ExpandPath).ToList(),
            Exclusions = settings.Exclusions.ToList()
        };

        var logFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PcCleaner",
            "Logs");
        Directory.CreateDirectory(logFolder);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(Path.Combine(logFolder, "pccleaner-.log"), rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(settings);
        serviceCollection.AddSingleton<IFileSystem, LocalFileSystem>();
        serviceCollection.AddSingleton<IAdminChecker, AdminChecker>();
        serviceCollection.AddSingleton<IRecycleBinService, RecycleBinService>();

        var allowedRoots = new List<string>
        {
            Path.GetTempPath(),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Explorer")
        };

        allowedRoots.AddRange(settings.CustomFolders);
        allowedRoots.AddRange(settings.LogFolders);

        var safePolicy = SafePathPolicy.CreateDefault(allowedRoots);
        serviceCollection.AddSingleton(safePolicy);
        serviceCollection.AddSingleton(new ExclusionPolicy(settings.Exclusions));
        serviceCollection.AddSingleton<FileScanner>();
        serviceCollection.AddSingleton<CleanExecutor>();
        serviceCollection.AddSingleton<CleanerEngine>();

        serviceCollection.AddSingleton<TempFilesCategory>();
        serviceCollection.AddSingleton<WindowsTempCategory>();
        serviceCollection.AddSingleton<ThumbnailCacheCategory>();
        serviceCollection.AddSingleton<RecycleBinCategory>();
        serviceCollection.AddSingleton<LogFilesCategory>();
        serviceCollection.AddSingleton<CustomFoldersCategory>();
        serviceCollection.AddSingleton<ICleanerCategory>(sp => sp.GetRequiredService<TempFilesCategory>());
        serviceCollection.AddSingleton<ICleanerCategory>(sp => sp.GetRequiredService<WindowsTempCategory>());
        serviceCollection.AddSingleton<ICleanerCategory>(sp => sp.GetRequiredService<ThumbnailCacheCategory>());
        serviceCollection.AddSingleton<ICleanerCategory>(sp => sp.GetRequiredService<RecycleBinCategory>());
        serviceCollection.AddSingleton<ICleanerCategory>(sp => sp.GetRequiredService<LogFilesCategory>());
        serviceCollection.AddSingleton<ICleanerCategory>(sp => sp.GetRequiredService<CustomFoldersCategory>());

        serviceCollection.AddSingleton<ILoggerFactory>(_ => new SerilogLoggerFactory(Log.Logger));
        serviceCollection.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        serviceCollection.AddSingleton<Services.IDialogService, Services.DialogService>();
        serviceCollection.AddSingleton<ViewModels.MainViewModel>();

        Services = serviceCollection.BuildServiceProvider();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    private static string ExpandPath(string path)
    {
        return Environment.ExpandEnvironmentVariables(path);
    }
}
