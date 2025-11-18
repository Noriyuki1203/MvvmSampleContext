using System.Windows;
using MvvmSampleContext.Models;
using MvvmSampleContext.ViewModels;
using MvvmSampleContext.Views;

namespace MvvmSampleContext.Services;

public class EditFamilyDialogService : IEditFamilyDialogService
{
    public FamilyMemberRecord? ShowInputDialog(FamilyMemberRecord record)
    {
        var window = new EditFamilyWindow
        {
            Owner = Application.Current?.MainWindow,
        };

        var viewModel = new EditFamilyViewModel(record);
        window.DataContext = viewModel;

        var dialogResult = window.ShowDialog();
        return dialogResult == true ? viewModel.UpdatedRecord : null;
    }
}
