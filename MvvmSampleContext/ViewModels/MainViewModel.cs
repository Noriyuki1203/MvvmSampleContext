using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    private readonly IEditEmployeeDialogService _employeeDialogService;
    private readonly IEditDepartmentDialogService _departmentDialogService;
    private readonly IEditFamilyDialogService _familyDialogService;

    [ObservableProperty]
    private DepartmentRecord? selectedDepartment;

    [ObservableProperty]
    private EmployeeRecord? selectedEmployee;

    [ObservableProperty]
    private FamilyMemberRecord? selectedFamily;

    [ObservableProperty]
    private bool isBusy;

    public MainViewModel(
        DatabaseService databaseService,
        IEditEmployeeDialogService employeeDialogService,
        IEditDepartmentDialogService departmentDialogService,
        IEditFamilyDialogService familyDialogService)
    {
        _databaseService = databaseService;
        _employeeDialogService = employeeDialogService;
        _departmentDialogService = departmentDialogService;
        _familyDialogService = familyDialogService;
    }

    public ObservableCollection<DepartmentRecord> Departments { get; } = new();

    public ObservableCollection<EmployeeRecord> Employees { get; } = new();

    public ObservableCollection<FamilyMemberRecord> Families { get; } = new();

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
            await LoadDepartmentsAsync();
            await LoadEmployeesForDepartmentAsync(SelectedDepartment);
            await LoadFamiliesForEmployeeAsync(SelectedEmployee);
        }
        catch (DataAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BusinessException("データの読み込みに失敗しました。", ex);
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
            var updated = _employeeDialogService.ShowEditDialog(editable);
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

    [RelayCommand]
    private async Task OpenDepartmentInputAsync()
    {
        try
        {
            var editable = new DepartmentRecord();
            var updated = _departmentDialogService.ShowInputDialog(editable);
            if (updated is null)
            {
                return;
            }

            await _databaseService.InsertDepartmentAsync(updated);
            await LoadDepartmentsAsync();
            SelectedDepartment = Departments.FirstOrDefault(x => x.Id == updated.Id) ?? Departments.FirstOrDefault();
            await LoadEmployeesForDepartmentAsync(SelectedDepartment);
        }
        catch (Exception ex)
        {
            throw new BusinessException("部署の追加に失敗しました。", ex);
        }
    }

    [RelayCommand]
    private async Task DeleteDepartmentAsync(DepartmentRecord? record)
    {
        var target = record ?? SelectedDepartment;
        if (target is null)
        {
            return;
        }

        try
        {
            await _databaseService.DeleteDepartmentAsync(target);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            throw new BusinessException("部署の削除に失敗しました。", ex);
        }
    }

    [RelayCommand]
    private async Task OpenEmployeeInputAsync()
    {
        try
        {
            var editable = new EmployeeRecord { Department = SelectedDepartment?.Name ?? string.Empty };
            var updated = _employeeDialogService.ShowEditDialog(editable);
            if (updated is null)
            {
                return;
            }

            await _databaseService.InsertAsync(updated);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            throw new BusinessException("従業員の追加に失敗しました。", ex);
        }
    }

    [RelayCommand]
    private async Task DeleteEmployeeAsync(EmployeeRecord? record)
    {
        var target = record ?? SelectedEmployee;
        if (target is null)
        {
            return;
        }

        try
        {
            await _databaseService.DeleteEmployeeAsync(target.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            throw new BusinessException("従業員の削除に失敗しました。", ex);
        }
    }

    [RelayCommand]
    private async Task OpenFamilyInputAsync()
    {
        if (SelectedEmployee is null)
        {
            throw new BusinessException("家族を追加する従業員を選択してください。");
        }

        try
        {
            var editable = new FamilyMemberRecord
            {
                EmployeeId = SelectedEmployee.Id,
            };

            var updated = _familyDialogService.ShowInputDialog(editable);
            if (updated is null)
            {
                return;
            }

            updated.EmployeeId = SelectedEmployee.Id;
            await _databaseService.InsertFamilyMemberAsync(updated);
            await LoadFamiliesForEmployeeAsync(SelectedEmployee);
        }
        catch (Exception ex)
        {
            throw new BusinessException("家族の追加に失敗しました。", ex);
        }
    }

    [RelayCommand]
    private async Task DeleteFamilyAsync(FamilyMemberRecord? record)
    {
        var target = record ?? SelectedFamily;
        if (target is null)
        {
            return;
        }

        try
        {
            await _databaseService.DeleteFamilyMemberAsync(target.Id);
            await LoadFamiliesForEmployeeAsync(SelectedEmployee);
        }
        catch (Exception ex)
        {
            throw new BusinessException("家族の削除に失敗しました。", ex);
        }
    }

    private bool CanEdit(EmployeeRecord? record)
    {
        return record is not null || SelectedEmployee is not null;
    }

    private async Task LoadDepartmentsAsync()
    {
        var items = await _databaseService.GetDepartmentsAsync();
        Departments.Clear();
        foreach (var item in items)
        {
            Departments.Add(item);
        }

        SelectedDepartment ??= Departments.FirstOrDefault();
    }

    private async Task LoadEmployeesForDepartmentAsync(DepartmentRecord? department)
    {
        var items = await _databaseService.GetEmployeesByDepartmentAsync(department?.Name);
        Employees.Clear();
        foreach (var item in items)
        {
            Employees.Add(item);
        }

        SelectedEmployee = Employees.FirstOrDefault();
    }

    private async Task LoadFamiliesForEmployeeAsync(EmployeeRecord? employee)
    {
        Families.Clear();
        if (employee is null)
        {
            return;
        }

        var items = await _databaseService.GetFamiliesByEmployeeAsync(employee.Id);
        foreach (var item in items)
        {
            Families.Add(item);
        }
    }

    partial void OnSelectedDepartmentChanged(DepartmentRecord? value)
    {
        _ = LoadEmployeesForDepartmentAsync(value);
    }

    partial void OnSelectedEmployeeChanged(EmployeeRecord? value)
    {
        EditCommand.NotifyCanExecuteChanged();
        _ = LoadFamiliesForEmployeeAsync(value);
    }
}
