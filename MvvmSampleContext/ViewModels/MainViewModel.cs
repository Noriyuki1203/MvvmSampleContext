using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmSampleContext.Models;
using MvvmSampleContext.Services;

namespace MvvmSampleContext.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private readonly IEditEmployeeDialogService _dialogService;

    [ObservableProperty]
    private EmployeeRecord? selectedEmployee;

    [ObservableProperty]
    private bool isBusy;

    public MainViewModel(DatabaseService databaseService, IEditEmployeeDialogService dialogService)
    {
        _databaseService = databaseService;
        _dialogService = dialogService;
    }

    public ObservableCollection<EmployeeRecord> Employees { get; } = new();

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var items = await _databaseService.GetAllAsync();
            Employees.Clear();
            foreach (var item in items)
            {
                Employees.Add(item);
            }
        }
        finally
        {
            IsBusy = false;
            EditCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanEdit))]
    private async Task EditAsync(EmployeeRecord? record)
    {
        var target = record ?? SelectedEmployee;
        if (target is null)
        {
            return;
        }

        var editable = (EmployeeRecord)target.Clone();
        var updated = _dialogService.ShowEditDialog(editable);
        if (updated is null)
        {
            return;
        }

        updated.Id = target.Id;
        await _databaseService.UpdateAsync(updated);
        await LoadAsync();
    }

    private bool CanEdit(EmployeeRecord? record)
    {
        return record is not null || SelectedEmployee is not null;
    }

    partial void OnSelectedEmployeeChanged(EmployeeRecord? value)
    {
        EditCommand.NotifyCanExecuteChanged();
    }
}
