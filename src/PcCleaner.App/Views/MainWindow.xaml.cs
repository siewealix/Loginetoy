using System.Windows;
using PcCleaner.App.ViewModels;

namespace PcCleaner.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.Services.GetService(typeof(MainViewModel));
    }
}
