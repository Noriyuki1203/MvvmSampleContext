using System.Windows;
using MvvmSampleContext.Models;
using MvvmSampleContext.ViewModels;
using MvvmSampleContext.Views;

namespace MvvmSampleContext.Services;

public class EditEmployeeDialogService : IEditEmployeeDialogService
{
    public EmployeeRecord? ShowEditDialog(EmployeeRecord record)
    {
        var window = new EditEmployeeWindow
        {
            Owner = Application.Current?.MainWindow,
        };

        var viewModel = new EditEmployeeViewModel(record);
        window.DataContext = viewModel;

        var dialogResult = window.ShowDialog();
        return dialogResult == true ? viewModel.UpdatedRecord : null;
    }
}
