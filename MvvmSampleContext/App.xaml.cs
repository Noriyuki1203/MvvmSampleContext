using System.Windows;
using MvvmSampleContext.Services;
using MvvmSampleContext.ViewModels;

namespace MvvmSampleContext;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var databaseService = new DatabaseService();
        var dialogService = new EditDroneDialogService();
        var mainViewModel = new MainViewModel(databaseService, dialogService);

        var window = new MainWindow
        {
            DataContext = mainViewModel,
        };

        MainWindow = window;
        window.Show();
    }
}
