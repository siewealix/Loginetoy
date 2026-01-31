using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Extensions.Logging;
using PcCleaner.App.Services;
using PcCleaner.Core.Abstractions;
using PcCleaner.Core.Models;
using PcCleaner.Core.Services;

namespace PcCleaner.App.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly CleanerEngine _engine;
    private readonly ILogger<MainViewModel> _logger;
    private readonly IDialogService _dialogService;
    private CancellationTokenSource? _cts;
    private bool _isScanning;
    private bool _isCleaning;
    private bool _isDryRun = true;
    private string _statusMessage = "Prêt";
    private string _lastRunSummary = "Aucune opération encore.";
    private long _totalBytes;

    public MainViewModel(
        CleanerEngine engine,
        ILogger<MainViewModel> logger,
        IDialogService dialogService,
        TempFilesCategory tempFiles,
        WindowsTempCategory windowsTemp,
        ThumbnailCacheCategory thumbnails,
        RecycleBinCategory recycleBin,
        LogFilesCategory logs,
        CustomFoldersCategory customFolders)
    {
        _engine = engine;
        _logger = logger;
        _dialogService = dialogService;

        Categories = new ObservableCollection<CategoryViewModel>(new[]
        {
            new CategoryViewModel(tempFiles),
            new CategoryViewModel(windowsTemp),
            new CategoryViewModel(thumbnails),
            new CategoryViewModel(recycleBin),
            new CategoryViewModel(logs),
            new CategoryViewModel(customFolders)
        });

        Items = new ObservableCollection<ScanItemViewModel>();
        Items.CollectionChanged += OnItemsChanged;
        Errors = new ObservableCollection<CleanErrorViewModel>();
        AuditEntries = new ObservableCollection<string>();

        ScanCommand = new AsyncRelayCommand(ScanAsync, () => !IsBusy);
        CleanCommand = new AsyncRelayCommand(CleanAsync, () => !IsBusy && Items.Any(item => item.IsSelected));
        CancelCommand = new RelayCommand(Cancel, () => IsBusy);
    }

    public ObservableCollection<CategoryViewModel> Categories { get; }
    public ObservableCollection<ScanItemViewModel> Items { get; }
    public ObservableCollection<CleanErrorViewModel> Errors { get; }
    public ObservableCollection<string> AuditEntries { get; }

    public AsyncRelayCommand ScanCommand { get; }
    public AsyncRelayCommand CleanCommand { get; }
    public RelayCommand CancelCommand { get; }

    public bool IsDryRun
    {
        get => _isDryRun;
        set => SetProperty(ref _isDryRun, value);
    }

    public bool IsScanning
    {
        get => _isScanning;
        private set
        {
            if (SetProperty(ref _isScanning, value))
            {
                OnPropertyChanged(nameof(IsBusy));
                UpdateCommands();
            }
        }
    }

    public bool IsCleaning
    {
        get => _isCleaning;
        private set
        {
            if (SetProperty(ref _isCleaning, value))
            {
                OnPropertyChanged(nameof(IsBusy));
                UpdateCommands();
            }
        }
    }

    public bool IsBusy => IsScanning || IsCleaning;

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string LastRunSummary
    {
        get => _lastRunSummary;
        private set => SetProperty(ref _lastRunSummary, value);
    }

    public long TotalBytes
    {
        get => _totalBytes;
        private set
        {
            if (SetProperty(ref _totalBytes, value))
            {
                OnPropertyChanged(nameof(TotalBytesDisplay));
            }
        }
    }

    public string TotalBytesDisplay => FormatBytes(TotalBytes);

    private async Task ScanAsync()
    {
        IsScanning = true;
        StatusMessage = "Analyse en cours...";
        Items.Clear();
        Errors.Clear();
        AuditEntries.Add($"[{DateTime.Now:HH:mm:ss}] Démarrage du scan.");

        _cts = new CancellationTokenSource();

        try
        {
            var selected = Categories.Where(c => c.IsSelected).Select(c => c.Category).ToList();
            var results = await _engine.ScanAsync(selected, _cts.Token);

            foreach (var item in results)
            {
                Items.Add(new ScanItemViewModel(item));
            }

            TotalBytes = Items.Sum(item => item.SizeBytes);
            StatusMessage = $"Scan terminé : {Items.Count} éléments trouvés.";
            AuditEntries.Add($"[{DateTime.Now:HH:mm:ss}] Scan terminé ({Items.Count} éléments)." );
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Scan annulé.";
            AuditEntries.Add($"[{DateTime.Now:HH:mm:ss}] Scan annulé.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du scan");
            StatusMessage = "Erreur lors du scan. Consultez le log.";
            AuditEntries.Add($"[{DateTime.Now:HH:mm:ss}] Erreur de scan : {ex.Message}.");
        }
        finally
        {
            IsScanning = false;
        }
    }

    private async Task CleanAsync()
    {
        if (!Items.Any(item => item.IsSelected))
        {
            return;
        }

        if (!IsDryRun)
        {
            var confirmed = _dialogService.Confirm(
                "Cette action supprimera définitivement les éléments sélectionnés. Continuer ?",
                "Confirmation de nettoyage");
            if (!confirmed)
            {
                return;
            }
        }

        IsCleaning = true;
        StatusMessage = IsDryRun ? "Simulation en cours..." : "Nettoyage en cours...";
        Errors.Clear();
        AuditEntries.Add($"[{DateTime.Now:HH:mm:ss}] Démarrage du nettoyage (dry-run={IsDryRun}).");

        _cts = new CancellationTokenSource();

        try
        {
            var selectedCategories = Categories.Where(c => c.IsSelected).Select(c => c.Category).ToList();
            var selectedItems = Items.Where(item => item.IsSelected).Select(item => item.Item).ToList();

            var results = await _engine.CleanAsync(selectedCategories, selectedItems, IsDryRun, _cts.Token);

            var freedBytes = results.Sum(result => result.BytesFreed);
            var deletedItems = results.Sum(result => result.ItemsDeleted);
            var errorList = results.SelectMany(result => result.Errors).ToList();

            foreach (var error in errorList)
            {
                Errors.Add(new CleanErrorViewModel(error));
            }

            LastRunSummary = IsDryRun
                ? $"Simulation : {selectedItems.Count} éléments, {FormatBytes(freedBytes)} récupérables."
                : $"Nettoyage : {deletedItems} éléments supprimés, {FormatBytes(freedBytes)} libérés.";

            StatusMessage = IsDryRun ? "Simulation terminée." : "Nettoyage terminé.";
            AuditEntries.Add($"[{DateTime.Now:HH:mm:ss}] Nettoyage terminé ({deletedItems} supprimés)." );

            if (!IsDryRun)
            {
                _dialogService.ShowInfo(LastRunSummary, "Nettoyage terminé");
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Nettoyage annulé.";
            AuditEntries.Add($"[{DateTime.Now:HH:mm:ss}] Nettoyage annulé.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du nettoyage");
            StatusMessage = "Erreur lors du nettoyage. Consultez le log.";
            AuditEntries.Add($"[{DateTime.Now:HH:mm:ss}] Erreur de nettoyage : {ex.Message}.");
        }
        finally
        {
            IsCleaning = false;
        }
    }

    private void Cancel()
    {
        _cts?.Cancel();
    }

    private void UpdateCommands()
    {
        ScanCommand.RaiseCanExecuteChanged();
        CleanCommand.RaiseCanExecuteChanged();
        CancelCommand.RaiseCanExecuteChanged();
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (ScanItemViewModel item in e.NewItems)
            {
                item.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == nameof(ScanItemViewModel.IsSelected))
                    {
                        CleanCommand.RaiseCanExecuteChanged();
                    }
                };
            }
        }

        TotalBytes = Items.Sum(item => item.SizeBytes);
        CleanCommand.RaiseCanExecuteChanged();
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        double len = bytes;
        var order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
