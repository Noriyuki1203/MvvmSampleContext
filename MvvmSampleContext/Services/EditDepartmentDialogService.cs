using System.Windows;
using MvvmSampleContext.Models;
using MvvmSampleContext.ViewModels;
using MvvmSampleContext.Views;

namespace MvvmSampleContext.Services;

public class EditDepartmentDialogService : IEditDepartmentDialogService
{
    public DepartmentRecord? ShowInputDialog(DepartmentRecord record)
    {
        var window = new EditDepartmentWindow
        {
            Owner = Application.Current?.MainWindow,
        };

        var viewModel = new EditDepartmentViewModel(record);
        window.DataContext = viewModel;

        var dialogResult = window.ShowDialog();
        return dialogResult == true ? viewModel.UpdatedRecord : null;
    }
}
