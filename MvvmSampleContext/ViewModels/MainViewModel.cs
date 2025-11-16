using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmSampleContext.Exceptions;
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
        catch (DataAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BusinessException("従業員一覧の読み込みに失敗しました。", ex);
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

        try
        {
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
        catch (BusinessException)
        {
            throw;
        }
        catch (DataAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BusinessException("従業員情報の更新に失敗しました。", ex);
        }
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
