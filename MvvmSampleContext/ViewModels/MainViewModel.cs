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
    private readonly IEditDroneDialogService _dialogService;

    [ObservableProperty]
    private DroneRecord? selectedDrone;

    [ObservableProperty]
    private bool isBusy;

    public MainViewModel(DatabaseService databaseService, IEditDroneDialogService dialogService)
    {
        _databaseService = databaseService;
        _dialogService = dialogService;
    }

    public ObservableCollection<DroneRecord> Drones { get; } = new();

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
            Drones.Clear();
            foreach (var item in items)
            {
                Drones.Add(item);
            }
        }
        finally
        {
            IsBusy = false;
            EditCommand.NotifyCanExecuteChanged();
        }
    }

    //[RelayCommand(CanExecute = nameof(CanEdit))]
    [RelayCommand]
    private async Task EditAsync(DroneRecord? record)
    {
        var target = record ?? SelectedDrone;
        if (target is null)
        {
            return;
        }

        var editable = (DroneRecord)target.Clone();
        var updated = _dialogService.ShowEditDialog(editable);
        if (updated is null)
        {
            return;
        }

        updated.Id = target.Id;
        await _databaseService.UpdateAsync(updated);
        await LoadAsync();
    }

    private bool CanEdit(DroneRecord? record)
    {
        return record is not null || SelectedDrone is not null;
    }

    partial void OnSelectedDroneChanged(DroneRecord? value)
    {
        EditCommand.NotifyCanExecuteChanged();
    }
}
