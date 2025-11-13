using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MvvmSampleContext.Models;

namespace MvvmSampleContext.ViewModels;

public partial class EditEmployeeViewModel : ObservableObject
{
    private readonly EmployeeRecord _original;
    private readonly IMessenger _messenger;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string employeeNumber = string.Empty;

    [ObservableProperty]
    private string department = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public EditEmployeeViewModel(EmployeeRecord record, IMessenger? messenger = null)
    {
        _messenger = messenger ?? WeakReferenceMessenger.Default;
        _original = (EmployeeRecord)record.Clone();
        Name = _original.Name;
        EmployeeNumber = _original.EmployeeNumber;
        Department = _original.Department;
    }

    public string Title => _original.Id == 0 ? "従業員の追加" : $"従業員の編集 (ID: {_original.Id})";

    public EmployeeRecord? UpdatedRecord { get; private set; }

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(EmployeeNumber))
        {
            ErrorMessage = "名前と社員番号は必須です。";
            return;
        }

        ErrorMessage = string.Empty;

        var updated = new EmployeeRecord
        {
            Id = _original.Id,
            Name = Name.Trim(),
            EmployeeNumber = EmployeeNumber.Trim(),
            Department = Department.Trim(),
        };

        UpdatedRecord = updated;
        _messenger.Send(new DialogCloseRequestedMessage(new DialogCloseRequestedEventArgs(true, updated)));
    }

    [RelayCommand]
    private void Cancel()
    {
        UpdatedRecord = null;
        _messenger.Send(new DialogCloseRequestedMessage(new DialogCloseRequestedEventArgs(false, null)));
    }
}
