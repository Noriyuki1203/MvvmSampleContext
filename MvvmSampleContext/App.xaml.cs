using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MvvmSampleContext.Exceptions;
using MvvmSampleContext.Services;
using MvvmSampleContext.ViewModels;

namespace MvvmSampleContext;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var databaseService = new DatabaseService();
        var dialogService = new EditEmployeeDialogService();
        var mainViewModel = new MainViewModel(databaseService, dialogService);

        var window = new MainWindow
        {
            DataContext = mainViewModel,
        };

        MainWindow = window;
        window.Show();
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        HandleUnhandledException(e.Exception);
        e.Handled = true;
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        HandleUnhandledException(e.Exception);
        e.SetObserved();
    }

    private static void HandleUnhandledException(Exception exception)
    {
        var actual = Unwrap(exception);
        var message = BuildUserMessage(actual);
        MessageBox.Show(message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private static Exception Unwrap(Exception exception)
    {
        if (exception is AggregateException aggregate)
        {
            var flattened = aggregate.Flatten();
            return flattened.InnerException ?? aggregate;
        }

        return exception;
    }

    private static string BuildUserMessage(Exception exception)
    {
        return exception switch
        {
            BusinessException or DataAccessException => exception.Message,
            _ => $"予期しないエラーが発生しました。{Environment.NewLine}{exception.Message}",
        };
    }
}
