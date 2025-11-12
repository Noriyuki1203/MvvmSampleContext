using System.Windows;
using MvvmSampleContext.Models;
using MvvmSampleContext.ViewModels;
using MvvmSampleContext.Views;

namespace MvvmSampleContext.Services;

public class EditDroneDialogService : IEditDroneDialogService
{
    public DroneRecord? ShowEditDialog(DroneRecord record)
    {
        var window = new EditDroneWindow
        {
            Owner = Application.Current?.MainWindow,
        };

        var viewModel = new EditDroneViewModel(record);
        window.DataContext = viewModel;

        var dialogResult = window.ShowDialog();
        return dialogResult == true ? window.Result : null;
    }
}
